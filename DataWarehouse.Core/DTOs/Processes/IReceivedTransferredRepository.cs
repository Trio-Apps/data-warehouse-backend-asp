using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Processes;

public interface IReceivedTransferredRepository
{
    Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAsDestinationWarehouseAndStatusAndDateWithPaginationForDashboardAsync(
   int warehouseId,
   string userId,
   int? sourceWarehouseId,
   DateTime? postingDate,
   DateTime? dueDate,
   string? status,
   int pageNumber,
   int pageSize,
   CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> UpdateReceivedQuantitiesAsync(string userId, ReceiveTransferredStockDTO dto);
    Task<GeneralResponse<ProcessItemIsProgressDto>> CompleteReceivingStatusIfDraftAsync(string userId, int transferredStockId);
}
