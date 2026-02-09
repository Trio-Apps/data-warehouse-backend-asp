using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredStockRepository : IBaseRepository<TransferredStock>
{
    Task<IEnumerable<TransferredStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);
    Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByWarehouseIdAsync(string userId, AddTransferredStockDTO dto);
    Task<GeneralResponse<TransferredStockDTO>> UpdateTransferredStockAsync(string userId, int transferredStockId, UpdateTransferredStockDTO dto);
    Task<GeneralResponse<List<NameStatus>>> GetTransferredStockStatus();
    Task<IEnumerable<TransferredStock>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId);
    Task<GeneralResponse<IEnumerable<TransferredStockDTO>>> GetByStatusAsync(string status);
    Task<IEnumerable<TransferredStock>> GetByUserIdAsync(string userId);
    Task<TransferredStock?> GetWithItemsAsync(int transferredStockId);
    Task<TransferredStock?> GetWithWarehousesAsync(int transferredStockId);
    Task<IEnumerable<TransferredStock>> GetPendingTransfersAsync();
    Task<IEnumerable<TransferredStock>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
}
