using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Based
{
    public interface ISapSyncStatusRepository
    {
        Task<DateTime> GetLastSyncDateAsync(int sapId,string entity);
        Task UpdateLastSyncDateAsync(int sapId, string entity, DateTime date);
        Task<int> GetLastSyncPaginationSkipAsync(int sapId, string entity);
        Task UpdateLastSyncPaginationSkipAsync(int sapId, string entity, int skip);
    }
}
