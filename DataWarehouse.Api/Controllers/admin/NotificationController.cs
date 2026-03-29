using System.Security.Claims;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IAppNotificationRepository _notificationRepository;
    private readonly IAppNotificationService _notificationService;

    public NotificationController(
        IAppNotificationRepository notificationRepository,
        IAppNotificationService notificationService)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    [HttpGet("my/{pageNumber:int}/{pageSize:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> GetMyNotifications(int pageNumber, int pageSize, [FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<PagedResult<AppNotificationDto>>.FailResponse("User ID not found in token."));

        var result = await _notificationRepository.GetMyNotificationsAsync(userId, pageNumber, pageSize, unreadOnly);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<int>.FailResponse("User ID not found in token."));

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
        return Ok(GeneralResponse<int>.SuccessResponse(unreadCount));
    }

    [HttpPatch("{notificationId:int}/read")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<bool>.FailResponse("User ID not found in token."));

        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPatch("mark-all-read")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<bool>.FailResponse("User ID not found in token."));

        var result = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(result);
    }
}
