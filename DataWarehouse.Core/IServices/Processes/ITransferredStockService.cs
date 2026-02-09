using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface ITransferredStockService : IBaseService<TransferredStock>
{
    Task<IEnumerable<TransferredStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<TransferredStock>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId);
    Task<IEnumerable<TransferredStock>> GetByStatusAsync(string status);
    Task<IEnumerable<TransferredStock>> GetByUserIdAsync(string userId);
    Task<TransferredStock?> GetWithItemsAsync(int transferredStockId);
    Task<TransferredStock?> GetWithWarehousesAsync(int transferredStockId);
    Task<IEnumerable<TransferredStock>> GetPendingTransfersAsync();
    Task<IEnumerable<TransferredStock>> GetInTransitTransfersAsync();
}
