using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Auth;

public class AuthResponseDTO
{
    public string Token { get; set; }
    public DateTime ExpiresOn { get; set; }
    public UserDTO User { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();

}

public class UserDTO
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int? companyId { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();

   
}
public class GetUserDTO
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();

    public int? CompanyId { get; set; }
    public ICollection<int>? SapIds { get; set; }
    public int? SapEmployeeId { get; set; }
    public ICollection<int>? WarehouseIds { get; set; }
}

public class AddUserDTO
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

   
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Role Name is required")]
    public string RoleName {  get; set; }

    public int? CompanyId { get; set; }
    public ICollection<int>? SapIds { get; set; }
    public ICollection<int>? WarehouseIds { get; set; }


}


public class UpdateUserDto
{
    public string Id { get; set; }
    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 200 characters")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; }





    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Role Name is required")]
    public string RoleName { get; set; }

    public int? CompanyId { get; set; }
    public ICollection<int>? SapIds { get; set; }
    public int? SapEmployeeId { get; set; }
    public ICollection<int>? WarehouseIds { get; set; }
}


public class RoleDTO
{
    public string? Id { get; set; }
    public string? RoleName { get; set; }
  
}



public class AddRoleDto
{
    public string RoleName { get; set; }

}



public class UpdateRoleDto
{
    public string Id { get; set; }
    public string RoleName { get; set; }

}