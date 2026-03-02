using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProductionOrderItemRepository : IBaseRepository<ProductionOrderItem>
{
    Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionOrderItemDTO>> GetByIdDetailsAsync(string userId, int productionOrderItemId);
    Task<GeneralResponse<ProductionOrderItemDTO>> CreateAsync(string userId, AddProductionOrderItemDTO dto);
    Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync(string userId, int productionItemId, UpdateProductionOrderItemDTO dto);
    Task<GeneralResponse<ProductionOrderItemDTO>> DeleteProductionItemAsync(string userId, int productionItemId);
}
