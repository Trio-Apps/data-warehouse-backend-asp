using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class ConfirmEmailDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }
}
