using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class ItemService : BaseService<Item>, IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository) : base(itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<Item?> GetByItemCodeAsync(string itemCode)
    {
        return await _itemRepository.GetByItemCodeAsync(itemCode);
    }

 

    public async Task<Item?> GetWithBinLocationsAsync(int itemId)
    {
        return await _itemRepository.GetWithBinLocationsAsync(itemId);
    }

    public async Task<Item?> GetWithSupplierItemsAsync(int itemId)
    {
        return await _itemRepository.GetWithSupplierItemsAsync(itemId);
    }

    public async Task<bool> ExistsByItemCodeAsync(string itemCode)
    {
        return await _itemRepository.ExistsByItemCodeAsync(itemCode);
    }

    public async Task<IEnumerable<Item>> GetByItemGroupAsync(string itemGroup)
    {
        return await _itemRepository.GetByItemGroupAsync(itemGroup);
    }

}
