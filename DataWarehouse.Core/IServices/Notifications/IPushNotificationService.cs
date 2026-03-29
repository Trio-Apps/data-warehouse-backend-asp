using DataWarehouse.Core.DTOs;

namespace DataWarehouse.Core.IServices.Notifications;

public interface IPushNotificationService
{
    Task<GeneralResponse<int>> SendToUserAsync(
        string userId,
        string title,
        string body,
        Dictionary<string, string>? data = null);
}
