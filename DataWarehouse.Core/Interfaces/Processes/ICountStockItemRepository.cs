using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface ICountStockItemRepository : IBaseRepository<CountStockItem>
{
    Task<IEnumerable<CountStockItemDTO>> GetByCountStockItemByCountStockIdAsync(int CountStockId);
    Task<GeneralResponse<PagedResult<CountStockItemDTO>>>
      GetByCountStockItemByCountStockIdWithPaginationAsync(int CountStockId, string? status, int pageNumber, int pageSize);
    Task<GeneralResponse<CountStockItemDTO>> AddCountStockItemByCountStockIdAsync(int CountStockid, bool isBarcode,
          DynamicBarcodesDto? dynamicDto,
          AddCountStockItemDTO? dto);

    Task<GeneralResponse<CountStockItemDTO>> UpdateCountStockItemAsync(int CountStockItemId,
            UpdateCountStockItemDTO dto);
}
