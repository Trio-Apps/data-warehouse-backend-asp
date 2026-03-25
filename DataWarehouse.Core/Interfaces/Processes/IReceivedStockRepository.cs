using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IReceivedStockRepository : IBaseRepository<ReceivedStock>
{
    Task<IEnumerable<ReceivedStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? sourceWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<GeneralResponse<ReceivedStockDTO>> GetReceivedStockByIdAsync(
        string userId,
        int receivedStockId,
        CancellationToken cancellationToken = default);

    Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockWitoutRefAsync(
      string userId,
      AddReceivedStockWithoutRefDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockByTransferredStockIdAsync(string userId, AddReceivedStockDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockAndItemsByTransferredStockIdAsync(string userId, AddReceivedStockDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> UpdateReceivedStockAsync(string userId, int receivedStockId, UpdateReceivedStockDTO dto);
    Task<GeneralResponse<ReceivedStockDTO>> DuplicateReceivedStockAsync(string userId, int receivedStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ReceivedStockDTO>> DeleteReceivedStockAsync(int receivedStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int receivedStockId);

    Task<GeneralResponse<ReceivedStockDTO>> GetByTransferredStockIdAsync(int transferredStockId);
    Task<IEnumerable<ReceivedStock>> GetByUserIdAsync(string userId);
    Task<ReceivedStock?> GetWithItemsAsync(int receivedStockId);
    Task<ReceivedStock?> GetWithTransferredStockAsync(int receivedStockId);
    Task<ReceivedStock?> GetWithWarehouseAsync(int receivedStockId);
    Task<GeneralResponse<ReceivedStockDTO>> GetWithItemsAndBatchesAsync(int receivedStockId);
}
