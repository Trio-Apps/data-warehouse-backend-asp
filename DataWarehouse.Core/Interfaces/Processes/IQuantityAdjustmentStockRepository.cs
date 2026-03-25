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

public interface IQuantityAdjustmentStockRepository : IBaseRepository<QuantityAdjustmentStock>
{
    Task<IEnumerable<QuantityAdjustmentStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId, string userId, DateTime? postingDate, DateTime? dueDate, string? status,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<QuantityAdjustmentStockDTO>> AddQuantityAdjustmentStockByWarehouseIdAsync(
        string userId, AddQuantityAdjustmentStockDTO dto, CancellationToken cancellationToken = default);
    Task<GeneralResponse<QuantityAdjustmentStockDTO>> UpdateQuantityAdjustmentStockAsync(
        string userId, int quantityAdjustmentStockId, UpdateQuantityAdjustmentStockDTO dto, CancellationToken cancellationToken = default);
    Task<GeneralResponse<QuantityAdjustmentStockDTO>> DuplicateQuantityAdjustmentStockAsync(string userId, int quantityAdjustmentStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int quantityAdjustmentStockId);
    Task<GeneralResponse<QuantityAdjustmentStockDTO>> DeleteQuantityAdjustmentStockAsync(
        int quantityAdjustmentStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<List<NameStatus>>> GetQuantityAdjustmentStockStatus();
    Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockDTO>>> GetByStatusAsync(string status);
    Task<QuantityAdjustmentStock?> GetWithItemsAsync(int quantityAdjustmentStockId);
    Task<GeneralResponse<QuantityAdjustmentStockDTO>> GetWithWarehouseAsync(
        int quantityAdjustmentStockId, string userId, CancellationToken cancellationToken = default);
}
