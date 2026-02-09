using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface IProductionOrderService : IBaseService<ProductionOrder>
{
    Task<IEnumerable<ProductionOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<ProductionOrder>> GetByItemIdAsync(int itemId);
    Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<ProductionOrder>> GetByUserIdAsync(string userId);
    Task<ProductionOrder?> GetWithItemsAsync(int productionOrderId);
    Task<ProductionOrder?> GetWithWarehouseAsync(int productionOrderId);
    Task<IEnumerable<ProductionOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionOrder>> GetPendingOrdersAsync();
}
