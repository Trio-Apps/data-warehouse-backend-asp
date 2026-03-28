using Azure;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Linq;
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


        public async Task<string> SyncItemsAsync(int sapId)
        {
            var state = await _syncRepo.GetLastSyncPaginationSkipAsync(sapId, EntitiesName.item.ToString());
            int skip = state;



            var lastSync = await _syncRepo.GetLastSyncDateAsync(sapId, EntitiesName.item.ToString());
            var filterDate = lastSync.ToString("yyyy-MM-dd");


            //const int pageSize = 1;

            int totalProcessed = 0;

            while (true)
            {
                // ✅ Paging صحيح + ترتيب ثابت + Select أخف
                var top = 20;

                //var url =
                //    $"Items?$filter=UpdateDate ge '{filterDate}'" +
                //    $"&$orderby=UpdateDate,ItemCode" +
                //    $"&$skip={skip}" +
                //    $"&$top={top}" +
                //    $"&$select=ItemCode,ItemName,ManageBatchNumbers,ItemsGroupCode,ProcurementMethod,UpdateDate," +
                //    $"ItemPrices,ItemWarehouseInfoCollection,ItemBarCodeCollection";

                var url =
                $"Items?$filter=UpdateDate ge '{filterDate}'&$skip={skip}&$top={top}&$select=ItemCode,ItemName,ManageBatchNumbers,ItemsGroupCode,PurchaseItem,SalesItem,InventoryItem,Valid,Frozen,ItemPrices,ItemWarehouseInfoCollection,ItemBarCodeCollection,ProcurementMethod";

                logger.LogInformation("SAP Items Sync. sapId={sapId}, skip={skip}, top={top}", sapId, skip, top);


                var json = await sap.GetAllSap(sapId, url);


             //   logger.LogInformation("SAP JSON. json={json}",json);


                SapItemsResponse? items;
                try
                {
                    items = JsonSerializer.Deserialize<SapItemsResponse>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
                catch (JsonException ex)
                {
                    throw new Exception("Failed to deserialize SAP items response", ex);
                }

                if (items?.Value == null || !items.Value.Any())
                {
                    // ✅ خلّصنا
                    await _syncRepo.UpdateLastSyncDateAsync(sapId, EntitiesName.item.ToString(), DateTime.UtcNow);
                    await _syncRepo.UpdateLastSyncPaginationSkipAsync(sapId, EntitiesName.item.ToString(), 0);

                    logger.LogInformation("Items sync finished. TotalProcessed={total}", totalProcessed);
                    return totalProcessed == 0 ? "No items to sync" : $"Synced {totalProcessed} items.";
                }

                // ✅ اعمل processing للدفعة فقط
                var processed = await AddOrUpdateItemsAsync(sapId, items.Value, skip + items.Value.Count);
                totalProcessed += processed;


                // ✅ حدّث skip
                skip += items.Value.Count;
            }
       
        }
        private async Task<int> AddOrUpdateItemsAsync(int sapId, List<SapItemDto> sapItems, int nextSkip)
        {
            if (sapItems == null || sapItems.Count == 0)
                return 0;

            var codes = sapItems
                .Select(s => s.ItemCode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            var existingItems = await _context.Items
                .Where(i => i.SapId == sapId && codes.Contains(i.ItemCode))
                .ToListAsync();

            var existingByCode = existingItems.ToDictionary(x => x.ItemCode, x => x);

            var itemsToAdd = new List<Item>(capacity: Math.Max(16, sapItems.Count));
            var itemWarehouses = new List<SapItemWarehouseDtoResponse>(capacity: sapItems.Count);
            var itemBarCodes = new List<ItemBarCodesDtoResponse>(capacity: sapItems.Count);
            var itemPrices = new List<SapItemPricesDtoResponse>(capacity: sapItems.Count);

            var prevAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                foreach (var sap in sapItems)
                {
                    if (string.IsNullOrWhiteSpace(sap.ItemCode))
                        continue;

                    var lastPrice = sap.ItemPrices?
                        .OrderBy(p => p.PriceList)
                        .LastOrDefault()?.Price ?? 0;

                    logger.LogInformation("One Item From Sap. sap={sap}", sap.ItemCode);

                    if (existingByCode.TryGetValue(sap.ItemCode, out var existingItem))
                    {
                        existingItem.ItemName = sap.ItemName ?? existingItem.ItemName;
                        existingItem.ItemGroup = sap.ItemsGroupCode.ToString();
                        existingItem.PurchasePrice = (decimal)lastPrice;
                        existingItem.SalesPrice = (decimal)lastPrice;
                        existingItem.UpdateDate = DateTime.UtcNow;
                        existingItem.BatchNumbers = sap.ManageBatchNumbers == "tYES";
                        existingItem.ProcurementType = sap.ProcurementMethod;
                        existingItem.PurchaseItem = sap.PurchaseItem == "tYES";
                        existingItem.SalesItem = sap.SalesItem == "tYES";
                        existingItem.InventoryItem = sap.InventoryItem == "tYES";
                        existingItem.Valid = sap.Valid == "tYES";
                        existingItem.Frozen = sap.Frozen == "tYES";
                    }
                    else
                    {
                        itemsToAdd.Add(new Item
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
                            ProcurementType = sap.ProcurementMethod,
                            Frozen = sap.Frozen == "tYES",
                            Valid = sap.Valid == "tYES",
                            InventoryItem = sap.InventoryItem == "tYES",
                            SalesItem = sap.SalesItem == "tYES",
                            PurchaseItem = sap.PurchaseItem == "tYES",
                        });
                    }

                    var sparseWh = sap.ItemWarehouseInfoCollection
                        .Where(w =>
                            !string.IsNullOrWhiteSpace(w.WarehouseCode) &&
                            ((w.InStock ?? 0m) != 0m || (w.MinimalStock ?? 0m) != 0m))
                        .ToList();

                    itemWarehouses.Add(new SapItemWarehouseDtoResponse
                    {
                        ItemCode = sap.ItemCode,
                        ItemWarehouseInfoCollection = sparseWh
                    });

                    itemBarCodes.Add(new ItemBarCodesDtoResponse
                    {
                        ItemCode = sap.ItemCode,
                        ItemBarCodeCollection = sap.ItemBarCodeCollection
                    });

                    itemPrices.Add(new SapItemPricesDtoResponse
                    {
                        ItemCode = sap.ItemCode,
                        ItemPrices = sap.ItemPrices ?? new List<SapItemPriceDto>()
                    });
                }

                if (itemsToAdd.Count > 0)
                    await _context.Items.AddRangeAsync(itemsToAdd);

                await _context.SaveChangesAsync();

                await UpsertItemWarehouseAsync(sapId, itemWarehouses);
                await UpsertItemBarCodeAsync(sapId, itemBarCodes);
                await UpsertItemPricesAsync(sapId, itemPrices);

                var row = await _context.SapSyncPaginations
                    .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == EntitiesName.item.ToString());

                row.Skip = nextSkip;
                _context.Entry(row).Property(x => x.Skip).IsModified = true;

                await _context.SaveChangesAsync();

                return sapItems.Count;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = prevAutoDetect;
            }
        }
        private async Task UpsertItemWarehouseAsync(int sapId, ICollection<SapItemWarehouseDtoResponse> itemWarehouses)
        {
            // لو الدفعة Sparse ومفيهاش warehouses أصلاً، متعملش حاجة
            if (itemWarehouses == null || itemWarehouses.Count == 0)
                return;

            // خُد بس اللي عنده Warehouses بعد الفلترة
            var relevant = itemWarehouses
                .Where(x => x.ItemWarehouseInfoCollection != null && x.ItemWarehouseInfoCollection.Count > 0)
                .ToList();

            if (relevant.Count == 0)
                return;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var itemCodes = relevant.Select(x => x.ItemCode).Distinct().ToList();

                // ✅ Items dict
                var items = await _context.Items
                    .Where(x => x.SapId == sapId && itemCodes.Contains(x.ItemCode))
                    .ToDictionaryAsync(x => x.ItemCode, x => x);

                // ✅ Warehouses المطلوبة فقط (من الـ sparse)
                var warehouseCodes = relevant
                    .SelectMany(x => x.ItemWarehouseInfoCollection.Select(w => w.WarehouseCode))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();

                var warehouses = await _context.Warehouses
                    .Where(x => x.SapId == sapId && warehouseCodes.Contains(x.WarehouseCode))
                    .ToDictionaryAsync(x => x.WarehouseCode, x => x);

                var itemIds = items.Values.Select(x => x.ItemId).Distinct().ToList();
                var warehouseIds = warehouses.Values.Select(x => x.WarehouseId).Distinct().ToList();

                // ✅ الموجود فقط للـ batch
                var existingWarehouseItems = await _context.WarehouseItems
                    .Where(x => itemIds.Contains(x.ItemId) && warehouseIds.Contains(x.WarehouseId))
                    .ToListAsync();

                // ✅ Key = (ItemId, WarehouseId)
                var existingByKey = existingWarehouseItems.ToDictionary(
                    x => (x.ItemId, x.WarehouseId),
                    x => x
                );

                var newWarehouseItems = new List<WarehouseItem>(capacity: 1024);

                // ✅ تسريع EF أثناء الدفعة
                var prevAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                try
                {
                    foreach (var iw in relevant)
                    {
                        if (!items.TryGetValue(iw.ItemCode, out var item))
                            continue;

                        foreach (var wh in iw.ItemWarehouseInfoCollection)
                        {
                            if (!warehouses.TryGetValue(wh.WarehouseCode, out var warehouse))
                                continue;

                            var key = (item.ItemId, warehouse.WarehouseId);

                            var inStock = wh.InStock ?? 0m;
                            var minStock = wh.MinimalStock ?? 0m;

                            if (existingByKey.TryGetValue(key, out var existing))
                            {
                                existing.InStock = (double)inStock;
                                existing.MinStock = (double)minStock;
                                existing.HasActiveBOM = item.ProcurementType == "bom_Make";
                                existing.IsBatchManaged = item.BatchNumbers;
                                existing.FinishedGood = inStock <= minStock;
                            }
                            else
                            {
                                var newWh = new WarehouseItem
                                {
                                    ItemId = item.ItemId,
                                    WarehouseId = warehouse.WarehouseId,
                                    ItemCode = item.ItemCode,
                                    WarehouseCode = warehouse.WarehouseCode,
                                    InStock = (double)inStock,
                                    MinStock = (double)minStock,
                                    HasActiveBOM = item.ProcurementType == "bom_Make",
                                    IsBatchManaged = item.BatchNumbers,
                                    FinishedGood = inStock <= minStock
                                };


                                newWarehouseItems.Add(newWh);
                                existingByKey[key] = newWh;
                            }
                        }
                    }

                    if (newWarehouseItems.Count > 0)
                        await _context.WarehouseItems.AddRangeAsync(newWarehouseItems);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = prevAutoDetect;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error upserting item warehouses for SapId: {SapId}", sapId);
                throw;
            }
        }

        private async Task UpsertItemPricesAsync(int sapId, ICollection<SapItemPricesDtoResponse> itemPrices)
        {
            if (itemPrices == null || itemPrices.Count == 0)
                return;

            var relevant = itemPrices
                .Where(x => !string.IsNullOrWhiteSpace(x.ItemCode) &&
                            x.ItemPrices != null &&
                            x.ItemPrices.Count > 0)
                .ToList();

            if (relevant.Count == 0)
                return;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var itemCodes = relevant
                    .Select(x => x.ItemCode)
                    .Distinct()
                    .ToList();

                var items = await _context.Items
                    .Where(x => x.SapId == sapId && itemCodes.Contains(x.ItemCode))
                    .ToDictionaryAsync(x => x.ItemCode, x => x);

                var itemIds = items.Values
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var existingItemPrices = await _context.ItemPrices
                    .Where(x => itemIds.Contains(x.ItemId))
                    .Include(x => x.UomPrices)
                    .ToListAsync();

                if (existingItemPrices.Count > 0)
                {
                    var existingUomPrices = existingItemPrices
                        .SelectMany(x => x.UomPrices)
                        .ToList();

                    if (existingUomPrices.Count > 0)
                        _context.ItemUomPrices.RemoveRange(existingUomPrices);

                    _context.ItemPrices.RemoveRange(existingItemPrices);
                    await _context.SaveChangesAsync();
                }

                var newItemPrices = new List<DataWarehouse.Domain.Entities.Actors.ItemPrice>(capacity: 1024);

                var prevAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                try
                {
                    foreach (var itemPriceDto in relevant)
                    {
                        if (!items.TryGetValue(itemPriceDto.ItemCode, out var item))
                            continue;

                        foreach (var sapPrice in itemPriceDto.ItemPrices)
                        {
                            var newItemPrice = new DataWarehouse.Domain.Entities.Actors.ItemPrice
                            {
                                ItemId = item.ItemId,
                                PriceList = sapPrice.PriceList,
                                Price = sapPrice.Price,
                                Currency = sapPrice.Currency ?? string.Empty,
                              
                                BasePriceList = sapPrice.BasePriceList,
                                Factor = sapPrice.Factor,
                                UomPrices = new List<ItemUomPrice>()
                            };

                            if (sapPrice.UoMPrices != null && sapPrice.UoMPrices.Count > 0)
                            {
                                foreach (var sapUom in sapPrice.UoMPrices)
                                {
                                    newItemPrice.UomPrices.Add(new ItemUomPrice
                                    {
                                        PriceList = sapUom.PriceList,
                                        UoMEntry = sapUom.UoMEntry,
                                        ReduceBy = sapUom.ReduceBy,
                                        Price = sapUom.Price,
                                        Currency = sapUom.Currency ?? string.Empty,
                                      
                                        Auto = string.Equals(sapUom.Auto, "tYES", StringComparison.OrdinalIgnoreCase)
                                    });
                                }
                            }

                            newItemPrices.Add(newItemPrice);
                        }
                    }

                    if (newItemPrices.Count > 0)
                        await _context.ItemPrices.AddRangeAsync(newItemPrices);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = prevAutoDetect;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error upserting item prices for SapId: {SapId}", sapId);
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
        //public async Task<string> SyncItemsAsync(int sapId)
        //{


        //    // 1️⃣ آخر Sync
        //    //  var lastSync = await _syncRepo.GetLastSyncDateAsync(EntitiesName.item.ToString());

        //    // 2️⃣ بناء Query
        //    var itemList = new List<SapItemDto>();
        //    var state = await _syncRepo.GetLastSyncPaginationSkipAsync(sapId, EntitiesName.item.ToString());
        //    int skip = state;
        //    bool hasMore = true;
        //    var lastSync = await _syncRepo.GetLastSyncDateAsync(sapId, EntitiesName.item.ToString());

        //    // 2️⃣ بناء Query
        //    var filterDate = lastSync.ToString("yyyy-MM-dd");



        //    while (hasMore)
        //    {
        //        var url =
        //        $"Items?$filter=UpdateDate ge '{filterDate}'&$skip={skip}&$select=ItemCode,ItemName,ManageBatchNumbers,ItemsGroupCode,ItemPrices,ItemWarehouseInfoCollection,ItemBarCodeCollection,ProcurementMethod";


        //        var json = await sap.GetAllSap(sapId, url);
        //        SapItemsResponse? items;

        //        try
        //        {
        //            items = JsonSerializer.Deserialize<SapItemsResponse>(json,
        //            new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            if (items?.Value != null && items.Value.Any())
        //            {
        //                itemList.AddRange(items.Value); // ضيف العناصر اللي رجع
        //                                                //  logger.LogInformation("Fetched {count} items. Total so far: {total}", items.Value.Count, itemList.Count);

        //                logger.LogInformation("Url is: {url}", url);

        //                logger.LogInformation("Mapping items: {items}", JsonSerializer.Serialize(items.Value, new JsonSerializerOptions { WriteIndented = true }));


        //                skip += items.Value.Count; // عشان الـ next batch

        //                await AddOrUpdateItemsAsync(sapId, items.Value, skip);

        //            }
        //            else
        //            {
        //                // 6️⃣ تحديث آخر Sync
        //                await _syncRepo.UpdateLastSyncDateAsync(sapId,
        //                    EntitiesName.item.ToString(),
        //                    DateTime.UtcNow
        //                );
        //                await _syncRepo.UpdateLastSyncPaginationSkipAsync(sapId,
        //                  EntitiesName.item.ToString(),
        //                   0
        //                );

        //                hasMore = false; // مفيش بيانات جديدة
        //            }
        //        }
        //        catch (JsonException ex)
        //        {
        //            throw new Exception("Failed to deserialize SAP items response", ex);
        //        }
        //    }

        //    logger.LogInformation("Finished fetching all items. Total count: {total}", itemList.Count);

        //    // ❌ Response نفسه غلط

        //    if (!itemList.Any())
        //        return "No items to sync";

        //    //// 6️⃣ تحديث آخر Sync

        //    return $"Synced {itemList.Count} items. Inserted new: {itemList.Count}";
        //}

        //private async Task<int> AddOrUpdateItemsAsync(int sapId, List<SapItemDto> sapItems, int skip)
        //{
        //    if (sapItems == null || !sapItems.Any())
        //        return 0;

        //    // 1️⃣ جلب الأكواد الموجودة + العناصر الموجودة
        //    var existingItems = await _context.Items
        //        .Where(i => i.SapId == sapId && sapItems.Select(s => s.ItemCode).Contains(i.ItemCode))
        //        .ToListAsync();

        //    var existingCodes = existingItems.Select(i => i.ItemCode).ToHashSet();

        //    var itemsToAdd = new List<Item>();
        //    var itemWarehouse = new List<SapItemWarehouseDtoResponse>();
        //    var itemBarCode = new List<ItemBarCodesDtoResponse>();
        //    int processedCount = 0;

        //    foreach (var sap in sapItems)
        //    {
        //        // 2️⃣ احنا عايزين نضيف جديد أو نحدث الموجود
        //        var existingItem = existingItems.FirstOrDefault(i => i.ItemCode == sap.ItemCode);

        //        var lastPrice = sap.ItemPrices?.OrderBy(p => p.PriceList).LastOrDefault()?.Price ?? 0;

        //        if (existingItem != null)
        //        {
        //            // 🔹 تحديث الحقول الموجودة
        //            existingItem.ItemName = sap.ItemName ?? existingItem.ItemName;
        //            existingItem.ItemGroup = sap.ItemsGroupCode.ToString();
        //            existingItem.PurchasePrice = (decimal)lastPrice;
        //            existingItem.SalesPrice = (decimal)lastPrice;
        //            existingItem.UpdateDate = DateTime.UtcNow;

        //            existingItem.BatchNumbers = sap.ManageBatchNumbers == "tYES";
        //            existingItem.ProcurementType = sap.ProcurementMethod;
        //        }
        //        else
        //        {
        //            // 🔹 إضافة جديد
        //            var newItem = new Item
        //            {
        //                ItemCode = sap.ItemCode,
        //                ItemName = sap.ItemName ?? "Unknown Item",
        //                ItemGroup = sap.ItemsGroupCode.ToString(),
        //                PurchasePrice = (decimal)lastPrice,
        //                SalesPrice = (decimal)lastPrice,
        //                UpdateDate = DateTime.UtcNow,
        //                UoM = "type",
        //                SapId = sapId,
        //                BatchNumbers = sap.ManageBatchNumbers == "tYES",
        //                ProcurementType = sap.ProcurementMethod
        //            };
        //            itemsToAdd.Add(newItem);
        //        }



        //        var newItemWarehouse = new SapItemWarehouseDtoResponse
        //        {
        //            ItemCode = sap.ItemCode,
        //            ItemWarehouseInfoCollection = sap.ItemWarehouseInfoCollection
        //        };
        //        var newItemBarCode = new ItemBarCodesDtoResponse
        //        {
        //            ItemCode = sap.ItemCode,
        //            ItemBarCodeCollection = sap.ItemBarCodeCollection
        //        };
        //        itemWarehouse.Add(newItemWarehouse);
        //        itemBarCode.Add(newItemBarCode);

        //        // 3️⃣ Upsert Warehouses لكل Item


        //        processedCount++;
        //    }

        //    // 4️⃣ Add new items مرة واحدة
        //    if (itemsToAdd.Any())
        //    {
        //        await _context.Items.AddRangeAsync(itemsToAdd);

        //    }
        //    await _context.SaveChangesAsync();

        //    await UpsertItemWarehouseAsync(sapId, itemWarehouse);
        //    await UpsertItemBarCodeAsync(sapId, itemBarCode);

        //    // 5️⃣ حفظ كل التغييرات مرة واحدة (Add + Update)

        //    // 6️⃣ تحديث pagination
        //    await _syncRepo.UpdateLastSyncPaginationSkipAsync(sapId,
        //        EntitiesName.item.ToString(),
        //        skip
        //    );

        //    return processedCount;
        //}

        //private async Task UpsertItemWarehouseAsync(int sapId, ICollection<SapItemWarehouseDtoResponse> itemWarehouses)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // 1️⃣ جيب كل الـ Items مرة واحدة
        //        var itemCodes = itemWarehouses.Select(x => x.ItemCode).Distinct().ToList();
        //        var items = await _context.Items
        //            .Where(x => x.SapId == sapId && itemCodes.Contains(x.ItemCode))
        //            .ToDictionaryAsync(x => x.ItemCode, x => x);

        //        // 2️⃣ جيب كل الـ Warehouses مرة واحدة
        //        var warehouseCodes = itemWarehouses
        //            .SelectMany(x => x.ItemWarehouseInfoCollection.Select(w => w.WarehouseCode))
        //            .Distinct()
        //            .ToList();

        //        var warehouses = await _context.Warehouses
        //            .Where(x => x.SapId == sapId && warehouseCodes.Contains(x.WarehouseCode))
        //            .ToDictionaryAsync(x => x.WarehouseCode, x => x);

        //        // 3️⃣ جيب كل الـ WarehouseItems الموجودة مرة واحدة
        //        var itemIds = items.Values.Select(x => x.ItemId).ToList();
        //        var warehouseIds = warehouses.Values.Select(x => x.WarehouseId).ToList();

        //        var existingWarehouseItems = await _context.WarehouseItems
        //            .Where(x => itemIds.Contains(x.ItemId) && warehouseIds.Contains(x.WarehouseId))
        //            .ToDictionaryAsync(x => new { x.ItemId, x.WarehouseId }, x => x);



        //        var newWarehouseItems = new List<WarehouseItem>();

        //        // 5️⃣ لف على الداتا وجهز التحديثات
        //        foreach (var iw in itemWarehouses)
        //        {
        //            if (!items.TryGetValue(iw.ItemCode, out var item))
        //            {
        //                logger.LogWarning("Item not found: {ItemCode}. Skipping...", iw.ItemCode);
        //                continue;
        //            }

        //            foreach (var wh in iw.ItemWarehouseInfoCollection)
        //            {
        //                if (!warehouses.TryGetValue(wh.WarehouseCode, out var warehouse))
        //                {
        //                    logger.LogWarning("Warehouse not found: {WarehouseCode}. Skipping...", wh.WarehouseCode);
        //                    continue;
        //                }

        //                var key = new { ItemId = item.ItemId, WarehouseId = warehouse.WarehouseId };

        //                // Update or Add WarehouseItem
        //                if (existingWarehouseItems.TryGetValue(key, out var existingWh))
        //                {
        //                    existingWh.InStock = wh.InStock;
        //                    existingWh.MinStock = wh.MinimalStock;
        //                }
        //                else
        //                {
        //                    var newWh = new WarehouseItem
        //                    {
        //                        ItemId = item.ItemId,
        //                        WarehouseId = warehouse.WarehouseId,
        //                        ItemCode = item.ItemCode,
        //                        WarehouseCode = warehouse.WarehouseCode,
        //                        InStock = wh.InStock,
        //                        MinStock = wh.MinimalStock,
        //                        HasActiveBOM = item.ProcurementType == "bom_Make",
        //                        IsBatchManaged = item.BatchNumbers,
        //                        FinishedGood = wh.InStock <= wh.MinimalStock,

        //                    };
        //                    newWarehouseItems.Add(newWh);
        //                    existingWarehouseItems[key] = newWh; // علشان نستخدمها في الـ FinishedGood check
        //                }


        //            }
        //        }

        //        // 6️⃣ Add الجديد مرة واحدة
        //        if (newWarehouseItems.Any())
        //            await _context.WarehouseItems.AddRangeAsync(newWarehouseItems);


        //        // 7️⃣ احفظ كل حاجة
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        logger.LogError(ex, "Error upserting item warehouses for SapId: {SapId}", sapId);
        //        throw;
        //    }
        //}



    }

}
