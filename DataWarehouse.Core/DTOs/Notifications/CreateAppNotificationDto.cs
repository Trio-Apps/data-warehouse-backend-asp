namespace DataWarehouse.Core.DTOs.Notifications;

public class CreateAppNotificationDto
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? ProcessType { get; set; }
    public int ReferenceId { get; set; }
}
