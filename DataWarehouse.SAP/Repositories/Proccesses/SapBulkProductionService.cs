using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Actors;
using DataWarehouse.SAP.Models.BarCode;
using DataWarehouse.SAP.Models.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
    public class SapBulkProductionService : ISapBulkProductionService
    {
        private readonly IBaseSap<BulkProductionReleasedDto> sap2;
        private readonly IBaseSap<BulkProductionPlannedDto> sap;
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapBulkProductionService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapBulkProductionService(IBaseSap<BulkProductionReleasedDto> sap2, IBaseSap<BulkProductionPlannedDto> sap, IDynamicBarCodeRepository dynamicBarCodeRepository,
            ISapSyncStatusRepository syncRepo, DataWarehouseDbContext context, ILogger<SapBulkProductionService> logger)
        {
            this.sap2 = sap2;
            this.sap = sap;
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }


        // Helper method لاستخراج الـ AbsoluteEntry من الـ SAP Response

        public async Task<string> SyncProductionItemsPlannedAsync(int sapId)
        {
            int batchSize = 500;
            int skip = 0;
            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                // ✅ جيب batch صغير من الـ database
                var batch = await _context.ProductionOrderItems
                    .Include(x => x.ProductionOrder)
                        .ThenInclude(po => po.Warehouse)
                    .Include(x => x.Item)
                    .Where(x => x.Status == ProductionItemStatus.Planned
                            && x.ProductionOrder.Warehouse.SapId == sapId)
                    .OrderBy(x => x.ProductionOrderItemId)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                // ✅ Process الـ batch بـ parallel
                var semaphore = new SemaphoreSlim(5);
                var tasks = batch.Select(item =>
                    ProcessProductionItemAsync(sapId, item, semaphore)
                );

                var results = await Task.WhenAll(tasks);

                // ✅ حدث الـ items
                foreach (var (item, success, error, absoluteEntry) in results)
                {
                    if (success)
                    {
                        item.Status = ProductionItemStatus.Released;
                        item.AbsoluteEntry = absoluteEntry;
                        item.ProcessedAt = DateTime.UtcNow;
                        item.ErrorMessage = null;
                    }
                    else
                    {
                      //  item.Status = ProductionItemStatus.Draft;
                        item.ErrorMessage = error;
                    }
                }

                // ✅ احفظ كل batch لوحده
                await _context.SaveChangesAsync();

                totalSuccess += results.Count(r => r.success);
                totalFail += results.Count(r => !r.success);

                logger.LogInformation(
                    "Batch processed: {Success} succeeded, {Failed} failed. Total so far: {TotalSuccess}/{TotalFail}",
                    results.Count(r => r.success),
                    results.Count(r => !r.success),
                    totalSuccess,
                    totalFail
                );

                skip += batchSize;
            }

            if (totalSuccess == 0 && totalFail == 0)
                return "No production items to sync";

            return $"Sync completed. Success: {totalSuccess}, Failed: {totalFail}";
        }
        private async Task<(ProductionOrderItem item, bool success, string error, int? absoluteEntry)>
            ProcessProductionItemAsync(int sapId, ProductionOrderItem productionItem, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            try
            {
                var sapDto = new BulkProductionPlannedDto
                {
                    Series = 23,
                    ItemNo = productionItem.Item.ItemCode,
                    PlannedQuantity = productionItem.PlannedQuantity,
                    PostingDate = ConvertToSapDateFormat(productionItem.ProductionOrder.PostingDate),
                    DueDate = ConvertToSapDateFormat(productionItem.ProductionOrder.DueDate),
                    Warehouse = productionItem.ProductionOrder.Warehouse.WarehouseCode
                };

                var url = "ProductionOrders";
                var response = await sap.AddPatchSapAsync(sapId, url, sapDto);
                var absoluteEntry = ExtractAbsoluteEntryFromResponse(response);

                logger.LogInformation("Production item synced: {Id}", productionItem.ProductionOrderItemId);

                return (productionItem, true, null, absoluteEntry);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync production item: {Id}", productionItem.ProductionOrderItemId);
                return (productionItem, false, ex.Message, null);
            }
            finally
            {
                semaphore.Release();
            }
        }
        private int? ExtractAbsoluteEntryFromResponse(dynamic response)
        {
            try
            {
                // افترض إن الـ response بيرجع JSON فيه AbsoluteEntry
                // عدل حسب الـ actual response من SAP
                return response?.AbsoluteEntry;
            }
            catch
            {
                return null;
            }
        }

        private static string ConvertToSapDateFormat(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public async Task<string> SyncProductionItemsReleasedAsync(int sapId)
        {
            int batchSize = 500;
            int skip = 0;
            int totalSuccess = 0;
            int totalFail = 0;

            while (true)
            {
                var batch = await _context.ProductionOrderItems
                    .Where(x => x.Status == ProductionItemStatus.Released
                            && x.AbsoluteEntry != null)
                    .OrderBy(x => x.ProductionOrderItemId)
                    .Skip(skip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any()) break;

                var semaphore = new SemaphoreSlim(5);
                var tasks = batch.Select(item => ProcessReceiveAsync(sapId, item, semaphore));
                var results = await Task.WhenAll(tasks);

                await _context.SaveChangesAsync(); // احفظ كل batch لوحده

                totalSuccess += results.Count(r => r);
                totalFail += results.Count(r => !r);

                skip += batchSize;
            }

            return $"Success: {totalSuccess}, Failed: {totalFail}";
        }
     
        private async Task<bool> ProcessReceiveAsync(int sapId, ProductionOrderItem item, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                var url = $"ProductionOrders({item.AbsoluteEntry})";
                var dto = new BulkProductionReleasedDto { ProductionOrderStatus = "R" };

                await sap2.AddPatchSapAsync(sapId, url, dto);

              //  item.Status = ProductionItemStatus.;
                item.ProcessedAt = DateTime.UtcNow;
                item.ErrorMessage = null;

                return true;
            }
            catch (Exception ex)
            {
              //  item.Status = ProductionItemStatus.Planned;
                item.ErrorMessage = ex.Message;

                logger.LogError(ex, "Failed to receive item {Id}", item.ProductionOrderItemId);
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public Task<string> SyncProductionItemsReceivedAsync(int sapId)
        {
            throw new NotImplementedException();
        }


        public Task<string> SyncProductionItemsClosedAsync(int sapId)
        {
            throw new NotImplementedException();
        }

        public Task<string> SyncProductionItemsRecievedAsync(int sapId)
        {
            throw new NotImplementedException();
        }
    }
}
