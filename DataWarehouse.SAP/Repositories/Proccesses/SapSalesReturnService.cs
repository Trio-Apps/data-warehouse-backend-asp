using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{

 

    public class SapSalesReturnService : ISapSalesReturnService
    {
        private readonly IBaseSap<SapSalesReturnDto> _sap;
        private readonly ILogger<SapSalesReturnService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapSalesReturnService(
            IBaseSap<SapSalesReturnDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapSalesReturnService> logger)
        {
            _sap = sap;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncSalesReturnsAsync(int salesReturnId)
        {
          
                // ✅ Approved IDs من جدول الـ Process
                // ✳️ غيّر ProcessType.SalesReturn لو enum عندك مختلف
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.SalesReturn && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();

            var order = await _context.SalesReturnOrders
                .AsTracking()
                .Include(o => o.Warehouse)
                .Include(o => o.Customer)
                .Include(o => o.DeliveryNoteOrder)
                .Include(o => o.SalesReturnOrderItems)
                    .ThenInclude(i => i.Item)
                .Include(o => o.SalesReturnOrderItems)
                    .ThenInclude(i => i.DeliveryNoteItem)
                .Include(o => o.SalesReturnOrderItems)
                    .ThenInclude(i => i.SalesReturnOrderBatches)
                .Where(o =>
                    o.Status == GeneralStatus.Processing &&
                    approvedIdsQuery.Contains(o.SalesReturnOrderId))
               .FirstOrDefaultAsync(sr => sr.SalesReturnOrderId == salesReturnId);

            if (order == null)
                return "This order must be Approval To send it to Sap";


            var (_, success, body, error) = await ProcessSalesReturnAsync(order.Warehouse.SapId, order);

                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;
                        order.ErrorMessage = null;

                        SalesReturnResponse? res = null;
                        try
                        {
                            res = JsonSerializer.Deserialize<SalesReturnResponse>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Could not deserialize Returns response for SalesReturnOrderId={Id}",
                                order.SalesReturnOrderId);
                        }

                        if (res != null)
                        {
                            order.DocEntry = res.DocEntry;
                            order.DocNum = res.DocNum;
                            order.DocType = res.DocType;

                            var sapLinesByItem = (res.DocumentLines ?? new List<SalesReturnLineResponse>())
                                .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                                .GroupBy(x => x.ItemCode!)
                                .ToDictionary(g => g.Key, g => g.Last().LineNum);

                            foreach (var it in order.SalesReturnOrderItems)
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
                            foreach (var it in order.SalesReturnOrderItems)
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

                    }
                    else
                    {
                        order.Status = GeneralStatus.PartiallyFailed;
                        order.ErrorMessage = error;

                        foreach (var it in order.SalesReturnOrderItems)
                        {
                            it.Status = GeneralItemStatus.Failed;
                            it.ErrorMessage = error;

                            _context.Entry(it).Property(x => x.Status).IsModified = true;
                            _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                        }

                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;

                    }
                

                try
                {
                    _context.ChangeTracker.DetectChanges();
                    var affected = await _context.SaveChangesAsync();
                    _logger.LogInformation("SalesReturn batch SaveChanges affected rows={Affected}", affected);
                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency issue while saving SalesReturn batch for sapId={SapId}", order.Warehouse.SapId);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "DB update issue while saving SalesReturn batch for sapId={SapId}", order.Warehouse.SapId);
                    throw;
                }

             

            return $"Sync completed. ";
        }

        private async Task<(SalesReturnOrder order, bool success, string res, string? error)>
            ProcessSalesReturnAsync(int sapId, SalesReturnOrder order)
        {
            try
            {
                if (order.Customer == null || string.IsNullOrWhiteSpace(order.Customer.CustomerCode))
                    throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: CustomerCode is missing.");

                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: WarehouseCode is missing.");

                if (order.SalesReturnOrderItems == null || order.SalesReturnOrderItems.Count == 0)
                    throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: No items.");

                // ✅ based-on delivery note ?
                // BaseType for Delivery Note = 15
                int? baseEntry = order.DeliveryNoteOrder?.DocEntry;
                bool canBeBasedOnDelivery = baseEntry.HasValue;

                var sapDto = new SapSalesReturnDto
                {
                    CardCode = order.Customer.CustomerCode,
                    CardName = order.Customer.CustomerName,
                    DocDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    NumAtCard = $"sr-{order.SalesReturnOrderId}",
                    Comments = order.Comment ?? string.Empty,
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.SalesReturnOrderItems.OrderBy(x => x.SalesReturnOrderItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: Item missing on line {line.SalesReturnOrderItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapSalesReturnLineDto
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

                    // ✅ set base only if we have delivery note docentry + delivery note line num
                    //if (canBeBasedOnDelivery && line.DeliveryNoteItem?.LineNum != null)
                    //{
                    //    sapLine.BaseType = 15;
                    //    sapLine.BaseEntry = baseEntry;
                    //    sapLine.BaseLine = line.DeliveryNoteItem.LineNum.Value;
                    //}

                    // ✅ batches
                    var batches = line.SalesReturnOrderBatches ?? new List<SalesReturnOrderBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"SalesReturn#{order.SalesReturnOrderId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null,
                           // BaseLineNumber = sapLine.BaseLine // only meaningful when based
                        });
                    }

                    // ✅ validate sum batches == line qty if batches exist
                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"SalesReturn#{order.SalesReturnOrderId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.DocumentLines.Add(sapLine);
                }

                // ✅ Sales Return endpoint
                var url = "Returns";

                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("Sales Return synced successfully. SalesReturnOrderId={Id}, sapResponse={Resp}",
                    order.SalesReturnOrderId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync sales return: SalesReturnOrderId={Id}", order.SalesReturnOrderId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(SalesReturnOrder order)
        {
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        // =========================
        // SAP DTOs (Returns)
        // =========================

        public sealed class SapSalesReturnDto
        {
            public string CardCode { get; set; } = string.Empty;
            public string? CardName { get; set; }
            public string DocDate { get; set; } = string.Empty;
            public string? NumAtCard { get; set; }
            public string? Comments { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapSalesReturnLineDto> DocumentLines { get; set; } = new();
        }

        public sealed class SapSalesReturnLineDto
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
            public int? BaseLineNumber { get; set; }
        }

        // =========================
        // SAP Response
        // =========================

        public sealed class SalesReturnResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<SalesReturnLineResponse>? DocumentLines { get; set; }
        }

        public sealed class SalesReturnLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
