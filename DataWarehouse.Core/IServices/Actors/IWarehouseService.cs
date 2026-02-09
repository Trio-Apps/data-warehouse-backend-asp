using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface IWarehouseService : IBaseService<Warehouse>
{
    Task<Warehouse?> GetByNameAsync(string warehouseName);
  
    Task<Warehouse?> GetWithUserWarehousesAsync(int warehouseId);
    Task<bool> ExistsByNameAsync(string warehouseName);
}
