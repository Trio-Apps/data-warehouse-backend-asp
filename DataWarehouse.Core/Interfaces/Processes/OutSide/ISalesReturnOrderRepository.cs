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
    Task<GeneralResponse<PagedResult<SalesReturnOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(int warehouseId, string userId, int? customerId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetWithCustomerAsync(int salesOrderId, string userId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetSalesReturnOrderByIdAsync(string userId, int salesReturnOrderId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderAndItemsByDeliveryNoteOrderIdAsync(
           string userId,
           AddSalesReturnOrderDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderWithoutRefAsync(string userId, AddSalesReturnOrderWithoutRefDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderAsync(string userId, AddSalesReturnOrderDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> UpdateSalesReturnOrderAsync(string userId, int salesReturnOrderId, UpdateSalesReturnOrderDTO dto);
    Task<GeneralResponse<SalesReturnOrderDTO>> DeleteSalesReturnOrderAsync(int salesReturnOrderId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetByDeliveryNoteOrderIdAsync(int salesOrderId);
    Task<IEnumerable<SalesReturnOrder>> GetByUserIdAsync(string userId);
    Task<SalesReturnOrder?> GetWithItemsAsync(int salesReturnOrderId);
    Task<SalesReturnOrder?> GetWithDeliveryNoteOrderAsync(int salesReturnOrderId);
    Task<SalesReturnOrder?> GetWithWarehouseAsync(int salesReturnOrderId);
    Task<GeneralResponse<SalesReturnOrderDTO>> GetWithItemsAndBatchesAsync(int salesReturnOrderId);
}

