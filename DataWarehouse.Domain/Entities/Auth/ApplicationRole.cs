using DataWarehouse.Domain.Entities.AllinAll;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Auth
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName)
        {
            Name = roleName;
            NormalizedName = roleName.ToUpper();
            CreatedAt = DateTime.UtcNow;
        }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}
