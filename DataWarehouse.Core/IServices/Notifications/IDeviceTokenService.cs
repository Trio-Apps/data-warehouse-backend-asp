using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Notifications;

namespace DataWarehouse.Core.IServices.Notifications;

public interface IDeviceTokenService
{
    Task<GeneralResponse<DeviceTokenDto>> RegisterOrUpdateAsync(string userId, RegisterDeviceTokenDto dto);
    Task<GeneralResponse<List<DeviceTokenDto>>> GetActiveTokensByUserAsync(string userId);
    Task<GeneralResponse<bool>> DeactivateAsync(string userId, DeactivateDeviceTokenDto dto);
    Task<GeneralResponse<int>> DeactivateInvalidTokensAsync(IEnumerable<string> tokens);
}
