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

public class ProcessItemIsProgressRepository : BaseRepository<ProcessItemIsProgress>, IProcessItemIsProgressRepository
{
    public ProcessItemIsProgressRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<ProcessItemIsProgress?> GetByProcessTypeAndIdAsync(ProcessType processType, int processId)
    {
        // processId here is the business reference id (for example ProductionOrderId), not the table PK.
        return await Query().FirstOrDefaultAsync(pip => pip.ProcessType == processType && pip.ReferenceId == processId);
    }

    public async Task<IEnumerable<ProcessItemIsProgress>> GetByProcessTypeAsync(ProcessType processType)
    {
        return await Query().Where(pip => pip.ProcessType == processType).ToListAsync();
    }

    public async Task<IEnumerable<ProcessItemIsProgress>> GetByStatusAsync(ProcessStatus status)
    {
        return await Query().Where(pip => pip.Status == status).ToListAsync();
    }

    public async Task<ProcessItemIsProgress?> GetWithApprovalsAsync(int processItemIsProgressId)
    {
        return await QueryIncluding(false, pip => pip.ProcessApprovals)
            .FirstOrDefaultAsync(pip => pip.ProcessItemIsProgressId == processItemIsProgressId);
    }

    public async Task<IEnumerable<ProcessItemIsProgress>> GetPendingProcessesAsync()
    {
        return await Query().Where(pip => pip.Status == ProcessStatus.InProgress).ToListAsync();
    }
}
