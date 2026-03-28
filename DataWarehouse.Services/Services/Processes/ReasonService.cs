using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.IServices.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Services.Based;

namespace DataWarehouse.Services.Services.Processes;

public class ReasonService : BaseService<Reason>, IReasonService
{
    private readonly IReasonRepository _reasonRepository;
    private readonly ReasonValidationService _reasonValidationService;

    public ReasonService(IReasonRepository reasonRepository, ReasonValidationService reasonValidationService) : base(reasonRepository)
    {
        _reasonRepository = reasonRepository;
        _reasonValidationService = reasonValidationService;
    }

    public async Task<GeneralResponse<IEnumerable<ReasonDto>>> GetActiveByProcessTypeAsync(ProcessType processType)
    {
        return await _reasonRepository.GetActiveByProcessTypeAsync(processType);
    }

    public async Task<GeneralResponse<bool>> ValidateReasonAsync(int? reasonId, ProcessType processType)
    {
        try
        {
            await _reasonValidationService.ValidateAsync(reasonId, processType);
            return GeneralResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return GeneralResponse<bool>.FailResponse(ex.Message);
        }
    }
}
