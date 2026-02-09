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
    Task<IEnumerable<ProductionOrderItemDTO>> GetByProductionItemByProductionOrderIdAsync(int productionOrderId);
    Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>> GetByProductionItemByProductionOrderIdWithPaginationAsync(int productionOrderId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionOrderItemDTO>> AddProductionItemByProductionOrderIdAsync(int productionOrderid,
           AddProductionOrderItemDTO dto);
   
    Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync( int productionItemId,bool? isRecevied, UpdateProductionOrderItemDTO dto);


}
