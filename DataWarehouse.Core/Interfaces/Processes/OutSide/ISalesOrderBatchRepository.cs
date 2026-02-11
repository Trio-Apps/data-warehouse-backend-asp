using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide;

public interface ISalesOrderBatchRepository : IBaseRepository<SalesOrderBatch>
{
    Task<GeneralResponse<IEnumerable<SalesOrderBatchDTO>>> GetBySalesOrderItemIdAsync(int salesOrderItemId);
    Task<GeneralResponse<PagedResult<SalesOrderBatchDTO>>> GetBySalesOrderItemIdWithPaginationAsync(int salesOrderItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<SalesOrderBatchDTO>> AddBySalesOrderItemIdAsync(int salesOrderItemId, GeneralBatchDto dto);
    Task<GeneralResponse<SalesOrderBatchDTO>> UpdateSalesOrderBatchAsync(int salesOrderBatchId, UpdateGeneralBatchDto dto);
    Task<GeneralResponse<SalesOrderBatchDTO>> DeleteSalesOrderBatchAsync(int salesOrderBatchId);
}

