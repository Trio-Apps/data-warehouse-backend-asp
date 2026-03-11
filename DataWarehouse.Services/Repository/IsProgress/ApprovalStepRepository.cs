using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Services.Repository.Based;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.IsProgress;

public class ApprovalStepRepository : BaseRepository<ApprovalStep>, IApprovalStepRepository
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly ICompanyCache companyCache;

    

    public ApprovalStepRepository(UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager, ICompanyCache companyCache, DataWarehouseDbContext context) : base(context)
    {
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.companyCache = companyCache;
    }

    public async Task<GeneralResponse<PagedResult<ApprovalStepDto>>> GetApprovalStepsAsync(string userId,int skip,int pageSize)
    {
        var user = await userManager.FindByIdAsync(userId);

       // var roles = await userManager.GetRolesAsync(user);

      //  if (!roles.Contains("admin"))
        //    return GeneralResponse<PagedResult<ApprovalStepDto>>.FailResponse("you can't add any step");

        var companyId = await companyCache.Get();

        var res = await PaginationWithFilterAsync(e => e.CompanyId == companyId, skip, pageSize);
        // ✅ Batch roles lookup (Query واحدة)
       // var roleIds = res.Data.Select(x => x.RoleId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();

        var roleNameMap = await roleManager.Roles
         .Select(r => new { r.Id, r.Name })
         .ToDictionaryAsync(x => x.Id, x => x.Name);

        // ✅ مفيش async هنا لأن RoleName جاي من Dictionary
        var dtoList = res.Data.Select(a => new ApprovalStepDto
        {
            ApprovalStepId = a.ApprovalStepId,
            StepName = a.StepName,
            StepOrder = a.StepOrder,
            RoleId = a.RoleId,
            RoleName = (a.RoleId != null && roleNameMap.TryGetValue(a.RoleId, out var name)) ? name : string.Empty,
            IsFinalStep = a.IsFinalStep,
            CompanyId = a.CompanyId
        }).ToList();

        return GeneralResponse<PagedResult<ApprovalStepDto>>.SuccessResponse(new PagedResult<ApprovalStepDto>
        {
            Data = dtoList,
            PageNumber = res.PageNumber,
            PageSize = res.PageSize,
            TotalRecords = res.TotalRecords
        });

    }

    public async Task<string> GetRoleNameByIdAsync(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return string.Empty;

        var role = await roleManager.FindByIdAsync(roleId);

        return role?.Name ?? string.Empty;
    }
    public async Task<GeneralResponse<ApprovalStepDto>> AddApprovalStepAsync(string userId, AddApprovalStepDto dto)
    {

        var user = await userManager.FindByIdAsync(userId);

        var roles = await userManager.GetRolesAsync(user);

        if (!roles.Contains("admin"))
            return GeneralResponse<ApprovalStepDto>.FailResponse("you can't get any step");


        var companyId = await companyCache.Get();

        var model = new ApprovalStep
        {
            CompanyId = companyId??0,
            IsFinalStep = false,
            RoleId = dto.RoleId,
            StepName = dto.StepName,
            StepOrder = dto.StepOrder,
            ProcessSettingApprovalId = dto.ProcessSettingApprovalId

        };


        var res = await AddAsync(model);

        await SaveChangesAsync();
        return GeneralResponse<ApprovalStepDto>.SuccessResponse(new ApprovalStepDto
        {
            ApprovalStepId = res.ApprovalStepId,
            StepName = res.StepName,
            StepOrder = res.StepOrder,
            RoleId = res.RoleId,
            IsFinalStep = res.IsFinalStep,
            CompanyId = res.CompanyId
        });


    }

    public async Task<GeneralResponse<ApprovalStepDto>> UpdateApprovalStepAsync(
    string userId,
    UpdateApprovalStepDto dto)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return GeneralResponse<ApprovalStepDto>.FailResponse("user not found");

        var roles = await userManager.GetRolesAsync(user);
       
        if (!roles.Contains("admin"))
            return GeneralResponse<ApprovalStepDto>.FailResponse("you can't update any step");


        var companyId = await companyCache.Get();
        var cid = companyId ?? 0;

        // هات الـ step (حسب ال repo اللي عندك)
        var step = await _context.ApprovalSteps.Where(a => a.ApprovalStepId == dto.ApprovalStepId).FirstOrDefaultAsync();

        if (step == null)
            return GeneralResponse<ApprovalStepDto>.FailResponse("step not found");

        
        // Update جزئي
        if (!string.IsNullOrWhiteSpace(dto.StepName))
            step.StepName = dto.StepName.Trim();

        if (!string.IsNullOrWhiteSpace(dto.RoleId))
            step.RoleId = dto.RoleId;


        if (dto.StepOrder.HasValue)
        {
            // (اختياري) منع تكرار StepOrder داخل نفس الشركة
            var orderUsed = await _context.ApprovalSteps.AnyAsync(x =>
                x.CompanyId == cid &&
                x.StepOrder == dto.StepOrder.Value &&
                x.ApprovalStepId != dto.ApprovalStepId
                );

            if (orderUsed)
                return GeneralResponse<ApprovalStepDto>.FailResponse("StepOrder already exists");

            step.StepOrder = dto.StepOrder.Value;
        }

        await SaveChangesAsync();
        
        return GeneralResponse<ApprovalStepDto>.SuccessResponse(new ApprovalStepDto
        {
            ApprovalStepId = step.ApprovalStepId,
            StepName = step.StepName,
            StepOrder = step.StepOrder,
            RoleId = step.RoleId,
            IsFinalStep = step.IsFinalStep,
            CompanyId = step.CompanyId
        });


    }

    public async Task<IEnumerable<ApprovalStep>> GetByRoleIdAsync(string roleId)
    {
        return await Query().Where(a => a.RoleId == roleId).ToListAsync();
    }

    public async Task<IEnumerable<ApprovalStep>> GetOrderedStepsAsync()
    {
        return await Query().OrderBy(a => a.StepOrder).ToListAsync();
    }

    public async Task<ApprovalStep?> GetWithApprovalsAsync(int approvalStepId)
    {
        return await QueryIncluding(false, a => a.ProcessApprovals)
            .FirstOrDefaultAsync(a => a.ApprovalStepId == approvalStepId);
    }

    public async Task<ApprovalStep?> GetByStepOrderAsync(int stepOrder)
    {
        return await Query().FirstOrDefaultAsync(a => a.StepOrder == stepOrder);
    }
}
