using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataWarehouse.Services.Repository.IsProgress.ApprovalRepository;

namespace DataWarehouse.Services.Repository.IsProgress
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly DataWarehouseDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApprovalRepository(RoleManager<ApplicationRole> roleManager,
            DataWarehouseDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// بداية عملية الـ Approval لأي Order
        /// </summary>
      
      
        public async Task<int> StartProcessAsync(ProcessType processType, int referenceId, int warehouseId, string userId)
        {
            // ✅ 1. Check if process already exists
            var existingProcess = await _context.ProcessItemIsProgresses
                .FirstOrDefaultAsync(p =>
                    p.ReferenceId == referenceId &&
                    p.ProcessType == processType);

            if (existingProcess != null)
            {
                // ✅ عملية موجودة فعلاً، رجع الـ ID
                return existingProcess.ProcessItemIsProgressId;
            }

            // ✅ 2. Get companyId from warehouse
            var companyId = await _context.Warehouses
                .Where(w => w.WarehouseId == warehouseId)
                .Select(w => w.Sap.CompanyId)
                .FirstOrDefaultAsync();

            // ✅ 3. Load approval steps
            var steps = await _context.ApprovalSteps
                .Where(s => s.IsActive && s.CompanyId == companyId && s.StepOrder > 0)
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            if (!steps.Any())
                throw new Exception("No approval steps defined for this process.");

            // ✅ 4. Create process
            var process = new ProcessItemIsProgress
            {
                ProcessType = processType,
                ReferenceId = referenceId,
                Status = ProcessStatus.InProgress,
                CurrentStepOrder = 1
            };

            _context.ProcessItemIsProgresses.Add(process);
            await _context.SaveChangesAsync();

            // ✅ 5. Add first approval step
            var firstStep = steps.First(s => s.StepOrder == 1);

            var approval = new ProcessApproval
            {
                ApprovalStepId = firstStep.ApprovalStepId,
                Status = ApprovalStatus.Pending,
                ProcessItemIsProgressId = process.ProcessItemIsProgressId,
                WarehouseId = warehouseId,
                UserId = userId,
            };

            _context.ProcessApprovals.Add(approval);
            await _context.SaveChangesAsync();

            return process.ProcessItemIsProgressId;
        }

        /// <summary>
        /// الموافقة على الخطوة الحالية
        /// </summary>
        
        public async Task<GeneralResponse<ProcessApprovalDto>> ApproveStepAsync(int approvalId, string userId, string? comment = null)
        {
            var approval = await _context.ProcessApprovals
                .Include(a => a.ApprovalStep)
                .Include(a => a.ProcessItemIsProgress)
                .FirstOrDefaultAsync(a => a.ProcessApprovalId == approvalId);


            if (approval == null)
                GeneralResponse<ProcessApprovalDto>.FailResponse("Approval step not found.");

            var process = approval.ProcessItemIsProgress;

            // ✅ تحقق إن الخطوة دي هي الخطوة الحالية للـ process
            if (approval.ApprovalStep.StepOrder != process.CurrentStepOrder)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("This step is not the current step of the process.");


            var role = await roleManager.FindByIdAsync(approval.ApprovalStep.RoleId);
            if (role == null)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("Invalid role configured for approval step.");

            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains(role.Name))
                return GeneralResponse<ProcessApprovalDto>.FailResponse("User does not have permission to approve this step.");



            var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
            if (!isInRole)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("User does not have permission to approve this step.");

            
            // Update current approval
            approval.Status = ApprovalStatus.Approved;
            approval.Comment = comment;
            approval.ActionDate = DateTime.UtcNow;
            approval.UserId = userId;

            _context.ProcessApprovals.Update(approval);

             process = approval.ProcessItemIsProgress;
            var companyId = approval.ApprovalStep.CompanyId;

            var currentStepOrder = approval.ApprovalStep.StepOrder;

            var nextStep = await _context.ApprovalSteps
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.IsActive &&
                    s.StepOrder > currentStepOrder &&
                    !s.ProcessApprovals.Any(p =>
                        p.ProcessItemIsProgressId == process.ProcessItemIsProgressId))
                .OrderBy(s => s.StepOrder)
                .FirstOrDefaultAsync();


            if (nextStep != null)
            {
                // Add next approval step
                var nextApproval = new ProcessApproval
                {
                    ApprovalStepId = nextStep.ApprovalStepId,
                    ProcessItemIsProgressId = process.ProcessItemIsProgressId,
                    WarehouseId = approval.WarehouseId,
                    Status = ApprovalStatus.Pending,
                    UserId = userId
                };

                _context.ProcessApprovals.Add(nextApproval);
                process.CurrentStepOrder = nextStep.StepOrder;
            }
            else
            {
                // No more steps -> approve whole process
                process.Status = ProcessStatus.Approved;
                process.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // ✅ رجع الـ DTO المطلوب
            return GeneralResponse<ProcessApprovalDto>.SuccessResponse( new ProcessApprovalDto
            {
                ProcessApprovalId = approval.ProcessApprovalId,
                Status = approval.Status.ToString(),
                Comment = approval.Comment,
                ActionDate = approval.ActionDate,
                CreatedDate = approval.CreatedDate,
                ApprovalStepId = approval.ApprovalStepId,
                ApprovalStep = new ApprovalStepDto
                {
                    ApprovalStepId = approval.ApprovalStep.ApprovalStepId,
                    StepName = approval.ApprovalStep.StepName,
                    StepOrder = approval.ApprovalStep.StepOrder,
                    RoleId = approval.ApprovalStep.RoleId,
                    RoleName = role.Name,
                    IsActive = approval.ApprovalStep.IsActive,
                    IsFinalStep = approval.ApprovalStep.IsFinalStep,
                    CompanyId = approval.ApprovalStep.CompanyId
                },
                UserId = approval.UserId,
                User = approval.User, // أو اعمل map لـ UserDto لو عندك واحد
                WarehouseId = approval.WarehouseId,
                Warehouse = approval.Warehouse, // برضو لو عندك DTO خاص بيه
                ProcessItemIsProgressId = approval.ProcessItemIsProgressId,
                ProcessItemIsProgress = new ProcessItemIsProgressDto
                {
                    ProcessItemIsProgressId = process.ProcessItemIsProgressId,
                    ProcessType = process.ProcessType.ToString(),
                    ReferenceId = process.ReferenceId,
                    CreatedDate = process.CreatedDate,
                    CompletedDate = process.CompletedDate,
                    Status = process.Status.ToString()
                }
            });
        }

        /// <summary>
        /// رفض الخطوة الحالية
        /// </summary>
        public async Task<GeneralResponse<ProcessApprovalDto>> RejectStepAsync(int approvalId, string userId, string? comment = null)
        {
            var approval = await _context.ProcessApprovals
                .Include(a => a.ApprovalStep)
                .Include(a => a.ProcessItemIsProgress)
                .FirstOrDefaultAsync(a => a.ProcessApprovalId == approvalId);

            if (approval == null)
                GeneralResponse<ProcessApprovalDto>.FailResponse("Approval step not found.");


            var process = approval.ProcessItemIsProgress;

            // ✅ تحقق إن الخطوة دي هي الخطوة الحالية للـ process
            if (approval.ApprovalStep.StepOrder != process.CurrentStepOrder)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("This step is not the current step of the process.");


            var role = await roleManager.FindByIdAsync(approval.ApprovalStep.RoleId);
            if (role == null)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("Invalid role configured for approval step.");

            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains(role.Name))
                return GeneralResponse<ProcessApprovalDto>.FailResponse("User does not have permission to approve this step.");



            var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
            if (!isInRole)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("User does not have permission to approve this step.");

            if (approval.Status == ApprovalStatus.Approved)
                return GeneralResponse<ProcessApprovalDto>.FailResponse("This process is approval you can't change to rejected!");

            // تحديث حالة الخطوة إلى Rejected
            approval.Status = ApprovalStatus.Rejected;
            approval.Comment = comment;
            approval.ActionDate = DateTime.UtcNow;
            approval.UserId = userId;

            _context.ProcessApprovals.Update(approval);

            // تحديث حالة العملية بالكامل
             process = approval.ProcessItemIsProgress;
            process.Status = ProcessStatus.Rejected;
            process.CompletedDate = DateTime.UtcNow;

            _context.ProcessItemIsProgresses.Update(process);

            await _context.SaveChangesAsync();
            return GeneralResponse<ProcessApprovalDto>.SuccessResponse(new ProcessApprovalDto
            {
                ProcessApprovalId = approval.ProcessApprovalId,
                Status = approval.Status.ToString(),
                Comment = approval.Comment,
                ActionDate = approval.ActionDate,
                CreatedDate = approval.CreatedDate,
                ApprovalStepId = approval.ApprovalStepId,
                ApprovalStep = new ApprovalStepDto
                {
                    ApprovalStepId = approval.ApprovalStep.ApprovalStepId,
                    StepName = approval.ApprovalStep.StepName,
                    StepOrder = approval.ApprovalStep.StepOrder,
                    RoleId = approval.ApprovalStep.RoleId,
                    RoleName = role.Name,
                    IsActive = approval.ApprovalStep.IsActive,
                    IsFinalStep = approval.ApprovalStep.IsFinalStep,
                    CompanyId = approval.ApprovalStep.CompanyId
                },
                UserId = approval.UserId,
                User = approval.User, // أو اعمل map لـ UserDto لو عندك واحد
                WarehouseId = approval.WarehouseId,
                Warehouse = approval.Warehouse, // برضو لو عندك DTO خاص بيه
                ProcessItemIsProgressId = approval.ProcessItemIsProgressId,
                ProcessItemIsProgress = new ProcessItemIsProgressDto
                {
                    ProcessItemIsProgressId = process.ProcessItemIsProgressId,
                    ProcessType = process.ProcessType.ToString(),
                    ReferenceId = process.ReferenceId,
                    CreatedDate = process.CreatedDate,
                    CompletedDate = process.CompletedDate,
                    Status = process.Status.ToString()
                }
            });
        }

        /// <summary>
        /// Get the basic status of the process without loading all approval details
        /// </summary>
        public async Task<ProcessItemIsProgress?> GetProcessStatusAsync(int processItemId)
        {
            return await _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Select(p => new ProcessItemIsProgress
                {
                    ProcessItemIsProgressId = p.ProcessItemIsProgressId,
                    Status = p.Status,
                    ProcessType = p.ProcessType,
                    ReferenceId = p.ReferenceId,
                    CurrentStepOrder = p.CurrentStepOrder,
                    CreatedDate = p.CreatedDate,
                    CompletedDate = p.CompletedDate
                })
                .FirstOrDefaultAsync(p => p.ProcessItemIsProgressId == processItemId);
        }


        /// جيب كل الـ Approvals اللي مستنية موافقة اليوزر ده (حسب الرول والمستودعات المرتبطة بيه)
        /// </summary>

        public async Task<GeneralResponse<PagedResult<ProcessApprovalDto>>> GetPendingApprovalsForUserAsync(
        string userId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // 1) user + warehouses
            var user = await _userManager.Users
                .Include(u => u.UserWarehouses)
                
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return GeneralResponse<PagedResult<ProcessApprovalDto>>.FailResponse("user id not found");

            // 2) role ids
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();


            // 3) warehouse ids
            var warehouseIds = user.UserWarehouses.Select(uw => uw.WarehouseId).ToList();

            // 4) base query (بدون ToList)
            var query = _context.ProcessApprovals
                .AsNoTracking()
                .Include(a => a.ApprovalStep)
                .Include(a => a.ProcessItemIsProgress)
                .Where(a =>
                    a.Status == ApprovalStatus.Pending &&
                    a.ProcessItemIsProgress.Status == ProcessStatus.InProgress &&
                    a.ProcessItemIsProgress.CurrentStepOrder == a.ApprovalStep.StepOrder &&
                    roleIds.Contains(a.ApprovalStep.RoleId) &&


            warehouseIds.Contains(a.WarehouseId));


            // 5) total count قبل pagination
            var totalRecords = await query.CountAsync();

            // 6) apply pagination في DB
            var pageData = await query
                .OrderBy(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleNameMap = await roleManager.Roles
       .Select(r => new { r.Id, r.Name })
       .ToDictionaryAsync(x => x.Id, x => x.Name);

            // 8) map to DTOs
            var result = pageData.Select(a => new ProcessApprovalDto
            {
                ProcessApprovalId = a.ProcessApprovalId,
                Status = a.Status.ToString(),
                Comment = a.Comment,
                ActionDate = a.ActionDate,
                CreatedDate = a.CreatedDate,
                ApprovalStepId = a.ApprovalStepId,
                UserId = a.UserId,
                User = a.User,
                WarehouseId = a.WarehouseId,
                Warehouse = a.Warehouse,
                ProcessItemIsProgressId = a.ProcessItemIsProgressId,

                ApprovalStep = new ApprovalStepDto
                {
                    ApprovalStepId = a.ApprovalStep.ApprovalStepId,
                    StepName = a.ApprovalStep.StepName,
                    StepOrder = a.ApprovalStep.StepOrder,
                    RoleId = a.ApprovalStep.RoleId,
                    RoleName = (a.ApprovalStep.RoleId != null && roleNameMap.TryGetValue(a.ApprovalStep.RoleId, out var name)) ? name : string.Empty,
                    IsActive = a.ApprovalStep.IsActive,
                    IsFinalStep = a.ApprovalStep.IsFinalStep,
                    CompanyId = a.ApprovalStep.CompanyId
                },

                ProcessItemIsProgress = new ProcessItemIsProgressDto
                {
                    ProcessItemIsProgressId = a.ProcessItemIsProgress.ProcessItemIsProgressId,
                    ReferenceId = a.ProcessItemIsProgress.ReferenceId,
                    CreatedDate = a.ProcessItemIsProgress.CreatedDate,
                    CompletedDate = a.ProcessItemIsProgress.CompletedDate,
                    Status = a.ProcessItemIsProgress.Status.ToString(),
                    ProcessType = a.ProcessItemIsProgress.ProcessType.ToString()
                }
            }).ToList();

            // 9) wrap in paged result
            var paged = new PagedResult<ProcessApprovalDto>
            {
                Data = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };

            return GeneralResponse<PagedResult<ProcessApprovalDto>>.SuccessResponse(paged);
        }


        /// <summary>
        /// تأكد إن اليوزر يقدر يعمل Approve
        /// </summary>
        public async Task<bool> CanUserApproveAsync(int processItemId, string userId)
        {
            var processItem = await _context.ProcessItemIsProgresses
                .Include(p => p.ProcessApprovals)
                    .ThenInclude(a => a.ApprovalStep)
                .FirstOrDefaultAsync(p => p.ProcessItemIsProgressId == processItemId);

            if (processItem == null || processItem.Status != ProcessStatus.InProgress)
                return false;

            // جيب الخطوة الحالية
            var currentApproval = processItem.ProcessApprovals
                .FirstOrDefault(a => a.ApprovalStep.StepOrder == processItem.CurrentStepOrder);

            if (currentApproval == null)
                return false;

            // تأكد إن اليوزر في الـ Role المطلوب
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var isInRole = await _userManager.IsInRoleAsync(user,
                await _context.Roles
                    .Where(r => r.Id == currentApproval.ApprovalStep.RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync() ?? "");

            return isInRole;
        }
        public async Task<ApprovalAccessResult> CheckUserCanApproveAsync(string userId, ProcessType processType, int referenceId)
        {
            var result = new ApprovalAccessResult
            {
                CanApprove = false,
                ProcessItemIsProgressId = null,
                ProcessApprovalId = null,
                Reason = "Unknown"
            };

            // 1. Get the process
            var process = await _context.ProcessItemIsProgresses
                .FirstOrDefaultAsync(p =>
                    p.ProcessType == processType &&
                    p.ReferenceId == referenceId);

            if (process == null)
            {
                result.Reason = "Process not found.";
                return result;
            }

            result.ProcessItemIsProgressId = process.ProcessItemIsProgressId;

            //if (process.Status != ProcessStatus.InProgress)
            //{
            //    result.Reason = $"Process is not in progress. Current status: {process.Status}";
            //    return result;
            //}

            // 2. Get current approval step (pending)
            var currentApproval = await _context.ProcessApprovals
                .Include(a => a.ApprovalStep)
                .Where(a =>
                    a.ProcessItemIsProgressId == process.ProcessItemIsProgressId &&
                    a.ApprovalStep.StepOrder == process.CurrentStepOrder)
                .FirstOrDefaultAsync();

            if (currentApproval == null)
            {
                result.Reason = "No pending approval step found.";
                return result;
            }

            result.ProcessApprovalId = currentApproval.ProcessApprovalId;

            // 3. Get the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                result.Reason = "User not found.";
                return result;
            }

            // 4. Get role name for current step
            var roleName = await _context.Roles
                .Where(r => r.Id == currentApproval.ApprovalStep.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleName))
            {
                result.Reason = "Role not found for current step.";
                return result;
            }

            // 5. Check if user is in role
            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                result.Reason = $"User is not in required role: {roleName}";
                return result;
            }

            // ✅ Passed all checks
            result.CanApprove = true;
            result.Reason = "User can approve.";

            return result;
        }

    }
}

