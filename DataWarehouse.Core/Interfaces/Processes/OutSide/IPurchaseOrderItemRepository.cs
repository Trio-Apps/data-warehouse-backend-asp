using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface IPurchaseOrderItemRepository : IBaseRepository<PurchaseOrderItem>
{
    Task<GeneralResponse<IEnumerable<PurchaseOrderItemDTO>>> GetByPurchaseItemByPurchaseOrderIdAsync(int PurchaseOrderId);
    Task<GeneralWithTwoGenericResponse<PagedResult<PurchaseOrderItemDTO>, string>>
       GetByPurchaseItemByPurchaseOrderIdWithPaginationAsync(int PurchaseOrderId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<PurchaseOrderItemDTO>> AddPurchaseItemByPurchaseOrderIdAsync(int PurchaseOrderid, bool isBarcode, 
          DynamicBarcodesDto? dynamicDto,
          AddPurchaseOrderItemDTO? dto);

    Task<GeneralResponse<PurchaseOrderItemDTO>> UpdatePurchaseItemAsync(int PurchaseItemId, 
            UpdatePurchaseOrderItemDTO dto);

}
