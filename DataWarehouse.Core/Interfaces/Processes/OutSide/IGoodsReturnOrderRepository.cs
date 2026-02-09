using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IGoodsReturnOrderRepository : IBaseRepository<GoodsReturnOrder>
{
    Task<IEnumerable<GoodsReturnOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<GoodsReturnOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderByReceiptPurchaseOrderIdAsync(string userId, AddGoodsReturnOrderDTO dto);
    Task<GeneralResponse<GoodsReturnOrderDTO>> UpdateGoodsReturnOrderAsync(string userId, int goodsReturnOrderId, UpdateGoodsReturnOrderDTO dto);
    Task<GeneralResponse<GoodsReturnOrderDTO>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId);
    Task<IEnumerable<GoodsReturnOrder>> GetByUserIdAsync(string userId);
    Task<GoodsReturnOrder?> GetWithItemsAsync(int goodsReturnOrderId);
    Task<GoodsReturnOrder?> GetWithReceiptPurchaseOrderAsync(int goodsReturnOrderId);
    Task<GoodsReturnOrder?> GetWithWarehouseAsync(int goodsReturnOrderId);
    Task<GeneralResponse<GoodsReturnOrderDTO>> GetWithItemsAndBatchesAsync(int goodsReturnOrderId);
}

