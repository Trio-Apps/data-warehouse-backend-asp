using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Notifications;

namespace DataWarehouse.Core.IServices.Notifications;

public interface IAppNotificationService : IBaseService<Notification>
{
    Task<GeneralResponse<AppNotificationDto>> AddNotificationAsync(CreateAppNotificationDto dto);
    Task<GeneralResponse<bool>> MarkAsReadAsync(int notificationId, string userId);
    Task<GeneralResponse<bool>> MarkAllAsReadAsync(string userId);
}
