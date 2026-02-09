using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes.OutSide;

public interface IReceiptPurchaseOrderService : IBaseService<ReceiptPurchaseOrder>
{
    Task<ReceiptPurchaseOrder?> GetByPurchaseOrderIdAsync(int purchaseOrderId);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetByUserIdAsync(string userId);
    Task<ReceiptPurchaseOrder?> GetWithItemsAsync(int receiptPurchaseOrderId);
    Task<ReceiptPurchaseOrder?> GetWithPurchaseOrderAsync(int receiptPurchaseOrderId);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetPendingReceiptsAsync();
}
