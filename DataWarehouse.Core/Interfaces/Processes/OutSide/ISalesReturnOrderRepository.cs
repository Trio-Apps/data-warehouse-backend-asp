using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesReturnOrderRepository : IBaseRepository<SalesReturnOrder>
{
    Task<IEnumerable<SalesReturnOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<SalesReturnOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetWithCustomerAsync(int salesOrderId, string userId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderBySalesOrderIdAsync(string userId, AddSalesReturnOrderDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> UpdateSalesReturnOrderAsync(string userId, int salesReturnOrderId, UpdateSalesReturnOrderDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetBySalesOrderIdAsync(int salesOrderId);
    Task<IEnumerable<SalesReturnOrder>> GetByUserIdAsync(string userId);
    Task<SalesReturnOrder?> GetWithItemsAsync(int salesReturnOrderId);
    Task<SalesReturnOrder?> GetWithSalesOrderAsync(int salesReturnOrderId);
    Task<SalesReturnOrder?> GetWithWarehouseAsync(int salesReturnOrderId);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetWithItemsAndBatchesAsync(int salesReturnOrderId);
}

