using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IGoodsReturnOrderBatchRepository : IBaseRepository<GoodsReturnOrderBatch>
{
    Task<GeneralResponse<IEnumerable<GoodsReturnOrderBatchDTO>>> GetByGoodsReturnOrderItemIdAsync(int goodsReturnOrderItemId);
    Task<GeneralResponse<PagedResult<GoodsReturnOrderBatchDTO>>> GetByGoodsReturnOrderItemIdWithPaginationAsync(int goodsReturnOrderItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<GoodsReturnOrderBatchDTO>> AddByGoodsReturnOrderItemIdAsync(int goodsReturnOrderItemId, AddGoodsReturnOrderBatchDTO dto);
    Task<GeneralResponse<GoodsReturnOrderBatchDTO>> UpdateGoodsReturnOrderBatchAsync(int goodsReturnOrderBatchId, UpdateGoodsReturnOrderBatchDTO dto);
}

