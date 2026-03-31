using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredStockRepository : IBaseRepository<TransferredStock>
{
    Task<IEnumerable<TransferredStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? destinationWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByTransferredRequestIdAndStatusAndDateWithPaginationForDashboardAsync(
        int transferredRequestId,
        string userId,
        int? destinationWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task<GeneralResponse<TransferredStockDTO>> GetTransferredStockByIdAsync(
        string userId, int transferredStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<TransferredStockDTO>> GetByTransferredRequestIdAsync(int transferredRequestId);
    Task<GeneralResponse<TransferredStockDTO>> GetWithItemsAndBatchesAsync(int transferredStockId);
    Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByWarehouseIdWithoutRefAsync
            (string userId, AddTransferredStockWithoutRefDTO dto);
    Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockAndItemsByTransferredRequestIdAsync(
          string userId,
          AddTransferredStockDTO dto);
    Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByTransferredRequestIdAsync(
          string userId, AddTransferredStockDTO dto);
    Task<GeneralResponse<TransferredStockDTO>> UpdateTransferredStockAsync(string userId, int transferredStockId, UpdateTransferredStockDTO dto);
    Task<GeneralResponse<TransferredStockDTO>> DuplicateTransferredStockAsync(string userId, int transferredStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int transferredStockId);
    Task<GeneralResponse<TransferredStockDTO>> DeleteTransferredStockAsync(int transferredStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<List<NameStatus>>> GetTransferredStockStatus();
    Task<IEnumerable<TransferredStock>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId);
    Task<GeneralResponse<IEnumerable<TransferredStockDTO>>> GetByStatusAsync(string status);
    Task<IEnumerable<TransferredStock>> GetByUserIdAsync(string userId);
    Task<TransferredStock?> GetWithItemsAsync(int transferredStockId);
    Task<TransferredStock?> GetWithWarehousesAsync(int transferredStockId);
    Task<IEnumerable<TransferredStock>> GetPendingTransfersAsync();
    Task<IEnumerable<TransferredStock>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);


    //Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAsDestinationWarehouseAndStatusAndDateWithPaginationForDashboardAsync(
    //     int warehouseId,
    //     string userId,
    //     int? transferredWarehouseId,
    //     DateTime? postingDate,
    //     DateTime? dueDate,
    //     string? status,
    //     int pageNumber,
    //     int pageSize,
    //     CancellationToken cancellationToken = default);
}
