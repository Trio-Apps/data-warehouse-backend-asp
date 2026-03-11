using DataWarehouse.Core.DTOs.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Queue
{
    public interface ISapJobQueuer
    {
        Task DistributionOrders(ProcessItemIsProgressDto res);

        Task PushPurchaseToSapAsync(int orderId);
        Task PushReceiptToSapAsync(int orderId);
        Task PushGoodsReturnToSapAsync(int orderId);
        Task PushDeliveryNoteToSapAsync(int orderId);
        Task PushSalesReturnToSapAsync(int orderId);

        Task PushQuantityAdjustmentToSapAsync(int orderId);
        Task PushTransferredRequestToSapAsync(int orderId);
        Task PushTransferredStockToSapAsync(int orderId);
        Task PushReceivedStockToSapAsync(int orderId);

    }
}
