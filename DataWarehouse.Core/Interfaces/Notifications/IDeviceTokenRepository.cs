using DataWarehouse.Domain.Entities.Notifications;

namespace DataWarehouse.Core.Interfaces.Notifications;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByUserAndDeviceIdAsync(string userId, string deviceId);
    Task<List<DeviceToken>> GetActiveByUserIdAsync(string userId);
    Task<DeviceToken?> GetByTokenAsync(string token);
    Task<List<DeviceToken>> GetByTokensAsync(IEnumerable<string> tokens);
    Task AddAsync(DeviceToken deviceToken);
    Task<int> SaveChangesAsync();
}
