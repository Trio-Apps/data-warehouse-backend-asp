using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Actors;

public interface ISapSyncStatusFrontRepository : IBaseRepository<SapSyncStatusFront>
{
    Task<SapSyncStatusFront?> GetByEntityNameAsync(string entityName, string userId);
    Task<SapSyncStatusFront> UpdateOrAddIncrementalSyncAsync(string userId, string EntityName);
}

