using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class SupplierItemRepository : BaseRepository<SupplierItem>, ISupplierItemRepository
{
    public SupplierItemRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupplierItem>> GetBySupplierIdAsync(int supplierId)
    {
        return await Query().Where(si => si.SupplierId == supplierId).ToListAsync();
    }

    public async Task<IEnumerable<SupplierItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(si => si.ItemId == itemId).ToListAsync();
    }

    public async Task<SupplierItem?> GetBySupplierAndItemAsync(int supplierId, int itemId)
    {
        return await Query().FirstOrDefaultAsync(si => si.SupplierId == supplierId && si.ItemId == itemId);
    }

    public async Task<IEnumerable<SupplierItem>> GetPreferredSuppliersByItemIdAsync(int itemId)
    {
        return await Query().Where(si => si.ItemId == itemId && si.IsPreferred).ToListAsync();
    }

    public async Task<SupplierItem?> GetWithSupplierAsync(int supplierItemId)
    {
        return await QueryIncluding(false, si => si.Supplier)
            .FirstOrDefaultAsync(si => si.SupplierItemId == supplierItemId);
    }

    public async Task<SupplierItem?> GetWithItemAsync(int supplierItemId)
    {
        return await QueryIncluding(false, si => si.Item)
            .FirstOrDefaultAsync(si => si.SupplierItemId == supplierItemId);
    }

    public async Task<bool> ExistsBySupplierAndItemAsync(int supplierId, int itemId)
    {
        return await ExistsAsync(si => si.SupplierId == supplierId && si.ItemId == itemId);
    }
}
