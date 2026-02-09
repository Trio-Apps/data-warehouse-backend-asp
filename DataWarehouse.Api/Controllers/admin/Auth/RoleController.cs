using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Auth;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleServices _roleServices;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleServices roleServices, ILogger<RoleController> logger)
    {
        _roleServices = roleServices;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]
    public async Task<IActionResult> GetAllRoles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = await _roleServices.GetAllRolesAsync(userId);
        return Ok(roles);
    }
    [HttpGet("{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

    public async Task<ActionResult<IEnumerable<IdentityRole>>> GetAllRolesWithPagination(int skip,int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = await _roleServices.GetAllRolesWithPaginationAsync(userId, skip,pageSize);
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

    public async Task<ActionResult<IdentityRole>> GetRoleById(string id)
    {
        var role = await _roleServices.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound($"Role with ID {id} not found.");

        return Ok(role);
    }

    [HttpGet("name/{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

    public async Task<ActionResult<IdentityRole>> GetRoleByName(string roleName)
    {
        var role = await _roleServices.GetRoleByNameAsync(roleName);
        if (role == null)
            return NotFound($"Role with name '{roleName}' not found.");

        return Ok(role);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Create}")]

    public async Task<ActionResult<IdentityRole>> CreateRole(AddRoleDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _roleServices.CreateRoleAsync(userId, dto.RoleName);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Edit}")]

    public async Task<IActionResult> UpdateRole(string id, UpdateRoleDto role)
    {
        if (id != role.Id)
            return BadRequest("Role ID mismatch.");

        var existingRole = await _roleServices.GetRoleByIdAsync(id);
        if (existingRole == null)
            return NotFound($"Role with ID {id} not found.");

        existingRole.Name = role.RoleName;
        existingRole.NormalizedName = role.RoleName.ToUpper();

        var result = await _roleServices.UpdateRoleAsync(existingRole);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Update is done" });
    }

    [HttpDelete("{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Delete}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        if (!await _roleServices.RoleExistsAsync(roleName))
            return NotFound($"Role '{roleName}' not found.");

        var result = await _roleServices.DeleteRoleAsync(roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }


    [HttpPost("{userId}/assign-role/{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Create}")]
    public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
    {
        if (!await _roleServices.RoleExistsAsync(roleName))
            return NotFound($"Role '{roleName}' not found.");

        var result = await _roleServices.AssignRoleToUserAsync(userId, roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"Role '{roleName}' assigned to user successfully." });
    }

    [HttpDelete("{userId}/remove-role/{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Delete}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
    {
        var result = await _roleServices.RemoveRoleFromUserAsync(userId, roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = $"Role '{roleName}' removed from user successfully." });
    }

    [HttpGet("{userId}/roles")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

    public async Task<ActionResult<IList<string>>> GetUserRoles(string userId)
    {
        var roles = await _roleServices.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("{userId}/in-role/{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

    public async Task<ActionResult<bool>> IsUserInRole(string userId, string roleName)
    {
        var isInRole = await _roleServices.IsUserInRoleAsync(userId, roleName);
        return Ok(new { isInRole });
    }

    [HttpGet("users-in-role/{roleName}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]
    public async Task<ActionResult<IEnumerable<IdentityUser>>> GetUsersInRole(string roleName)
    {
        if (!await _roleServices.RoleExistsAsync(roleName))
            return NotFound($"Role '{roleName}' not found.");

        var users = await _roleServices.GetUsersInRoleAsync(roleName);
        return Ok(users);
    }
}

