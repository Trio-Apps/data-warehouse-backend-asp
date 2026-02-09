using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface ICountStockItemService : IBaseService<CountStockItem>
{
    Task<IEnumerable<CountStockItem>> GetByCountStockIdAsync(int countStockId);
    Task<IEnumerable<CountStockItem>> GetByItemIdAsync(int itemId);
    Task<CountStockItem?> GetWithCountStockAsync(int countStockItemId);
    Task<CountStockItem?> GetWithItemAsync(int countStockItemId);
    Task<CountStockItem?> GetWithCommentAsync(int countStockItemId);
}
