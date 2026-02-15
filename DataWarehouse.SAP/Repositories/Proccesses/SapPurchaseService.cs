using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using DataWarehouse.SAP.Models.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
    public class SapPurchaseService : ISapPurchaseService
    {
        private readonly IBaseSap<SapPurchaseOrderDto> sap;
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapBulkProductionService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapPurchaseService(IBaseSap<SapPurchaseOrderDto> sap, IDynamicBarCodeRepository dynamicBarCodeRepository,
            ISapSyncStatusRepository syncRepo, DataWarehouseDbContext context, ILogger<SapBulkProductionService> logger)
        {
            this.sap = sap;
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }

        /// <summary>
        /// Sync Approved Purchase Orders for a given sapId (Warehouse.SapId)
        /// </summary>
        public async Task<string> SyncPurchaseAsync(int sapId)
        {
            int batchSize = 200; // PO أقل من production items غالباً
            int skip = 0;
            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ هات IDs للأوامر Approved من جدول الـ Process
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.Purchase && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();

                // ✅ جيب Batch من PurchaseOrders اللي Approval + تبع sapId + موجودة في approvedIds
                var batch = await _context.PurchaseOrders
                    .Include(po => po.Warehouse)
                    .Include(po => po.Supplier)
                    .Include(po => po.PurchaseOrderItems)
                        .ThenInclude(i => i.Item)
                    .Where(po => po.Status == GeneralStatus.Approval
                                 && po.Warehouse.SapId == sapId
                                 && approvedIdsQuery.Contains(po.PurchaseOrderId))
                    .OrderBy(po => po.PurchaseOrderId)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                // ✅ Parallel processing (زي production)
                var semaphore = new SemaphoreSlim(5);
                var tasks = batch.Select(order =>
                    ProcessPurchaseOrderAsync(sapId, order, semaphore)
                );

                var results = await Task.WhenAll(tasks);

                // ✅ Update statuses in-memory
                foreach (var (order, success, error) in results)
                {
                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;

                        // كمان نقدر نعلّم الـ items إنها synced (اختار status مناسب عندك)
                        foreach (var it in order.PurchaseOrderItems)
                        {
                            it.Status = GeneralItemStatus.Received; // "Released" = تم إرساله/اعتماده في SAP
                            it.ErrorMessage = null;
                        }
                    }
                    else
                    {
                        order.Status = GeneralStatus.PartiallyFailed;
                        foreach (var it in order.PurchaseOrderItems)
                        {
                            it.Status = GeneralItemStatus.Failed;
                            it.ErrorMessage = error;
                        }
                    }
                }

                // ✅ Save batch once
                await _context.SaveChangesAsync();

                totalSuccess += results.Count(r => r.success);
                totalFail += results.Count(r => !r.success);

                logger.LogInformation(
                    "PO Batch processed: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    results.Count(r => r.success),
                    results.Count(r => !r.success),
                    totalSuccess,
                    totalFail
                );

                skip += batchSize;
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No approved purchase orders to sync";

            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }

        private async Task<(PurchaseOrder order, bool success, string? error)>
            ProcessPurchaseOrderAsync(int sapId, PurchaseOrder order, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            try
            {
                // ✅ Validate minimum
                if (order.Supplier == null || string.IsNullOrWhiteSpace(order.Supplier.SupplierCode))
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: SupplierCode is missing.");

                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: WarehouseCode is missing.");

                if (order.PurchaseOrderItems == null || order.PurchaseOrderItems.Count == 0)
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: No items.");

                // ✅ Build DTO
                var sapDto = new SapPurchaseOrderDto
                {
                    CardCode = order.Supplier.SupplierCode,
                    CardName = order.Supplier.SupplierName, // optional
                    DocDate = ConvertToSapDateFormat(order.PostingDate == default ? order.CreatedAt : order.PostingDate),
                    NumAtCard = $"PO-{order.PurchaseOrderId}",
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };

                foreach (var line in order.PurchaseOrderItems)
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: Item missing on line {line.PurchaseOrderItemId}.");

                    // افترض إن عندك ItemCode على الـ Item
                    var itemCode = line.Item.ItemCode;
                    if (string.IsNullOrWhiteSpace(itemCode))
                        throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: ItemCode is missing for item {line.ItemId}.");

                    sapDto.DocumentLines.Add(new SapPurchaseOrderLineDto
                    {
                        ItemCode = itemCode,
                        ItemDescription = line.Item.ItemName, // لو عندك
                        Quantity = line.Quantity,
                        WarehouseCode = order.Warehouse.WarehouseCode,
                        UoMEntry = line.UoMEntry > 0 ? line.UoMEntry : null,
                        BarCode = line.BarCode,
                        UnitPrice = line.UnitPrice
                    });
                }

                var url = "PurchaseOrders";

                // ✅ إرسال لـ SAP
                // ملاحظة: لو عندك AddPostSapAsync استخدمه هنا
                var response = await sap.AddSapAsync(sapId, url, sapDto);

               // var (docEntry, docNum) = ExtractDocEntryAndDocNum(response);

                //if (docEntry == null)
                //    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: SAP response has no DocEntry.");

                // ✅ حدّث Process table + Sync repo (اختياري)
               // await MarkProcessCompletedAsync(order.PurchaseOrderId);
               // await _syncRepo.MarkSuccessAsync(ProcessType.Purchase, order.PurchaseOrderId, docEntry.Value, docNum ?? 0);

                logger.LogInformation("Purchase order synced: {Id} -> DocEntry={DocEntry}", order.PurchaseOrderId);

                return (order, true, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync purchase order: {Id}", order.PurchaseOrderId);
                //await _syncRepo.MarkFailedAsync(ProcessType.Purchase, order.PurchaseOrderId, ex.Message);
                return (order, false, ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task MarkProcessCompletedAsync(int purchaseOrderId)
        {
            var process = await _context.ProcessItemIsProgresses
                .Where(p => p.ProcessType == ProcessType.Purchase && p.ReferenceId == purchaseOrderId)
                .OrderByDescending(p => p.ProcessItemIsProgressId)
                .FirstOrDefaultAsync();

            if (process != null)
                process.CompletedDate = DateTime.UtcNow;
        }

        private static string ConvertToSapDateFormat(DateTime date)
            => date.Date.ToString("yyyy-MM-dd");

        private int? TryResolveBplId(PurchaseOrder order)
        {
            // لو عندك BPL مربوط بالـ Warehouse
            // عدّل حسب Warehouse entity عندك
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }

        //private static (int? docEntry, int? docNum) ExtractDocEntryAndDocNum(string response)
        //{
        //    // حاول JSON parsing أولاً
        //    try
        //    {
        //        using var doc = JsonDocument.Parse(response);
        //        var root = doc.RootElement;

        //        int? docEntry = null;
        //        int? docNum = null;

        //        if (root.TryGetProperty("DocEntry", out var de) && de.ValueKind == JsonValueKind.Number)
        //            docEntry = de.GetInt32();

        //        if (root.TryGetProperty("DocNum", out var dn) && dn.ValueKind == JsonValueKind.Number)
        //            docNum = dn.GetInt32();

        //        return (docEntry, docNum);
        //    }
        //    catch
        //    {
        //        // fallback regex
        //        var entryMatch = Regex.Match(response, @"""DocEntry""\s*:\s*(\d+)");
        //        var numMatch = Regex.Match(response, @"""DocNum""\s*:\s*(\d+)");

        //        int? docEntry = entryMatch.Success ? int.Parse(entryMatch.Groups[1].Value) : null;
        //        int? docNum = numMatch.Success ? int.Parse(numMatch.Groups[1].Value) : null;

        //        return (docEntry, docNum);
        //    }
        //}
    }

    public class SapPurchaseOrderDto
    {
        public string CardCode { get; set; } = string.Empty;
        public string? CardName { get; set; }
        public string DocDate { get; set; } = string.Empty;
        public string? NumAtCard { get; set; }
        public int? BPL_IDAssignedToInvoice { get; set; }
        public List<SapPurchaseOrderLineDto> DocumentLines { get; set; } = new();
    }

    public class SapPurchaseOrderLineDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public int? UoMEntry { get; set; }
        public string? BarCode { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}
