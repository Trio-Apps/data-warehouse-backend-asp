using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ICountStockRepository : IBaseRepository<CountStock>
{
    Task<IEnumerable<CountStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<CountStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<CountStockDTO>> AddCountStockByWarehouseIdAsync(string userId, AddCountStockDTO dto);
    Task<GeneralResponse<CountStockDTO>> UpdateCountStockAsync(string userId, int countStockId, UpdateCountStockDTO dto);
    Task<GeneralResponse<CountStockDTO>> DuplicateCountStockAsync(string userId, int countStockId, CancellationToken cancellationToken = default);
    Task<GeneralResponse<List<NameStatus>>> GetCountStockStatus();
    Task<GeneralResponse<IEnumerable<CountStockDTO>>> GetByStatusAsync(string status);
    Task<IEnumerable<CountStock>> GetByUserIdAsync(string userId);
    Task<CountStock?> GetWithItemsAsync(int countStockId);
    Task<CountStock?> GetWithWarehouseAsync(int countStockId);
    Task<IEnumerable<CountStock>> GetPendingCountsAsync();
    Task<IEnumerable<CountStock>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    Task<GeneralResponse<CountStockDTO>> SubmitCountStockAsync(string userId, int countStockId, string? note = null);
    Task<GeneralResponse<DataWarehouse.Core.DTOs.Approval.ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int countStockId);
}
