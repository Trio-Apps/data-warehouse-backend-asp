using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.SAP.Jobs.Actors;
using DataWarehouse.SAP.Repositories.Actors;
using DataWarehouse.SAP.Repositories.BarCode;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Api
{
    public static class HangfireJobScheduler
    {
        public static void RegisterJobs(
        IServiceProvider serviceProvider,
        IConfiguration config)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();


            var sapIds = context.Saps
                .Where(x => x.IsActive)
                .Select(x => x.SapId)
                .ToList();

            foreach (var sapId in sapIds)
            {
               
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

                RecurringJob.AddOrUpdate<SapJobsExecutor>(
                $"sap:{sapId}:purchases-orders-sync",
                j => j.SyncPurchaseAsync(sapId),
                "*/2 * * * *");

                //   RecurringJob.AddOrUpdate<SapWarehouseService>(
                //       $"sap:{sapId}:warehouses-sync",
                //       job => job.SyncWarehouseAsync(sapId),
                //        "*/2 * * * *"
                //   );

                //   RecurringJob.AddOrUpdate<SapItemService>(
                //       $"sap:{sapId}:items-sync",
                //       job => job.SyncItemsAsync(sapId),
                //    "*/2 * * * *"
                //   );

                //   RecurringJob.AddOrUpdate<SapBarCodeService>(
                //       $"sap:{sapId}:barcodes-sync",
                //       job => job.SyncBarCodeAsync(sapId),
                //       "*/2 * * * *"
                //   );
                //   RecurringJob.AddOrUpdate<SapBarCodeService>(
                //    $"sap:{sapId}:barcodes-Add-UomGeoup-sync",
                //    job => job.SyncItemUomGroupAsync(sapId),
                //   "*/2 * * * *"
                //);

                //   RecurringJob.AddOrUpdate<SapBarCodeService>(
                //      $"sap:{sapId}:barcodes-delete-sync",
                //      job => job.SyncDeleteBarCodeAsync(sapId),
                //      "*/2 * * * *"
                //  );
                //   RecurringJob.AddOrUpdate<SapDynamicBarCodeService>(
                //    $"sap:{sapId}:dynamic-barcodes-sync",
                //    job => job.SyncDynamicBarcodeAsync(sapId),
                //    "*/2 * * * *"
                //  );


                //   RecurringJob.AddOrUpdate<SapDynamicBarCodeService>(
                //      $"sap:{sapId}:dynamic-barcodes-delete-sync",
                //      job => job.SyncDeleteDynamicBarCodeAsync(sapId),
                //      "*/2 * * * *"
                //    );


            }



            RecurringJob.AddOrUpdate<SapJobDiscoveryService>(
                      $"check-new-sap",
                      job => job.DiscoverAndRegisterAsync(),
                      "*/1 * * * *"
             );

        }
    }
    
 }

