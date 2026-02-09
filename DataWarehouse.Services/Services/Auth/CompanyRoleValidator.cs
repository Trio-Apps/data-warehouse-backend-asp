using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Auth
{

    public sealed class CompanyRoleValidator : IRoleValidator<ApplicationRole>
    {
        private readonly DataWarehouseDbContext _context;

        public CompanyRoleValidator(DataWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
        {
            if (role is null)
                return IdentityResult.Failed(new IdentityError { Description = "Role is null." });

            if (string.IsNullOrWhiteSpace(role.Name))
                return IdentityResult.Failed(new IdentityError { Description = "Role name is required." });

            // نفس Normalization بتاع Identity
            var normalized = manager.NormalizeKey(role.Name);

            // ✅ uniqueness داخل نفس الشركة فقط
            var exists = await _context.Roles.AnyAsync(r =>
                r.Id != role.Id &&
                r.CompanyId == role.CompanyId &&
                r.NormalizedName == normalized);

            if (exists)
                return IdentityResult.Failed(new IdentityError { Description = $"Role name '{role.Name}' already exists in this company." });

            // ✅ لو Global role (CompanyId null) يبقى Unique global
            if (role.CompanyId is null)
            {
                var globalExists = await _context.Roles.AnyAsync(r =>
                    r.Id != role.Id &&
                    r.CompanyId == null &&
                    r.NormalizedName == normalized);

                if (globalExists)
                    return IdentityResult.Failed(new IdentityError { Description = $"Global role name '{role.Name}' already exists." });
            }

            return IdentityResult.Success;
        }
    }
}
