using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static DataWarehouse.Core.IServices.Auth.IAuthServices;

namespace DataWarehouse.Services.Services.Auth;

public class AuthServices : IAuthServices
{
    private readonly ICompanyCache companyCache;
    private readonly DataWarehouseDbContext context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthServices(ICompanyCache companyCache, DataWarehouseDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        this.companyCache = companyCache;
        this.context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return SignInResult.Failed;

        return await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
    }

    public async Task LogoutAsync(string userId)
    {

        var user = await _userManager.FindByIdAsync(userId);

        var existingClaims = await _userManager.GetClaimsAsync(user);
        //  var existing = existingClaims.FirstOrDefault(c => c.Type == "CurrentCompanyId");

        if (existingClaims.Any())
            await _userManager.RemoveClaimsAsync(user, existingClaims);

        await _signInManager.SignOutAsync();
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.CompanyUser)
            .FirstOrDefaultAsync(u => u.Email == email);

     
        return user;
    }


    public async Task<LoginContext> GetLoginContextAsync(string userId, IList<string> roleNames)
    {
        // CompanyId (لو موجود)
        int? companyId = await context.Set<CompanyUser>()
            .Where(cu => cu.UserId == userId)
            .Select(cu => (int?)cu.CompanyId)
            .FirstOrDefaultAsync();

        // ✅ Global SuperAdmin: لازم نتأكد انه Global role (CompanyId = null)
        var IsGlobal = await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
            .AnyAsync(r => r.CompanyId == null);



        return new LoginContext(
            IsGlobal: IsGlobal,
            HasCompany: companyId is not null,
            CompanyId: companyId
        );

   
    }
    public async Task<int?> GetLoginContextForUserAsync(string userId)
    {
        // CompanyId (لو موجود)
        int? companyId = await context.Set<CompanyUser>()
            .Where(cu => cu.UserId == userId)
            .Select(cu => (int?)cu.CompanyId)
            .FirstOrDefaultAsync();


        return companyId;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> GetUserByClaimsAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal);
    }

    public async Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<bool> IsUserLockedOutAsync(ApplicationUser user)
    {
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<IdentityResult> LockUserAsync(ApplicationUser user, int minutes)
    {
        return await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.UtcNow.AddMinutes(minutes));
    }

    public async Task<IdentityResult> UnlockUserAsync(ApplicationUser user)
    {
        return await _userManager.SetLockoutEndDateAsync(user, null);
    }
}
