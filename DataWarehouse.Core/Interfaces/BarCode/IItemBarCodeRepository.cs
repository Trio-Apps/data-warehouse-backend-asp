using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.BarCode;

namespace DataWarehouse.Core.Interfaces.BarCode;

public interface IItemBarCodeRepository : IBaseRepository<ItemBarCode>
{
    Task<GeneralResponse<PagedResult<BarCodeDto>>> GetByItemIdAsync(int itemId, int skip, int pageSize,
        string? barCode);
    Task<GeneralResponse<PagedResult<BarCodeDto>>> GetByItemIdOrNoAsync(int pageNumber, int pageSize, int? itemId, string? barCode);
    Task<GeneralResponse<BarCodeDto>> GetBarCodeByBarCodeIdAsync(int barCodeId);
    Task<GeneralResponse<BarCodeDto>> AddBarCodeForItem(
     int itemId,
     AddBarCodeDto dto);
    //
    Task<GeneralResponse<BarCodeDto>> GetWithItemAsync(int BarCodeId);
    Task<bool> ExistsByBarCodeAsync(string barCode);

    Task<GeneralResponse<BarCodeDto>> UpdateBarCodeAsync(int barCodeId,
   UpdateBarCodeDto dto);

    Task<GeneralResponse<BarCodeDto>> DeleteBarCodeAsync(int barCodeId);

    Task<GeneralResponse<List<ItemUomGroupDto>>> GetItemUomGroupAsync(int itemId);
}



