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
    Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredStockIdAsync(int TransferredStockid, bool isBarcode,
          DynamicBarcodesDto? dynamicDto,
          AddTransferredItemDTO? dto);

    Task<GeneralResponse<TransferredItemDTO>> UpdateTransferredItemAsync(int TransferredItemId,
            UpdateTransferredItemDTO dto);
}
