using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Auth
{
    public class SapSessionCache : ISapSessionCache
    {
        private readonly IMemoryCache _cache;

        public SapSessionCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string GetCacheKey(int sapId)
            => $"Sap:Session:{sapId}";

        public SapSession? Get(int sapId)
        {
            _cache.TryGetValue(GetCacheKey(sapId), out SapSession session);
            return session;
        }

        public void Set(int sapId, SapSession session)
        {
            var expiration = session.SessionTimeout - DateTime.UtcNow;

            if (expiration <= TimeSpan.Zero)
                expiration = TimeSpan.FromMinutes(5); // safety fallback

            _cache.Set(
                GetCacheKey(sapId),
                session,
                expiration
            );
        }

        public void Clear(int sapId)
        {
            _cache.Remove(GetCacheKey(sapId));
        }
    }


}
