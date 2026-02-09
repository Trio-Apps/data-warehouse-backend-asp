using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Auth;

public interface IAuthServices
{
    public sealed record LoginContext(
bool IsGlobal,
bool HasCompany,
int? CompanyId
        );

    Task<IdentityResult> RegisterAsync(ApplicationUser user, string password);
    Task<SignInResult> LoginAsync(string email, string password, bool rememberMe = false);
    Task<LoginContext> GetLoginContextAsync(string userId, IList<string> roleNames);
    Task<int?> GetLoginContextForUserAsync(string userId);
    Task LogoutAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByClaimsAsync(ClaimsPrincipal claimsPrincipal);
    Task<bool> IsEmailConfirmedAsync(ApplicationUser user);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
    Task<bool> IsUserLockedOutAsync(ApplicationUser user);
    Task<IdentityResult> LockUserAsync(ApplicationUser user, int minutes);
    Task<IdentityResult> UnlockUserAsync(ApplicationUser user);
}
