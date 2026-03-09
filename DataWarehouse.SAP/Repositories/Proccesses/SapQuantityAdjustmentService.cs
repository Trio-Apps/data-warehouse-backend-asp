using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DataWarehouse.SAP.Interfaces.Proccesses;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
   
    public class SapQuantityAdjustmentService : ISapQuantityAdjustmentService
    {
        private readonly IBaseSap<SapInventoryGenExitDto> _sap;
        private readonly DataWarehouseDbContext _context;
        private readonly ILogger<SapQuantityAdjustmentService> _logger;

        public SapQuantityAdjustmentService(
            IBaseSap<SapInventoryGenExitDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapQuantityAdjustmentService> logger)
        {
            _sap = sap;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncQuantityAdjustmentsAsync(int quantityAdjustmentStockId)
        {
            // ✅ Approved IDs من جدول الـ Process
            var approvedIdsQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.QuantityAdjustment &&
                            p.Status == ProcessStatus.Approved)
                .Select(p => p.ReferenceId)
                .Distinct();

            // ✅ هات الأوردر المطلوب (واحد بس) اللي لسه Processing + Approved
            var order = await _context.QuantityAdjustmentStocks
                .AsTracking()
                .Include(o => o.Warehouse)
                .Include(o => o.QuantityAdjustmentStockItems)
                    .ThenInclude(i => i.Item)
                .Include(o => o.QuantityAdjustmentStockItems)
                    .ThenInclude(i => i.QuantityAdjustmentStockBatches)
                .Where(o =>
                    o.Status == GeneralStatus.Processing &&
                    approvedIdsQuery.Contains(o.QuantityAdjustmentStockId))
                .FirstOrDefaultAsync(o => o.QuantityAdjustmentStockId == quantityAdjustmentStockId);

            if (order == null)
                return "This quantity adjustment must be Approved to send it to Sap";

            var sapId = order.Warehouse?.SapId ?? 0;
            if (sapId <= 0)
                throw new InvalidOperationException(
                    $"QuantityAdjustmentStock#{order.QuantityAdjustmentStockId}: Warehouse.SapId is missing/invalid.");

            var (_, success, body, error) = await ProcessQuantityAdjustmentAsync(sapId, order);

            if (success)
            {
                order.Status = GeneralStatus.Completed;

                InventoryGenExitResponse? res = null;
                try
                {
                    res = JsonSerializer.Deserialize<InventoryGenExitResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not deserialize InventoryGenExits response for QuantityAdjustmentStockId={Id}",
                        order.QuantityAdjustmentStockId);
                }

                if (res != null)
                {
                    order.DocEntry = res.DocEntry;
                    order.DocNum = res.DocNum;
                    order.DocType = res.DocType;

                    var sapLinesByItem = (res.DocumentLines ?? new List<InventoryGenExitLineResponse>())
                        .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                        .GroupBy(x => x.ItemCode!)
                        .ToDictionary(g => g.Key, g => g.Last().LineNum);

                    foreach (var it in order.QuantityAdjustmentStockItems)
                    {
                        it.Status = GeneralItemStatus.Received;
                        it.ErrorMessage = null;

                        var itemCode = it.Item?.ItemCode;
                        if (!string.IsNullOrWhiteSpace(itemCode) && sapLinesByItem.TryGetValue(itemCode!, out var lineNum))
                            it.LineNum = lineNum;

                        _context.Entry(it).Property(x => x.Status).IsModified = true;
                        _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                        _context.Entry(it).Property(x => x.LineNum).IsModified = true;
                    }

                    _context.Entry(order).Property(x => x.DocEntry).IsModified = true;
                    _context.Entry(order).Property(x => x.DocNum).IsModified = true;
                    _context.Entry(order).Property(x => x.DocType).IsModified = true;
                }
                else
                {
                    // حتى لو response اتقراش: على الأقل حدّث statuses
                    foreach (var it in order.QuantityAdjustmentStockItems)
                    {
                        it.Status = GeneralItemStatus.Received;
                        it.ErrorMessage = null;

                        _context.Entry(it).Property(x => x.Status).IsModified = true;
                        _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                    }
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                // لو عندك ErrorMessage في order:
                 _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }
            else
            {
                order.Status = GeneralStatus.PartiallyFailed;
                // لو عندك ErrorMessage في order:
                 order.ErrorMessage = error;

                foreach (var it in order.QuantityAdjustmentStockItems)
                {
                    it.Status = GeneralItemStatus.Failed;
                    it.ErrorMessage = error;

                    _context.Entry(it).Property(x => x.Status).IsModified = true;
                    _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                // لو عندك ErrorMessage:
                 _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }

            try
            {
                _context.ChangeTracker.DetectChanges();
                var affected = await _context.SaveChangesAsync();
                _logger.LogInformation("QuantityAdjustment SaveChanges affected rows={Affected} quantityAdjustmentStockId={Id}",
                    affected, order.QuantityAdjustmentStockId);

                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency issue while saving quantity adjustment for sapId={SapId}, quantityAdjustmentStockId={Id}",
                    sapId, order.QuantityAdjustmentStockId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "DB update issue while saving quantity adjustment for sapId={SapId}, quantityAdjustmentStockId={Id}",
                    sapId, order.QuantityAdjustmentStockId);
                throw;
            }

            return "Sync completed.";
        }
        private async Task<(QuantityAdjustmentStock order, bool success, string res, string? error)>
            ProcessQuantityAdjustmentAsync(int sapId, QuantityAdjustmentStock order)
        {
            try
            {
                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: WarehouseCode is missing.");

                if (order.QuantityAdjustmentStockItems == null || order.QuantityAdjustmentStockItems.Count == 0)
                    throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: No items.");

                var sapDto = new SapInventoryGenExitDto
                {
                    DocDate = ConvertToSapDateFormat(order.PostingDate ?? order.CreatedAt),
                    Comments = order.Comment ?? $"Quantity Adjustment #{order.QuantityAdjustmentStockId}",
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.QuantityAdjustmentStockItems.OrderBy(x => x.QuantityAdjustmentStockItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: Item missing on line {line.QuantityAdjustmentStockItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapInventoryGenExitLineDto
                    {
                        ItemCode = itemCode,
                        ItemDescription = line.Item.ItemName,
                        Quantity = line.Quantity,
                        WarehouseCode = order.Warehouse.WarehouseCode,
                        UoMEntry = line.UoMEntry > 0 ? line.UoMEntry : null,
                        BarCode = line.BarCode,
                        UnitPrice = line.UnitPrice,
                        BatchNumbers = new List<SapBatchNumberDto>()
                    };

                    // ✅ batches (لو item batch-managed)
                    var batches = line.QuantityAdjustmentStockBatches ?? new List<QuantityAdjustmentStockBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"Adj#{order.QuantityAdjustmentStockId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null
                        });
                    }

                    // ✅ validate sum batches == line qty if batches exist
                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"Adj#{order.QuantityAdjustmentStockId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.DocumentLines.Add(sapLine);
                }

                var url = "InventoryGenExits";
                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("Quantity Adjustment synced successfully. QuantityAdjustmentStockId={Id}, sapResponse={Resp}",
                    order.QuantityAdjustmentStockId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync quantity adjustment: QuantityAdjustmentStockId={Id}", order.QuantityAdjustmentStockId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(QuantityAdjustmentStock order)
        {
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        // =========================
        // SAP DTOs (InventoryGenExits)
        // =========================

        public sealed class SapInventoryGenExitDto
        {
            public string DocDate { get; set; } = string.Empty;
            public string? Comments { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapInventoryGenExitLineDto> DocumentLines { get; set; } = new();
        }

        public sealed class SapInventoryGenExitLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal Quantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public int? UoMEntry { get; set; }
            public string? BarCode { get; set; }
            public decimal? UnitPrice { get; set; }
            public List<SapBatchNumberDto>? BatchNumbers { get; set; }
        }

        public sealed class SapBatchNumberDto
        {
            public string BatchNumber { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string? ExpiryDate { get; set; } // yyyy-MM-dd
        }

        // =========================
        // SAP Response
        // =========================

        public sealed class InventoryGenExitResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<InventoryGenExitLineResponse>? DocumentLines { get; set; }
        }

        public sealed class InventoryGenExitLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
