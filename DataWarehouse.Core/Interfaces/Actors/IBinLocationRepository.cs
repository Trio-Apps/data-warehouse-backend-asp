using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface IBinLocationRepository : IBaseRepository<BinLocation>
{
    Task<IEnumerable<BinLocation>> GetByItemIdAsync(int itemId);
    Task<BinLocation?> GetByLocationAsync(string location);
    Task<BinLocation?> GetWithItemAsync(int binLocationId);
    Task<bool> ExistsByLocationAsync(string location);
}
