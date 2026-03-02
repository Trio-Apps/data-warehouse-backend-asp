using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface ISupplierRepository : IBaseRepository<Supplier>
{
    Task<GeneralResponse<PagedResult<Supplier>>> GetSuppliersAsync(
          string? supplierCode,
          string? supplierName,
          int pageNumber = 1,
          int pageSize = 20);
    Task<GeneralResponse<Supplier>> GetSupplierByIdAsync(int id);
    Task<GeneralResponse<Supplier>> GetBySupplierCodeAsync(string supplierCode);
    Task<GeneralResponse<Supplier>> GetByNameAsync(string supplierName);
    Task<GeneralResponse<IEnumerable<Supplier>>> GetActiveSuppliersAsync();
    Task<GeneralResponse<IEnumerable<Supplier>>> SearchByNameAsync(string searchTerm);
    Task<GeneralResponse<Supplier>> GetWithSupplierItemsAsync(int supplierId);
    Task<GeneralResponse<SupplierDTO>> GetWithPurchaseOrdersAsync(int supplierId);
    Task<GeneralResponse<Supplier>> AddSupplierAsync(SupplierDTO dto);
    Task<GeneralResponse<Supplier>> UpdateSupplierAsync(int id, SupplierDTO dto);
    Task<GeneralResponse<bool>> DeleteSupplierAsync(int id);
}
