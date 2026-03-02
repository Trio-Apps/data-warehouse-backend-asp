using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionComponentLineRepository : IBaseRepository<ProductionComponentLine>
{
    Task<GeneralResponse<PagedResult<ProductionComponentLineDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionComponentLineDTO>> GetByIdDetailsAsync(string userId, int productionComponentLineId);
    Task<GeneralResponse<ProductionComponentLineDTO>> CreateAsync(string userId, AddProductionComponentLineDTO dto);
    Task<GeneralResponse<ProductionComponentLineDTO>> UpdateAsync(string userId, int productionComponentLineId, UpdateProductionComponentLineDTO dto);
    Task<GeneralResponse<ProductionComponentLineDTO>> DeleteAsync(string userId, int productionComponentLineId);
}
