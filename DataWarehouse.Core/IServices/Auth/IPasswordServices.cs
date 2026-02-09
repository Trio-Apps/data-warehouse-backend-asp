using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Auth;

public interface IPasswordServices
{
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<bool> ValidatePasswordAsync(ApplicationUser user, string password);
    Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password);
    Task<IdentityResult> RemovePasswordAsync(ApplicationUser user);
    Task<bool> HasPasswordAsync(ApplicationUser user);
}
