using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface ISupplierService : IBaseService<Supplier>
{
    Task<Supplier?> GetBySupplierCodeAsync(string supplierCode);
    Task<Supplier?> GetByNameAsync(string supplierName);
    Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
    Task<Supplier?> GetWithSupplierItemsAsync(int supplierId);
    Task<Supplier?> GetWithPurchaseOrdersAsync(int supplierId);
    Task<bool> ExistsBySupplierCodeAsync(string supplierCode);
    Task<IEnumerable<Supplier>> SearchByNameAsync(string searchTerm);
}
