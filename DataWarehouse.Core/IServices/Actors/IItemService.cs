using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Actors;

public interface IItemService : IBaseService<Item>
{
    Task<Item?> GetByItemCodeAsync(string itemCode);

    Task<Item?> GetWithBinLocationsAsync(int itemId);
    Task<Item?> GetWithSupplierItemsAsync(int itemId);
    Task<bool> ExistsByItemCodeAsync(string itemCode);
    Task<IEnumerable<Item>> GetByItemGroupAsync(string itemGroup);
}
