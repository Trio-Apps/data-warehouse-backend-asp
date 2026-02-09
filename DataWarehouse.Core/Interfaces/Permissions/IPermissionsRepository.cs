using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Permissions
{
    public interface IPermissionsRepository
    {
        Task<GeneralResponse<IEnumerable<PermissionsDto>>> GetPermissionsAsync();
        Task<GeneralResponse<RoleWithPermissionsDto>> CreateRoleWithPermissionsAsync(string userId, CreateRoleWithPermissionsDto dto);
        Task<GeneralResponse<RoleWithPermissionsDto>> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto);
        Task<GeneralResponse<IEnumerable<PermissionForRoleDto>>> GetPermissionsForRoleAsync(string roleId);
        Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId);
    }
}
