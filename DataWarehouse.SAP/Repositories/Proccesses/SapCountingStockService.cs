using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
   

    //public interface ISapCountingStockService
    //{
    //    Task<string> SyncCountingStockAsync(int countStockId);
    //}

    public class SapCountingStockService : ISapCountingStockService
    {
        private readonly ISapAttachmentService sapAttachmentService;
        private readonly IBaseSap<SapInventoryDocumentDto> _sap;
        private readonly ILogger<SapCountingStockService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapCountingStockService(
            ISapAttachmentService sapAttachmentService,
            IBaseSap<SapInventoryDocumentDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapCountingStockService> logger)
        {
            this.sapAttachmentService = sapAttachmentService;
            _sap = sap;
            _context = context;
            _logger = logger;
        }

        public async Task<string> SyncCountingStockAsync(int countStockId)
        {
            // ? Approved IDs from process table
            // ?? غيّر ProcessType.CountStock لو اسم الـ enum مختلف عندك
            var approvedIdsQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.Counting && p.Status == ProcessStatus.Approved)
                .Select(p => p.ReferenceId)
                .Distinct();

            // ? Load order
            var order = await _context.Set<CountStock>()
                .AsTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.User)
                .Include(x => x.CountStockItem)
                    .ThenInclude(i => i.Item)
                .Include(x => x.CountStockItem)
                    .ThenInclude(i => i.CountStockBatches)
                .Where(x =>
                    x.Status == GeneralStatus.Processing &&
                    approvedIdsQuery.Contains(x.CountStockId))
                .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

            if (order == null)
                return "This stock count must be approved before sending it to SAP.";

            var (_, success, body, error) = await ProcessCountingStockAsync(order.Warehouse.SapId, order);

            if (success)
            {
                order.Status = GeneralStatus.Completed;
                order.ErrorMessage = null;

                Dictionary<string, int> sapLinesByItem = new();
                try
                {
                    var res = JsonSerializer.Deserialize<InventoryCountingResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (res != null)
                    {
                        order.DocEntry = res.DocEntry;
                        order.DocNum = res.DocNum;
                     //   order.DocType = res.DocType;

                        var responseLines = (res.InventoryCountingLines ?? new List<InventoryLineResponse>())
                            .Concat(res.InventoryPostingLines ?? new List<InventoryLineResponse>());

                        sapLinesByItem = responseLines
                            .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode))
                            .GroupBy(x => x.ItemCode!)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Last().LineNum ?? g.Last().BaseLineNumber ?? 0);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not deserialize InventoryCounting response for CountStockId={Id}",
                        order.CountStockId);
                }

                foreach (var it in order.CountStockItem)
                {
                    it.Status = GeneralItemStatus.Received;
                    it.ErrorMessage = null;

                    var itemCode = it.Item?.ItemCode;
                    if (!string.IsNullOrWhiteSpace(itemCode) &&
                        sapLinesByItem.TryGetValue(itemCode!, out var lineNum))
                    {
                        it.LineNum = lineNum;
                    }

                    _context.Entry(it).Property(x => x.Status).IsModified = true;
                    _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                    _context.Entry(it).Property(x => x.LineNum).IsModified = true;
                }

                _context.Entry(order).Property(x => x.Status).IsModified = true;
                _context.Entry(order).Property(x => x.DocEntry).IsModified = true;
                _context.Entry(order).Property(x => x.DocNum).IsModified = true;
             //   _context.Entry(order).Property(x => x.DocType).IsModified = true;
                _context.Entry(order).Property(x => x.ErrorMessage).IsModified = true;
            }
            else
            {
                order.Status = GeneralStatus.PartiallyFailed;
                order.ErrorMessage = error;

                foreach (var it in order.CountStockItem)
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
                _logger.LogInformation("CountingStock SaveChanges affected rows={Affected}", affected);

                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Concurrency issue while saving CountingStock for sapId={SapId}",
                    order.Warehouse.SapId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "DB update issue while saving CountingStock for sapId={SapId}",
                    order.Warehouse.SapId);
                throw;
            }

            return "Sync completed.";
        }

        private async Task<(CountStock order, bool success, string res, string? error)>
            ProcessCountingStockAsync(int sapId, CountStock order)
        {
            try
            {
                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"CountStock#{order.CountStockId}: WarehouseCode is missing.");

                if (order.CountStockItem == null || !order.CountStockItem.Any())
                    throw new InvalidOperationException($"CountStock#{order.CountStockId}: No items.");

                var attachments = await _context.DocumentAttachments
                    .Where(x => x.DocumentType == ProcessType.Counting
                        && x.DocumentId == order.CountStockId
                        && x.IsActive)
                    .ToListAsync();

                int? attachmentEntry = null;
                if (attachments.Any())
                {
                    attachmentEntry = await sapAttachmentService
                        .CreateAttachmentEntryAsync(order.Warehouse.SapId, attachments);
                }

                var isPosting = string.Equals(order.DocType?.Trim(), "Posting", StringComparison.OrdinalIgnoreCase);
                var postingOrCountDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate);
                var bplId = TryResolveBplId(order);

                var sapDto = new SapInventoryDocumentDto
                {
                    CountDate = isPosting ? null : postingOrCountDate,
                    PostingDate = isPosting ? postingOrCountDate : null,
                    Remarks = string.IsNullOrWhiteSpace(order.Comment)
                        ? $"CountStock-{order.CountStockId}"
                        : order.Comment,
                    BranchID = bplId,
                    BPLID = bplId,
                    InventoryCountingLines = isPosting ? null : new List<SapInventoryCountingLineDto>(),
                    InventoryPostingLines = isPosting ? new List<SapInventoryPostingLineDto>() : null
                };
                if (attachmentEntry.HasValue)
                {
                    sapDto.AttachmentEntry = attachmentEntry.Value;
                }

                var lineIndex = 0;
                foreach (var line in order.CountStockItem.OrderBy(x => x.CountStockItemId))
                {
                    if (line.Item == null)
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: Item missing on line {line.CountStockItemId}.");

                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: ItemCode missing for itemId={line.ItemId}.");

                    var countedQty = line.Quantity;
                    if (countedQty < 0)
                        throw new InvalidOperationException(
                            $"CountStock#{order.CountStockId}: Counted quantity cannot be negative for ItemCode={itemCode}.");

                    var batches = line.CountStockBatches ?? new List<CountStockBatch>();
                    ValidateBatches(order, itemCode, countedQty, batches);
                    if (isPosting)
                    {
                        var sapPostingLine = new SapInventoryPostingLineDto
                        {
                            ItemCode = itemCode,
                            ItemDescription = line.Item.ItemName,
                            UoMCountedQuantity = countedQty,
                            WarehouseCode = order.Warehouse.WarehouseCode,
                            UoMCode = line.Item.UoM,
                            BarCode = line.BarCode
                        };

                        foreach (var b in batches)
                        {
                            sapPostingLine.InventoryPostingBatchNumbers.Add(new SapInventoryBatchNumberDto
                            {
                                BatchNumber = b.BatchNumber!,
                                ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null,
                                Quantity = b.Quantity,
                                BaseLineNumber = lineIndex
                            });
                        }

                        sapDto.InventoryPostingLines!.Add(sapPostingLine);
                    }
                    else
                    {
                        var sapCountingLine = new SapInventoryCountingLineDto
                        {
                            ItemCode = itemCode,
                            ItemDescription = line.Item.ItemName,
                            UoMCountedQuantity = countedQty,
                            WarehouseCode = order.Warehouse.WarehouseCode,
                            UoMCode = line.Item.UoM,
                            BarCode = line.BarCode
                        };

                        foreach (var b in batches)
                        {
                            sapCountingLine.InventoryCountingBatchNumbers.Add(new SapInventoryBatchNumberDto
                            {
                                BatchNumber = b.BatchNumber!,
                                ExpiryDate = b.ExpiryDate.HasValue ? ConvertToSapDateFormat(b.ExpiryDate.Value) : null,
                                Quantity = b.Quantity,
                                BaseLineNumber = lineIndex
                            });
                        }

                        sapDto.InventoryCountingLines!.Add(sapCountingLine);
                    }

                    lineIndex++;
                }

                var url = isPosting ? "InventoryPostings" : "InventoryCountings";
                var responseJson = await _sap.AddSapAsync(sapId, url, sapDto);

                _logger.LogInformation(
                    "{Mode} stock synced successfully. CountStockId={Id}, sapResponse={Resp}",
                    isPosting ? "Posting" : "Counting",
                    order.CountStockId,
                    responseJson);

                return (order, true, responseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync counting stock: CountStockId={Id}", order.CountStockId);
                return (order, false, "", ex.Message);
            }
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(CountStock order)
        {
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;


            return null;
        }

        private static void ValidateBatches(
            CountStock order,
            string itemCode,
            decimal countedQty,
            ICollection<CountStockBatch> batches)
        {
            if (batches.Count == 0)
                return;

            foreach (var b in batches)
            {
                if (string.IsNullOrWhiteSpace(b.BatchNumber))
                    throw new InvalidOperationException(
                        $"CountStock#{order.CountStockId}: Missing BatchNumber for ItemCode={itemCode}.");

                if (b.Quantity <= 0)
                    throw new InvalidOperationException(
                        $"CountStock#{order.CountStockId}: Batch quantity must be > 0 for ItemCode={itemCode}, Batch={b.BatchNumber}.");
            }

            var sum = batches.Sum(x => x.Quantity);
            if (sum != countedQty)
            {
                throw new InvalidOperationException(
                    $"CountStock#{order.CountStockId}: Sum(BatchNumbers.Quantity)={sum} != CountedQuantity={countedQty} for ItemCode={itemCode}.");
            }
        }

        // =========================
        // SAP DTOs (InventoryCountings)
        // =========================

        public sealed class SapInventoryDocumentDto
        {
            public string? CountDate { get; set; }
            public string? PostingDate { get; set; }
            public int? BranchID { get; set; }
            public string? Remarks { get; set; }
            public int? BPLID { get; set; }
            public int? AttachmentEntry { get; set; }
            public List<SapInventoryCountingLineDto>? InventoryCountingLines { get; set; }
            public List<SapInventoryPostingLineDto>? InventoryPostingLines { get; set; }
        }

        public sealed class SapInventoryCountingLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal UoMCountedQuantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public string? UoMCode { get; set; }
            public string? BarCode { get; set; }
            public List<SapInventoryBatchNumberDto> InventoryCountingBatchNumbers { get; set; } = new();
        }

        public sealed class SapInventoryPostingLineDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal UoMCountedQuantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public string? UoMCode { get; set; }
            public string? BarCode { get; set; }
            public List<SapInventoryBatchNumberDto> InventoryPostingBatchNumbers { get; set; } = new();
        }

        public sealed class SapInventoryBatchNumberDto
        {
            public string BatchNumber { get; set; } = string.Empty;
            public string? ExpiryDate { get; set; }
            public decimal Quantity { get; set; }
            public int BaseLineNumber { get; set; }
        }

        // =========================
        // SAP Response
        // =========================

        public sealed class InventoryCountingResponse
        {
            public int? DocEntry { get; set; }
            public int? DocNum { get; set; }
            public string? DocType { get; set; }
            public List<InventoryLineResponse>? InventoryCountingLines { get; set; }
            public List<InventoryLineResponse>? InventoryPostingLines { get; set; }
        }

        public sealed class InventoryLineResponse
        {
            public int? LineNum { get; set; }
            public int? BaseLineNumber { get; set; }
            public string? ItemCode { get; set; }
        }
    }

}


