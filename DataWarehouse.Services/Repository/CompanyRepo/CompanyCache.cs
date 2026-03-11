using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Domain.Entities.AllinAll;
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

namespace DataWarehouse.Services.Repository.CompanyRepo
{
    public class CompanyCache : ICompanyCache
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyCache(UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }
        private static string CacheKey(int companyId)
            => $"Company:Config:{companyId}";
        private async Task<int> GetCurrentCompanyIdAsync()
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
            var claim = claims.FirstOrDefault(c => c.Type == "CurrentCompanyId");

            if (claim == null)
                return 0;
         //       throw new Exception("Company not selected");

            return int.Parse(claim.Value);
        }

        public async Task<int?> Get()
        {
            int? companyId = await GetCurrentCompanyIdAsync();
            if (companyId == null)
                return null;
            //_cache.TryGetValue(CacheKey(companyId), out CompanyDto company);
            return companyId;
        }


        public async Task SetCompanyToUserClaimAsync(string claimValue)
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

            var existingClaims = await _userManager.GetClaimsAsync(user);

            var companyClaims = existingClaims
                .Where(c => c.Type == "CurrentCompanyId")
                .ToList();

            if (companyClaims.Any())
            {
                await _userManager.RemoveClaimsAsync(user, companyClaims);
            }


            await _userManager.AddClaimAsync(user, new Claim("CurrentCompanyId", claimValue));
        }

        public Task<bool> CheckOnCompanyInClaimAsync()
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new Exception("No HttpContext available");

            var companyIdValue = context.User?
                .FindFirst("companyId")?
                .Value;

            var hasValidValue =
                !string.IsNullOrWhiteSpace(companyIdValue) &&
                int.TryParse(companyIdValue, out _);

            return Task.FromResult(hasValidValue);
        }



        public void Set(int companyId, CompanyDto company)
        {
            _cache.Set(
                CacheKey(companyId),
                company,
                TimeSpan.FromHours(6) // config يعيش شوية
            );

        }

        public void Clear(int sapId)
        {
            _cache.Remove(CacheKey(sapId));
        }


    }
}
