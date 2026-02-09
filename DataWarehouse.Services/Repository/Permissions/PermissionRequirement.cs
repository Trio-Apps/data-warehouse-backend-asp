using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Permissions
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionKey { get; }
        public PermissionRequirement(string permissionKey) => PermissionKey = permissionKey;
    }
}
