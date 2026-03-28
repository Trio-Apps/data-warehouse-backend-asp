using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IReasonRepository : IBaseRepository<Reason>
{
    Task<GeneralResponse<IEnumerable<ReasonDto>>> GetActiveByProcessTypeAsync(ProcessType processType);
    Task<GeneralResponse<ReasonDto>> AddReasonAsync(AddReasonDto dto);
    Task<GeneralResponse<ReasonDto>> UpdateReasonAsync(int reasonId, UpdateReasonDto dto);
    Task<GeneralResponse<bool>> DeleteReasonAsync(int reasonId);
}
