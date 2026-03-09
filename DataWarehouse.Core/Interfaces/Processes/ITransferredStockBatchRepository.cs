using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredStockBatchRepository : IBaseRepository<TransferredStockBatch>
{
    Task<GeneralResponse<IEnumerable<TransferredStockBatchDTO>>> GetByTransferredItemIdAsync(int transferredItemId);
    Task<GeneralResponse<PagedResult<TransferredStockBatchDTO>>> GetByTransferredItemIdWithPaginationAsync(int transferredItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<TransferredStockBatchDTO>> AddByTransferredItemIdAsync(int transferredItemId, GeneralBatchDto dto);
    Task<GeneralResponse<TransferredStockBatchDTO>> UpdateTransferredStockBatchAsync(int transferredStockBatchId, UpdateGeneralBatchDto dto);
    Task<GeneralResponse<TransferredStockBatchDTO>> DeleteTransferredStockBatchAsync(int transferredStockBatchId);
}

