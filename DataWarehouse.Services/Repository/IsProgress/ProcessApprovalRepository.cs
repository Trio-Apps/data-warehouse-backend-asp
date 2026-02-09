using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.IsProgress;

public class ProcessApprovalRepository : BaseRepository<ProcessApproval>, IProcessApprovalRepository
{
    public ProcessApprovalRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProcessApproval>> GetByProcessItemIsProgressIdAsync(int processItemIsProgressId)
    {
        return await Query().Where(pa => pa.ProcessItemIsProgressId == processItemIsProgressId).ToListAsync();
    }

    public async Task<IEnumerable<ProcessApproval>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(pa => pa.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<ProcessApproval>> GetByApprovalStepIdAsync(int approvalStepId)
    {
        return await Query().Where(pa => pa.ApprovalStepId == approvalStepId).ToListAsync();
    }

    public async Task<IEnumerable<ProcessApproval>> GetByStatusAsync(ApprovalStatus status)
    {
        return await Query().Where(pa => pa.Status == status).ToListAsync();
    }

    public async Task<ProcessApproval?> GetWithUserAsync(int processApprovalId)
    {
        return await QueryIncluding(false, pa => pa.User)
            .FirstOrDefaultAsync(pa => pa.ProcessApprovalId == processApprovalId);
    }

    public async Task<ProcessApproval?> GetWithApprovalStepAsync(int processApprovalId)
    {
        return await QueryIncluding(false, pa => pa.ApprovalStep)
            .FirstOrDefaultAsync(pa => pa.ProcessApprovalId == processApprovalId);
    }

    public async Task<IEnumerable<ProcessApproval>> GetPendingApprovalsAsync()
    {
        return await Query().Where(pa => pa.Status == ApprovalStatus.Pending).ToListAsync();
    }
}
