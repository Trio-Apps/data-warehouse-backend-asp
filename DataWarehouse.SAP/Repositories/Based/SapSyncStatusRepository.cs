using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.SAP.Interfaces.Based;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Based
{
    public class SapSyncStatusRepository : ISapSyncStatusRepository
    {
        private readonly DataWarehouseDbContext _context;

        public SapSyncStatusRepository(DataWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<DateTime> GetLastSyncDateAsync(int sapId, string entity)
        {
            var status = await _context.SapSyncStatuses
                .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entity );

            if (status == null)
            {
                status = new SapSyncStatus
                {
                    SapId = sapId,
                    EntityName = entity,
                    LastSyncDate = new DateTime(2000, 1, 1)
                };

                _context.SapSyncStatuses.Add(status);
                await _context.SaveChangesAsync();
            }

            return status.LastSyncDate;
        }




        public async Task UpdateLastSyncDateAsync(int sapId, string entity, DateTime date)
        {
            var row = await _context.SapSyncStatuses
         .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entity);


            row.LastSyncDate = date;
            
            await _context.SaveChangesAsync();
        }
        /// <summary>
      
        public async Task<int> GetLastSyncPaginationSkipAsync( int sapId, string entity)
        {
            var res = await _context.SapSyncPaginations
                .Where(x => x.SapId == sapId && x.EntityName == entity)
                
                .FirstOrDefaultAsync();

            if (res == null)
            {
                var dto = new SapSyncPagination
                {
                    EntityName = entity,
                    Skip = 0,
                    SapId = sapId,
                };
                _context.SapSyncPaginations.Add(dto);
                await _context.SaveChangesAsync(); // مهم جداً لحفظه في الداتابيس

                return dto.Skip;
            }


            // إذا موجود بالفعل، رجع القيمة
            return res.Skip; // res هنا nullable، فنستخدم Value
        }

        public async Task UpdateLastSyncPaginationSkipAsync(int sapId, string entity, int skip)
        {
            var row = await _context.SapSyncPaginations
             .FirstOrDefaultAsync(x => x.SapId == sapId && x.EntityName == entity );

              row.Skip = skip;


            await _context.SaveChangesAsync();
        }
    }

}
