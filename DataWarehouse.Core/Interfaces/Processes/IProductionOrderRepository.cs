using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionOrderRepository : IBaseRepository<ProductionOrder>
{
    Task<GeneralResponse<PagedResult<ProductionOrderDTO>>> GetListAsync(string userId, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionOrderDTO>> GetDetailsAsync(string userId, int productionOrderId);
    Task<GeneralResponse<ProductionOrderDTO>> CreateAsync(string userId, AddProductionOrderDTO dto);
    Task<GeneralResponse<ProductionOrderDTO>> UpdateAsync(string userId, int productionOrderId, UpdateProductionOrderDTO dto);
    Task<GeneralResponse<ProductionOrderDTO>> DeleteProductionOrderAsync(string userId, int productionOrderId);
    Task<GeneralResponse<ProductionOrderDTO>> SubmitAsync(string userId, int productionOrderId, SubmitProductionOrderDTO? dto = null);
}
