using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IReceivedItemRepository : IBaseRepository<ReceivedItem>
{
    Task<IEnumerable<ReceivedItemDTO>> GetByReceivedItemByReceivedStockIdAsync(int ReceivedStockId);
    Task<GeneralResponse<PagedResult<ReceivedItemDTO>>> GetByReceivedItemByReceivedStockIdWithPaginationAsync(int ReceivedStockId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceivedItemDTO>> AddReceivedItemByTransferredItemIdAsync(
         string userId,
         int transferredStockid,
         AddReceivedItemDTO dto);
    Task<GeneralResponse<ReceivedItemDTO>> UpdateReceivedItemAsync(int ReceivedItemId, UpdateReceivedItemDTO dto);
    Task<IEnumerable<ReceivedItem>> GetByReceivedStockIdEntitiesAsync(int receivedStockId);
    Task<IEnumerable<ReceivedItem>> GetByItemIdAsync(int itemId);
    Task<ReceivedItem?> GetWithReceivedStockAsync(int receivedItemId);
    Task<ReceivedItem?> GetWithTransferredItemAsync(int receivedItemId);
    Task<ReceivedItem?> GetWithItemAsync(int receivedItemId);
    Task<ReceivedItem?> GetWithBatchesAsync(int receivedItemId);
}
