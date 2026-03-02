using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface IWarehouseRepository : IBaseRepository<Warehouse>
{
    Task<GeneralResponse<WarehouseDTO>> AddWarehouseAsync(AddWarehouseDTO dto);
    Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehouses(string userId,IList<string> roles);
    Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehousesForEmployeeAsync(string userId);
    Task<int?> GetSap();
    Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetSapByIdAsync(int sapId);
    Task<Warehouse?> GetByNameAsync(string warehouseName);
    Task<Warehouse?> GetWithUserWarehousesAsync(int warehouseId);
    Task<bool> ExistsByNameAsync(string warehouseName);
    Task<GeneralResponse<IEnumerable<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
       int warehouseId);
    Task<GeneralResponse<PagedResult<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
       int warehouseId,
        int pageNumber,
   int pageSize);

    Task<GeneralResponse<PagedResult<ItemResponseDTO>>>
      GetItemsByWarehouseIdWithItemCodeAndName(
          int warehouseId,
          string? itemCode,
          string? itemName,
          int pageNumber,
          int pageSize);
}
