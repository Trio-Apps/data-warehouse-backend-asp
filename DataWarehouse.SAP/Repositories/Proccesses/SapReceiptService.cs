using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
   
    public class SapReceiptService : ISapReceiptService
    {
        private readonly IBaseSap<SapReceiptOrderDto> _sap;
        private readonly ILogger<SapReceiptService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapReceiptService(
            IBaseSap<SapReceiptOrderDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapReceiptService> logger)
        {
            _sap = sap;
            _logger = logger;
            _context = context;
        }

        public async Task<string> SyncReceiptAsync(int sapId)
        {
            const int batchSize = 200;

            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ Approved IDs من جدول الـ Process
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.Receipt && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();

                // ✅ هات أول دفعة من اللي لسه Processing
                var batch = await _context.ReceiptPurchaseOrders
                    .AsTracking()
                    .Include(o => o.Warehouse)
                    .Include(o => o.Supplier)
                    .Include(o => o.PurchaseOrder)
                    .Include(o => o.ReceiptPurchaseOrderItems)
                        .ThenInclude(i => i.Item)
                    .Include(o => o.ReceiptPurchaseOrderItems)
                        .ThenInclude(i => i.ReceiptPurchaseOrderBatches) // ✅ مهم جدًا للباتش
                    .Where(o =>
                        o.Status == GeneralStatus.Processing &&
                        o.Warehouse.SapId == sapId &&
                        approvedIdsQuery.Contains(o.ReceiptPurchaseOrderId))
                    .OrderBy(o => o.ReceiptPurchaseOrderId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                int taskSuccess = 0;
                int taskFail = 0;

                foreach (var order in batch)
                {
                    var (_, success, body, error) = await ProcessReceiptOrderAsync(sapId, order);

                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;
                        var res = JsonSerializer.Deserialize<PurchaseAsBasedOn>(body,
                      new JsonSerializerOptions
                      {
                          PropertyNameCaseInsensitive = true
                      });

                        order.DocEntry = res.DocEntry;
                        order.DocNum = res.DocNum;
                        order.DocType = res.DocType;
                        var sapLinesByItem = res.DocumentLines
                        .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                        .ToDictionary(x => x.ItemCode!, x => x.LineNum);

                        foreach (var it in order.ReceiptPurchaseOrderItems)
                        {
                            it.Status = GeneralItemStatus.Received;
                            it.ErrorMessage = null;
                            // لو عندك ItemCode في it.Item.ItemCode
                            var itemCode = it.Item?.ItemCode;
                            if (itemCode != null && sapLinesByItem.TryGetValue(itemCode, out var lineNum))
                                it.LineNum = lineNum;

                            _context.Entry(it).Property(x => x.Status).IsModified = true;
                            _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                            _context.Entry(it).Property(x => x.LineNum).IsModified = true;

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

                        foreach (var it in order.ReceiptPurchaseOrderItems)
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
                    _logger.LogInformation("Receipt batch SaveChanges affected rows={Affected}", affected);

                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency issue while saving receipt batch for sapId={SapId}", sapId);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "DB update issue while saving receipt batch for sapId={SapId}", sapId);
                    throw;
                }

                totalSuccess += taskSuccess;
                totalFail += taskFail;

                _logger.LogInformation(
                    "Receipt batch processed for sapId={SapId}: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    sapId, taskSuccess, taskFail, totalSuccess, totalFail
                );
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No approved receipt orders to sync";


            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }

        private async Task<(ReceiptPurchaseOrder order, bool success, string res, string? error)>
            ProcessReceiptOrderAsync(int sapId, ReceiptPurchaseOrder order)
        {
            try
            {
                // ✅ Validate minimum
                if (order.Supplier == null || string.IsNullOrWhiteSpace(order.Supplier.SupplierCode))
                    throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: SupplierCode is missing.");


                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: WarehouseCode is missing.");


                if (order.ReceiptPurchaseOrderItems == null || order.ReceiptPurchaseOrderItems.Count == 0)
                    throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: No items.");

                // ✅ Build SAP Receipt DTO (PurchaseDeliveryNotes = GRPO)
                var sapDto = new SapReceiptOrderDto
                {
                    CardCode = order.Supplier.SupplierCode,
                    DocDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    NumAtCard = $"ro-{order.ReceiptPurchaseOrderId}",
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.ReceiptPurchaseOrderItems.OrderBy(x => x.ReceiptPurchaseOrderItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: Item missing on line {line.ReceiptPurchaseOrderItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapReceiptOrderLineDto
                    {
                        ItemCode = itemCode,
                        Quantity = line.Quantity,
                        WarehouseCode = order.Warehouse.WarehouseCode,
                        UoMEntry = line.UoMEntry > 0 ? line.UoMEntry : null,
                        BarCode = line.BarCode,
                        UnitPrice = line.UnitPrice,
                        BatchNumbers = new List<SapBatchNumberDto>()
                    };

                    // ✅ Map batches (لو مفيش batches: ابعتها [] أو null حسب إعدادات SAP عندك)
                    var batches = line.ReceiptPurchaseOrderBatches ?? new List<ReceiptPurchaseOrderBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"Receipt#{order.ReceiptPurchaseOrderId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null
                        });
                    }

                    // ✅ Optional: validate sum batches == line quantity (لو عندك batch-managed لازم)
                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"Receipt#{order.ReceiptPurchaseOrderId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.DocumentLines.Add(sapLine);
                }

                var url = "PurchaseDeliveryNotes";

                // ✅ POST to SAP
                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("Receipt synced successfully. ReceiptId={Id}, sapResponse={Resp}",
                    order.ReceiptPurchaseOrderId, responseJson);

                return (order, true,responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync receipt order: ReceiptId={Id}", order.ReceiptPurchaseOrderId);
                return (order, false,"", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(ReceiptPurchaseOrder order)
        {
            // لو عندك BPLId على الـ Warehouse
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        // =========================
        // SAP DTOs (Receipt / GRPO)
        // =========================

        public sealed class SapReceiptOrderDto
        {
            public string CardCode { get; set; } = string.Empty;
            public string DocDate { get; set; } = string.Empty;
            public string? NumAtCard { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapReceiptOrderLineDto> DocumentLines { get; set; } = new();
        }

        public sealed class SapReceiptOrderLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public int? UoMEntry { get; set; }
            public string? BarCode { get; set; }
            public decimal? UnitPrice { get; set; }

            public List<SapBatchNumberDto>? BatchNumbers { get; set; } // ✅
        }

        public sealed class SapBatchNumberDto
        {
            public string BatchNumber { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string? ExpiryDate { get; set; } // yyyy-MM-dd
        }

        public class ReceiptAsBasedOn
        {

            public int? DocEntry { get; set; }
            public int? DocNum { set; get; }
            public string? DocType { get; set; }

            public List<PurchaseItemAsBasedOn> DocumentLines { get; set; } = new();


        }
        public class ReceiptItemAsBasedOn
        {
            public int LineNum { get; set; }
            public string ItemCode { set; get; } = string.Empty;

        }
    }

}
