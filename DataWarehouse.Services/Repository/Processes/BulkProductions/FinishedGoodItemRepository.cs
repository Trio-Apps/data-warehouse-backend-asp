using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class FinishedGoodItemRepository : BaseRepository<WarehouseItem>, IFinishedGoodItemRepository
{
    public FinishedGoodItemRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetByWarehouseIdAsync(int warehouseId)
    {
        return GeneralResponse< IEnumerable < WarehouseItemDto >>.SuccessResponse( await Query()
            .Where(iw => iw.HasActiveBOM && iw.IsActive && iw.WarehouseId == warehouseId)
            .Select(wi => new WarehouseItemDto
            {
                WarehouseItemId = wi.WarehouseItemId,
                ItemId = wi.ItemId,
                WarehouseId = wi.WarehouseId,
                ItemName = wi.Item.ItemName,
                ItemCode = wi.Item.ItemCode,
                WarehouseCode = wi.Warehouse.WarehouseCode,
                InStock = wi.InStock,
                MinStock = wi.MinStock
            })
            .ToListAsync());
    }


    public async Task<IEnumerable<WarehouseItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(fgi => fgi.ItemId == itemId).ToListAsync();
    }

    public async Task<WarehouseItem?> GetByItemAndWarehouseAsync(int itemId, int warehouseId)
    {
        return await Query().FirstOrDefaultAsync(fgi => fgi.ItemId == itemId && fgi.WarehouseId == warehouseId);
    }

    public async Task<GeneralResponse<PagedResult<WarehouseItemDto>>>  GetFinishedGoodBomItemsByWarehouseIdAsync( int warehouseId, int? itemId,int pageNumber,int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.HasActiveBOM && iw.IsActive && iw.WarehouseId == warehouseId);

        // 🔹 Filtering
     

        if (itemId != null)
        {
            query = query.Where(f => f.ItemId == itemId);
        }

        var totalRecords = await query.CountAsync();

        var data = await query
          //  .OrderBy(iw => iw.Item.ItemName) // مهم جدًا مع Pagination
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(wi => new WarehouseItemDto
            {
                WarehouseItemId = wi.WarehouseItemId,
                ItemId = wi.ItemId,
                WarehouseId = wi.WarehouseId,
                ItemName = wi.Item.ItemName,
                ItemCode = wi.Item.ItemCode,
                WarehouseCode = wi.Warehouse.WarehouseCode,
                InStock = wi.InStock,
                MinStock = wi.MinStock

            })
            .ToListAsync();

        return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(
            new PagedResult<WarehouseItemDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }






    public async Task<WarehouseItem?> GetWithItemAsync(int finishedGoodItemId)
    {
        return await QueryIncluding(false, fgi => fgi.Item)
            .FirstOrDefaultAsync(fgi => fgi.WarehouseItemId == finishedGoodItemId);
    }

    public async Task<WarehouseItem?> GetWithWarehouseAsync(int finishedGoodItemId)
    {
        return await QueryIncluding(false, fgi => fgi.Warehouse)
            .FirstOrDefaultAsync(fgi => fgi.WarehouseItemId == finishedGoodItemId);
    }

    public async Task<IEnumerable<WarehouseItem>> GetActiveItemsAsync()
    {
        return await Query().Where(fgi => fgi.IsActive).ToListAsync();
    }
}
