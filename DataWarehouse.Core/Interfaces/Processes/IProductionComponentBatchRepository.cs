using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionComponentBatchRepository : IBaseRepository<ProductionComponentBatch>
{
    Task<GeneralResponse<PagedResult<ProductionComponentBatchDTO>>> GetListAsync(string userId, int productionComponentLineId, int pageNumber, int pageSize);
    Task<GeneralResponse<IEnumerable<AvailableComponentBatchDTO>>> GetAvailableBatchesAsync(string userId, int productionComponentLineId);
    Task<GeneralResponse<ProductionComponentBatchDTO>> GetByIdDetailsAsync(string userId, int productionComponentBatchId);
    Task<GeneralResponse<ProductionComponentBatchDTO>> CreateAsync(string userId, AddProductionComponentBatchDTO dto);
    Task<GeneralResponse<ProductionComponentBatchDTO>> UpdateAsync(string userId, int productionComponentBatchId, UpdateProductionComponentBatchDTO dto);
    Task<GeneralResponse<ProductionComponentBatchDTO>> DeleteAsync(string userId, int productionComponentBatchId);
}
