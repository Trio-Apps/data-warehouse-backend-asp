namespace DataWarehouse.Core.DTOs.Notifications;

public class RegisterDeviceTokenDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
