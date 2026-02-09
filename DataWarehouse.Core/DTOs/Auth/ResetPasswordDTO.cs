using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class ResetPasswordDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm New Password is required")]
    [Compare("NewPassword", ErrorMessage = "New Password and Confirm New Password do not match")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; }
}
