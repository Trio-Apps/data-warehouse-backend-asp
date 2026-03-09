using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IQuantityAdjustmentStockBatchRepository : IBaseRepository<QuantityAdjustmentStockBatch>
{
    Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemIdAsync(int quantityAdjustmentStockItemId);
    Task<GeneralResponse<PagedResult<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemIdWithPaginationAsync(int quantityAdjustmentStockItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> AddByQuantityAdjustmentStockItemIdAsync(int quantityAdjustmentStockItemId, GeneralBatchDto dto);
    Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> UpdateQuantityAdjustmentStockBatchAsync(int quantityAdjustmentStockBatchId, UpdateGeneralBatchDto dto);
    Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> DeleteQuantityAdjustmentStockBatchAsync(int quantityAdjustmentStockBatchId);
}
