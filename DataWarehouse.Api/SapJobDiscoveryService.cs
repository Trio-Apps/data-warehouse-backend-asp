using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Repositories.Actors;
using DataWarehouse.SAP.Repositories.BarCode;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Api
{
    public class SapJobDiscoveryService
    {
        private readonly DataWarehouseDbContext _context;

        public SapJobDiscoveryService(DataWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task DiscoverAndRegisterAsync()
        {
            var saps = await _context.Saps
                .Where(x => !x.IsActive) // 👈 اللي مالهاش Jobs
                .ToListAsync();

            foreach (var sap in saps)
            {
                int sapId = sap.SapId;
                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:warehouses-sync",
                       j => j.SyncWarehousesAsync(sapId),
                        "*/2 * * * *"
                      );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:items-sync",
                    j => j.SyncItemsAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:barcodes-sync",
                    j => j.SyncBarcodesAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:barcodes-uom-sync",
                    j => j.SyncUomGroupsAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:barcodes-delete-sync",
                    j => j.SyncDeleteBarcodesAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:dynamic-barcodes-sync",
                    j => j.SyncDynamicBarcodesAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                    $"sap:{sapId}:dynamic-barcodes-delete-sync",
                    j => j.SyncDeleteDynamicBarcodesAsync(sapId),
                    "*/2 * * * *"
                );

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                   $"sap:{sapId}:business-partners-sync",
                   j => j.SyncBusinessPartnersAsync(sapId),
                   "*/2 * * * *"
               );
                // 👇 نعلّم إن الـ Jobs اتخلقت
                sap.IsActive = true;
            }
           if(saps != null)
            await _context.SaveChangesAsync();
        }
    }

}
