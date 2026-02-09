using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.SapRepo
{
    public class SapCache : ISapCache
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SapCache(UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }
        private static string CacheKey(int sapId)
            => $"Sap:Config:{sapId}";

        private async Task<int?> GetCurrentSapIdAsync()
        {
            // جلب الـ HttpContext
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                throw new Exception("No HttpContext available");

            // جلب اليوزر ID من الـ token
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not found in token");

            // جلب اليوزر من UserManager
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found in database");

            // جلب الـ claims الخاصة باليوزر
            var claims = await _userManager.GetClaimsAsync(user);
            var claim = claims.FirstOrDefault(c => c.Type == "CurrentSapId");

            if (claim == null)
                return null;

            return int.Parse(claim.Value);
        }

        public async Task<int?> Get()
        {
            int? sapId = await GetCurrentSapIdAsync();

            if (sapId == null)
                return null;

           // _cache.TryGetValue(CacheKey(sapId??0), out SapDto sap);
            return sapId;
        }
        public async Task UpdateSapUserClaimAsync(string claimValue)
        {   // جلب الـ HttpContext
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                throw new Exception("No HttpContext available");

            // جلب اليوزر ID من الـ token
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not found in token");

            // جلب اليوزر من UserManager
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found in database");


            var existingClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "CurrentSapId");

            if (existingClaim != null)
                await _userManager.RemoveClaimAsync(user, existingClaim);

            await _userManager.AddClaimAsync(user, new Claim("CurrentSapId", claimValue));
        }

        public void Set(int sapId, SapDto sap)
        {
            _cache.Set(
                CacheKey(sapId),
                sap,
                TimeSpan.FromHours(6)
            );
        }

        public void Clear(int sapId)
        {
            _cache.Remove(CacheKey(sapId));
        }
       
    }

}
