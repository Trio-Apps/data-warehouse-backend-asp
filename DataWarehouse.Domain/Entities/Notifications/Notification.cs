using DataWarehouse.Domain.Entities.Auth;

namespace DataWarehouse.Domain.Entities.Notifications;

public class Notification
{
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? ProcessType { get; set; }
    public int ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public ApplicationUser? User { get; set; }
}
