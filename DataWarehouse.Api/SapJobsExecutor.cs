
using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Repositories.Actors;
using DataWarehouse.SAP.Repositories.BarCode;
using Hangfire;


namespace DataWarehouse.Api
{
   

    public class SapJobsExecutor
    {
        private readonly ISapWarehouseService _warehouseService;
        private readonly ISapItemService _itemService;
        private readonly ISapBarCodeService _barCodeService;
        private readonly ISapDynamicBarCodeService _dynamicBarCodeService;

        public SapJobsExecutor(
            ISapWarehouseService warehouseService,
            ISapItemService itemService,
            ISapBarCodeService barCodeService,
            ISapDynamicBarCodeService dynamicBarCodeService)
        {
            _warehouseService = warehouseService;
            _itemService = itemService;
            _barCodeService = barCodeService;
            _dynamicBarCodeService = dynamicBarCodeService;
        }

        // =========================
        // 🔒 LOCKED JOBS
        // =========================

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncWarehousesAsync(int sapId)
        {
            await _warehouseService.SyncWarehouseAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncItemsAsync(int sapId)
        {
            await _itemService.SyncItemsAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncBarcodesAsync(int sapId)
        {
            await _barCodeService.SyncBarCodeAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncUomGroupsAsync(int sapId)
        {
            await _barCodeService.SyncItemUomGroupAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDeleteBarcodesAsync(int sapId)
        {
            await _barCodeService.SyncDeleteBarCodeAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDynamicBarcodesAsync(int sapId)
        {
            await _dynamicBarCodeService.SyncDynamicBarcodeAsync(sapId);
        }

        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDeleteDynamicBarcodesAsync(int sapId)
        {
            await _dynamicBarCodeService.SyncDeleteDynamicBarCodeAsync(sapId);
        }
    }

}
