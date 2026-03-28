using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;

namespace DataWarehouse.Core.IServices.Processes;

public interface IReasonService : IBaseService<Reason>
{
    Task<GeneralResponse<IEnumerable<ReasonDto>>> GetActiveByProcessTypeAsync(ProcessType processType);
    Task<GeneralResponse<bool>> ValidateReasonAsync(int? reasonId, ProcessType processType);
}
