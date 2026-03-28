using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class ReasonRepository : BaseRepository<Reason>, IReasonRepository
{
    private readonly ICompanyCache _companyCache;

    public ReasonRepository(DataWarehouseDbContext context, ICompanyCache companyCache) : base(context)
    {
        _companyCache = companyCache;
    }

    public async Task<GeneralResponse<IEnumerable<ReasonDto>>> GetActiveByProcessTypeAsync(ProcessType processType)
    {
        var companyId = await _companyCache.Get();
        if (companyId == null || companyId <= 0)
            return GeneralResponse<IEnumerable<ReasonDto>>.FailResponse("Select Company");

        var data = await _context.Reasons
            .AsNoTracking()
            .Where(r => r.IsActive && r.ProcessType == processType && r.CompanyId == companyId.Value)
            .OrderBy(r => r.Name)
            .Select(r => new ReasonDto
            {
                ReasonId = r.ReasonId,
                Name = r.Name,
                ProcessType = r.ProcessType
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<ReasonDto>>.SuccessResponse(data);
    }

    public async Task<GeneralResponse<ReasonDto>> AddReasonAsync(AddReasonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return GeneralResponse<ReasonDto>.FailResponse("Reason name is required.");

        var companyId = await _companyCache.Get();
        if (companyId == null || companyId <= 0)
            return GeneralResponse<ReasonDto>.FailResponse("Select Company");

        var entity = new Reason
        {
            Name = dto.Name.Trim(),
            ProcessType = dto.ProcessType,
            IsActive = dto.IsActive,
            CompanyId = companyId.Value
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        return GeneralResponse<ReasonDto>.SuccessResponse(new ReasonDto
        {
            ReasonId = entity.ReasonId,
            Name = entity.Name,
            ProcessType = entity.ProcessType
        });
    }

    public async Task<GeneralResponse<ReasonDto>> UpdateReasonAsync(int reasonId, UpdateReasonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return GeneralResponse<ReasonDto>.FailResponse("Reason name is required.");

        var companyId = await _companyCache.Get();
        if (companyId == null || companyId <= 0)
            return GeneralResponse<ReasonDto>.FailResponse("Select Company");

        var entity = await _context.Reasons
            .FirstOrDefaultAsync(r => r.ReasonId == reasonId && r.CompanyId == companyId.Value);

        if (entity == null)
            return GeneralResponse<ReasonDto>.FailResponse("Reason not found.");

        entity.Name = dto.Name.Trim();
        entity.ProcessType = dto.ProcessType;
        entity.IsActive = dto.IsActive;

        await SaveChangesAsync();

        return GeneralResponse<ReasonDto>.SuccessResponse(new ReasonDto
        {
            ReasonId = entity.ReasonId,
            Name = entity.Name,
            ProcessType = entity.ProcessType
        });
    }

    public async Task<GeneralResponse<bool>> DeleteReasonAsync(int reasonId)
    {
        var companyId = await _companyCache.Get();
        if (companyId == null || companyId <= 0)
            return GeneralResponse<bool>.FailResponse("Select Company");

        var entity = await _context.Reasons
            .FirstOrDefaultAsync(r => r.ReasonId == reasonId && r.CompanyId == companyId.Value);

        if (entity == null)
            return GeneralResponse<bool>.FailResponse("Reason not found.");

        _context.Reasons.Remove(entity);
        await SaveChangesAsync();

        return GeneralResponse<bool>.SuccessResponse(true);
    }

}
