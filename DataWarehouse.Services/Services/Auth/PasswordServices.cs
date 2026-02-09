using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Auth;

public class PasswordServices : IPasswordServices
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PasswordServices(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<bool> ValidatePasswordAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CheckPasswordAsync(user, password);
        return result;
    }

    public async Task<IdentityResult> AddPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.AddPasswordAsync(user, password);
    }

    public async Task<IdentityResult> RemovePasswordAsync(ApplicationUser user)
    {
        return await _userManager.RemovePasswordAsync(user);
    }

    public async Task<bool> HasPasswordAsync(ApplicationUser user)
    {
        return await _userManager.HasPasswordAsync(user);
    }
}
