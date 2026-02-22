using Azure;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Actors;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Text.Json;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;

namespace Dataitem.SAP.Repositories.Actors
{
    public class SapItemService : ISapItemService
    {
        private readonly IBaseSap<ItemSapModel> sap;
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapItemService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapItemService(IBaseSap<ItemSapModel> sap, IDynamicBarCodeRepository dynamicBarCodeRepository,
            ISapSyncStatusRepository syncRepo,DataWarehouseDbContext context, ILogger<SapItemService> logger)
        {
            this.sap = sap;
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }

        public async Task<string> SyncItemsAsync(int sapId) { 
            
            
            // 1️⃣ آخر Sync
            //  var lastSync = await _syncRepo.GetLastSyncDateAsync(EntitiesName.item.ToString());

            // 2️⃣ بناء Query
            var itemList = new List<SapItemDto>();
            var state = await _syncRepo.GetLastSyncPaginationSkipAsync( sapId,EntitiesName.item.ToString());
            int skip = state;
            bool hasMore = true;
            var lastSync = await _syncRepo.GetLastSyncDateAsync( sapId,EntitiesName.item.ToString());

            // 2️⃣ بناء Query
            var filterDate = lastSync.ToString("yyyy-MM-dd");

         

            while (hasMore)
            {
                var url =
                $"Items?$filter=UpdateDate ge '{filterDate}'&$skip={skip}&$select=ItemCode,ItemName,ManageBatchNumbers,ItemsGroupCode,ItemPrices,ItemWarehouseInfoCollection,ItemBarCodeCollection,ProcurementMethod";


                var json = await sap.GetAllSap(sapId,url);
                SapItemsResponse? items;

                try
                {
                        items = JsonSerializer.Deserialize<SapItemsResponse>( json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (items?.Value != null && items.Value.Any())
                    {
                        itemList.AddRange(items.Value); // ضيف العناصر اللي رجع
                                                                  //  logger.LogInformation("Fetched {count} items. Total so far: {total}", items.Value.Count, itemList.Count);

                        logger.LogInformation("Url is: {url}", url);

                        logger.LogInformation("Mapping items: {items}", JsonSerializer.Serialize(items.Value, new JsonSerializerOptions { WriteIndented = true }));


                        skip += items.Value.Count; // عشان الـ next batch

                        await AddOrUpdateItemsAsync(sapId,items.Value, skip);

                    }
                    else
                    {
                        // 6️⃣ تحديث آخر Sync
                        await _syncRepo.UpdateLastSyncDateAsync(sapId,
                            EntitiesName.item.ToString(),
                            DateTime.UtcNow
                        );
                        await _syncRepo.UpdateLastSyncPaginationSkipAsync( sapId,
                          EntitiesName.item.ToString(),
                           0
                        );

                        hasMore = false; // مفيش بيانات جديدة
                    }
                }
                catch (JsonException ex)
                {
                    throw new Exception("Failed to deserialize SAP items response", ex);
                }
            }

            logger.LogInformation("Finished fetching all items. Total count: {total}", itemList.Count);

            // ❌ Response نفسه غلط

            if (!itemList.Any())
                return "No items to sync";

            //// 6️⃣ تحديث آخر Sync

            return $"Synced {itemList.Count} items. Inserted new: {itemList.Count}";
        }

        private async Task<int> AddOrUpdateItemsAsync(int sapId,List<SapItemDto> sapItems, int skip)
        {
            if (sapItems == null || !sapItems.Any())
                return 0;

            // 1️⃣ جلب الأكواد الموجودة + العناصر الموجودة
            var existingItems = await _context.Items
                .Where(i => i.SapId == sapId && sapItems.Select(s => s.ItemCode).Contains(i.ItemCode))
                .ToListAsync();

            var existingCodes = existingItems.Select(i => i.ItemCode).ToHashSet();

            var itemsToAdd = new List<Item>();
            var itemWarehouse = new List<SapItemWarehouseDtoResponse>();
            var itemBarCode = new List<ItemBarCodesDtoResponse>();
            int processedCount = 0;

            foreach (var sap in sapItems)
            {
                // 2️⃣ احنا عايزين نضيف جديد أو نحدث الموجود
                var existingItem = existingItems.FirstOrDefault(i => i.ItemCode == sap.ItemCode);

                var lastPrice = sap.ItemPrices?.OrderBy(p => p.PriceList).LastOrDefault()?.Price ?? 0;

                if (existingItem != null)
                {
                    // 🔹 تحديث الحقول الموجودة
                    existingItem.ItemName = sap.ItemName ?? existingItem.ItemName;
                    existingItem.ItemGroup = sap.ItemsGroupCode.ToString();
                    existingItem.PurchasePrice = (decimal)lastPrice;
                    existingItem.SalesPrice = (decimal)lastPrice;
                    existingItem.UpdateDate = DateTime.UtcNow;

                    existingItem.BatchNumbers = sap.ManageBatchNumbers == "tYES";
                    existingItem.ProcurementType = sap.ProcurementMethod;
                }
                else
                {
                    // 🔹 إضافة جديد
                    var newItem = new Item
                    {
                        ItemCode = sap.ItemCode,
                        ItemName = sap.ItemName ?? "Unknown Item",
                        ItemGroup = sap.ItemsGroupCode.ToString(),
                        PurchasePrice = (decimal)lastPrice,
                        SalesPrice = (decimal)lastPrice,
                        UpdateDate = DateTime.UtcNow,
                        UoM = "type",
                        SapId = sapId,
                        BatchNumbers = sap.ManageBatchNumbers == "tYES",
                        ProcurementType = sap.ProcurementMethod
                    };
                    itemsToAdd.Add(newItem);
                }
              
                
                
                var newItemWarehouse = new SapItemWarehouseDtoResponse
                {
                    ItemCode = sap.ItemCode,
                    ItemWarehouseInfoCollection = sap.ItemWarehouseInfoCollection
                };
                var newItemBarCode = new ItemBarCodesDtoResponse
                {
                    ItemCode = sap.ItemCode,
                    ItemBarCodeCollection = sap.ItemBarCodeCollection
                };
                itemWarehouse.Add(newItemWarehouse);
                itemBarCode.Add(newItemBarCode);

                // 3️⃣ Upsert Warehouses لكل Item


                processedCount++;
            }

            // 4️⃣ Add new items مرة واحدة
            if (itemsToAdd.Any())
            {
                await _context.Items.AddRangeAsync(itemsToAdd);

            }
            await _context.SaveChangesAsync();

            await UpsertItemWarehouseAsync(sapId, itemWarehouse);
            await UpsertItemBarCodeAsync(sapId,itemBarCode);

            // 5️⃣ حفظ كل التغييرات مرة واحدة (Add + Update)

            // 6️⃣ تحديث pagination
            await _syncRepo.UpdateLastSyncPaginationSkipAsync(sapId,
                EntitiesName.item.ToString(),
                skip
            );

            return processedCount;
        }
     
        private async Task UpsertItemWarehouseAsync(int sapId, ICollection<SapItemWarehouseDtoResponse> itemWarehouses)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ جيب كل الـ Items مرة واحدة
                var itemCodes = itemWarehouses.Select(x => x.ItemCode).Distinct().ToList();
                var items = await _context.Items
                    .Where(x => x.SapId == sapId && itemCodes.Contains(x.ItemCode))
                    .ToDictionaryAsync(x => x.ItemCode, x => x);

                // 2️⃣ جيب كل الـ Warehouses مرة واحدة
                var warehouseCodes = itemWarehouses
                    .SelectMany(x => x.ItemWarehouseInfoCollection.Select(w => w.WarehouseCode))
                    .Distinct()
                    .ToList();

                var warehouses = await _context.Warehouses
                    .Where(x => x.SapId == sapId && warehouseCodes.Contains(x.WarehouseCode))
                    .ToDictionaryAsync(x => x.WarehouseCode, x => x);

                // 3️⃣ جيب كل الـ WarehouseItems الموجودة مرة واحدة
                var itemIds = items.Values.Select(x => x.ItemId).ToList();
                var warehouseIds = warehouses.Values.Select(x => x.WarehouseId).ToList();

                var existingWarehouseItems = await _context.WarehouseItems
                    .Where(x => itemIds.Contains(x.ItemId) && warehouseIds.Contains(x.WarehouseId))
                    .ToDictionaryAsync(x => new { x.ItemId, x.WarehouseId }, x => x);

             

                var newWarehouseItems = new List<WarehouseItem>();

                // 5️⃣ لف على الداتا وجهز التحديثات
                foreach (var iw in itemWarehouses)
                {
                    if (!items.TryGetValue(iw.ItemCode, out var item))
                    {
                        logger.LogWarning("Item not found: {ItemCode}. Skipping...", iw.ItemCode);
                        continue;
                    }

                    foreach (var wh in iw.ItemWarehouseInfoCollection)
                    {
                        if (!warehouses.TryGetValue(wh.WarehouseCode, out var warehouse))
                        {
                            logger.LogWarning("Warehouse not found: {WarehouseCode}. Skipping...", wh.WarehouseCode);
                            continue;
                        }

                        var key = new { ItemId = item.ItemId, WarehouseId = warehouse.WarehouseId };

                        // Update or Add WarehouseItem
                        if (existingWarehouseItems.TryGetValue(key, out var existingWh))
                        {
                            existingWh.InStock = wh.InStock;
                            existingWh.MinStock = wh.MinimalStock;
                        }
                        else
                        {
                            var newWh = new WarehouseItem
                            {
                                ItemId = item.ItemId,
                                WarehouseId = warehouse.WarehouseId,
                                ItemCode = item.ItemCode,
                                WarehouseCode = warehouse.WarehouseCode,
                                InStock = wh.InStock,
                                MinStock = wh.MinimalStock,
                                HasActiveBOM = item.ProcurementType == "bom_Make",
                                IsBatchManaged = item.BatchNumbers,
                                FinishedGood = wh.InStock <= wh.MinimalStock,
                                
                            };
                            newWarehouseItems.Add(newWh);
                            existingWarehouseItems[key] = newWh; // علشان نستخدمها في الـ FinishedGood check
                        }

                   
                    }
                }

                // 6️⃣ Add الجديد مرة واحدة
                if (newWarehouseItems.Any())
                    await _context.WarehouseItems.AddRangeAsync(newWarehouseItems);

              
                // 7️⃣ احفظ كل حاجة
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error upserting item warehouses for SapId: {SapId}", sapId);
                throw;
            }
        }
        private async Task UpsertItemBarCodeAsync(int sapId, ICollection<ItemBarCodesDtoResponse> itemBarCodes)
        {
            if (itemBarCodes == null || !itemBarCodes.Any())
                return;

            // 1️⃣ جيب كل الـ Items مرة واحدة
            var itemCodes = itemBarCodes.Select(x => x.ItemCode).Distinct().ToList();

            var items = await _context.Items
                .Where(x => x.SapId == sapId && itemCodes.Contains(x.ItemCode))
                .ToDictionaryAsync(x => x.ItemCode);

            // 2️⃣ جيب كل الـ BarCode settings مرة واحدة (للـ validation)
            var barCodeSettings = await GetBarCodeSettingsAsync(sapId);

            foreach (var ib in itemBarCodes)
            {
                if (!items.TryGetValue(ib.ItemCode, out var item))
                {
                    logger.LogWarning("Item not found: {ItemCode}. Skipping...", ib.ItemCode);
                    continue;
                }

                foreach (var ibc in ib.ItemBarCodeCollection)
                {
                    // ✅ Check dynamic validation (بدون query)
                    var staticBarCode = CheckDynamicCodeValidationLocal(barCodeSettings, ibc.Barcode);
                    var originalBarCode = ibc.Barcode;

                    if (staticBarCode != null)
                    {
                        ibc.Barcode = staticBarCode;
                    }

                    // ✅ Validate barcode (بدون query)
                    var isValid = CheckCodeValidationLocal(barCodeSettings, ibc.Barcode);
                    if (!isValid)
                    {
                        logger.LogWarning("Invalid barcode: {BarCode}. Skipping...", ibc.Barcode);
                        continue;
                    }

                    // 3️⃣ جيب الـ BarCode من DB
                    var barCode = await _context.ItemBarCodes
                        .Include(e => e.DynamicBarCodes)
                        .FirstOrDefaultAsync(x => x.SapId == sapId && x.BarCode == ibc.Barcode);

                    // 4️⃣ Add or Update
                    if (barCode == null)
                    {
                        barCode = new ItemBarCode
                        {
                            ItemId = item.ItemId,
                            BarCode = ibc.Barcode,
                            FreeText = ibc.FreeText,
                            UoMEntry = ibc.UoMEntry,
                            AbsEntry = ibc.AbsEntry,
                            SapFlag = true,
                            CreatedDate = DateTime.UtcNow,
                            SapId = sapId,
                            DynamicBarCodes = new List<DynamicBarCode>()
                        };
                        await _context.ItemBarCodes.AddAsync(barCode);
                    }
                    else
                    {
                        barCode.FreeText = ibc.FreeText;
                        barCode.UoMEntry = ibc.UoMEntry;
                        barCode.AbsEntry = ibc.AbsEntry;
                        barCode.SapFlag = true;
                    }

                    await _context.SaveChangesAsync(); // ✅ Save عشان نجيب ItemBarCodeId

                    // 5️⃣ Handle dynamic barcode
                    if (staticBarCode != null)
                    {
                        var dynamicBarCodeModel = barCode.DynamicBarCodes
                            .FirstOrDefault(e => e.BarCode == originalBarCode);

                        if (dynamicBarCodeModel == null)
                        {
                            dynamicBarCodeModel = new DynamicBarCode
                            {
                                BarCode = originalBarCode,
                                ItemBarCodeId = barCode.ItemBarCodeId,
                                AbsEntry = ibc.AbsEntry,
                                SapFlag = true,
                                SapId = sapId,
                                IsActive = true,
                            };
                            await _context.DynamicBarCodes.AddAsync(dynamicBarCodeModel);
                        }
                        else
                        {
                            dynamicBarCodeModel.BarCode = originalBarCode;
                            dynamicBarCodeModel.AbsEntry = ibc.AbsEntry;
                            dynamicBarCodeModel.SapFlag = true;
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        // ✅ Helper methods بدون database queries
        private string? CheckDynamicCodeValidationLocal(List<BarCodeSetting> settings, string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode) || !settings.Any())
                return null;

            foreach (var setting in settings)
            {
                if (!string.IsNullOrEmpty(setting.StartsWith) &&
                    !barCode.StartsWith(setting.StartsWith))
                    continue;

                if (barCode.Length != setting.TotalLength)
                    continue;

                if (setting.SapLength <= 0 || setting.SapLength > barCode.Length)
                    continue;

                return barCode.Substring(0, setting.SapLength);
            }

            return null;
        }

        private bool CheckCodeValidationLocal(List<BarCodeSetting> settings, string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode) || !settings.Any())
                return false;

            return settings.Any(setting =>
                (string.IsNullOrEmpty(setting.StartsWith) || barCode.StartsWith(setting.StartsWith)) &&
                barCode.Length == setting.SapLength
            );
        }

        private async Task<List<BarCodeSetting>> GetBarCodeSettingsAsync(int sapId)
        {
            return await _context.BarCodeSettings
                .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
                .ToListAsync();
        }
      
      
   
    }

}
