using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Sync;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.Sync;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DataWarehouse.Services.Repository.Sync;

public class SapSyncResetRepository : ISapSyncResetRepository
{
    private static readonly DateTime ResetDate = new(1900, 1, 1);
    private readonly ICompanyCache companyCache;
    private readonly DataWarehouseDbContext _context;

    public SapSyncResetRepository(ICompanyCache companyCache, DataWarehouseDbContext context)
    {
        this.companyCache = companyCache;
        _context = context;
    }

    public Task<GeneralResponse<bool>> ResetItemSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "item");

    public Task<GeneralResponse<bool>> ResetWarehouseSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "warehouse");

    public Task<GeneralResponse<bool>> ResetPurchaseSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "purchase");

    public Task<GeneralResponse<bool>> ResetCountSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "count");

    public Task<GeneralResponse<bool>> ResetBusinessPartnersSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "businessPartners");

    public Task<GeneralResponse<bool>> ResetItemUomGroupSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "itemUomGroup");

    public Task<GeneralResponse<bool>> ResetSalesSyncAsync(int sapId) => ResetEntitySyncAsync(sapId, "sales");

    public async Task<GeneralResponse<List<SapSyncStateDto>>> GetCompanySyncStateAsync()
    {

        var companyId = await companyCache.Get();//?? (await _context.Companies.AnyAsync(x => x.CompanyId == companyId));
      
        if (companyId == null)
            return GeneralResponse<List<SapSyncStateDto>>.FailResponse($"Company with id {companyId} not found.");

        var statuses = await _context.SapSyncStatuses
            .AsNoTracking()
            .Where(x => x.Sap.CompanyId == companyId)
            .Select(x => new SapSyncStateDto
            {
                SapId = x.SapId,
                EntityName = x.EntityName,
                LastSyncDate = x.LastSyncDate,
                Skip = null
            })
            .ToListAsync();

        var paginations = await _context.SapSyncPaginations
            .AsNoTracking()
            .Where(x => x.Sap.CompanyId == companyId)
            .Select(x => new SapSyncStateDto
            {
                SapId = x.SapId,
                EntityName = x.EntityName,
                LastSyncDate = null,
                Skip = x.Skip
            })
            .ToListAsync();

        var result = statuses
            .Concat(paginations)
            .GroupBy(x => new { x.SapId, x.EntityName })
            .Select(g => new SapSyncStateDto
            {
                SapId = g.Key.SapId,
                EntityName = g.Key.EntityName,
                LastSyncDate = g.Select(x => x.LastSyncDate).FirstOrDefault(x => x.HasValue),
                Skip = g.Select(x => x.Skip).FirstOrDefault(x => x.HasValue)
            })
            .OrderBy(x => x.SapId)
            .ThenBy(x => x.EntityName)
            .ToList();

        return GeneralResponse<List<SapSyncStateDto>>.SuccessResponse(result, "Company sync state retrieved successfully.");
    }

    private async Task<GeneralResponse<bool>> ResetEntitySyncAsync(int sapId, string entityName)
    {
        var sapExists = await _context.Saps.AnyAsync(x => x.SapId == sapId);
        if (!sapExists)
            return GeneralResponse<bool>.FailResponse($"SAP with id {sapId} not found.");

        var syncStatus = await _context.SapSyncStatuses
            .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entityName);

        if (syncStatus != null)
        {
            syncStatus.LastSyncDate = ResetDate;

        }
       

        var syncPagination = await _context.SapSyncPaginations
            .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entityName);

        if (syncPagination == null)
        {
            syncPagination = new SapSyncPagination
            {
                SapId = sapId,
                EntityName = entityName,
                Skip = 0
            };
            _context.SapSyncPaginations.Add(syncPagination);
        }
        else
        {
            syncPagination.Skip = 0;
        }

        await _context.SaveChangesAsync();
        return GeneralResponse<bool>.SuccessResponse(true, $"{entityName} sync reset successfully.");
    }
}
