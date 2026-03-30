using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Notifications;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Notifications;

public class AppNotificationRepository : BaseRepository<Notification>, IAppNotificationRepository
{
    public AppNotificationRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    private static IQueryable<Notification> ApplyApprovalOnlyFilter(IQueryable<Notification> query)
    {
        return query.Where(x =>
            x.ActionType == "Approved" ||
            x.ActionType == "Rejected" ||
            x.ActionType == "تمت الموافقة" ||
            x.ActionType == "تم الرفض" ||
            EF.Functions.Like(x.Title, "%approved%") ||
            EF.Functions.Like(x.Title, "%rejected%") ||
            EF.Functions.Like(x.Title, "%الموافقة%") ||
            EF.Functions.Like(x.Title, "%الرفض%"));
    }

    public async Task<GeneralResponse<PagedResult<AppNotificationDto>>> GetMyNotificationsAsync(
        string userId,
        int pageNumber,
        int pageSize,
        bool unreadOnly = false)
    {
        var safePageNumber = pageNumber <= 0 ? 1 : pageNumber;
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        query = ApplyApprovalOnlyFilter(query);

        if (unreadOnly)
            query = query.Where(x => !x.IsRead);

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

        return GeneralResponse<PagedResult<AppNotificationDto>>.SuccessResponse(new PagedResult<AppNotificationDto>
        {
            Data = notifications,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
            TotalRecords = totalRecords
        });
    }

    public Task<int> GetUnreadCountAsync(string userId)
    {
        return ApplyApprovalOnlyFilter(
            _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId))
            .CountAsync(x => !x.IsRead);
    }

    public Task<Notification?> GetByIdForUserAsync(int notificationId, string userId)
    {
        return _context.Notifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);
    }

    public Task<List<Notification>> GetUnreadForUserAsync(string userId)
    {
        return ApplyApprovalOnlyFilter(_context.Notifications
            .Where(x => x.UserId == userId))
            .Where(x => !x.IsRead)
            .ToListAsync();
    }
}
