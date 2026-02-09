using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Auth
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string Key { get; set; } = default!;        // e.g. "Users.Create"
        public string Name { get; set; } = default!;       // e.g. "Create User"
        public string? Group { get; set; }                 // e.g. "Users"
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
