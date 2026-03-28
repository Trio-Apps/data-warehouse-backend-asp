using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
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
   

    public class SapTransferredStockService : ISapTransferredStockService
    {
        private readonly ISapAttachmentService sapAttachmentService;
        private readonly IBaseSap<SapInventoryTransferDto> _sap;
        private readonly DataWarehouseDbContext _context;
        private readonly ILogger<SapTransferredStockService> _logger;

        public SapTransferredStockService(
            ISapAttachmentService sapAttachmentService,
            IBaseSap<SapInventoryTransferDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapTransferredStockService> logger)
        {
            this.sapAttachmentService = sapAttachmentService;
            _sap = sap;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncTransferredStockAsync(int transferredStockId)
        {
            var approvedIdsQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.Transferred &&
                            p.Status == ProcessStatus.Approved)
                .Select(p => p.ReferenceId)
                .Distinct();

            var order = await _context.TransferredStocks
                .AsTracking()
                .Include(o => o.Warehouse)
                .Include(o => o.DistinationWarehouse)
                .Include(o => o.TransferredRequest)
                .Include(o => o.TransferredItems)
                    .ThenInclude(i => i.Item)
                .Include(o => o.TransferredItems)
                    .ThenInclude(i => i.TransferredRequestItem)
                .Include(o => o.TransferredItems)
                    .ThenInclude(i => i.TransferredStockBatches)
                .Where(o =>
                    o.Status == GeneralStatus.Processing && o.ReceivingStatus == ReceivingStatus.Completed &&
                    approvedIdsQuery.Contains(o.TransferredStockId))
                .FirstOrDefaultAsync(o => o.TransferredStockId == transferredStockId);



            if (order == null)
                return "This transferred stock must be Approved to send it to Sap";


            var sapId = order.Warehouse?.SapId ?? 0;
            if (sapId <= 0)
                throw new InvalidOperationException($"TransferredStock#{order.TransferredStockId}: Warehouse.SapId is missing/invalid.");

            var (_, success, body, error) = await ProcessTransferAsync(sapId, order);

            if (success)
            {
                order.Status = GeneralStatus.Completed;

                InventoryTransferResponse? res = null;
                try
                {
                    res = JsonSerializer.Deserialize<InventoryTransferResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not deserialize InventoryTransfer response for TransferredStockId={Id}",
                        order.TransferredStockId);
                }

                if (res != null)
                {
                    order.DocEntry = res.DocEntry;
                    order.DocNum = res.DocNum;
                    order.DocType = res.DocType;

                    // ItemCode -> LineNum (لو فيه تكرار ItemCode، آخر واحدة هتفوز)
                    var sapLinesByItem = (res.StockTransferLines ?? new List<InventoryTransferLineResponse>())
                        .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                        .GroupBy(x => x.ItemCode!)
                        .ToDictionary(g => g.Key, g => g.Last().LineNum);

                    foreach (var it in order.TransferredItems)
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
                    foreach (var it in order.TransferredItems)
                    {
                        it.Status = GeneralItemStatus.Received;
                        it.ErrorMessage = null;

                        _context.Entry(it).Property(x => x.Status).IsModified = true;
                        _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                    }
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                // لو عندك ErrorMessage في order:
                // _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }
            else
            {
                order.Status = GeneralStatus.PartiallyFailed;
                // لو عندك ErrorMessage في order:
                 order.ErrorMessage = error;

                foreach (var it in order.TransferredItems)
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
                _logger.LogInformation("TransferredStock SaveChanges affected rows={Affected} transferredStockId={Id}",
                    affected, order.TransferredStockId);

                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency issue while saving transferred stock for sapId={SapId}, transferredStockId={Id}",
                    sapId, order.TransferredStockId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "DB update issue while saving transferred stock for sapId={SapId}, transferredStockId={Id}",
                    sapId, order.TransferredStockId);
                throw;
            }

            return "Sync completed.";
        }
        private async Task<(TransferredStock order, bool success, string res, string? error)>
            ProcessTransferAsync(int sapId, TransferredStock order)
        {
            try
            {
                if (order.Warehouse == null ||
                    string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException("Source warehouse missing.");

                if (order.DistinationWarehouse == null ||
                    string.IsNullOrWhiteSpace(order.DistinationWarehouse.WarehouseCode))
                    throw new InvalidOperationException("Destination warehouse missing.");


                var attachments = await _context.DocumentAttachments
         .Where(x => x.DocumentType == ProcessType.Transferred
         && x.DocumentId == order.TransferredStockId
         && x.IsActive)
         .ToListAsync();

                int? attachmentEntry = null;

                if (attachments.Any())
                {
                    attachmentEntry = await sapAttachmentService
                        .CreateAttachmentEntryAsync(order.Warehouse.SapId, attachments);
                }


                var sapDto = new SapInventoryTransferDto
                {
                    DocDate = ConvertToSapDate(order.PostingDate ?? order.CreatedAt),
                    Comments = order.Comment,
                    FromWarehouse = order.Warehouse.WarehouseCode,
                    ToWarehouse = order.DistinationWarehouse.WarehouseCode
                };
                if (attachmentEntry.HasValue)
                {
                    sapDto.AttachmentEntry = attachmentEntry.Value;
                }

                var requestEntry = order.TransferredRequest?.DocEntry;

                foreach (var line in order.TransferredItems)
                {
                    var itemCode = line.Item?.ItemCode;

                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException("ItemCode missing.");

                    var sapLine = new SapInventoryTransferLineDto
                    {
                        ItemCode = itemCode,
                        Quantity = line.Quantity,
                        FromWarehouseCode = order.Warehouse.WarehouseCode,
                        WarehouseCode = order.DistinationWarehouse.WarehouseCode,
                        UoMEntry = line.UoMEntry > 0 ? line.UoMEntry : null,
                       // Barcode = line.BarCode,
                        UnitPrice = line.UnitPrice,
                        BatchNumbers = new List<SapBatchNumberDto>()
                    };

                    //if (requestEntry != null &&
                    //    line.TransferredRequestItem?.LineNum != null)
                    //{
                    //    sapLine.BaseType = 1250000001;
                    //    sapLine.BaseEntry = requestEntry;
                    //    sapLine.BaseLine = line.TransferredRequestItem.LineNum;
                    //}

                    foreach (var b in line.TransferredStockBatches)
                    {
                        sapLine.BatchNumbers.Add(new SapBatchNumberDto
                        {
                            BatchNumber = b.BatchNumber!,
                            Quantity = b.Quantity,
                            ExpiryDate = b.ExpiryDate?.ToString("yyyy-MM-dd")
                        });
                    }

                    sapDto.StockTransferLines.Add(sapLine);
                }

                var response = await _sap.AddSapAsync(sapId, "StockTransfers", sapDto);

                _logger.LogInformation("InventoryTransfer synced {Id}", order.TransferredStockId);

                return (order, true, response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transfer failed {Id}", order.TransferredStockId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDate(DateTime date)
            => date.ToString("yyyy-MM-dd");

        // ================= SAP DTO =================

        public class SapInventoryTransferDto
        {
            public string DocDate { get; set; }
            public string? Comments { get; set; }
            public string FromWarehouse { get; set; }
            public string ToWarehouse { get; set; }
            public int? AttachmentEntry { get; set; }

            public List<SapInventoryTransferLineDto> StockTransferLines { get; set; } = new();
        }

        public class SapInventoryTransferLineDto
        {
            public string ItemCode { get; set; }
            public decimal Quantity { get; set; }
            public string FromWarehouseCode { get; set; }
            public string WarehouseCode { get; set; }

            //public int? BaseType { get; set; }
            //public int? BaseEntry { get; set; }
            //public int? BaseLine { get; set; }

            public int? UoMEntry { get; set; }
            //public string? Barcode { get; set; }
            public decimal? UnitPrice { get; set; }

            public List<SapBatchNumberDto>? BatchNumbers { get; set; }
        }

        public class SapBatchNumberDto
        {
            public string BatchNumber { get; set; }
            public decimal Quantity { get; set; }
            public string? ExpiryDate { get; set; }
        }

        // ================= SAP RESPONSE =================

        public class InventoryTransferResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<InventoryTransferLineResponse> StockTransferLines { get; set; } = new();
        }

        public class InventoryTransferLineResponse
        {
            public int LineNum { get; set; }
            public string? ItemCode { get; set; }
        }
    }
}
