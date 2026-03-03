
using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Interfaces.Proccesses;
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
        // Partners
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncBusinessPartnersAsync(int sapId)
        {
            await businessPartnersService.SyncBusinessPartnersAsync(sapId);
        }

        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncPurchaseAsync(int sapId)
        {
            await purchaseService.SyncPurchaseAsync(sapId);
        }
        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncReceiptAsync(int sapId)
        {
            await receiptService.SyncReceiptAsync(sapId);
        }

        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncGoodsReturnAsync(int sapId)
        {
            await goodsReturnService.SyncGoodsReturnAsync(sapId);
        }

        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncSalesAsync(int sapId)
        {
            await salesService.SyncSalesOrdersAsync(sapId);
        }

        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncDeliveryNoteAsync(int sapId)
        {
            await deliveryNoteService.SyncDeliveryNotesAsync(sapId);
        }
        // purchase
        [DisableConcurrentExecution(1800)]
        [AutomaticRetry(Attempts = 0)]
        public async Task SyncSalesReturnAsync(int sapId)
        {
            await salesReturnService.SyncSalesReturnsAsync(sapId);
        }
    }

}
