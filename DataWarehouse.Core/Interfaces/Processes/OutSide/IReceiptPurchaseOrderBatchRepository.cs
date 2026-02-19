using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IReceiptPurchaseOrderBatchRepository : IBaseRepository<ReceiptPurchaseOrderBatch>
{
    Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderBatchDTO>>> GetByReceiptPurchaseOrderItemIdAsync(int receiptPurchaseOrderItemId);
    Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderBatchDTO>>> GetByReceiptPurchaseOrderItemIdWithPaginationAsync(int receiptPurchaseOrderItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> AddByReceiptPurchaseOrderItemIdAsync(int receiptPurchaseOrderItemId, GeneralBatchDto dto);
    Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> UpdateReceiptPurchaseOrderBatchAsync(int receiptPurchaseOrderBatchId, UpdateGeneralBatchDto dto);
}

