using System.Security.Claims;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Domain.Context;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Api.Controllers.admin;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly DataWarehouseDbContext _context;

    public NotificationController(DataWarehouseDbContext context)
    {
        _context = context;
    }

    [HttpGet("my/{pageNumber:int}/{pageSize:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> GetMyNotifications(int pageNumber, int pageSize, [FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<PagedResult<AppNotificationDto>>.FailResponse("User ID not found in token."));

        var safePageNumber = pageNumber <= 0 ? 1 : pageNumber;
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(x => !x.IsRead);
        }

        var totalRecords = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new AppNotificationDto
            {
                NotificationId = x.NotificationId,
                Title = x.Title,
                Message = x.Message,
                ActionType = x.ActionType,
                DocumentType = x.DocumentType,
                ProcessType = x.ProcessType,
                ReferenceId = x.ReferenceId,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
                ReadAt = x.ReadAt
            })
            .ToListAsync();

        var paged = new PagedResult<AppNotificationDto>
        {
            Data = notifications,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
            TotalRecords = totalRecords
        };

        return Ok(GeneralResponse<PagedResult<AppNotificationDto>>.SuccessResponse(paged));
    }

    [HttpGet("unread-count")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<int>.FailResponse("User ID not found in token."));

        var unreadCount = await _context.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead);

        return Ok(GeneralResponse<int>.SuccessResponse(unreadCount));
    }

    [HttpPatch("{notificationId:int}/read")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<bool>.FailResponse("User ID not found in token."));

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

        if (notification == null)
            return NotFound(GeneralResponse<bool>.FailResponse("Notification not found."));

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(GeneralResponse<bool>.SuccessResponse(true));
    }

    [HttpPatch("mark-all-read")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(GeneralResponse<bool>.FailResponse("User ID not found in token."));

        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
            return Ok(GeneralResponse<bool>.SuccessResponse(true));

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync();

        return Ok(GeneralResponse<bool>.SuccessResponse(true));
    }
}
