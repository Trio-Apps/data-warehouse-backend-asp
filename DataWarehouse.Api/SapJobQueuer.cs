using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Hangfire;

namespace DataWarehouse.Api
{ 
   

    public class SapJobQueuer : ISapJobQueuer
    {
        private readonly IBackgroundJobClient jobs;
        private readonly ISapPurchaseService purchaseService;
        private readonly ISapReceiptService receiptService;
        private readonly ISapGoodsReturnService goodsReturnService;
        private readonly ISapDeliveryNoteService deliveryNoteService;
        private readonly ISapSalesReturnService salesReturnService;
        private readonly ISapQuantityAdjustmentService quantityAdjustmentService;
        private readonly ISapTransferredRequestService transferredRequestService;
        private readonly ISapTransferredStockService transferredStockService;
        private readonly ILogger<SapJobQueuer> logger;

        public SapJobQueuer(
            IBackgroundJobClient jobs,
            ISapPurchaseService purchaseService,
            ISapReceiptService receiptService,
            ISapGoodsReturnService goodsReturnService,
            ISapDeliveryNoteService deliveryNoteService,
            ISapSalesReturnService salesReturnService,
            ISapQuantityAdjustmentService quantityAdjustmentService,
            ISapTransferredRequestService transferredRequestService,
            ISapTransferredStockService transferredStockService,
            ILogger<SapJobQueuer> logger)
        {
            this.jobs = jobs;
            this.purchaseService = purchaseService;
            this.receiptService = receiptService;
            this.goodsReturnService = goodsReturnService;
            this.deliveryNoteService = deliveryNoteService;
            this.salesReturnService = salesReturnService;
            this.quantityAdjustmentService = quantityAdjustmentService;
            this.transferredRequestService = transferredRequestService;
            this.transferredStockService = transferredStockService;
            this.logger = logger;
        }

        public Task DistributionOrders(ProcessItemIsProgressDto res)
        {
            if (res == null)
                throw new ArgumentNullException(nameof(res));

            if (!string.Equals(res.Status, ProcessStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            switch (res.ProcessType)
            {
                case var x when x == ProcessType.Purchase.ToString():
                    EnqueuePurchase(res.ReferenceId);
                    break;

                case var x when x == ProcessType.Receipt.ToString():
                    EnqueueReceipt(res.ReferenceId);
                    break;

                case var x when x == ProcessType.GoodsReturn.ToString():
                    EnqueueGoodsReturn(res.ReferenceId);
                    break;

                case var x when x == ProcessType.DeliveryNote.ToString():
                    EnqueueDeliveryNote(res.ReferenceId);
                    break;

                case var x when x == ProcessType.SalesReturn.ToString():
                    EnqueueSalesReturn(res.ReferenceId);
                    break;

                case var x when x == ProcessType.QuantityAdjustment.ToString():
                    EnqueueQuantityAdjustment(res.ReferenceId);
                    break;

                case var x when x == ProcessType.TransferredRequest.ToString():
                    EnqueueTransferredRequest(res.ReferenceId);
                    break;

                case var x when x == ProcessType.Transferred.ToString():
                    EnqueueTransferredStock(res.ReferenceId);
                    break;

                case var x when x == ProcessType.Received.ToString():
                    EnqueueReceivedStock(res.ReferenceId);
                    break;

                default:
                    logger.LogWarning(
                        "No Hangfire SAP job mapping found for ProcessType={ProcessType}, ReferenceId={ReferenceId}",
                        res.ProcessType,
                        res.ReferenceId);
                    break;
            }

            return Task.CompletedTask;
        }

        public string EnqueuePurchase(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushPurchaseToSapAsync(orderId));
            logger.LogInformation("Enqueued Purchase SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueReceipt(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushReceiptToSapAsync(orderId));
            logger.LogInformation("Enqueued Receipt SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueGoodsReturn(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushGoodsReturnToSapAsync(orderId));
            logger.LogInformation("Enqueued GoodsReturn SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueDeliveryNote(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushDeliveryNoteToSapAsync(orderId));
            logger.LogInformation("Enqueued DeliveryNote SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueSalesReturn(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushSalesReturnToSapAsync(orderId));
            logger.LogInformation("Enqueued SalesReturn SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueQuantityAdjustment(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushQuantityAdjustmentToSapAsync(orderId));
            logger.LogInformation("Enqueued QuantityAdjustment SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueTransferredRequest(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushTransferredRequestToSapAsync(orderId));
            logger.LogInformation("Enqueued TransferredRequest SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueTransferredStock(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushTransferredStockToSapAsync(orderId));
            logger.LogInformation("Enqueued TransferredStock SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }

        public string EnqueueReceivedStock(int orderId)
        {
            var jobId = jobs.Enqueue<SapJobQueuer>(x => x.PushReceivedStockToSapAsync(orderId));
            logger.LogInformation("Enqueued TransferredStock SAP job. OrderId={OrderId}, JobId={JobId}", orderId, jobId);
            return jobId;
        }


        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushPurchaseToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-purchase-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing Purchase document to SAP. OrderId={OrderId}", orderId);

            await purchaseService.SyncPurchaseAsync(orderId);

            logger.LogInformation("Purchase document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushReceiptToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-receipt-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing Receipt document to SAP. OrderId={OrderId}", orderId);

            await receiptService.SyncReceiptAsync(orderId);

            logger.LogInformation("Receipt document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushGoodsReturnToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-goods-return-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing GoodsReturn document to SAP. OrderId={OrderId}", orderId);

            await goodsReturnService.SyncGoodsReturnAsync(orderId);

            logger.LogInformation("GoodsReturn document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushDeliveryNoteToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-delivery-note-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing DeliveryNote document to SAP. OrderId={OrderId}", orderId);

            await deliveryNoteService.SyncDeliveryNotesAsync(orderId);

            logger.LogInformation("DeliveryNote document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushSalesReturnToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-sales-return-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing SalesReturn document to SAP. OrderId={OrderId}", orderId);

            await salesReturnService.SyncSalesReturnsAsync(orderId);

            logger.LogInformation("SalesReturn document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushQuantityAdjustmentToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-quantity-adjustment-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing QuantityAdjustment document to SAP. OrderId={OrderId}", orderId);

            await quantityAdjustmentService.SyncQuantityAdjustmentsAsync(orderId);

            logger.LogInformation("QuantityAdjustment document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushTransferredRequestToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-transferred-request-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing TransferredRequest document to SAP. OrderId={OrderId}", orderId);

            await transferredRequestService.SyncTransferredRequestsAsync(orderId);

            logger.LogInformation("TransferredRequest document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }

        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushTransferredStockToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-transferred-stock-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing TransferredStock document to SAP. OrderId={OrderId}", orderId);

            await transferredStockService.SyncTransferredStockAsync(orderId);

            logger.LogInformation("TransferredStock document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }


        [Queue("sap")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
        public async Task PushReceivedStockToSapAsync(int orderId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var distributedLock = connection.AcquireDistributedLock($"sap-transferred-stock-{orderId}", TimeSpan.FromMinutes(10));

            logger.LogInformation("Start pushing TransferredStock document to SAP. OrderId={OrderId}", orderId);

            await transferredStockService.SyncTransferredStockAsync(orderId);

            logger.LogInformation("TransferredStock document pushed to SAP successfully. OrderId={OrderId}", orderId);
        }


    }
}
