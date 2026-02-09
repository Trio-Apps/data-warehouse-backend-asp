using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Permissions
{

    public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataWarehouseDbContext _db;

        public PermissionHandler(UserManager<ApplicationUser> userManager, DataWarehouseDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
                return;

            // Get RoleIds
            var roleIds = await _db.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            // Check permission exists for any role
            var has = await _db.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .AnyAsync(rp => rp.Permission.Key == requirement.PermissionKey);

            if (has)
                context.Succeed(requirement);
        }
    }
    }
