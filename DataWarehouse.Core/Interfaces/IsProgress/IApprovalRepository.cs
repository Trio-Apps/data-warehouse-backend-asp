using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.IsProgress
{
    public interface IApprovalRepository
    {
        Task<int> StartProcessAsync(ProcessType processType, int referenceId, int warehouseId, string userId);
        Task<GeneralResponse<ProcessApprovalDto>> ApproveStepAsync(int approvalId, string userId, string? comment = null);
        Task<GeneralResponse<ProcessApprovalDto>> RejectStepAsync(int approvalId, string userId, string? comment = null);
        Task<ProcessItemIsProgress?> GetProcessStatusAsync(int processItemId);
        Task<GeneralResponse<PagedResult<ProcessApprovalDto>>> GetPendingApprovalsForUserAsync(
         string userId, int pageNumber, int pageSize);
        Task<bool> CanUserApproveAsync(int processItemId, string userId);
        Task<ApprovalAccessResult> CheckUserCanApproveAsync(string userId, ProcessType processType, int referenceId);
        Task<ProcessItemIsProgress> GetProcessItem(int OrderId, ProcessType type, CancellationToken cancellationToken = default);
    }
}
