using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionHeaderBatchRepository : IBaseRepository<ProductionHeaderBatch>
{
    Task<GeneralResponse<PagedResult<ProductionHeaderBatchDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionHeaderBatchDTO>> GetByIdDetailsAsync(string userId, int productionHeaderBatchId);
    Task<GeneralResponse<ProductionHeaderBatchDTO>> CreateAsync(string userId, AddProductionHeaderBatchDTO dto);
    Task<GeneralResponse<ProductionHeaderBatchDTO>> UpdateAsync(string userId, int productionHeaderBatchId, UpdateProductionHeaderBatchDTO dto);
    Task<GeneralResponse<ProductionHeaderBatchDTO>> DeleteAsync(string userId, int productionHeaderBatchId);
}
