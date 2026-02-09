using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class UserWarehousesService : BaseService<UserWarehouses>, IUserWarehousesService
{
    private readonly IUserWarehousesRepository _userWarehousesRepository;

    public UserWarehousesService(IUserWarehousesRepository userWarehousesRepository) : base(userWarehousesRepository)
    {
        _userWarehousesRepository = userWarehousesRepository;
    }

    public async Task<IEnumerable<UserWarehouses>> GetByUserIdAsync(string userId)
    {
        return await _userWarehousesRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserWarehouses>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _userWarehousesRepository.GetByWarehouseIdAsync(warehouseId);
    }

    public async Task<UserWarehouses?> GetByUserAndWarehouseAsync(string userId, int warehouseId)
    {
        return await _userWarehousesRepository.GetByUserAndWarehouseAsync(userId, warehouseId);
    }

    public async Task<UserWarehouses?> GetWithUserAsync(int userWarehousesId)
    {
        return await _userWarehousesRepository.GetWithUserAsync(userWarehousesId);
    }

    public async Task<UserWarehouses?> GetWithWarehouseAsync(int userWarehousesId)
    {
        return await _userWarehousesRepository.GetWithWarehouseAsync(userWarehousesId);
    }

    public async Task<bool> ExistsByUserAndWarehouseAsync(string userId, int warehouseId)
    {
        return await _userWarehousesRepository.ExistsByUserAndWarehouseAsync(userId, warehouseId);
    }
}
