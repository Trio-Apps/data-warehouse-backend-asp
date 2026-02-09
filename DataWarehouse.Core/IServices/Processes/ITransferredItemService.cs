using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface ITransferredItemService : IBaseService<TransferredItem>
{
    Task<IEnumerable<TransferredItem>> GetByTransferredStockIdAsync(int transferredStockId);
    Task<IEnumerable<TransferredItem>> GetByItemIdAsync(int itemId);
    Task<TransferredItem?> GetWithTransferredStockAsync(int transferredItemId);
    Task<TransferredItem?> GetWithItemAsync(int transferredItemId);
}
