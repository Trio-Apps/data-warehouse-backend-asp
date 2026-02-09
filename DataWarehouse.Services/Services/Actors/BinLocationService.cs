using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class BinLocationService : BaseService<BinLocation>, IBinLocationService
{
    private readonly IBinLocationRepository _binLocationRepository;

    public BinLocationService(IBinLocationRepository binLocationRepository) : base(binLocationRepository)
    {
        _binLocationRepository = binLocationRepository;
    }

    public async Task<IEnumerable<BinLocation>> GetByItemIdAsync(int itemId)
    {
        return await _binLocationRepository.GetByItemIdAsync(itemId);
    }

    public async Task<BinLocation?> GetByLocationAsync(string location)
    {
        return await _binLocationRepository.GetByLocationAsync(location);
    }

    public async Task<BinLocation?> GetWithItemAsync(int binLocationId)
    {
        return await _binLocationRepository.GetWithItemAsync(binLocationId);
    }

    public async Task<bool> ExistsByLocationAsync(string location)
    {
        return await _binLocationRepository.ExistsByLocationAsync(location);
    }
}
