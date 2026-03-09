using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide
{
    public interface IFinishedGoodPurchaseOrderRepository : IBaseRepository<WarehouseItem>
    {
        Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetByWarehouseIdAsync(int warehouseId);

        Task<GeneralResponse<PagedResult<WarehouseItemDto>>> GetByWarehouseIdAsync(
   int warehouseId,
   string? search = null,
   int pageNumber = 1,
   int pageSize = 20);
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
}
