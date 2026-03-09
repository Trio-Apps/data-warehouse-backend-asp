using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredRequestOrderRepository : IBaseRepository<TransferredRequest>
{
    Task<GeneralResponse<PagedResult<WarehouseItemDto>>> GetByWarehouseIdAsync(
     int warehouseId,
     string? search = null,
     int pageNumber = 1,
     int pageSize = 20);
    Task<IEnumerable<TransferredRequest>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<TransferredRequestDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<PagedResult<TransferredRequestDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? destinationWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? liveStatus,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<GeneralResponse<TransferredRequestDTO>> AddTransferredRequestByWarehouseIdAsync(
        string userId, AddTransferredRequestDTO dto, CancellationToken cancellationToken = default);
    Task<GeneralResponse<TransferredRequestDTO>> UpdateTransferredRequestAsync(
        string userId, int transferredRequestId, UpdateTransferredRequestDTO dto, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int transferredRequestId);
    Task<GeneralResponse<TransferredRequestDTO>> DeleteTransferredRequestAsync(
        int transferredRequestId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<List<NameStatus>>> GetTransferredRequestStatus();
    Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetItemsByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<TransferredRequestDTO>> GetWithWarehousesAndApprovalAsync(
        int transferredRequestId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransferredRequest>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId);
    Task<GeneralResponse<IEnumerable<TransferredRequestDTO>>> GetByStatusAsync(string status);
    Task<IEnumerable<TransferredRequest>> GetByUserIdAsync(string userId);
    Task<TransferredRequest?> GetWithItemsAsync(int transferredRequestId);
    Task<TransferredRequest?> GetWithWarehousesAsync(int transferredRequestId);
    Task<IEnumerable<TransferredRequest>> GetPendingRequestsAsync();
    Task<IEnumerable<TransferredRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
