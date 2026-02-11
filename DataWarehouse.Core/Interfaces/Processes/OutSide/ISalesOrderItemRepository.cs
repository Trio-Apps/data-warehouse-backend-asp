using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesOrderItemRepository : IBaseRepository<SalesOrderItem>
{
    Task<GeneralResponse<IEnumerable<SalesOrderItemDTO>>> GetBySalesItemBySalesOrderIdAsync(int SalesOrderId);
    Task<GeneralResponse<PagedResult<SalesOrderItemDTO>>>
      GetBySalesItemBySalesOrderIdWithPaginationAsync(int SalesOrderId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<SalesOrderItemDTO>> AddSalesItemBySalesOrderIdAsync(int SalesOrderid, bool isBarcode, 
          DynamicBarcodesDto? dynamicDto,
          AddGeneralItemDto? dto);

    Task<GeneralResponse<SalesOrderItemDTO>> UpdateSalesItemAsync(int SalesItemId,
            UpdateGeneralItemDto dto);
    Task<GeneralResponse<SalesOrderItemDTO>> DeleteSalesItemAsync(int SalesItemId);
}
