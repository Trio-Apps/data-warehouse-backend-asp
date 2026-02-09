using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.IsProgress;

public interface IProcessApprovalRepository : IBaseRepository<ProcessApproval>
{
    Task<IEnumerable<ProcessApproval>> GetByProcessItemIsProgressIdAsync(int processItemIsProgressId);
    Task<IEnumerable<ProcessApproval>> GetByUserIdAsync(string userId);
    Task<IEnumerable<ProcessApproval>> GetByApprovalStepIdAsync(int approvalStepId);
    Task<IEnumerable<ProcessApproval>> GetByStatusAsync(ApprovalStatus status);
    Task<ProcessApproval?> GetWithUserAsync(int processApprovalId);
    Task<ProcessApproval?> GetWithApprovalStepAsync(int processApprovalId);
    Task<IEnumerable<ProcessApproval>> GetPendingApprovalsAsync();
}
