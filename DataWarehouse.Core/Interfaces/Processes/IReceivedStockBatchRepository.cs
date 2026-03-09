using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IReceivedStockBatchRepository : IBaseRepository<ReceivedStockBatch>
{
    Task<GeneralResponse<IEnumerable<ReceivedStockBatchDTO>>> GetByReceivedItemIdAsync(int receivedItemId);
    Task<GeneralResponse<PagedResult<ReceivedStockBatchDTO>>> GetByReceivedItemIdWithPaginationAsync(int receivedItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceivedStockBatchDTO>> AddByReceivedItemIdAsync(int receivedItemId, GeneralBatchDto dto);
    Task<GeneralResponse<ReceivedStockBatchDTO>> UpdateReceivedStockBatchAsync(int receivedStockBatchId, UpdateGeneralBatchDto dto);
    Task<GeneralResponse<ReceivedStockBatchDTO>> DeleteReceivedStockBatchAsync(int receivedStockBatchId);
}

