using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IReceivedStockRepository : IBaseRepository<ReceivedStock>
{
    Task<IEnumerable<ReceivedStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockByTransferredStockIdAsync(string userId, AddReceivedStockDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> UpdateReceivedStockAsync(string userId, int receivedStockId, UpdateReceivedStockDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> GetByTransferredStockIdAsync(int transferredStockId);
    Task<IEnumerable<ReceivedStock>> GetByUserIdAsync(string userId);
    Task<ReceivedStock?> GetWithItemsAsync(int receivedStockId);
    Task<ReceivedStock?> GetWithTransferredStockAsync(int receivedStockId);
    Task<ReceivedStock?> GetWithWarehouseAsync(int receivedStockId);
    Task<GeneralResponse<ReceivedStockDTO>> GetWithItemsAndBatchesAsync(int receivedStockId);
}
