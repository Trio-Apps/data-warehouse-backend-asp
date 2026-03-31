using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IGoodsReturnOrderItemRepository : IBaseRepository<GoodsReturnOrderItem>
{
    Task<GeneralResponse<IEnumerable<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdAsync(int goodsReturnOrderId);
    Task<GeneralResponse<PagedResult<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdWithPaginationAsync(int goodsReturnOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<GoodsReturnOrderItemDTO>> AddGoodsReturnItemByGoodsReturnOrderIdWithoutRefAsync(int goodsReturnOrderId,
       bool isBarcode
        , DynamicBarcodesDto? barcodeDto,
         AddGeneralItemDto? dto);
    Task<GeneralResponse<GoodsReturnOrderItemDTO>> UpdateGoodsReturnItemWithoutRefAsync(int goodsReturnOrderItemId,
        UpdateGeneralItemDto dto);
    Task<IEnumerable<GoodsReturnOrderItem>> GetByGoodsReturnOrderIdEntitiesAsync(int goodsReturnOrderId);
    Task<IEnumerable<GoodsReturnOrderItem>> GetByItemIdAsync(int itemId);
    Task<GoodsReturnOrderItem?> GetWithGoodsReturnOrderAsync(int goodsReturnOrderItemId);
    Task<GoodsReturnOrderItem?> GetWithItemAsync(int goodsReturnOrderItemId);
}

