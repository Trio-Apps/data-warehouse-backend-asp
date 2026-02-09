using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class BinLocationRepository : BaseRepository<BinLocation>, IBinLocationRepository
{
    public BinLocationRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BinLocation>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(b => b.ItemId == itemId).ToListAsync();
    }

    public async Task<BinLocation?> GetByLocationAsync(string location)
    {
        return await Query().FirstOrDefaultAsync(b => b.Location == location);
    }

    public async Task<BinLocation?> GetWithItemAsync(int binLocationId)
    {
        return await QueryIncluding(false, b => b.Item)
            .FirstOrDefaultAsync(b => b.BinLocationId == binLocationId);
    }

    public async Task<bool> ExistsByLocationAsync(string location)
    {
        return await ExistsAsync(b => b.Location == location);
    }
}
