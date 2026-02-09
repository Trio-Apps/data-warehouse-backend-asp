using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface IUserWarehousesService : IBaseService<UserWarehouses>
{
    Task<IEnumerable<UserWarehouses>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserWarehouses>> GetByWarehouseIdAsync(int warehouseId);
    Task<UserWarehouses?> GetByUserAndWarehouseAsync(string userId, int warehouseId);
    Task<UserWarehouses?> GetWithUserAsync(int userWarehousesId);
    Task<UserWarehouses?> GetWithWarehouseAsync(int userWarehousesId);
    Task<bool> ExistsByUserAndWarehouseAsync(string userId, int warehouseId);
}
