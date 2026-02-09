using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Services.Repository.CompanyRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Auth;

public class RoleServices : IRoleServices
{
    private readonly ICompanyCache companyCache;
    private readonly DataWarehouseDbContext context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<DataWarehouse.Domain.Entities.Auth.ApplicationUser> _userManager;

    public RoleServices(ICompanyCache companyCache, DataWarehouseDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<DataWarehouse.Domain.Entities.Auth.ApplicationUser> userManager)
    {
        this.companyCache = companyCache;
        this.context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<GeneralResponse<IdentityResult>> CreateRoleAsync(
     string userId,
     string roleName)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GeneralResponse<IdentityResult>.FailResponse("userId is required.");

        if (string.IsNullOrWhiteSpace(roleName))
            return GeneralResponse<IdentityResult>.FailResponse("roleName is required.");

        // 1️⃣ نجيب CompanyId لو موجود
        int? companyId = await context.Set<CompanyUser>()
            .Where(cu => cu.UserId == userId)
            .Select(cu => (int?)cu.CompanyId)
            .FirstOrDefaultAsync();

        // 2️⃣ Check role already exists (same scope)
        bool roleExists = await context.Roles
            .AnyAsync(r =>
                r.Name == roleName &&
                r.CompanyId == companyId);

        if (roleExists)
            return GeneralResponse<IdentityResult>
                .FailResponse($"Role '{roleName}' already exists.");

        // 3️⃣ Create role (Company or Global)
        var role = new ApplicationRole
        {
            Name = roleName,
           // NormalizedName = roleName.ToUpper(),
            CompanyId = companyId
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return GeneralResponse<IdentityResult>
                .FailResponse("Failed to create role.");

        return GeneralResponse<IdentityResult>
            .SuccessResponse(result, "Role created successfully.");
    }



    public async Task<IdentityResult> UpdateRoleAsync(ApplicationRole role)
    {

        return await _roleManager.UpdateAsync(role);
    }



    public async Task<IdentityResult> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = "Role not found" });


        return await _roleManager.DeleteAsync(role);
    }

    public async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
    {

        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
    {
        return await _roleManager.FindByNameAsync(roleName);
    }

    public async Task<GeneralResponse<IEnumerable<RoleDTO>>> GetAllRolesAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GeneralResponse<IEnumerable<RoleDTO>>.FailResponse("userId is required.");

        int? companyId = await context.Set<CompanyUser>()
            .Where(cu => cu.UserId == userId)
            .Select(cu => (int?)cu.CompanyId)
            .FirstOrDefaultAsync();

        var query = _roleManager.Roles.AsNoTracking();

        query = companyId.HasValue
            ? query.Where(r => r.CompanyId == companyId.Value)
            : query.Where(r => r.CompanyId == null);

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new RoleDTO
            {
                Id = r.Id,
                RoleName = r.Name!
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<RoleDTO>>.SuccessResponse(roles);
    }

    public async Task<GeneralResponse<PagedResult<RoleDTO>>> GetAllRolesWithPaginationAsync(
      string userId, int pageNumber, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GeneralResponse<PagedResult<RoleDTO>>.FailResponse("userId is required.");

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        int? companyId = await context.Set<CompanyUser>()
            .Where(cu => cu.UserId == userId)
            .Select(cu => (int?)cu.CompanyId)
            .FirstOrDefaultAsync();

        var query = _roleManager.Roles.AsNoTracking();

        query = companyId.HasValue
            ? query.Where(r => r.CompanyId == companyId.Value)
            : query.Where(r => r.CompanyId == null);

        var totalRecords = await query.CountAsync();

        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleDTO
            {
                Id = r.Id,
                RoleName = r.Name!
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<RoleDTO>>.SuccessResponse(new PagedResult<RoleDTO>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Data = roles
        });
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        var companyId = await companyCache.Get();

        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<IEnumerable<IdentityUser>> GetUsersInRoleAsync(string roleName)
    {
        return await _userManager.GetUsersInRoleAsync(roleName);
    }

}
