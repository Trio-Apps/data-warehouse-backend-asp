using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface IReceivedItemService : IBaseService<ReceivedItem>
{
    Task<IEnumerable<ReceivedItem>> GetByReceivedStockIdAsync(int receivedStockId);
    Task<IEnumerable<ReceivedItem>> GetByItemIdAsync(int itemId);
    Task<ReceivedItem?> GetWithReceivedStockAsync(int receivedItemId);
    Task<ReceivedItem?> GetWithItemAsync(int receivedItemId);
}
