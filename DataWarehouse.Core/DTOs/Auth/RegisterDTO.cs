using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class RegisterDTO
{
    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 200 characters")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? PhoneNumber { get; set; }
}
