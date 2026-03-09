using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IQuantityAdjustmentStockItemRepository : IBaseRepository<QuantityAdjustmentStockItem>
{
    Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(int quantityAdjustmentStockId);
    Task<GeneralResponse<PagedResult<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdWithPaginationAsync(
        int quantityAdjustmentStockId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> AddQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(
        int quantityAdjustmentStockId, bool isBarcode, DynamicBarcodesDto? dynamicDto, AddGeneralItemDto? dto);
    Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> UpdateQuantityAdjustmentStockItemAsync(
        int quantityAdjustmentStockItemId, UpdateGeneralItemDto dto);
    Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> DeleteQuantityAdjustmentStockItemAsync(int quantityAdjustmentStockItemId);
}
