using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ITransferredItemRepository : IBaseRepository<TransferredItem>
{
    Task<IEnumerable<TransferredItemDTO>> GetByTransferredItemByTransferredStockIdAsync(int TransferredStockId);
    Task<GeneralResponse<PagedResult<TransferredItemDTO>>>
      GetByTransferredItemByTransferredStockIdWithPaginationAsync(int TransferredStockId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredStockIdWithoutRefAsync(
        int transferredStockId,
        bool isBarcode,
        DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto);

    Task<GeneralResponse<TransferredItemDTO>> UpdateTransferredItemWithoutRefAsync(
           int transferredItemId,
           UpdateGeneralItemDto dto);
    Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredRequestItemIdAsync(
           string userId,
           int transferredStockId,
           AddTransferredItemDTO dto);

    Task<GeneralResponse<TransferredItemDTO>> DeleteTransferredItemAsync(int transferredItemId);
    Task<IEnumerable<TransferredItem>> GetByTransferredStockIdEntitiesAsync(int transferredStockId);
    Task<IEnumerable<TransferredItem>> GetByItemIdAsync(int itemId);
    Task<TransferredItem?> GetWithTransferredStockAsync(int transferredItemId);
    Task<TransferredItem?> GetWithTransferredRequestItemAsync(int transferredItemId);
    Task<TransferredItem?> GetWithItemAsync(int transferredItemId);
    Task<TransferredItem?> GetWithBatchesAsync(int transferredItemId);
}
