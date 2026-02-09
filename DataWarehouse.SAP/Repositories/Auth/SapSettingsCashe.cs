using DataWarehouse.Core.DTOs;
using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Auth
{
    public class SapSettingsCache : ISapSettingsCache
    {
        private readonly IMemoryCache _cache;
        private readonly DataWarehouseDbContext _context;

        public SapSettingsCache(
            IMemoryCache cache,
            DataWarehouseDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        private static string CacheKey(int sapId)
            => $"SAP:SETTINGS:{sapId}";

        public async Task<SapDto> GetOrSetAsync(int sapId)
        {
            if (_cache.TryGetValue(CacheKey(sapId), out SapDto cached))
                return cached;

            var sap = await _context.Saps
                .AsNoTracking()
                .Where(x => x.SapId == sapId && x.IsActive)
                .Select(x => new SapDto
                {
                    SapId = x.SapId,
                    SapUrl = x.SapUrl,
                    CompanyDB = x.CompanyDB,
                    UserName = x.UserName,
                    Password = x.Password
                   
                })
                .FirstOrDefaultAsync();

            if (sap == null)
                throw new Exception($"SAP with Id {sapId} not found or inactive");

            _cache.Set(
                CacheKey(sapId),
                sap,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });

            return sap;
        }

        public void Clear(int sapId)
        {
            _cache.Remove(CacheKey(sapId));
        }
    }

}
