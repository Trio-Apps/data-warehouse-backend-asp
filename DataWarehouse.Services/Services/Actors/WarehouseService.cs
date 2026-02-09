using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class WarehouseService : BaseService<Warehouse>, IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository) : base(warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<Warehouse?> GetByNameAsync(string warehouseName)
    {
        return await _warehouseRepository.GetByNameAsync(warehouseName);
    }

  

    public async Task<Warehouse?> GetWithUserWarehousesAsync(int warehouseId)
    {
        return await _warehouseRepository.GetWithUserWarehousesAsync(warehouseId);
    }

    public async Task<bool> ExistsByNameAsync(string warehouseName)
    {
        return await _warehouseRepository.ExistsByNameAsync(warehouseName);
    }
}
