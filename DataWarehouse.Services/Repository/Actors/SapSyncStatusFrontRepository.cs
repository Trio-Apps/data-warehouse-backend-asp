using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;


namespace DataWarehouse.Services.Repository.Actors;

public class SapSyncStatusFrontRepository : BaseRepository<SapSyncStatusFront>, ISapSyncStatusFrontRepository
{
    public SapSyncStatusFrontRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<SapSyncStatusFront?> GetByEntityNameAsync(string entityName,string userId)
    {
        return await Query().FirstOrDefaultAsync(x => x.EntityName == entityName && x.UserId == userId);
    }
    public async Task<SapSyncStatusFront> UpdateOrAddIncrementalSyncAsync(string EntityName,string userId)
    {
        var existingEntity = await GetByEntityNameAsync(userId,EntityName);
        if (existingEntity == null)
        {
            var newEntity = new SapSyncStatusFront()
            {
                EntityName = EntityName,
                LastSyncDate = DateTime.UtcNow,
                UserId = userId,
            };

            await AddAsync(newEntity);
            await SaveChangesAsync();

            return newEntity;

        }

        existingEntity.EntityName = EntityName;
        existingEntity.LastSyncDate = DateTime.UtcNow;

        await UpdateAsync(existingEntity);
        await SaveChangesAsync();

        return existingEntity;
    }


}

