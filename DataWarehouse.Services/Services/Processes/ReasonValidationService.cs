using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Core.Interfaces.Company;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Services.Processes;

public class ReasonValidationService
{
    private readonly DataWarehouseDbContext _context;
    private readonly ICompanyCache _companyCache;

    public ReasonValidationService(DataWarehouseDbContext context, ICompanyCache companyCache)
    {
        _context = context;
        _companyCache = companyCache;
    }

    public async Task ValidateAsync(int? reasonId, ProcessType processType)
    {
        if (reasonId == null)
            throw new Exception("Reason is required.");

        var companyId = await _companyCache.Get();
        if (companyId == null || companyId <= 0)
            throw new Exception("Select Company.");

        var reason = await _context.Reasons
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReasonId == reasonId.Value && r.CompanyId == companyId.Value);

        if (reason == null)
            throw new Exception("Invalid reason.");

        if (!reason.IsActive)
            throw new Exception("Selected reason is inactive.");

        if (reason.ProcessType != processType)
            throw new Exception("Reason is not valid for this process type.");
    }
}
