using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredRequestBatchRepository : IBaseRepository<TransferredRequestBatch>
{
    Task<GeneralResponse<IEnumerable<TransferredRequestBatchDTO>>> GetByTransferredRequestItemIdAsync(int transferredRequestItemId);
    Task<GeneralResponse<PagedResult<TransferredRequestBatchDTO>>> GetByTransferredRequestItemIdWithPaginationAsync(int transferredRequestItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<TransferredRequestBatchDTO>> AddByTransferredRequestItemIdAsync(int transferredRequestItemId, GeneralBatchDto dto);
    Task<GeneralResponse<TransferredRequestBatchDTO>> UpdateTransferredRequestBatchAsync(int transferredRequestBatchId, UpdateGeneralBatchDto dto);
    Task<GeneralResponse<TransferredRequestBatchDTO>> DeleteTransferredRequestBatchAsync(int transferredRequestBatchId);
}
