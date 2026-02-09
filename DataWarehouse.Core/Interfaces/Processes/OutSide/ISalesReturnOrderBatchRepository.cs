using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesReturnOrderBatchRepository : IBaseRepository<SalesReturnOrderBatch>
{
    Task<GeneralResponse<IEnumerable<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId);
    Task<GeneralResponse<PagedResult<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdWithPaginationAsync(int salesReturnOrderItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<SalesReturnOrderBatchDTO>> AddBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId, AddSalesReturnOrderBatchDTO dto);
    Task<GeneralResponse<SalesReturnOrderBatchDTO>> UpdateSalesReturnOrderBatchAsync(int salesReturnOrderBatchId, UpdateSalesReturnOrderBatchDTO dto);
}

