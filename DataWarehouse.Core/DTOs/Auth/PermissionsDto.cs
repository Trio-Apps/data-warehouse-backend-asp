using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Auth
{
    public class PermissionsDto
    {
        public int PermissionId { get; set; }
        public string Key { get; set; } = default!;        // e.g. "Users.Create"
        public string Name { get; set; } = default!;       // e.g. "Create User"
        public string? Group { get; set; }                 // e.g. "Users"
        public string? Description { get; set; }
    }

    public sealed class CreateRoleWithPermissionsDto
    {
        public string RoleName { get; set; } = default!;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public sealed class RoleWithPermissionsDto
    {
        public string RoleId { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public string? Description { get; set; }
        public List<PermissionsDto> Permissions { get; set; } = new();
    }
    public sealed class UpdateRolePermissionsDto
    {
        public string RoleId { get; set; } = default!;
        public string? RoleName { get; set; } = default!;
        public List<int>? PermissionIds { get; set; } = new();
    }
    public sealed class PermissionForRoleDto
    {
        public int PermissionId { get; set; }
        public string Key { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Group { get; set; }

        public bool IsSelected { get; set; }
    }


}
