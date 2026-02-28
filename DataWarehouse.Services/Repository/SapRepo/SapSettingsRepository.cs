using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataWarehouse.Services.Repository.SapRepo
{
    public class SapSettingsRepository : BaseRepository<Sap> ,ISapSettingsRepository
    {
        private readonly IAuthServices authServices;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISapCache sapCache;
        private readonly ICompanyCache companyCache;
        private readonly UserManager<ApplicationUser> userManager;

        public SapSettingsRepository(IAuthServices authServices,  IHttpClientFactory httpClientFactory, DataWarehouseDbContext context,ISapCache sapCache,ICompanyCache companyCache, UserManager<ApplicationUser> userManager) : base(context)
        {
            this.authServices = authServices;
            this.httpClientFactory = httpClientFactory;
            this.sapCache = sapCache;
            this.companyCache = companyCache;
            this.userManager = userManager;
        }
       
        
        public async Task<GeneralResponse<SapDto>> AddSapAuthasync(string userId, AddSapDto dto)
        {

            var testResult = await TestAsync(dto);

            if (!testResult.IsSuccess)
            {
                var errors = new List<string>() { testResult.Error };
                return GeneralResponse<SapDto>.FailResponse("This Sap is not vaild!", errors);

            }


            var user = await userManager.FindByIdAsync(userId);

            var role = (await userManager.GetRolesAsync(user)).ToList();


                 Sap mapping;

           
                var companyId = await companyCache.Get();
                var settingBarcode = await _context.BarCodeSettings.Where(s => s.CompanyId == companyId).FirstOrDefaultAsync();

                if (settingBarcode == null)
                    return GeneralResponse<SapDto>.FailResponse("Add the barcode setting before adding any SAP, in order to retrieve all valid barcodes.");

                mapping = new Sap
                {
                    SapUrl = dto.SapUrl,
                    Name = dto.Name,
                    CompanyDB = dto.CompanyDB,
                    UserName = dto.UserName,
                    Password = dto.Password,
                    CompanyId = companyId??0
                };
            
         
            var res = await AddAsync(mapping);
            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new SapDto
            {
                SapId = res.SapId,
                Name = res.Name,
                SapUrl = res.SapUrl,
                CompanyDB = res.CompanyDB,
                UserName = res.UserName,
                Password = res.Password,
                CompanyId = res.CompanyId
            };

            return GeneralResponse<SapDto>.SuccessResponse(result);
        }

        private async Task<SapConnectionTestResult> TestAsync(AddSapDto dto )
        {
            try
            {
                var client = httpClientFactory.CreateClient("SAP");
                client.BaseAddress = new Uri(dto.SapUrl);

                var loginData = new
                {
                    CompanyDB = dto.CompanyDB,
                    UserName = dto.UserName,
                    Password = dto.Password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

               var response = await client.PostAsync("Login", content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return SapConnectionTestResult.Fail(
                        $"SAP login failed: {response.StatusCode} - {error}"
                    );
                }

                return SapConnectionTestResult.Success();
            }
            catch (Exception ex)
            {
                return SapConnectionTestResult.Fail($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<GeneralResponse<SapDto>> UpdateSapAuthasync(string userId, UpdateSapDto dto)
        {
          
            var user = await userManager.FindByIdAsync(userId);

            var role = (await userManager.GetRolesAsync(user)).ToList();
            // 1️⃣ Get existing SAP auth (record الوحيد)
            var sap = await _context.Saps.FirstOrDefaultAsync(e=>e.SapId == dto.SapId);


            if (sap == null)
            {
                return GeneralResponse<SapDto>.FailResponse("SAP configuration not found");
            }

            // 2️⃣ Update fields
            if (role.Contains("super-admin"))
            {
                sap.CompanyId = dto.CompanyId;
            }


            sap.CompanyDB = dto.CompanyDB;
            sap.Name = dto.Name;
            sap.UserName = dto.UserName;
            sap.Password = dto.Password; // ⚠️ يفضل تكون encrypted
            
            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new SapDto
            {
                SapId = sap.SapId,
                CompanyDB = sap.CompanyDB,
                UserName = sap.UserName,
                Name = sap.Name,
                SapUrl = sap.SapUrl,
                Password = sap.Password
            };

            return GeneralResponse<SapDto>.SuccessResponse(result);
        }


        public async Task<GeneralResponse<IEnumerable<SapDto>>> GetAllSapAsync(string userId,IList<string> roles)
        {



            var checkRole = await authServices.GetLoginContextAsync(userId, roles);

            IQueryable<Sap> query = _context.Saps.AsNoTracking();

            if (checkRole.IsGlobal)
            {
                var companyId = await companyCache.Get();

                query = query.Where(sap => sap.CompanyId == companyId);
            }
            else
            {
                query =
                  from sap in query
                   join su in _context.SapUsers.AsNoTracking()
                      on sap.SapId equals su.SapId
                 where su.UserId == userId
                  select sap;
            }




            var mapping = await query.Select(e => new SapDto
            {
                SapId = e.SapId,
                SapUrl = e.SapUrl,
                 Name = e.Name,
                IsActive = e.IsActive,
                CompanyDB = e.CompanyDB,
                UserName = e.UserName,
                Password = e.Password
            }).ToListAsync();

            return GeneralResponse<IEnumerable<SapDto>>.SuccessResponse(mapping);
        }
     
        
        public async Task<GeneralResponse<PagedResult<SapDto>>> GetSapsAsync(string userId,IList<string> roles,
         int pageNumber,
          int pageSize,
         int? companyIdDto,
         string? userName,
         string? sapName)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;


           // var user = await userManager.FindByIdAsync(userId);

           // var role = (await userManager.GetRolesAsync(user)).ToList();

            var checkRole = await authServices.GetLoginContextAsync(userId, roles);
            IQueryable<Sap> query = _context.Saps.AsNoTracking();


            if (checkRole.IsGlobal)
            {
                var companyId = await companyCache.Get();

                query = query.Where(sap => sap.CompanyId == companyId);

              //  query = query.Where(sap => sap.CompanyId == compaanyIdActive);
            }
            else
            {
                query =
                    from sap in query
                    join su in _context.SapUsers.AsNoTracking()
                     on sap.SapId equals su.SapId
                    where su.UserId == userId
                    select sap;

            }



            // 🔹 Filtering
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(iw =>
                    iw.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(sapName))
            {
                query = query.Where(iw => iw.Name.Contains(sapName));
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new SapDto
                {
                    Name = e.Name,
                    SapId = e.SapId,
                    CompanyDB = e.CompanyDB,
                    UserName = e.UserName,
                    IsActive = e.IsActive,
                    Password = e.Password,
                    SapUrl = e.SapUrl,
                     CompanyId = e.CompanyId
                })
                .ToListAsync();



            return GeneralResponse<PagedResult<SapDto>>.SuccessResponse(
                new PagedResult<SapDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    Data = data
                });
        }
     
      
        
        public async Task<GeneralResponse<SapDto>> GetSapSettingAsync()
        {

            var sapId = await sapCache.Get();
            var res = await _context.Saps
                .AsNoTracking()
                .Where(e=>e.SapId == sapId)
                .Select(e => new SapDto { SapId = e.SapId, SapUrl = e.SapUrl, Name = e.Name,
                    CompanyDB = e.CompanyDB, UserName = e.UserName, Password = e.Password })
                .FirstOrDefaultAsync();
            return GeneralResponse<SapDto>.SuccessResponse(res);
        }
        public async Task<GeneralResponse<IEnumerable<SapByCompanyIdDto>>> GetSapsByCompanyId(int companyId)
        {
            var data = await _context.Saps.Where(s=>s.CompanyId == companyId && s.IsActive).Select(s=>new SapByCompanyIdDto
            {
                 Name = s.Name,
                  SapId = s.SapId,
            }).ToListAsync();

            return GeneralResponse<IEnumerable<SapByCompanyIdDto>>.SuccessResponse(data);

        }
        public async Task<GeneralResponse<SapDto>> GetCurruntCompany()
        {
            var companyId = await sapCache.Get();
            if (companyId == null)
                return GeneralResponse<SapDto>.FailResponse("Select Company");

            var company = await GetByIdAsync(companyId ?? 0);

            return GeneralResponse<SapDto>.SuccessResponse(new SapDto {  CompanyDB = company.CompanyDB, SapUrl= company.SapUrl,
                Name = company.Name, IsActive = company.IsActive, SapId = company.SapId, Password =company.Password, UserName = company.UserName });

        }
        public async Task<GeneralResponse<SapDto>> ChangeActiveCompanyAuthasync(int sapId)
        {
            // 1️⃣ Get existing SAP auth (record الوحيد)
            var sap = await _context.Saps.FirstOrDefaultAsync(e => e.SapId == sapId);

            if (sap == null)
            {
                return GeneralResponse<SapDto>.FailResponse("SAP configuration not found");
            }

          
            sap.IsActive = !sap.IsActive; // ⚠️ يفضل تكون encrypted

            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new SapDto
            {
                SapId = sap.SapId,
                CompanyDB = sap.CompanyDB,
                UserName = sap.UserName,
                Password = sap.Password
            };

            return GeneralResponse<SapDto>.SuccessResponse(result);
        }
        
        public async Task<GeneralResponse<SapDto>> PutSapInCache(int sapId,string userId)
        {
            // 1️⃣ Get SAP from DB
            var sap = await _context.Saps
                .AsNoTracking()
                .Where(e => e.SapId == sapId && e.IsActive)
                .Select(e => new SapDto
                {
                    SapId = e.SapId,
                    SapUrl = e.SapUrl,
                    CompanyDB = e.CompanyDB,
                    UserName = e.UserName,
                    Password = e.Password
                })
                .FirstOrDefaultAsync();

            if (sap == null)
            {
                return GeneralResponse<SapDto>
                    .FailResponse("SAP configuration not found or inactive");
            }

            // تحديث Claims
            await sapCache.UpdateSapUserClaimAsync(sapId.ToString());

            // 2️⃣ Put in Cache
           // sapCache.Set(sapId, sap);

            // 3️⃣ Return result
            return GeneralResponse<SapDto>.SuccessResponse(sap);
        }

        public async Task<GeneralResponse<SapDto>> PutSapInCacheToUsers(string userId)
        {
            // 1️⃣ Get User-SAP relation
            var userSap = await _context.SapUsers
                .Include(us => us.Sap)
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userSap == null || userSap.Sap == null)
            {
                return GeneralResponse<SapDto>
                    .FailResponse("User is not linked to any SAP");
            }

            var sap = userSap.Sap;
            var sapId = sap.SapId;

            // 2️⃣ Map to DTO
            var sapDto = new SapDto
            {
                SapId = sap.SapId,
                SapUrl = sap.SapUrl,
                CompanyDB = sap.CompanyDB,
                UserName = sap.UserName,
                Password = sap.Password,
                IsActive = sap.IsActive
            };

            // 3️⃣ Update Claims (CurrentSapId)
            await sapCache.UpdateSapUserClaimAsync(sapId.ToString());

            // 4️⃣ Put SAP config in Cache
           // sapCache.Set(sapId, sapDto);

            // 5️⃣ Return
            return GeneralResponse<SapDto>.SuccessResponse(sapDto);
        }

        //public async Task<GeneralResponse<SapDto>> PutSapInCacheToEmployees(string userId)
        //{
        //    // 1️⃣ Get User-SAP relation
        //    var empSap = await _context.SapEmployees.Include(e=>e.Sap).FirstOrDefaultAsync(us => us.UserId == userId);
               

        //    if (empSap == null || empSap.Sap == null)
        //    {
        //        return GeneralResponse<SapDto>
        //            .FailResponse("User is not linked to any SAP");
        //    }

        //    var sap = empSap.Sap;
        //    var sapId = sap.SapId;

        //    // 2️⃣ Map to DTO
        //    var sapDto = new SapDto
        //    {
        //        SapId = sap.SapId,
        //        SapUrl = sap.SapUrl,
        //        CompanyDB = sap.CompanyDB,
        //        UserName = sap.UserName,
        //        Password = sap.Password,
        //        IsActive = sap.IsActive
        //    };

        //    // 3️⃣ Update Claims (CurrentSapId)
        //    await UpdateUserClaimAsync(userId, "CurrentSapId", sapId.ToString());

        //    // 4️⃣ Put SAP config in Cache
        //    sapCache.Set(sapId, sapDto);

        //    // 5️⃣ Return
        //    return GeneralResponse<SapDto>.SuccessResponse(sapDto);
        //}

        public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetYourWarehousesToEmployees(string userId)
        {
            // 1️⃣ Get User-SAP relation
            var empSap = await _context.UserWarehouses.AsNoTracking().Where(us => us.UserId == userId)
                .Select(w=> new WarehouseDTO
                {
                    SapId = w.Warehouse.SapId,
                    WarehouseId = w.WarehouseId,
                    WarehouseName = w.Warehouse.WarehouseName

                }).ToListAsync();



            if (empSap == null)
            {
                return GeneralResponse<IEnumerable<WarehouseDTO>>
                    .FailResponse("User is not linked to any warehouse");
            }

         

            // 5️⃣ Return
            return GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(empSap);
        }


        //public async Task UpdateUserClaimAsync(string userId, string claimType, string claimValue)
        //{
        //    var user = await userManager.FindByIdAsync(userId);

        //    var existingClaim = (await userManager.GetClaimsAsync(user))
        //        .FirstOrDefault(c => c.Type == claimType);

        //    if (existingClaim != null)
        //        await userManager.RemoveClaimAsync(user, existingClaim);

        //    await userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        //}



    }
}
