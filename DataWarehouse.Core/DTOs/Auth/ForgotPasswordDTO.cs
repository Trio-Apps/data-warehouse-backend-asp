using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class ForgotPasswordDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
}
