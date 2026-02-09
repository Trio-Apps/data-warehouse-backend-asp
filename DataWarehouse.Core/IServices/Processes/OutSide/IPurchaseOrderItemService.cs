using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes.OutSide;

public interface IPurchaseOrderItemService : IBaseService<PurchaseOrderItem>
{
    Task<IEnumerable<PurchaseOrderItem>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
    Task<IEnumerable<PurchaseOrderItem>> GetByItemIdAsync(int itemId);
    Task<PurchaseOrderItem?> GetWithPurchaseOrderAsync(int purchaseOrderItemId);
    Task<PurchaseOrderItem?> GetWithItemAsync(int purchaseOrderItemId);
}
