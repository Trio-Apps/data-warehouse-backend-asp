using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataWarehouse.Services.Services.Auth;

public class UserServices : IUserServices
{
    private readonly IAuthServices authServices;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserServices> logger;
    private readonly ISapCache sapCache;
    private readonly ICompanyCache companyCache;
    private readonly DataWarehouseDbContext context;

    public UserServices(IAuthServices authServices, UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager,ILogger<UserServices> logger
        ,ISapCache sapCache
        ,ICompanyCache companyCache
        ,DataWarehouseDbContext context)
    {
        this.authServices = authServices;
        _userManager = userManager;
        _roleManager = roleManager;
        this.logger = logger;
        this.sapCache = sapCache;
        this.companyCache = companyCache;
        this.context = context;

    }
    public async Task<GeneralResponse<UserDTO>> CreateUserAsync(string userId,AddUserDTO userDto)
    {
     //   var sapId = await sapCache.Get();
      //  var companyId = await companyCache.Get();

        // 1️⃣ Check Email Exists
        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            return GeneralResponse<UserDTO>.FailResponse("Email already exists");
        }

        if (!string.IsNullOrWhiteSpace(userDto.RoleName))
        {
            if (!await _roleManager.RoleExistsAsync(userDto.RoleName))
            {
             //   logger.LogWarning("Role '{RoleName}' does not exist", userDto.RoleName);
                return GeneralResponse<UserDTO>.FailResponse(
                    $"Role '{userDto.RoleName}' does not exist"
                );
            }
        }
    

            // 2️⃣ Create ApplicationUser
           var user = new ApplicationUser
             {
            UserName = userDto.Email,
            Email = userDto.Email,
            PhoneNumber = userDto.PhoneNumber,
            FullName = userDto.FullName
            };


        // 3️⃣ Create User with Password
        var createResult = await _userManager.CreateAsync(user, userDto.Password);

        if (!createResult.Succeeded)
        {
            return GeneralResponse<UserDTO>.FailResponse("Operation Failed",
                createResult.Errors.Select(e => e.Description).ToList()
            );
        }

       
        
        var checkTypeUser = await authServices.GetLoginContextForUserAsync(userId);

        // 1) هات الرول الصح بالاسم + الشركة
        var role = await context.Roles.FirstOrDefaultAsync(r =>
            r.Name == userDto.RoleName &&
            r.CompanyId == checkTypeUser);

        if (role is null)
        {
            return GeneralResponse<UserDTO>.FailResponse(
                $"Role '{userDto.RoleName}' not found for this company.");
        }

        // 2) تأكد إن اليوزر مش واخد الرول قبل كده
        var alreadyAssigned = await context.UserRoles.AnyAsync(ur =>
            ur.UserId == user.Id &&
            ur.RoleId == role.Id);

        if (!alreadyAssigned)
        {
            // 3) اربط اليوزر بالرول بالـ RoleId
            context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role.Id
            });

        }

        // Success (مفيش Errors زي IdentityResult)







            if (checkTypeUser == null)
            {
            var sapModel = new CompanyUser
            {
                CompanyId = userDto.CompanyId??0,
                UserId = user.Id
            };
            var sap = await context.CompanyUsers.AddAsync(sapModel);
           }
          
        else
        {
            var companyId = checkTypeUser;
            var sapModel = new CompanyUser
            {
                CompanyId = companyId ?? 0,
                UserId = user.Id
            };
            await context.CompanyUsers.AddAsync(sapModel);

            var sapIds = userDto.SapIds?.Distinct().ToList() ?? new List<int>();
            if (sapIds.Count > 0)
            {
                var sapModels = sapIds.Select(e => new SapUser
                {
                    SapId = e,
                    UserId = user.Id

                });

                await context.SapUsers.AddRangeAsync(sapModels);
            }

            var warehouseIds = userDto.WarehouseIds?.Distinct().ToList() ?? new List<int>();
            if (warehouseIds.Count > 0)
            {
                var warehouseModels = warehouseIds.Select(e => new UserWarehouses
                {
                    WarehouseId = e,
                    UserId = user.Id

                });

                await context.UserWarehouses.AddRangeAsync(warehouseModels);
            }
           }
       
      

        await context.SaveChangesAsync();
        // 5️⃣ Prepare Response DTO
        var roles = await _userManager.GetRolesAsync(user);

        var response = new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
        
            Roles = roles
        };

        return GeneralResponse<UserDTO>.SuccessResponse(response);
    }

    public async Task<bool> IsRoleNotAdminOrManagerOrSuperAdmin(string roleName)
    {
        var allowedRoles = new[] { "admin", "manager", "super-admin" };

        return !allowedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
    public async Task<GeneralResponse<UserDTO>> UpdateUserAsync(string userId, UpdateUserDto userDto)
    {
        // 1️⃣ Get User
        var user = await _userManager.FindByIdAsync(userDto.Id);
        if (user == null)
            return GeneralResponse<UserDTO>.FailResponse("User not found");

        // 2️⃣ Update Basic Info
        user.FullName = userDto.FullName;
        user.PhoneNumber = userDto.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(userDto.Email) &&
            user.Email != userDto.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(userDto.Email);
            if (emailExists != null && emailExists.Id != user.Id)
                return GeneralResponse<UserDTO>.FailResponse("Email already exists");

            user.Email = userDto.Email;
            user.UserName = userDto.Email;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return GeneralResponse<UserDTO>.FailResponse(
                "Update failed",
                updateResult.Errors.Select(e => e.Description).ToList()
            );
        }

        // 3️⃣ Update Role
        if (!string.IsNullOrWhiteSpace(userDto.RoleName))
        {
            if (!await _roleManager.RoleExistsAsync(userDto.RoleName))
                return GeneralResponse<UserDTO>.FailResponse(
                    $"Role '{userDto.RoleName}' does not exist"
                );

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, userDto.RoleName);
        }

        // 4️⃣ Remove OLD Relations (cleanup)
        context.CompanyUsers.RemoveRange(
            context.CompanyUsers.Where(x => x.UserId == user.Id));

        context.SapUsers.RemoveRange(
            context.SapUsers.Where(x => x.UserId == user.Id));

      

        context.UserWarehouses.RemoveRange(
            context.UserWarehouses.Where(x => x.UserId == user.Id));


        var checkTypeUser = await authServices.GetLoginContextForUserAsync(userId);


        if (checkTypeUser == null)
        {

            var sapModel = new CompanyUser
            {
                CompanyId = userDto.CompanyId ?? 0,
                UserId = user.Id
            };
            var sap = await context.CompanyUsers.AddAsync(sapModel);
        }
       
        else
        {
            var sapIds = userDto.SapIds?.Distinct().ToList() ?? new List<int>();
            if (sapIds.Count > 0)
            {
                var sapModels = sapIds.Select(e => new SapUser
                {
                    SapId = e,
                    UserId = user.Id

                });

                await context.SapUsers.AddRangeAsync(sapModels);
            }

            var warehouseIds = userDto.WarehouseIds?.Distinct().ToList() ?? new List<int>();
            if (warehouseIds.Count > 0)
            {
                var warehouseModels = warehouseIds.Select(e => new UserWarehouses
                {
                    WarehouseId = e,
                    UserId = user.Id

                });

                await context.UserWarehouses.AddRangeAsync(warehouseModels);
            }
        }

        await context.SaveChangesAsync();

        // 6️⃣ Prepare Response
        var roles = await _userManager.GetRolesAsync(user);

        var response = new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles
        };

        return GeneralResponse<UserDTO>.SuccessResponse(response);
    }

    public async Task<IdentityResult> DeleteUserAsync(ApplicationUser user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {


        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return _userManager.Users.ToList();
    }

    public async Task<GeneralResponse<PagedResult<GetUserDTO>>>
       GetAllUsersWithPaginationAsync(string userId, int pageNumber, int pageSize, int? companyIdDto,
         int? sapIdDto,
         string? email,
         string? fullName)
    {


        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var user1 = await _userManager.FindByIdAsync(userId);
        var roles1 = await _userManager.GetRolesAsync(user1);

        var query = _userManager.Users;
        if (roles1.Contains("super-admin"))
        {
            var companyId = companyIdDto ?? await companyCache.Get();
           
            query = query
              .Where(u => u.CompanyUser.CompanyId == companyId)
              .AsNoTracking();
        }
        else 
        {
            var sapId = sapIdDto ?? await sapCache.Get();

            query = query.Where(u => u.UserSaps.Any(e => e.SapId == sapId))
         .AsNoTracking()
         .OrderBy(u => u.UserName); // مهم مع Pagination
        }
        //else if (roles1.Contains("manager"))
        //{
        //  var sapId = sapIdDto ?? await sapCache.Get();

        //  query = query.Where(u => u.SapEmployee.SapId == sapId  )
        // .AsNoTracking()
        // .OrderBy(u => u.UserName); // مهم مع Pagination
        //}

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(iw =>
                iw.Email.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(fullName))
        {
            query = query.Where(iw => iw.FullName.Contains(fullName));
        }

        var totalRecords = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new GetUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyId = user.CompanyUser.CompanyId,
              //  SapEmployeeId = user.SapEmployee.SapId,
                SapIds = user.UserSaps.Select(e => e.SapId).ToList(),
                WarehouseIds = user.UserWarehouses.Select(w => w.WarehouseId).ToList()
            })
            .ToListAsync();

        // 🔹 تحميل Roles (بأقل تكلفة ممكنة)
        var result = new List<GetUserDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(
                new ApplicationUser { Id = user.Id });

            result.Add(new GetUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                CompanyId = user.CompanyId,
                SapEmployeeId = user.SapEmployeeId,
                SapIds = user.SapIds,
                WarehouseIds = user.WarehouseIds
            });
        }


        return GeneralResponse<PagedResult<GetUserDTO>>.SuccessResponse(
            new PagedResult<GetUserDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = result
            });
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user != null;
    }

    public async Task<IdentityResult> UpdateUserEmailAsync(ApplicationUser user, string newEmail, string token)
    {
        return await _userManager.ChangeEmailAsync(user, newEmail, token);
    }

    public async Task<string> GenerateEmailChangeTokenAsync(ApplicationUser user, string newEmail)
    {
        return await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public async Task<IdentityResult> SetPhoneNumberAsync(ApplicationUser user, string phoneNumber)
    {
        return await _userManager.SetPhoneNumberAsync(user, phoneNumber);
    }

    public async Task<string> GeneratePhoneNumberTokenAsync(ApplicationUser user, string phoneNumber)
    {
        return await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
    }

    public async Task<IdentityResult> ChangePhoneNumberAsync(ApplicationUser user, string phoneNumber, string token)
    {
        return await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);
    }

    public async Task<bool> IsPhoneNumberConfirmedAsync(ApplicationUser user)
    {
        return await _userManager.IsPhoneNumberConfirmedAsync(user);
    }

    public async Task<IdentityResult> SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
    {
        return await _userManager.SetTwoFactorEnabledAsync(user, enabled);
    }

    public async Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
    {
        return await _userManager.GetTwoFactorEnabledAsync(user);
    }
}
