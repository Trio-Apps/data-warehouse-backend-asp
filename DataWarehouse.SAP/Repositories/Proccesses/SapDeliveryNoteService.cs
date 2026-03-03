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
  

  
    public class SapDeliveryNoteService : ISapDeliveryNoteService
    {
        private readonly IBaseSap<SapDeliveryNoteDto> _sap;
        private readonly ILogger<SapDeliveryNoteService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapDeliveryNoteService(
            IBaseSap<SapDeliveryNoteDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapDeliveryNoteService> logger)
        {
            _sap = sap;
            _logger = logger;
            _context = context;
        }

        public async Task<string> SyncDeliveryNotesAsync(int sapId)
        {
            const int batchSize = 200;

            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ Approved IDs من جدول الـ Process
                // ✳️ غيّر ProcessType.DeliveryNote لو enum عندك مختلف
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.DeliveryNote && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();

                // ✅ هات أول دفعة من اللي لسه Processing
                var batch = await _context.DeliveryNoteOrders
                    .AsTracking()
                    .Include(o => o.Warehouse)
                    .Include(o => o.Customer)
                    .Include(o => o.SalesOrder) // عشان BaseEntry لو based on SO
                    .Include(o => o.DeliveryNoteItems)
                        .ThenInclude(i => i.Item)
                    .Include(o => o.DeliveryNoteItems)
                        .ThenInclude(i => i.SalesOrderItem) // عشان BaseLine لو based on SO
                    .Include(o => o.DeliveryNoteItems)
                        .ThenInclude(i => i.DeliveryNoteBatches)
                    .Where(o =>
                        o.Status == GeneralStatus.Processing &&
                        o.Warehouse.SapId == sapId &&
                        approvedIdsQuery.Contains(o.DeliveryNoteOrderId))
                    .OrderBy(o => o.DeliveryNoteOrderId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                int taskSuccess = 0;
                int taskFail = 0;

                foreach (var order in batch)
                {
                    var (_, success, body, error) = await ProcessDeliveryNoteAsync(sapId, order);

                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;
                        order.ErrorMessage = null;

                        // ✅ Response: نقرأ LineNum لكل Item (اختياري)
                        DeliveryNoteResponse? res = null;
                        try
                        {
                            res = JsonSerializer.Deserialize<DeliveryNoteResponse>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Could not deserialize DeliveryNotes response for DeliveryNoteOrderId={Id}",
                                order.DeliveryNoteOrderId);
                        }

                        Dictionary<string, int> sapLinesByItem = new();
                        if (res?.DocumentLines != null)
                        {
                            sapLinesByItem = res.DocumentLines
                                .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                                .GroupBy(x => x.ItemCode!)
                                .ToDictionary(g => g.Key, g => g.Last().LineNum);
                        }

                        foreach (var it in order.DeliveryNoteItems)
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

                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;

                        taskSuccess++;
                    }
                    else
                    {
                        order.Status = GeneralStatus.PartiallyFailed;
                        order.ErrorMessage = error;

                        foreach (var it in order.DeliveryNoteItems)
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
                    _logger.LogInformation("DeliveryNotes batch SaveChanges affected rows={Affected}", affected);

                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency issue while saving DeliveryNotes batch for sapId={SapId}", sapId);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "DB update issue while saving DeliveryNotes batch for sapId={SapId}", sapId);
                    throw;
                }

                totalSuccess += taskSuccess;
                totalFail += taskFail;

                _logger.LogInformation(
                    "DeliveryNotes batch processed for sapId={SapId}: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    sapId, taskSuccess, taskFail, totalSuccess, totalFail
                );
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No approved delivery notes to sync";

            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }

        private async Task<(DeliveryNoteOrder order, bool success, string res, string? error)>
            ProcessDeliveryNoteAsync(int sapId, DeliveryNoteOrder order)
        {
            try
            {
                // ✅ Validate minimum
                if (order.Customer == null || string.IsNullOrWhiteSpace(order.Customer.CustomerCode))
                    throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: CustomerCode is missing.");

                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: WarehouseCode is missing.");

                if (order.DeliveryNoteItems == null || order.DeliveryNoteItems.Count == 0)
                    throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: No items.");

                // ✅ Determine if based on Sales Order
                // BaseType for Sales Order = 17
                int? baseEntry = order.SalesOrder?.DocEntry;
                bool canBeBasedOnSalesOrder = baseEntry.HasValue;

                var sapDto = new SapDeliveryNoteDto
                {
                    CardCode = order.Customer.CustomerCode,
                    CardName = order.Customer.CustomerName,
                    DocDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    NumAtCard = $"dn-{order.DeliveryNoteOrderId}",
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.DeliveryNoteItems.OrderBy(x => x.DeliveryNoteItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: Item missing on line {line.DeliveryNoteItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapDeliveryNoteLineDto
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

                    // ✅ Based on SalesOrder (اختياري)
                    // لازم يكون عندنا baseEntry + BaseLine (من SalesOrderItem.LineNum)
                    //if (canBeBasedOnSalesOrder && line.SalesOrderItem?.LineNum != null)
                    //{
                    //    sapLine.BaseType = 17;
                    //    sapLine.BaseEntry = baseEntry;
                    //    sapLine.BaseLine = line.SalesOrderItem.LineNum.Value;
                    //}

                    // ✅ batches
                    var batches = line.DeliveryNoteBatches ?? new List<DeliveryNoteBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"Delivery#{order.DeliveryNoteOrderId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null,
                            // BaseLineNumber only if based on SO (matching BaseLine)
                           // BaseLineNumber = sapLine.BaseLine
                        });
                    }

                    // ✅ validate sum batches == line qty (لو item batch-managed عندكم)
                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"Delivery#{order.DeliveryNoteOrderId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.DocumentLines.Add(sapLine);
                }

                var url = "DeliveryNotes";

                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("Delivery Note synced successfully. DeliveryNoteOrderId={Id}, sapResponse={Resp}",
                    order.DeliveryNoteOrderId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync delivery note: DeliveryNoteOrderId={Id}", order.DeliveryNoteOrderId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(DeliveryNoteOrder order)
        {
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        // =========================
        // SAP DTOs (DeliveryNotes)
        // =========================

        public sealed class SapDeliveryNoteDto
        {
            public string CardCode { get; set; } = string.Empty;
            public string? CardName { get; set; }
            public string DocDate { get; set; } = string.Empty;
            public string? NumAtCard { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapDeliveryNoteLineDto> DocumentLines { get; set; } = new();
        }

        public sealed class SapDeliveryNoteLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal Quantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public int? UoMEntry { get; set; }
            public string? BarCode { get; set; }
            public decimal? UnitPrice { get; set; }

            //public int? BaseType { get; set; }
            //public int? BaseEntry { get; set; }
            //public int? BaseLine { get; set; }

            public List<SapBatchNumberDto>? BatchNumbers { get; set; }
        }

        public sealed class SapBatchNumberDto
        {
            public string BatchNumber { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string? ExpiryDate { get; set; } // yyyy-MM-dd
            public int? BaseLineNumber { get; set; } // used when based on a document
        }

        // =========================
        // SAP Response (optional)
        // =========================

        public sealed class DeliveryNoteResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<DeliveryNoteLineResponse>? DocumentLines { get; set; }
        }

        public sealed class DeliveryNoteLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
