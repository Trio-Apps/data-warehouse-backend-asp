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


  

    public class SapTransferredRequestService : ISapTransferredRequestService
    {
        private readonly ISapAttachmentService sapAttachmentService;
        private readonly IBaseSap<SapInventoryTransferredRequestDto> _sap;
        private readonly ILogger<SapTransferredRequestService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapTransferredRequestService(
            ISapAttachmentService sapAttachmentService,
            IBaseSap<SapInventoryTransferredRequestDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapTransferredRequestService> logger)
        {
            this.sapAttachmentService = sapAttachmentService;
            _sap = sap;
            _context = context;
            _logger = logger;
        }
        public async Task<string> SyncTransferredRequestsAsync(int transferredRequestId)
        {
            // ✅ Approved IDs من جدول الـ Process
            var approvedIdsQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.TransferredRequest && p.Status == ProcessStatus.Approved)
                .Select(p => p.ReferenceId)
                .Distinct();

            // ✅ هات الأوردر المطلوب (واحد بس) اللي لسه Processing + Approved
            var order = await _context.TransferredRequests
                .AsTracking()
                .Include(o => o.Warehouse)
                .Include(o => o.DistinationWarehouse)
                .Include(o => o.TransferredRequestItems)
                    .ThenInclude(i => i.Item)
                .Include(o => o.TransferredRequestItems)
                    .ThenInclude(i => i.TransferredRequestBatches)
                .Where(o =>
                    o.Status == GeneralStatus.Processing &&
                    approvedIdsQuery.Contains(o.TransferredRequestId))
                .FirstOrDefaultAsync(o => o.TransferredRequestId == transferredRequestId);

            if (order == null)
                return "This request must be Approved to send it to Sap";

            // sapId جاي من الأوردر نفسه
            var sapId = order.Warehouse?.SapId ?? 0;
            if (sapId <= 0)
                throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: Warehouse.SapId is missing/invalid.");

            var (_, success, body, error) = await ProcessTransferredRequestAsync(sapId, order);

            if (success)
            {
                order.Status = GeneralStatus.Completed;
                // لو عندك ErrorMessage في order ضيفها هنا
                // order.ErrorMessage = null;

                InventoryTransferredRequestResponse? res = null;
                try
                {
                    res = JsonSerializer.Deserialize<InventoryTransferredRequestResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not deserialize InventoryTransferredRequests response for TransferredRequestId={Id}",
                        order.TransferredRequestId);
                }

                if (res != null)
                {
                    order.DocEntry = res.DocEntry;
                    order.DocNum = res.DocNum;
                    order.DocType = res.DocType;

                    // ItemCode -> LineNum
                    var sapLinesByItem = (res.StockTransferLines ?? new List<InventoryTransferredRequestLineResponse>())
                        .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                        .GroupBy(x => x.ItemCode!)
                        .ToDictionary(g => g.Key, g => g.Last().LineNum);

                    foreach (var it in order.TransferredRequestItems)
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
                    // لو response مش معروف / deserialization فشل: على الأقل حدّث statuses
                    foreach (var it in order.TransferredRequestItems)
                    {
                        it.Status = GeneralItemStatus.Received;
                        it.ErrorMessage = null;

                        _context.Entry(it).Property(x => x.Status).IsModified = true;
                        _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                    }
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                // لو عندك ErrorMessage:
                // _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }
            else
            {
                order.Status = GeneralStatus.PartiallyFailed;
                // لو عندك ErrorMessage في order:
                 order.ErrorMessage = error;

                foreach (var it in order.TransferredRequestItems)
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
                _logger.LogInformation("TransferredRequest SaveChanges affected rows={Affected} requestId={Id}", affected, order.TransferredRequestId);

                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency issue while saving transferred request for sapId={SapId}, requestId={Id}", sapId, order.TransferredRequestId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB update issue while saving transferred request for sapId={SapId}, requestId={Id}", sapId, order.TransferredRequestId);
                throw;
            }

            return "Sync completed.";
        }
        private async Task<(TransferredRequest order, bool success, string res, string? error)>
            ProcessTransferredRequestAsync(int sapId, TransferredRequest order)
        {
            try
            {
                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: FromWarehouse is missing.");

                if (order.DistinationWarehouse == null || string.IsNullOrWhiteSpace(order.DistinationWarehouse.WarehouseCode))
                    throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: ToWarehouse is missing.");

                if (order.TransferredRequestItems == null || order.TransferredRequestItems.Count == 0)
                    throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: No items.");

                var attachments = await _context.DocumentAttachments
             .Where(x => x.DocumentType == ProcessType.TransferredRequest
             && x.DocumentId == order.TransferredRequestId
             && x.IsActive)
             .ToListAsync();

                int? attachmentEntry = null;

                if (attachments.Any())
                {
                    attachmentEntry = await sapAttachmentService
                        .CreateAttachmentEntryAsync(order.Warehouse.SapId, attachments);
                }



                var sapDto = new SapInventoryTransferredRequestDto
                {
                    DocDate = ConvertToSapDateFormat(order.PostingDate ?? order.CreatedAt),
                    DueDate = ConvertToSapDateFormat(order.DueDate),
                    Comments = order.Comment ?? string.Empty,
                    FromWarehouse = order.Warehouse.WarehouseCode,
                    ToWarehouse = order.DistinationWarehouse.WarehouseCode
                };
                if (attachmentEntry.HasValue)
                {
                    sapDto.AttachmentEntry = attachmentEntry.Value;
                }

                foreach (var line in order.TransferredRequestItems.OrderBy(x => x.TransferredRequestItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: Item missing on line {line.TransferredRequestItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: ItemCode missing for itemId={line.ItemId}.");

                    if (line.Quantity <= 0)
                        throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: Quantity must be > 0 for ItemCode={itemCode}.");

                    var sapLine = new SapInventoryTransferredRequestLineDto
                    {
                        ItemCode = itemCode,
                        ItemDescription = line.Item.ItemName,
                        Quantity = line.Quantity,
                        FromWarehouseCode = order.Warehouse.WarehouseCode,
                        WarehouseCode = order.DistinationWarehouse.WarehouseCode, // في بعض البيئات اسمها WarehouseCode للـ To
                        UoMEntry = line.UoMEntry > 0 ? line.UoMEntry : null,
                        BarCode = string.IsNullOrWhiteSpace(line.BarCode) ? null : line.BarCode.Trim(),
                        UnitPrice = line.UnitPrice,
                        BatchNumbers = new List<SapBatchNumberDto>()
                    };


                    // batches (لو item batch-managed)
                    var batches = line.TransferredRequestBatches ?? new List<TransferredRequestBatch>();
                    foreach (var b in batches)
                    {
                        if (string.IsNullOrWhiteSpace(b.BatchNumber))
                            throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: Missing BatchNumber for ItemCode={itemCode}.");

                        if (b.Quantity <= 0)
                            throw new InvalidOperationException($"TransferredRequest#{order.TransferredRequestId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");

                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null
                        });
                    }

                    if (sapLine.BatchNumbers.Count > 0)
                    {
                        var sum = sapLine.BatchNumbers.Sum(x => x.Quantity);
                        if (sum != line.Quantity)
                            throw new InvalidOperationException(
                                $"TransferredRequest#{order.TransferredRequestId}: Sum(BatchNumbers.Quantity)={sum} != Line.Quantity={line.Quantity} for ItemCode={itemCode}.");
                    }

                    sapDto.StockTransferLines.Add(sapLine);
                }

                var url = "InventoryTransferRequests";

                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation("InventoryTransferredRequest synced successfully. TransferredRequestId={Id}, sapResponse={Resp}",
                    order.TransferredRequestId, responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync transfer request: TransferredRequestId={Id}", order.TransferredRequestId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        // =========================
        // SAP DTOs (InventoryTransferredRequests)
        // =========================

        public sealed class SapInventoryTransferredRequestDto
        {
            public string DocDate { get; set; } = string.Empty;
            public string? DueDate { get; set; }
            public string? Comments { get; set; }

            // inventory transfer request header
            public string FromWarehouse { get; set; } = string.Empty;
            public string ToWarehouse { get; set; } = string.Empty;

            public int? AttachmentEntry { get; set; }


            // lines
            public List<SapInventoryTransferredRequestLineDto> StockTransferLines { get; set; } = new();
        }

        public sealed class SapInventoryTransferredRequestLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal Quantity { get; set; }

            // SAP sometimes uses both - keep both to be safe
            public string? FromWarehouseCode { get; set; }
            public string? WarehouseCode { get; set; }

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

        public sealed class InventoryTransferredRequestResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<InventoryTransferredRequestLineResponse>? StockTransferLines { get; set; }
        }

        public sealed class InventoryTransferredRequestLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
