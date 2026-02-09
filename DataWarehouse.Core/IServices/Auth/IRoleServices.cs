using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.IServices.Auth;

public interface IRoleServices
{
    Task<GeneralResponse<IdentityResult>> CreateRoleAsync(
     string userId,
     string roleName);
    Task<IdentityResult> UpdateRoleAsync(ApplicationRole role);
    Task<IdentityResult> DeleteRoleAsync(string roleName);
    Task<ApplicationRole?> GetRoleByIdAsync(string roleId);
    Task<ApplicationRole?> GetRoleByNameAsync(string roleName);
    Task<GeneralResponse<IEnumerable<RoleDTO>>> GetAllRolesAsync(string userId);
    Task<GeneralResponse<PagedResult<RoleDTO>>> GetAllRolesWithPaginationAsync(string userId, int pageNumber, int pageSize);
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName);
    Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> IsUserInRoleAsync(string userId, string roleName);
    Task<IEnumerable<IdentityUser>> GetUsersInRoleAsync(string roleName);
}
