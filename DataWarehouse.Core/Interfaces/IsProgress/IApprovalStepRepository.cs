using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.IsProgress;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.IsProgress;

public interface IApprovalStepRepository : IBaseRepository<ApprovalStep>
{
    Task<GeneralResponse<PagedResult<ApprovalStepDto>>> GetApprovalStepsAsync(string userId, int skip, int pageSize);
    Task<GeneralResponse<ApprovalStepDto>> AddApprovalStepAsync(string userId, AddApprovalStepDto dto);
    Task<GeneralResponse<ApprovalStepDto>> UpdateApprovalStepAsync(
    string userId,
    UpdateApprovalStepDto dto);
    Task<IEnumerable<ApprovalStep>> GetByRoleIdAsync(string roleId);
    Task<IEnumerable<ApprovalStep>> GetOrderedStepsAsync();
    Task<ApprovalStep?> GetWithApprovalsAsync(int approvalStepId);
    Task<ApprovalStep?> GetByStepOrderAsync(int stepOrder);
}
