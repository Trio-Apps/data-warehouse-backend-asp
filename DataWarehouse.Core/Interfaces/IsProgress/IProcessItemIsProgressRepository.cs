using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.IsProgress;

public interface IProcessItemIsProgressRepository : IBaseRepository<ProcessItemIsProgress>
{
    Task<ProcessItemIsProgress?> GetByProcessTypeAndIdAsync(ProcessType processType, int processId);
    Task<IEnumerable<ProcessItemIsProgress>> GetByProcessTypeAsync(ProcessType processType);
    Task<IEnumerable<ProcessItemIsProgress>> GetByStatusAsync(ProcessStatus status);
    Task<ProcessItemIsProgress?> GetWithApprovalsAsync(int processItemIsProgressId);
    Task<IEnumerable<ProcessItemIsProgress>> GetPendingProcessesAsync();
}
