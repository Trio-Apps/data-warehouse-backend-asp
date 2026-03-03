using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DataWarehouse.SAP.Interfaces.Proccesses;


namespace DataWarehouse.SAP.Repositories.Proccesses
{
    public class SapGoodsReturnService : ISapGoodsReturnService
    {
        private readonly IBaseSap<SapPurchaseReturnDto> _sap;
        private readonly ILogger<SapGoodsReturnService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapGoodsReturnService(
            IBaseSap<SapPurchaseReturnDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapGoodsReturnService> logger)
        {
            _sap = sap;
            _context = context;
            _logger = logger;
        }


        public async Task<string> SyncGoodsReturnAsync(int sapId)
        {
            const int batchSize = 200;

            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ Approved IDs من جدول الـ Process
                // ✳️ لو ProcessType عندك اسمه مختلف غيّره هنا
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.GoodsReturn && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();

                // ✅ هات أول دفعة من اللي لسه Processing
                var batch = await _context.GoodsReturnOrders
                    .AsTracking()
                    .Include(o => o.Warehouse)
                    .Include(o => o.Supplier)
                    .Include(o => o.GoodsReturnOrderItems)
                        .ThenInclude(i => i.Item)
                    .Include(o => o.GoodsReturnOrderItems)
                        .ThenInclude(i => i.GoodsReturnOrderBatches)
                    .Where(o =>
                        o.Status == GeneralStatus.Processing &&
                        o.Warehouse.SapId == sapId &&
                        approvedIdsQuery.Contains(o.GoodsReturnOrderId))
                    .OrderBy(o => o.GoodsReturnOrderId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                int taskSuccess = 0;
                int taskFail = 0;

                foreach (var order in batch)
                {
                    var (_, success, body, error) = await ProcessGoodsReturnAsync(sapId, order);

                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;
                        order.ErrorMessage = null;

                        // ✅ SAP response (DocEntry/DocNum/DocType/DocumentLines)
                        PurchaseReturnResponse? res = null;
                        try
                        {
                            res = JsonSerializer.Deserialize<PurchaseReturnResponse>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (Exception ex)
                        {
                            // لو الـ body مش نفس الشكل المتوقع، منخليش العملية تفشل — بس نسجل.
                            _logger.LogWarning(ex, "Could not deserialize PurchaseReturn response for GoodsReturnOrderId={Id}", order.GoodsReturnOrderId);
                        }

                        if (res != null)
                        {
                            order.DocEntry = res.DocEntry;
                            order.DocNum = res.DocNum;
                            order.DocType = res.DocType;

                            // ItemCode -> LineNum mapping (لو في duplicated item codes، آخر واحد هيفوز)
                            var sapLinesByItem = (res.DocumentLines ?? new List<PurchaseReturnLineResponse>())
                                .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                                .GroupBy(x => x.ItemCode!)
                                .ToDictionary(g => g.Key, g => g.Last().LineNum);

                            foreach (var it in order.GoodsReturnOrderItems)
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
                        }
                        else
                        {
                            // حتى لو مش عرفنا نقرأ response، نعتبره success طالما SAP رجّع 200
                            foreach (var it in order.GoodsReturnOrderItems)
                            {
                                it.Status = GeneralItemStatus.Received;
                                it.ErrorMessage = null;

                                _context.Entry(it).Property(x => x.Status).IsModified = true;
                                _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                            }
                        }

                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
                        _context.Entry(order).Property(x => x.DocEntry).IsModified = true;
                        _context.Entry(order).Property(x => x.DocNum).IsModified = true;
                        _context.Entry(order).Property(x => x.DocType).IsModified = true;

                        taskSuccess++;
                    }
                    else
                    {
                        order.Status = GeneralStatus.PartiallyFailed;
                        order.ErrorMessage = error;

                        foreach (var it in order.GoodsReturnOrderItems)
                        {
                            it.Status = GeneralItemStatus.Failed;
                            it.ErrorMessage = error;

                            _context.Entry(it).Property(x => x.Status).IsModified = true;
                            _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                        }

                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;

                        taskFail++;
                    }
                }

                try
                {
                    _context.ChangeTracker.DetectChanges();
                    var affected = await _context.SaveChangesAsync();
                    _logger.LogInformation("GoodsReturn batch SaveChanges affected rows={Affected}", affected);

                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency issue while saving goods return batch for sapId={SapId}", sapId);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "DB update issue while saving goods return batch for sapId={SapId}", sapId);
                    throw;
                }

                totalSuccess += taskSuccess;
                totalFail += taskFail;

                _logger.LogInformation(
                    "GoodsReturn batch processed for sapId={SapId}: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    sapId, taskSuccess, taskFail, totalSuccess, totalFail
                );
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No approved goods return orders to sync";

            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }

        private async Task<(GoodsReturnOrder order, bool success, string res, string? error)>
            ProcessGoodsReturnAsync(int sapId, GoodsReturnOrder order)
        {
            try
            {
                // ✅ Validate minimum
                if (order.Supplier == null || string.IsNullOrWhiteSpace(order.Supplier.SupplierCode))
                    throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: SupplierCode is missing.");

                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: WarehouseCode is missing.");

                if (order.GoodsReturnOrderItems == null || order.GoodsReturnOrderItems.Count == 0)
                    throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: No items.");

                // ✅ Build SAP Purchase Return DTO (without base)
                var sapDto = new SapPurchaseReturnDto
                {
                    CardCode = order.Supplier.SupplierCode,
                    DocDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    NumAtCard = $"gr-{order.GoodsReturnOrderId}",
                    Comments = order.Comment ?? string.Empty,
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.GoodsReturnOrderItems.OrderBy(x => x.GoodsReturnOrderItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: Item missing on line {line.GoodsReturnOrderItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapPurchaseReturnLineDto
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

                    // ✅ Map batches
                    var batches = line.GoodsReturnOrderBatches ?? new List<GoodsReturnOrderBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"Return#{order.GoodsReturnOrderId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity, // ✅ رقم مش string
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null
                        });
                    }

                    // ✅ validate sum batches == line qty (لو عندك batch-managed items)
                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"Return#{order.GoodsReturnOrderId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.DocumentLines.Add(sapLine);
                }

                var url = "PurchaseReturns"; // ✅ endpoint اللي طلبته

                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("Goods return synced successfully. GoodsReturnOrderId={Id}, sapResponse={Resp}",
                    order.GoodsReturnOrderId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync goods return: GoodsReturnOrderId={Id}", order.GoodsReturnOrderId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(GoodsReturnOrder order)
        {
            // لو عندك BPLId على الـ Warehouse
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        // =========================
        // SAP DTOs (PurchaseReturns)
        // =========================

        public sealed class SapPurchaseReturnDto
        {
            public string CardCode { get; set; } = string.Empty;
            public string DocDate { get; set; } = string.Empty;
            public string? NumAtCard { get; set; }
            public string? Comments { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapPurchaseReturnLineDto> DocumentLines { get; set; } = new();
        }

        public sealed class SapPurchaseReturnLineDto
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
        // SAP Response model
        // =========================

        public sealed class PurchaseReturnResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<PurchaseReturnLineResponse>? DocumentLines { get; set; }
        }

        public sealed class PurchaseReturnLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
