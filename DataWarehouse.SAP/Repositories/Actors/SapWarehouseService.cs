using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.SAP.Enums;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Actors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.WarehouseSapModel;

namespace DataWarehouse.SAP.Repositories.Actors
{
    public class SapWarehouseService : ISapWarehouseService
    {
        private readonly IBaseSap<WarehouseSapResponse> sap;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapWarehouseService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapWarehouseService(IBaseSap<WarehouseSapResponse> sap, ISapSyncStatusRepository syncRepo, DataWarehouseDbContext context, ILogger<SapWarehouseService> logger)
        {
            this.sap = sap;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }

        public async Task<string> SyncWarehouseAsync(int sapId)
        {
            // 1️⃣ آخر Sync
            //  var lastSync = await _syncRepo.GetLastSyncDateAsync(EntitiesName.warehouse.ToString());

            // 2️⃣ بناء Query
            var warehouseList = new List<WarehouseDto>();
            var state = await _syncRepo.GetLastSyncPaginationSkipAsync( sapId,EntitiesName.warehouse.ToString());
            int skip = state;
            bool hasMore = true;

            while (hasMore)
            {
                var url = $"Warehouses?$skip={skip}&$select=WarehouseCode,WarehouseName";
                // لو عايز ال expand ممكن تفك التعليق
                // + "&$expand=warehouseWarehouseInfoCollection";
                

                var json = await sap.GetAllSap(sapId,url);
                WarehouseSapResponse? warehouses;

                try
                {
                    warehouses = JsonSerializer.Deserialize<WarehouseSapResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (warehouses?.Value != null && warehouses.Value.Any())
                    {
                        warehouseList.AddRange(warehouses.Value); // ضيف العناصر اللي رجع
                        //  logger.LogInformation("Fetched {count} warehouses. Total so far: {total}", warehouses.Value.Count, warehouseList.Count);
                       
                        logger.LogInformation("Url is: {url}", url);

                        logger.LogInformation("Mapping warehouses: {warehouses}", JsonSerializer.Serialize(warehouses.Value, new JsonSerializerOptions { WriteIndented = true }));


                        skip += warehouses.Value.Count; // عشان الـ next batch
                      
                        await AddNewWarehousesAsync(sapId,warehouses.Value,skip);

                    }
                    else
                    {
                        hasMore = false; // مفيش بيانات جديدة
                    }
                }
                catch (JsonException ex)
                {
                    throw new Exception("Failed to deserialize SAP warehouses response", ex);
                }


            }

            logger.LogInformation("Finished fetching all warehouses. Total count: {total}", warehouseList.Count);

            // ❌ Response نفسه غلط
           
            if (!warehouseList.Any())
                return "No warehouses to sync";

            //// 5️⃣ Upsert في WMS


            //// 6️⃣ تحديث آخر Sync
           


            return $"Synced {warehouseList.Count} warehouses. Inserted new: {warehouseList.Count}";
        }

        private async Task<int> AddNewWarehousesAsync(int  sapId, List<WarehouseDto> sapWarehouses,int skip)
        {
            // 1️⃣ هات كل الأكواد الموجودة في الداتا بيز
            var existingCodes = await _context.Warehouses
                .Where(e=>e.SapId == sapId)
                .Select(w => w.WarehouseCode )
                .ToListAsync();

            // 2️⃣ فلترة الجديد بس
            var newWarehouses = sapWarehouses
                .Where(sap => !existingCodes.Contains(sap.WarehouseCode))
                .Select(sap => new Warehouse
                {
                    WarehouseCode = sap.WarehouseCode,
                    WarehouseName = sap.WarehouseName,
                    SapId = sapId,
                    UpdateDate = DateTime.UtcNow,
                })
                .ToList();

            await _syncRepo.UpdateLastSyncPaginationSkipAsync( sapId,
              EntitiesName.warehouse.ToString(), skip);
            // 3️⃣ مفيش جديد
            if (!newWarehouses.Any())
                return 0;

            // 4️⃣ Add + Save
             _context.Warehouses.AddRangeAsync(newWarehouses);

            await _syncRepo.UpdateLastSyncPaginationSkipAsync( sapId,
               EntitiesName.warehouse.ToString(),skip);

            await _context.SaveChangesAsync();

            return newWarehouses.Count;
        }

    }
}
