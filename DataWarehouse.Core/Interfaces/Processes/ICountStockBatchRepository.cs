using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ICountStockBatchRepository : IBaseRepository<CountStockBatch>
{
    Task<GeneralResponse<IEnumerable<CountStockBatchDTO>>> GetByCountStockItemIdAsync(int countStockItemId);
    Task<GeneralResponse<PagedResult<CountStockBatchDTO>>> GetByCountStockItemIdWithPaginationAsync(int countStockItemId, int pageNumber, int pageSize);
    Task<GeneralResponse<CountStockBatchDTO>> AddByCountStockItemIdAsync(int countStockItemId, AddCountStockBatchDTO dto);
    Task<GeneralResponse<CountStockBatchDTO>> UpdateCountStockBatchAsync(int countStockBatchId, UpdateCountStockBatchDTO dto);
}

