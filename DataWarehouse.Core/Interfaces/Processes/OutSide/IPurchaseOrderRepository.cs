using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IPurchaseOrderRepository : IBaseRepository<PurchaseOrder>
{
    Task<IEnumerable<PurchaseOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<PurchaseOrderDTO>> GetWithSupplierAsync(string userId,int PurchaseOrderId,CancellationToken cancellationToken = default);

    //Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationAsync
    //    (int? warehouseId, string userId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
       (int warehouseId, string userId, int? supplierId, DateTime? postingDate, DateTime? DueDate, string? liveStatus, string? status, int pageNumber, int pageSize,CancellationToken cancellationToken=default);
    Task<GeneralResponse<PurchaseOrderDTO>> AddPurchaseOrderByWarehouseIdAsync(string userId,
           AddPurchaseOrderDTO dto);
    Task<GeneralResponse<PurchaseOrderDTO>> UpdatePurchaseOrderAsync(string userId, int PurchaseId, UpdatePurchaseOrderDTO dto);
    Task<GeneralResponse<PurchaseOrderDTO>> DuplicatePurchaseOrderAsync(string userId, int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int purchaseOrderId);
    Task<GeneralResponse<PurchaseOrderDTO>> DeletePurchaseOrderAsync(
   int PurchaseOrderId,
   CancellationToken cancellationToken = default);
    Task<GeneralResponse<List<NameStatus>>> GetPurchaseOrderStatus();
    Task<IEnumerable<PurchaseOrder>> GetByItemIdAsync(int itemId);
    Task<GeneralResponse<IEnumerable<PurchaseOrderDTO>>> GetByStatusAsync(string status);
    Task<IEnumerable<PurchaseOrder>> GetByUserIdAsync(string userId);
    Task<PurchaseOrder?> GetWithItemsAsync(int PurchaseOrderId);
    Task<PurchaseOrder?> GetWithWarehouseAsync(int PurchaseOrderId);
    Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync();
}
