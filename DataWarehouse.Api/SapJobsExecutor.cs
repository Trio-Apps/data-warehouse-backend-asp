
using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Interfaces.Proccesses;
using DataWarehouse.SAP.Repositories.Actors;
using DataWarehouse.SAP.Repositories.BarCode;
using Hangfire;
using Hangfire.Storage;


namespace DataWarehouse.Api
{
   

    public class SapJobsExecutor
    {
        private readonly ISapWarehouseService _warehouseService;
        private readonly ISapItemService _itemService;
        private readonly ISapBarCodeService _barCodeService;
        private readonly ISapDynamicBarCodeService _dynamicBarCodeService;
        private readonly IBusinessPartnersSupplierService businessPartnersService;
        private readonly ISapPurchaseService purchaseService;
        private readonly ISapReceiptService receiptService;
        private readonly ISapGoodsReturnService goodsReturnService;
        private readonly ISapDeliveryNoteService deliveryNoteService;
        private readonly ISapSalesReturnService salesReturnService;
        private readonly ISapSalesService salesService;

        public SapJobsExecutor(
            ISapWarehouseService warehouseService,
            ISapItemService itemService,
            ISapBarCodeService barCodeService,
            ISapDynamicBarCodeService dynamicBarCodeService,
            IBusinessPartnersSupplierService businessPartnersService,
            ISapPurchaseService purchaseService,
            ISapReceiptService receiptService,
            ISapGoodsReturnService goodsReturnService,
            ISapDeliveryNoteService deliveryNoteService,
            ISapSalesReturnService salesReturnService,
            ISapSalesService salesService)
        {
            _warehouseService = warehouseService;
            _itemService = itemService;
            _barCodeService = barCodeService;
            _dynamicBarCodeService = dynamicBarCodeService;
            this.businessPartnersService = businessPartnersService;
            this.purchaseService = purchaseService;
            this.receiptService = receiptService;
            this.goodsReturnService = goodsReturnService;
            this.deliveryNoteService = deliveryNoteService;
            this.salesReturnService = salesReturnService;
            this.salesService = salesService;
        }

        // =========================
        // 🔒 LOCKED JOBS
        // =========================
        private async Task ExecuteWithSkipIfRunningAsync(
                string jobKey,
                int sapId,
                Func<Task> action)
        {
            using var connection = JobStorage.Current.GetConnection();

            try
            {
                using var distributedLock = connection.AcquireDistributedLock(
                    $"{jobKey}:{sapId}",
                    TimeSpan.FromSeconds(1));

              //  _logger.LogInformation("Starting job {JobKey} for SapId={SapId}", jobKey, sapId);

                await action();

             //   _logger.LogInformation("Completed job {JobKey} for SapId={SapId}", jobKey, sapId);
            }
            catch (DistributedLockTimeoutException)
            {
                //_logger.LogInformation(
                //    "Skipping job {JobKey} for SapId={SapId} because another instance is already running.",
                //    jobKey,
                //    sapId);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Job {JobKey} failed for SapId={SapId}",
                //    jobKey,
                //    sapId);

                throw;
            }
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncWarehousesAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-warehouses",
                sapId,
                () => _warehouseService.SyncWarehouseAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncItemsAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-items",
                sapId,
                () => _itemService.SyncItemsAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncBarcodesAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-barcodes",
                sapId,
                () => _barCodeService.SyncBarCodeAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncUomGroupsAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-uom-groups",
                sapId,
                () => _barCodeService.SyncItemUomGroupAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDeleteBarcodesAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-delete-barcodes",
                sapId,
                () => _barCodeService.SyncDeleteBarCodeAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDynamicBarcodesAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-dynamic-barcodes",
                sapId,
                () => _dynamicBarCodeService.SyncDynamicBarcodeAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDeleteDynamicBarcodesAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-delete-dynamic-barcodes",
                sapId,
                () => _dynamicBarCodeService.SyncDeleteDynamicBarCodeAsync(sapId));
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task SyncBusinessPartnersAsync(int sapId)
        {
            await ExecuteWithSkipIfRunningAsync(
                "sync-business-partners",
                sapId,
                () => businessPartnersService.SyncBusinessPartnersAsync(sapId));
        }
        // sales
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncSalesAsync(int sapId)
        //{
        //    await salesService.SyncSalesOrdersAsync(sapId);
        //}
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncSalesAsync(int sapId)
        {
            using var connection = JobStorage.Current.GetConnection();

            try
            {
                // لو في واحدة شغالة بالفعل لنفس sapId، ما تستناش
                using var distributedLock = connection.AcquireDistributedLock(
                    $"sync-sales:{sapId}",
                    TimeSpan.FromSeconds(1));

                await salesService.SyncSalesOrdersAsync(sapId);
            }
            catch (DistributedLockTimeoutException)
            {
                // نسخة أخرى ما زالت شغالة -> تجاهل هذه الدورة
                // لا ترمي exception علشان ما تتحسبش failure
            }
        }

        // purchase
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncPurchaseAsync(int sapId)
        //{
        //    await purchaseService.SyncPurchaseAsync(sapId);
        //}
        //// purchase
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncReceiptAsync(int sapId)
        //{
        //    await receiptService.SyncReceiptAsync(sapId);
        //}

        //// purchase
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncGoodsReturnAsync(int sapId)
        //{
        //    await goodsReturnService.SyncGoodsReturnAsync(sapId);
        //}



        //// purchase
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncDeliveryNoteAsync(int sapId)
        //{
        //    await deliveryNoteService.SyncDeliveryNotesAsync(sapId);
        //}
        //// purchase
        //[DisableConcurrentExecution(1800)]
        //[AutomaticRetry(Attempts = 0)]
        //public async Task SyncSalesReturnAsync(int sapId)
        //{
        //    await salesReturnService.SyncSalesReturnsAsync(sapId);
        //}
    }

}
