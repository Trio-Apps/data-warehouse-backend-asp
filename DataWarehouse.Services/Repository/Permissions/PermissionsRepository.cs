using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Permissions;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Services.Repository.Based;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Permissions
{
    public class PermissionsRepository : BaseRepository<Permission>, IPermissionsRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PermissionsRepository( UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, DataWarehouseDbContext context) : base(context)
        {
            this.userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<GeneralResponse<IEnumerable<PermissionsDto>>> GetPermissionsAsync()
        {
            var perms = await _context.Permissions.Select(p => new PermissionsDto
            {
                PermissionId = p.PermissionId,
                Description = p.Description,
                Group = p.Group,
                Key = p.Key,
                Name = p.Name

            }).ToListAsync();

            return GeneralResponse<IEnumerable<PermissionsDto>>.SuccessResponse(perms);
        }

        public async Task<GeneralResponse<RoleWithPermissionsDto>> CreateRoleWithPermissionsAsync(string userId, CreateRoleWithPermissionsDto dto)
        {
            dto.PermissionIds ??= new List<int>();
            dto.PermissionIds = dto.PermissionIds.Distinct().ToList();

            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return GeneralResponse<RoleWithPermissionsDto>.FailResponse("Role name is required.");

            // لو عندك Convention: خلي أسماء الرول lower-case مثلا
            var roleName = dto.RoleName.Trim();

         
            // 2) Validate permissions IDs
            var existingPermissionIds = await _context.Permissions
                .Where(p => dto.PermissionIds.Contains(p.PermissionId))
                .Select(p => p.PermissionId)
                .ToListAsync();

            var missingIds = dto.PermissionIds.Except(existingPermissionIds).ToList();
            if (missingIds.Count > 0)
                return GeneralResponse<RoleWithPermissionsDto>.FailResponse($"Invalid PermissionIds: {string.Join(", ", missingIds)}");

            // 3) Transaction (عشان لو فشل إنشاء الرول أو الربط.. ما يحصلش half data)
            await using var tx = await _context.Database.BeginTransactionAsync();



            // 1️⃣ نجيب CompanyId لو موجود
            int? companyId = await _context.Set<CompanyUser>()
                .Where(cu => cu.UserId == userId)
                .Select(cu => (int?)cu.CompanyId)
                .FirstOrDefaultAsync();

            // 2️⃣ Check role already exists (same scope)
            bool roleExists = await _context.Roles
                .AnyAsync(r =>
                    r.Name == roleName &&
                    r.CompanyId == companyId);

            if (roleExists)
                return GeneralResponse<RoleWithPermissionsDto>
                    .FailResponse($"Role '{roleName}' already exists.");
            // 4) Create Role via RoleManager (الأصح مع Identity)
            var role = new ApplicationRole
            {
                Name = roleName,
               // NormalizedName = roleName.ToUpperInvariant(),
                Description = dto.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CompanyId = companyId,
            };


            var createResult = await _roleManager.CreateAsync(role);
            if (!createResult.Succeeded)
            {
                await tx.RollbackAsync();
                var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                return GeneralResponse<RoleWithPermissionsDto>.FailResponse($"Failed to create role: {errors}");
            }

            // 5) Insert RolePermissions
            if (dto.PermissionIds.Count > 0)
            {
                var rolePerms = dto.PermissionIds.Select(pid => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid
                });

                await _context.RolePermissions.AddRangeAsync(rolePerms);
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();

            // 6) Return created role with permissions
            var permissions = await _context.Permissions
                .Where(p => dto.PermissionIds.Contains(p.PermissionId))
                .Select(p => new PermissionsDto
                {
                    PermissionId = p.PermissionId,
                    Description = p.Description,
                    Group = p.Group,
                    Key = p.Key,
                    Name = p.Name
                })
                .ToListAsync();

            var resultDto = new RoleWithPermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                Description = role.Description,
                Permissions = permissions
            };

            return GeneralResponse<RoleWithPermissionsDto>.SuccessResponse(resultDto);
        }
        public async Task<GeneralResponse<RoleWithPermissionsDto>> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleId))
                return GeneralResponse<RoleWithPermissionsDto>.FailResponse("RoleId is required.");

            // 1) تأكد الرول موجود
            var role = await _roleManager.FindByIdAsync(dto.RoleId);
            if (role is null)
                return GeneralResponse<RoleWithPermissionsDto>.FailResponse("Role not found.");

            // 2) تحديث اسم الرول (لو اتبعت)
            if (!string.IsNullOrWhiteSpace(dto.RoleName))
            {
                var newName = dto.RoleName.Trim();
                var normalizedNewName = newName.ToUpperInvariant();

                // منع التكرار في نفس الـ scope (CompanyId)
                var nameExists = await _context.Roles.AnyAsync(r =>
                    r.Id != role.Id &&
                    r.NormalizedName == normalizedNewName &&
                    r.CompanyId == role.CompanyId);

                if (nameExists)
                    return GeneralResponse<RoleWithPermissionsDto>.FailResponse($"Role name '{newName}' already exists.");

                role.Name = newName;
               // role.NormalizedName = normalizedNewName;

                var updateRes = await _roleManager.UpdateAsync(role);
                if (!updateRes.Succeeded)
                    return GeneralResponse<RoleWithPermissionsDto>.FailResponse("Failed to update role name.");
            }

            // 3) تحديث Permissions (لو PermissionIds اتبعتت)
            if (dto.PermissionIds is not null)
            {
                dto.PermissionIds = dto.PermissionIds.Distinct().ToList();

                // Validate ids (لو اللست مش فاضية)
                if (dto.PermissionIds.Count > 0)
                {
                    var existingPermissionIds = await _context.Permissions
                        .Where(p => dto.PermissionIds.Contains(p.PermissionId))
                        .Select(p => p.PermissionId)
                        .ToListAsync();

                    var missingIds = dto.PermissionIds.Except(existingPermissionIds).ToList();
                    if (missingIds.Count > 0)
                        return GeneralResponse<RoleWithPermissionsDto>.FailResponse(
                            $"Invalid PermissionIds: {string.Join(", ", missingIds)}");
                }

                // امسح القديم
                var oldLinks = await _context.RolePermissions
                    .Where(rp => rp.RoleId == dto.RoleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(oldLinks);

                // اضف الجديد
                if (dto.PermissionIds.Count > 0)
                {
                    var newLinks = dto.PermissionIds.Select(pid => new RolePermission
                    {
                        RoleId = dto.RoleId,
                        PermissionId = pid
                    });

                    await _context.RolePermissions.AddRangeAsync(newLinks);
                }

                await _context.SaveChangesAsync();
            }

            // 4) رجّع role + permissions الحالية (سواء اتعدلت أو لا)
            var currentPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == dto.RoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var permissions = await _context.Permissions
                .Where(p => currentPermissionIds.Contains(p.PermissionId))
                .Select(p => new PermissionsDto
                {
                    PermissionId = p.PermissionId,
                    Description = p.Description,
                    Group = p.Group,
                    Key = p.Key,
                    Name = p.Name
                })
                .ToListAsync();

            var result = new RoleWithPermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                Description = role.Description,
                Permissions = permissions
            };

            return GeneralResponse<RoleWithPermissionsDto>.SuccessResponse(result);
        }
        public async Task<GeneralResponse<IEnumerable<PermissionForRoleDto>>> GetPermissionsForRoleAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return GeneralResponse<IEnumerable<PermissionForRoleDto>>.FailResponse("roleId is required.");

            // تأكد إن الرول موجود (اختياري بس مفيد)
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
                return GeneralResponse<IEnumerable<PermissionForRoleDto>>.FailResponse("Role not found.");

            // PermissionIds المرتبطة بالرول
            var selectedIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // كل permissions + IsSelected
            var result = await _context.Permissions
                .OrderBy(p => p.Group)
                .ThenBy(p => p.Name)
                .Select(p => new PermissionForRoleDto
                {
                    PermissionId = p.PermissionId,
                    Key = p.Key,
                    Name = p.Name,
                    Description = p.Description,
                    Group = p.Group,
                    IsSelected = selectedIds.Contains(p.PermissionId)
                })
                .ToListAsync();

            return GeneralResponse<IEnumerable<PermissionForRoleDto>>.SuccessResponse(result);
        }
        public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            var roleNames = await userManager.GetRolesAsync(user);

            // هات RoleIds من الأسماء
            var roleIds = await _context.Roles
                .Where(r => roleNames.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var perms = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Key)
                .Distinct()
                .ToListAsync();

            return perms;
        }
   
    }
}
