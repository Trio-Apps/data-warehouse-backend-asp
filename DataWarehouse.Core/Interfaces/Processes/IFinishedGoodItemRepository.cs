using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IFinishedGoodItemRepository : IBaseRepository<WarehouseItem>
{
    Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<WarehouseItem>> GetByItemIdAsync(int itemId);

    Task<GeneralResponse<PagedResult<WarehouseItemDto>>> GetFinishedGoodBomItemsByWarehouseIdAsync(
        int warehouseId,
        int? itemId,
        int pageNumber,
        int pageSize);
    Task<WarehouseItem?> GetByItemAndWarehouseAsync(int itemId, int warehouseId);
    Task<WarehouseItem?> GetWithItemAsync(int WarehouseItemId);
    Task<WarehouseItem?> GetWithWarehouseAsync(int WarehouseItemId);
    Task<IEnumerable<WarehouseItem>> GetActiveItemsAsync();
}

