using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IReceiptPurchaseOrderRepository : IBaseRepository<ReceiptPurchaseOrder>
{
    Task<IEnumerable<ReceiptPurchaseOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetReceiptOrderByIdAsync(string userId, int receiptOrderId);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderByWarehouseIdAsync(string userId, AddReceiptPurchaseOrderDTO dto);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> UpdateReceiptPurchaseOrderAsync(string userId, int receiptPurchaseOrderId, UpdateReceiptPurchaseOrderDTO dto);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> DeleteReceiptOrderAsync(
    int receiptOrderId,
    CancellationToken cancellationToken = default);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetByPurchaseOrderIdAsync(string userId, int purchaseOrderId);
    Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>> GetByStatusAsync(string status);
    Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetWithItemsAndBatchesAsync(int receiptPurchaseOrderId);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetByUserIdAsync(string userId);
    Task<ReceiptPurchaseOrder?> GetWithItemsAsync(int receiptPurchaseOrderId);
    Task<ReceiptPurchaseOrder?> GetWithPurchaseOrderAsync(int receiptPurchaseOrderId);
    Task<ReceiptPurchaseOrder?> GetWithWarehouseAsync(int receiptPurchaseOrderId);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    Task<IEnumerable<ReceiptPurchaseOrder>> GetPendingReceiptsAsync();
}
