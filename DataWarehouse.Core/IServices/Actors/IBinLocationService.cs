using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface IBinLocationService : IBaseService<BinLocation>
{
    Task<IEnumerable<BinLocation>> GetByItemIdAsync(int itemId);
    Task<BinLocation?> GetByLocationAsync(string location);
    Task<BinLocation?> GetWithItemAsync(int binLocationId);
    Task<bool> ExistsByLocationAsync(string location);
}
