using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
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
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
    public class SapReceiptService : ISapReceiptService
    {
        private readonly IBaseSap<SapPurchaseOrderDto> sap;
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapPurchaseService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapReceiptService(IBaseSap<SapPurchaseOrderDto> sap, IDynamicBarCodeRepository dynamicBarCodeRepository,
            ISapSyncStatusRepository syncRepo, DataWarehouseDbContext context, ILogger<SapPurchaseService> logger)
        {
            this.sap = sap;
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }

        public async Task<string> SyncReceiptAsync(int sapId)
        {
            const int batchSize = 200;     // PO أقل من production items غالباً
                                           //   const int parallelism = 1;     // عدد الـ tasks في نفس الوقت

            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ IDs للأوامر Approved من جدول الـ Process
                var approvedIdsQuery = _context.ProcessItemIsProgresses
                    .AsNoTracking()
                    .Where(p => p.ProcessType == ProcessType.Purchase && p.Status == ProcessStatus.Approved)
                    .Select(p => p.ReferenceId)
                    .Distinct();


                // ✅ أهم تعديل: مفيش Skip
                // كل مرة هجيب "أول batch" من اللي لسه Processing
                var batch = await _context.ReceiptPurchaseOrders
                    .AsTracking()
                    .Include(po => po.Warehouse)
                    .Include(po => po.Supplier)
                    .Include(po => po.PurchaseOrder)
                    .Include(po => po.ReceiptPurchaseOrderItems)
                        .ThenInclude(i => i.Item)
                    .Where(po => po.Status == GeneralStatus.Processing
                                 && po.Warehouse.SapId == sapId
                                 && approvedIdsQuery.Contains(po.ReceiptPurchaseOrderId))
                    .OrderBy(po => po.ReceiptPurchaseOrderId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                // ✅ Parallel processing
                //  using var semaphore = new SemaphoreSlim(parallelism);

                //   var tasks = batch.Select(order =>

                var taskSuccess = 0;
                var taskFail = 0;
                foreach (var order in batch)
                {
                    var (_, success, error) = await ProcessPurchaseOrderAsync(sapId, order);

                    if (success)
                    {
                        order.Status = GeneralStatus.Completed;

                        foreach (var it in order.ReceiptPurchaseOrderItems)
                        {
                            it.Status = GeneralItemStatus.Received;
                            it.ErrorMessage = null;

                            _context.Entry(it).Property(x => x.Status).IsModified = true;
                            _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                        }


                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        taskSuccess++;
                    }
                    else
                    {
                        order.Status = GeneralStatus.PartiallyFailed;

                        foreach (var it in order.ReceiptPurchaseOrderItems)
                        {
                            it.Status = GeneralItemStatus.Failed;
                            it.ErrorMessage = error;

                            _context.Entry(it).Property(x => x.Status).IsModified = true;
                            _context.Entry(it).Property(x => x.ErrorMessage).IsModified = true;
                        }

                        _context.Entry(order).Property(x => x.Status).IsModified = true;
                        taskFail++;
                    }
                }



                // ✅ Save batch once
                try
                {
                    // مهم جدًا لو AutoDetectChanges مقفول في أي مكان
                    _context.ChangeTracker.DetectChanges();

                    var affected = await _context.SaveChangesAsync();
                    logger.LogInformation("SaveChanges affected rows={Affected}", affected);

                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogError(ex, "Concurrency issue while saving purchase orders batch for sapId={SapId}", sapId);
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex, "DB update issue while saving purchase orders batch for sapId={SapId}", sapId);
                    throw;
                }

                totalSuccess += taskSuccess;
                totalFail += taskFail;

                logger.LogInformation(
                    "PO Batch processed for sapId={SapId}: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    sapId,
                   taskSuccess,
                    taskFail,
                    totalSuccess,
                    totalFail
                );

                // مفيش skip هنا
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No approved purchase orders to sync";

            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }
        private async Task<(ReceiptPurchaseOrder order, bool success, string? error)>
            ProcessPurchaseOrderAsync(int sapId, ReceiptPurchaseOrder order)
        {
            try
            {
                // ✅ Validate minimum
                if (order.Supplier == null || string.IsNullOrWhiteSpace(order.Supplier.SupplierCode))
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: SupplierCode is missing.");

                if (order.Warehouse == null || string.IsNullOrWhiteSpace(order.Warehouse.WarehouseCode))
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: WarehouseCode is missing.");

                if (order.ReceiptPurchaseOrderItems == null || order.ReceiptPurchaseOrderItems.Count == 0)
                    throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: No items.");

                // ✅ Build DTO
                var sapDto = new SapPurchaseOrderDto
                {
                    CardCode = order.Supplier.SupplierCode,
                    CardName = order.Supplier.SupplierName, // optional
                    DocDate = ConvertToSapDateFormat(order.DueDate == default ? order.CreatedAt : order.DueDate),
                    NumAtCard = $"PO-{order.PurchaseOrderId}",
                    BPL_IDAssignedToInvoice = TryResolveBplId(order)
                };


                foreach (var line in order.ReceiptPurchaseOrderItems)
                {
                    if (line.Item == null)
                        throw new InvalidOperationException($"PO#{order.PurchaseOrderId}: Item missing on line {line.ReceiptPurchaseOrderItemId}.");

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


                logger.LogInformation("Purchase order synced: {Id}", order.PurchaseOrderId);

                //var orderFetch = await _context.PurchaseOrders.FirstOrDefaultAsync(e => e.PurchaseOrderId == order.PurchaseOrderId);

                //orderFetch.Status = GeneralStatus.Completed;

                //await _context.SaveChangesAsync();

                return (order, true, null);
            }
            catch (Exception ex)
            {
                //order.Status = GeneralStatus.PartiallyFailed;
                //await _context.SaveChangesAsync();

                logger.LogError(ex, "Failed to sync purchase order: {Id}", order.PurchaseOrderId);
                //await _syncRepo.MarkFailedAsync(ProcessType.Purchase, order.PurchaseOrderId, ex.Message);
                return (order, false, ex.Message);
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

        private int? TryResolveBplId(ReceiptPurchaseOrder order)
        {
            // لو عندك BPL مربوط بالـ Warehouse
            // عدّل حسب Warehouse entity عندك
            var prop = order.Warehouse?.GetType().GetProperty("BplId");
            if (prop?.GetValue(order.Warehouse) is int bpl && bpl > 0)
                return bpl;

            return null;
        }


        public class SapReceiptOrderDto
        {
            public string CardCode { get; set; } = string.Empty;
            public string? CardName { get; set; }
            public string DocDate { get; set; } = string.Empty;
            public string? NumAtCard { get; set; }
            public int? BPL_IDAssignedToInvoice { get; set; }
            public List<SapPurchaseOrderLineDto> DocumentLines { get; set; } = new();
        }

        public class SapReceiptOrderLineWithoutRefDto
        {
            public string ItemCode { get; set; } = string.Empty;
            public string? ItemDescription { get; set; }
            public decimal Quantity { get; set; }
            public string WarehouseCode { get; set; } = string.Empty;
            public int? UoMEntry { get; set; }
            public string? BarCode { get; set; }
            public decimal? UnitPrice { get; set; }
        }

        public class SapReceiptOrderLineDto
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
}
