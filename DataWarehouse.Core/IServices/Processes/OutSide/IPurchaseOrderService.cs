using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes.OutSide;

public interface IPurchaseOrderService : IBaseService<PurchaseOrder>
{
    Task<IEnumerable<PurchaseOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId);
    Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseOrder>> GetByUserIdAsync(string userId);
    Task<PurchaseOrder?> GetWithItemsAsync(int purchaseOrderId);
    Task<PurchaseOrder?> GetWithSupplierAsync(int purchaseOrderId);
    Task<PurchaseOrder?> GetWithWarehouseAsync(int purchaseOrderId);
    Task<PurchaseOrder?> GetWithReceiptAsync(int purchaseOrderId);
    Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync();
    Task<IEnumerable<PurchaseOrder>> GetDraftOrdersAsync();
}
