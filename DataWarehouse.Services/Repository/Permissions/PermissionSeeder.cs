using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Permissions
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            //   await db.Database.MigrateAsync();

            // 1) Seed Permissions
            var existingKeys = await db.Permissions.Select(p => p.Key).ToListAsync();
            var missing = AppPermissions.All.Where(x => !existingKeys.Contains(x.Key)).ToList();

            if (missing.Count > 0)
            {
                db.Permissions.AddRange(missing.Select(x => new Permission
                {
                    Key = x.Key,
                    Name = x.Name,
                    Group = x.Group
                }));
                await db.SaveChangesAsync();
            }


            // 2) Seed Roles
            async Task<ApplicationRole> EnsureRole(string name, string desc)
            {
                var role = await roleManager.FindByNameAsync(name);
                if (role != null) return role;

                role = new ApplicationRole { Name = name, Description = desc };
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

                return role;
            }


            var adminRole = await EnsureRole("admin", "Company Admin");
            var superRole = await EnsureRole("super-admin", "System Super Admin");

            // 3) Assign permissions to roles (DB join table)
            var permissions = await db.Permissions.ToListAsync();

            async Task GrantAll(ApplicationRole role)
            {
                var roleId = role.Id;

                var toAdd = permissions
                    .Where(p => !db.RolePermissions.Any(rp => rp.RoleId == roleId && rp.PermissionId == p.PermissionId))
                    .Select(p => new RolePermission { RoleId = roleId, PermissionId = p.PermissionId })
                    .ToList();

                if (toAdd.Count > 0)
                {
                    db.RolePermissions.AddRange(toAdd);
                    await db.SaveChangesAsync();
                }

            }



            // مثال: SuperAdmin ياخد كل حاجة
            await GrantAll(superRole);

            // Admin ياخد مجموعة معينة (مثال)
            // var adminPermKeys = new[] { AppPermissions.Users_Create, AppPermissions.Users_Edit };
            var adminPermKeys = new List<string>();
            var adminPerms = permissions.Where(p => !adminPermKeys.Contains(p.Key)).ToList();

            foreach (var p in adminPerms)
            {
                var exists = await db.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == p.PermissionId);

                if (!exists)
                    db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.PermissionId });
            }

            await db.SaveChangesAsync();
        }
    }
}