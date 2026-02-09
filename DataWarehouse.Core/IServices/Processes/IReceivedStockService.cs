using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface IReceivedStockService : IBaseService<ReceivedStock>
{
    Task<IEnumerable<ReceivedStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<ReceivedStock>> GetBySourceWarehouseIdAsync(int sourceWarehouseId);
    Task<IEnumerable<ReceivedStock>> GetByStatusAsync(string status);
    Task<IEnumerable<ReceivedStock>> GetByUserIdAsync(string userId);
    Task<ReceivedStock?> GetWithItemsAsync(int receivedStockId);
    Task<ReceivedStock?> GetWithWarehouseAsync(int receivedStockId);
    Task<IEnumerable<ReceivedStock>> GetPendingReceiptsAsync();
    Task<IEnumerable<ReceivedStock>> GetInTransitReceiptsAsync();
}
