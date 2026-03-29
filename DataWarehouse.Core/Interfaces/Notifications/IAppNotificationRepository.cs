using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Notifications;

namespace DataWarehouse.Core.Interfaces.Notifications;

public interface IAppNotificationRepository : IBaseRepository<Notification>
{
    Task<GeneralResponse<PagedResult<AppNotificationDto>>> GetMyNotificationsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        bool unreadOnly = false);

    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification?> GetByIdForUserAsync(int notificationId, string userId);
    Task<List<Notification>> GetUnreadForUserAsync(string userId);
}
