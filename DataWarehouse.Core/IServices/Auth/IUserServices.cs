using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Auth;

public interface IUserServices
{
    Task<GeneralResponse<UserDTO>> CreateUserAsync(string userId, AddUserDTO userDto);
    Task<GeneralResponse<UserDTO>> UpdateUserAsync(
        string userId,
      UpdateUserDto userDto);
    Task<IdentityResult> DeleteUserAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByUserNameAsync(string userName);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<GeneralResponse<PagedResult<GetUserDTO>>> GetAllUsersWithPaginationAsync(string userId,int pageNumber,
         int pageSize,
         int? companyId,
         int? sapId,
         string? email,
         string? fullName);
    Task<bool> UserExistsAsync(string userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
    Task<IdentityResult> UpdateUserEmailAsync(ApplicationUser user, string newEmail, string token);
    Task<string> GenerateEmailChangeTokenAsync(ApplicationUser user, string newEmail);
    Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string phoneNumber);
    Task<string> GeneratePhoneNumberTokenAsync(ApplicationUser user, string phoneNumber);
    Task<IdentityResult> ChangePhoneNumberAsync(ApplicationUser user, string phoneNumber, string token);
    Task<bool> IsPhoneNumberConfirmedAsync(ApplicationUser user);
    Task<IdentityResult> SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled);
    Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user);
    Task<bool> IsRoleNotAdminOrManagerOrSuperAdmin(string roleName);
}
