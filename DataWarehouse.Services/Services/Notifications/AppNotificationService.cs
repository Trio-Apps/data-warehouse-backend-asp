using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using DataWarehouse.Domain.Entities.Notifications;
using DataWarehouse.Services.Services.Based;

namespace DataWarehouse.Services.Services.Notifications;

public class AppNotificationService : BaseService<Notification>, IAppNotificationService
{
    private readonly IAppNotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public AppNotificationService(
        IAppNotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService) : base(notificationRepository)
    {
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<GeneralResponse<AppNotificationDto>> AddNotificationAsync(CreateAppNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            ActionType = dto.ActionType,
            DocumentType = dto.DocumentType,
            ProcessType = dto.ProcessType,
            ReferenceId = dto.ReferenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        var pushData = new Dictionary<string, string>
        {
            ["notificationId"] = notification.NotificationId.ToString(),
            ["actionType"] = notification.ActionType,
            ["documentType"] = notification.DocumentType,
            ["referenceId"] = notification.ReferenceId.ToString()
        };

        if (!string.IsNullOrWhiteSpace(notification.ProcessType))
            pushData["processType"] = notification.ProcessType;

        await _pushNotificationService.SendToUserAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            pushData);

        return GeneralResponse<AppNotificationDto>.SuccessResponse(new AppNotificationDto
        {
            NotificationId = notification.NotificationId,
            Title = notification.Title,
            Message = notification.Message,
            ActionType = notification.ActionType,
            DocumentType = notification.DocumentType,
            ProcessType = notification.ProcessType,
            ReferenceId = notification.ReferenceId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        });
    }

    public async Task<GeneralResponse<bool>> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _notificationRepository.GetByIdForUserAsync(notificationId, userId);
        if (notification == null)
            return GeneralResponse<bool>.FailResponse("Notification not found.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.SaveChangesAsync();
        }

        return GeneralResponse<bool>.SuccessResponse(true);
    }

    public async Task<GeneralResponse<bool>> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _notificationRepository.GetUnreadForUserAsync(userId);
        if (notifications.Count == 0)
            return GeneralResponse<bool>.SuccessResponse(true);

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _notificationRepository.SaveChangesAsync();
        return GeneralResponse<bool>.SuccessResponse(true);
    }
}
