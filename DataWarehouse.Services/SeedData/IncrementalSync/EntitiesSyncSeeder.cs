using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.SeedData.IncrementalSync
{
    public static class EntitiesSyncSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();


            string[] entitiesNames = { "item", "warehouse", "purchase", "count" ,  "barcode" };


            // Sap 
            var entities = context.SapSyncStatuses.Select(e=>e.EntityName).ToList();
            foreach (var entityName in entitiesNames)
            {
                if (!entities.Contains(entityName))
                {
                    var dto = new SapSyncStatus
                    {
                        EntityName = entityName,
                        LastSyncDate = new DateTime(2000, 1, 1) // هنا التاريخ الثابت
                         , SapId = 1
                    };
                    context.SapSyncStatuses.Add(dto);
                }
            }



            // Sap pagination 

            var pagination = context.SapSyncPaginations.Select(e => e.EntityName).ToList();
            // 1️⃣ Seed Roles

            foreach (var entityName in entitiesNames)
            {
                if (!pagination.Contains(entityName))
                {
                    var dto = new SapSyncPagination
                    {
                        EntityName = entityName,
                        Skip = 0,
                         SapId = 1,
                    };
                    context.SapSyncPaginations.Add(dto);
                }
            }




            // wms 

            var wmsIncrememntal = context.WmsSyncStatuses.Select(e => e.EntityName).ToList();
            // 1️⃣ Seed Roles

            foreach (var entityName in entitiesNames)
            {
                if (!wmsIncrememntal.Contains(entityName))
                {
                    var dto = new WmsSyncStatus
                    {
                        EntityName = entityName,
                        LastSyncDate = new DateTime(2000, 1, 1) // هنا التاريخ الثابت
                         , SapId = 1,
                         

                    };
                    context.WmsSyncStatuses.Add(dto);
                }
            }


            await context.SaveChangesAsync();

        }
    }
}
