using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesReturnOrderItemRepository : IBaseRepository<SalesReturnOrderItem>
{
    Task<GeneralResponse<IEnumerable<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdAsync(int salesReturnOrderId);
    Task<GeneralResponse<PagedResult<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdWithPaginationAsync(int salesReturnOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnOrderItemBySalesOrderItemIdAsync(string userId,int salesReturnOrderId, AddSalesReturnOrderItemDTO dto);
    Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnOrderItemAsync(int salesReturnOrderItemId, UpdateSalesReturnOrderItemDTO dto);
    Task<IEnumerable<SalesReturnOrderItem>> GetBySalesReturnOrderIdEntitiesAsync(int salesReturnOrderId);
    Task<IEnumerable<SalesReturnOrderItem>> GetByItemIdAsync(int itemId);
    Task<SalesReturnOrderItem?> GetWithSalesReturnOrderAsync(int salesReturnOrderItemId);
    Task<SalesReturnOrderItem?> GetWithSalesOrderItemAsync(int salesReturnOrderItemId);
    Task<SalesReturnOrderItem?> GetWithItemAsync(int salesReturnOrderItemId);
    Task<SalesReturnOrderItem?> GetWithBatchesAsync(int salesReturnOrderItemId);
}

