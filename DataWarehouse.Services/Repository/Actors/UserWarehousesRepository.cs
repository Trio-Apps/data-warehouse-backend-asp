using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class UserWarehousesRepository : BaseRepository<UserWarehouses>, IUserWarehousesRepository
{
    public UserWarehousesRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserWarehouses>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(uw => uw.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<UserWarehouses>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(uw => uw.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<UserWarehouses?> GetByUserAndWarehouseAsync(string userId, int warehouseId)
    {
        return await Query().FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WarehouseId == warehouseId);
    }

    public async Task<UserWarehouses?> GetWithUserAsync(int userWarehousesId)
    {
        return await QueryIncluding(false, uw => uw.User)
            .FirstOrDefaultAsync(uw => uw.UserWarehousesId == userWarehousesId);
    }

    public async Task<UserWarehouses?> GetWithWarehouseAsync(int userWarehousesId)
    {
        return await QueryIncluding(false, uw => uw.Warehouse)
            .FirstOrDefaultAsync(uw => uw.UserWarehousesId == userWarehousesId);
    }

    public async Task<bool> ExistsByUserAndWarehouseAsync(string userId, int warehouseId)
    {
        return await ExistsAsync(uw => uw.UserId == userId && uw.WarehouseId == warehouseId);
    }
}
