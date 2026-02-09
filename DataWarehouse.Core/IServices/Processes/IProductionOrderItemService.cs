using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface IProductionOrderItemService : IBaseService<ProductionOrderItem>
{
    Task<IEnumerable<ProductionOrderItem>> GetByProductionOrderIdAsync(int productionOrderId);
    Task<IEnumerable<ProductionOrderItem>> GetByItemIdAsync(int itemId);
    Task<ProductionOrderItem?> GetWithProductionOrderAsync(int productionOrderItemId);
    Task<ProductionOrderItem?> GetWithItemAsync(int productionOrderItemId);
}
