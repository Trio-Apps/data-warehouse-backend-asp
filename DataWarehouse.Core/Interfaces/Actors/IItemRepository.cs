using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface IItemRepository : IBaseRepository<Item>
{
    Task<GeneralResponse<PagedResult<ItemResponseDTO>>>
      GetItemsWithItemCodeAndName(

          string? itemCode,
          string? itemName,
          int pageNumber,
          int pageSize);
    Task<Item?> GetByItemCodeAsync(string itemCode);
    Task<Item?> GetWithBinLocationsAsync(int itemId);
    Task<Item?> GetWithSupplierItemsAsync(int itemId);
    Task<bool> ExistsByItemCodeAsync(string itemCode);
    Task<IEnumerable<Item>> GetByItemGroupAsync(string itemGroup);
    Task<List<ItemPriceWithUomResponse>> GetItemPricesWithUomsAsync(int itemId);
}
