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
    Task<IEnumerable<ProductionOrder>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<ProductionOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<ProductionOrderDTO>> AddProductionOrderByWarehouseIdAsync(string userId,
           AddProductionOrderDTO dto);
    Task<GeneralResponse<ProductionOrderDTO>> UpdateProductionOrderAsync(string userId, int productionId, UpdateProductionOrderDTO dto);
    Task<GeneralResponse<List<NameStatus>>> GetProductionOrderStatus();
    Task<IEnumerable<ProductionOrder>> GetByItemIdAsync(int itemId);
    Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status);
    Task<IEnumerable<ProductionOrder>> GetByUserIdAsync(string userId);
    Task<ProductionOrder?> GetWithItemsAsync(int productionOrderId);
    Task<ProductionOrder?> GetWithWarehouseAsync(int productionOrderId);
    Task<IEnumerable<ProductionOrder>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    Task<IEnumerable<ProductionOrder>> GetPendingOrdersAsync();
}
