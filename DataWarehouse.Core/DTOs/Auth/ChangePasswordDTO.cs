using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "Current Password is required")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm New Password is required")]
    [Compare("NewPassword", ErrorMessage = "New Password and Confirm New Password do not match")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; }
}
