using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IReceiptPurchaseOrderItemRepository : IBaseRepository<ReceiptPurchaseOrderItem>
{
    Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderItemDTO>>> GetByReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(int ReceiptPurchaseOrderId);
    Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderItemDTO>>> GetByReceiptPurchaseItemByReceiptPurchaseOrderIdWithPaginationAsync(int ReceiptPurchaseOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> AddReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(int ReceiptPurchaseOrderid,
         bool isBarcode
          , DynamicBarcodesDto? barcodeDto,
           AddReceiptPurchaseOrderItemDTO? dto);
    Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> UpdateReceiptPurchaseItemAsync(int ReceiptPurchaseItemId,
         UpdateReceiptPurchaseOrderItemDTO dto);
    Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId);
    Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByItemIdAsync(int itemId);
    Task<ReceiptPurchaseOrderItem?> GetWithReceiptPurchaseOrderAsync(int receiptPurchaseOrderItemId);
    Task<ReceiptPurchaseOrderItem?> GetWithItemAsync(int receiptPurchaseOrderItemId);
    Task<ReceiptPurchaseOrderItem?> GetWithCommentAsync(int receiptPurchaseOrderItemId);
}
