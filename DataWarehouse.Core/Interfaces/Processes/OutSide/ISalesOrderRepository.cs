using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesOrderRepository : IBaseRepository<SalesOrder>
{
    Task<IEnumerable<SalesOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetItemForSalesByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesOrderDTO>> AddSalesOrderByWarehouseIdAsync(string userId, AddSalesOrderDTO dto, CancellationToken cancellationToken = default);

    //Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationAsync
    // (int? warehouseId, string userId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
      (int warehouseId, string userId, int? customerId, DateTime? postingDate, DateTime? DueDate, string? liveStatus, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesOrderDTO>> UpdateSalesOrderAsync(string userId, int salesOrderId, UpdateSalesOrderDTO dto, CancellationToken cancellationToken = default);
    Task<GeneralResponse<SalesOrderDTO>> DeleteSalesOrderAsync(int salesOrderId, CancellationToken cancellationToken = default);

    Task<GeneralResponse<List<NameStatus>>> GetSalesOrderStatus();
    Task<GeneralResponse<IEnumerable<WarehouseItemDto>>>
  GetItemsByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<SalesOrder>> GetByCustomerIdAsync(int customerId);
    Task<GeneralResponse<IEnumerable<SalesOrderDTO>>> GetByStatusAsync(string status);
   // Task<IEnumerable<SalesOrder>> GetByUserIdAsync(string userId);
    Task<SalesOrder?> GetWithItemsAsync(int salesOrderId);
    Task<GeneralResponse<SalesOrderDTO>> GetWithCustomerAsync(int salesOrderId, string userId, CancellationToken cancellationToken = default);
   // Task<SalesOrder?> GetWithWarehouseAsync(int salesOrderId);
   // Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync();
   // Task<IEnumerable<SalesOrder>> GetDraftOrdersAsync();
   // Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
}
