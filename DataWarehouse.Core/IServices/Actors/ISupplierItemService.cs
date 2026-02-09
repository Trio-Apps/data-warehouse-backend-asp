using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface ISupplierItemService : IBaseService<SupplierItem>
{
    Task<IEnumerable<SupplierItem>> GetBySupplierIdAsync(int supplierId);
    Task<IEnumerable<SupplierItem>> GetByItemIdAsync(int itemId);
    Task<SupplierItem?> GetBySupplierAndItemAsync(int supplierId, int itemId);
    Task<IEnumerable<SupplierItem>> GetPreferredSuppliersByItemIdAsync(int itemId);
    Task<SupplierItem?> GetWithSupplierAsync(int supplierItemId);
    Task<SupplierItem?> GetWithItemAsync(int supplierItemId);
    Task<bool> ExistsBySupplierAndItemAsync(int supplierId, int itemId);
}
