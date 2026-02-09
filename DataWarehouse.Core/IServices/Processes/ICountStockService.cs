using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Processes;

public interface ICountStockService : IBaseService<CountStock>
{
    Task<IEnumerable<CountStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<CountStock>> GetByStatusAsync(string status);
    Task<IEnumerable<CountStock>> GetByUserIdAsync(string userId);
    Task<CountStock?> GetWithItemsAsync(int countStockId);
    Task<CountStock?> GetWithWarehouseAsync(int countStockId);
    Task<IEnumerable<CountStock>> GetPendingCountsAsync();
    Task<IEnumerable<CountStock>> GetCompletedCountsAsync();
}
