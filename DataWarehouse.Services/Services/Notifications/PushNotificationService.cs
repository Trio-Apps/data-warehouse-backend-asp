using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace DataWarehouse.Services.Services.Notifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IDeviceTokenRepository deviceTokenRepository,
        ILogger<PushNotificationService> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;
    }

    public async Task<GeneralResponse<int>> SendToUserAsync(
        string userId,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GeneralResponse<int>.FailResponse("UserId is required.");

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
            return GeneralResponse<int>.FailResponse("Title and body are required.");

        var activeTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId);
        if (activeTokens.Count == 0)
            return GeneralResponse<int>.SuccessResponse(0, "No active device tokens found for this user.");

        var sentCount = 0;
        var now = DateTime.UtcNow;
        var hasChanges = false;

        foreach (var deviceToken in activeTokens)
        {
            try
            {
                var message = new Message
                {
                    Token = deviceToken.Token,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
                sentCount++;
            }
            catch (FirebaseMessagingException ex) when (IsInvalidToken(ex))
            {
                deviceToken.IsActive = false;
                deviceToken.UpdatedAt = now;
                hasChanges = true;

                _logger.LogWarning(
                    ex,
                    "Deactivated invalid Firebase token for user {UserId}, device {DeviceId}.",
                    userId,
                    deviceToken.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send push notification to user {UserId}, device {DeviceId}.",
                    userId,
                    deviceToken.DeviceId);
            }
        }

        if (hasChanges)
            await _deviceTokenRepository.SaveChangesAsync();

        return GeneralResponse<int>.SuccessResponse(sentCount, "Push notifications processed successfully.");
    }

    private static bool IsInvalidToken(FirebaseMessagingException ex)
    {
        return ex.MessagingErrorCode == MessagingErrorCode.Unregistered
            || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument
            || ex.MessagingErrorCode == MessagingErrorCode.SenderIdMismatch;
    }
}
