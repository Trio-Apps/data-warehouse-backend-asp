using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Auth
{
    public class RolePermission
    {
        public string RoleId { get; set; } = default!;
        public ApplicationRole Role { get; set; } = default!;
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;
    }

}
