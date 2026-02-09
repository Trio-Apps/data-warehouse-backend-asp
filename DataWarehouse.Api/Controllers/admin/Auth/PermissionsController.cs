using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Permissions;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionsRepository repository;

        public PermissionsController(IPermissionsRepository repository )
        {
            this.repository = repository;
        }

        [HttpGet]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]

        public async Task<IActionResult> GetAllPermissions()
        {
            var users = await repository.GetPermissionsAsync();
            return Ok(users);
        }

        // GET: api/permissions/role/{roleId}
        // يرجّع كل permissions ومعاهم IsSelected = true/false حسب الرول
        [HttpGet("role/{roleId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Get}")]
        public async Task<IActionResult> GetPermissionsForRole([FromRoute] string roleId)
        {
            var result = await repository.GetPermissionsForRoleAsync(roleId);
            return Ok(result);
        }

        // POST: api/permissions/roles
        // إنشاء رول جديد + ربط permissions
        [HttpPost("roles")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Create}")]

        public async Task<IActionResult> CreateRoleWithPermissions([FromBody] CreateRoleWithPermissionsDto dto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await repository.CreateRoleWithPermissionsAsync(userId, dto);
         
            return Ok(result);
        }

        // PUT: api/permissions/roles/{roleId}/permissions
        // تعديل صلاحيات رول موجود (استبدال/Update)
        [HttpPut("roles/{roleId}/permissions")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Roles_Edit}")]
        public async Task<IActionResult> UpdateRolePermissions([FromRoute] string roleId, [FromBody] UpdateRolePermissionsDto dto)
        {
            // ضمان إن roleId اللي في الRoute هو اللي هيتستخدم
            dto.RoleId = roleId;

            var result = await repository.UpdateRolePermissionsAsync(dto);
            return Ok(result);
        }
   
    
    }
}
