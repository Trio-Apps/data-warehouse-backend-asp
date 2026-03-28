using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    public ItemRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    //GetItemsByWarehouseIdWithItemCodeAndName
    public async Task<GeneralResponse<PagedResult<ItemResponseDTO>>>
      GetItemsWithItemCodeAndName(
          string? itemCode,
          string? itemName,
          int pageNumber,
          int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Items
            .AsNoTracking();

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(itemCode))
        {

            query = query.Where(iw =>
                iw.ItemCode.Contains(itemCode));
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            query = query.Where(iw =>
        iw.ItemName.Contains(itemName));
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(iw => iw.ItemName) // مهم جدًا مع Pagination
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ItemResponseDTO
            {
                ItemId = iw.ItemId,
                ItemCode = iw.ItemCode,
                ItemName = iw.ItemName,
                PurchasePrice = iw.PurchasePrice,
                SalesPrice = iw.SalesPrice,
                UoM = iw.UoM,
                UpdateDate = iw.UpdateDate
            
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ItemResponseDTO>>.SuccessResponse(
            new PagedResult<ItemResponseDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<Item?> GetByItemCodeAsync(string itemCode)
    {
        return await Query().FirstOrDefaultAsync(i => i.ItemCode == itemCode);
    }

   

    public async Task<Item?> GetWithBinLocationsAsync(int itemId)
    {
        return await QueryIncluding(false, i => i.BinLocations)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);
    }

    public async Task<Item?> GetWithSupplierItemsAsync(int itemId)
    {
        return await QueryIncluding(false, i => i.SupplierItems)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);
    }

    public async Task<bool> ExistsByItemCodeAsync(string itemCode)
    {
        return await ExistsAsync(i => i.ItemCode == itemCode);
    }

    public async Task<IEnumerable<Item>> GetByItemGroupAsync(string itemGroup)
    {
        return await Query().Where(i => i.ItemGroup == itemGroup).ToListAsync();
    }

    public async Task<List<ItemPriceWithUomResponse>> GetItemPricesWithUomsAsync(int itemId)
    {
        var itemPrices = await _context.ItemPrices
            .AsNoTracking()
            .Where(p => p.ItemId == itemId)
            .Include(p => p.UomPrices)
            .OrderBy(p => p.PriceList)
            .ToListAsync();

        if (itemPrices.Count == 0)
        {
            return new List<ItemPriceWithUomResponse>();
        }

        var itemUomGroups = await _context.ItemUomGroups
            .AsNoTracking()
            .Where(g => g.ItemId == itemId)
            .Select(g => new { g.UomEntry, g.UomCode })
            .ToListAsync();

        var uomCodeByEntry = itemUomGroups
            .GroupBy(g => g.UomEntry)
            .ToDictionary(g => g.Key, g => g.First().UomCode);

        var totalRows = itemPrices.Count + itemPrices.Sum(p => p.UomPrices.Count);
        var result = new List<ItemPriceWithUomResponse>(totalRows);

        foreach (var itemPrice in itemPrices)
        {
            if (itemPrice.Price != null)
            {
                result.Add(new ItemPriceWithUomResponse
                {
                    ItemPriceId = itemPrice.ItemPriceId,
                    PriceList = itemPrice.PriceList,
                    Price = itemPrice.Price,
                    Currency = itemPrice.Currency,
                    UoMEntry = null,
                    UomCode = null
                });

                foreach (var uomPrice in itemPrice.UomPrices)
                {
                    uomCodeByEntry.TryGetValue(uomPrice.UoMEntry, out var uomCode);

                    result.Add(new ItemPriceWithUomResponse
                    {
                        ItemPriceId = itemPrice.ItemPriceId,
                        PriceList = uomPrice.PriceList,
                        Price = uomPrice.Price,
                        Currency = uomPrice.Currency,
                        UoMEntry = uomPrice.UoMEntry,
                        UomCode = uomCode
                    });
                }
            }
         
        }

        return result;
    }
}
