using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.BarCode;

namespace DataWarehouse.Core.Interfaces.BarCode;

public interface IBarCodeSettingRepository : IBaseRepository<BarCodeSetting>
{
    Task<GeneralResponse<PagedResult<BarCodeSettingDto>>> GetBarCodeSettingAsync(int skip, int pageSize);
    Task<GeneralResponse<BarCodeSettingDto>> AddBarCodeSettingAsync(
      AddBarCodeSettingDto dto);

    Task<GeneralResponse<BarCodeSettingDto>> UpdateBarCodeSettingAsync(
      UpdateBarCodeSettingDto dto);
    Task<GeneralResponse<BarCodeSettingDto>> DeleteBarCodeSettingAsync(int id);

}

