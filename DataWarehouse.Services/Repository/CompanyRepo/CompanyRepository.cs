using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.CompanyRepo
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        private readonly ICompanyCache companyCache;
        private readonly UserManager<ApplicationUser> userManager;

        public CompanyRepository(DataWarehouseDbContext context, ICompanyCache companyCache, UserManager<ApplicationUser> userManager) : base(context)
        {
            this.companyCache = companyCache;
            this.userManager = userManager;
        }
        public async Task<GeneralResponse<CompanyDto>> AddCompanyAuthasync(AddCompanyDto dto)
        {

            var mapping = new Company
            {
                Name = dto.Name,
                IsActive = true
            };


            var res = await AddAsync(mapping);
            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 3️⃣ إضافة الـ ProcessesTypes التلقائية
            var processesTypes = new List<ProcessesType>
    {
        new ProcessesType
        {
            ProcessesName = ProcessesTypes.Production.ToString(),
            CompanyId = res.CompanyId
        },
        new ProcessesType
        {
            ProcessesName = ProcessesTypes.Purchase.ToString(),
            CompanyId = res.CompanyId
        },
        new ProcessesType
        {
            ProcessesName = ProcessesTypes.Sales.ToString(),
            CompanyId = res.CompanyId
        }
    };
            await _context.ProcessesTypes.AddRangeAsync(processesTypes);
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new CompanyDto
            {
                CompanyId = res.CompanyId,
                Name = dto.Name,
                IsActive = true
            };

            return GeneralResponse<CompanyDto>.SuccessResponse(result);
        }


        public async Task<GeneralResponse<CompanyDto>> UpdateCompanyAuthasync(UpdateCompanyDto dto)
        {
            // 1️⃣ Get existing Company auth (record الوحيد)
            var Company = await _context.Companies.FirstOrDefaultAsync(e => e.CompanyId == dto.CompanyId);

            if (Company == null)
            {
                return GeneralResponse<CompanyDto>.FailResponse("Company configuration not found");
            }

            // 2️⃣ Update fields

           
            Company.Name = dto.Name;
            Company.IsActive = dto.IsActive;
          
            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new CompanyDto
            {
                CompanyId = Company.CompanyId,
                Name = Company.Name,
                IsActive = Company.IsActive
            };

            return GeneralResponse<CompanyDto>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<IEnumerable<CompanyDto>>> GetAllCompanyAsync()
        {


            var res = await GetAllAsync();


            var mapping = res.Where(s => s.IsActive).Select(e => new CompanyDto
            {
                CompanyId = e.CompanyId,
               
                Name = e.Name
              
            });

            return GeneralResponse<IEnumerable<CompanyDto>>.SuccessResponse(mapping);
        }
        public async Task<GeneralResponse<PagedResult<CompanyDto>>> GetCompaniesAsync(
        int skip,
        int pageSize)
        {


            bool cpmpanyFlag = await companyCache.CheckOnCompanyInClaimAsync();
            var res = new PagedResult<Company> ();

            if (cpmpanyFlag == false)
                res = await PaginationWithoutFilterAsync(skip, pageSize);
            else
            {
                int? companyId = await companyCache.Get();
                res = await PaginationWithFilterAsync(e => e.CompanyId == companyId, skip, pageSize);
            }


            var data = res.Data.Select(e => new CompanyDto
            { CompanyId = e.CompanyId,
             Name = e.Name,
              IsActive = e.IsActive
            }).ToList();

            return GeneralResponse<PagedResult<CompanyDto>>.SuccessResponse(
                new PagedResult<CompanyDto>
                {
                    Data = data,
                    PageNumber = res.PageNumber,
                    PageSize = res.PageSize,
                    TotalRecords = res.TotalRecords
                });
        }

        public async Task<GeneralResponse<CompanyDto>> GetCurruntCompany()
        {
            var companyId = await companyCache.Get();
            if (!companyId.HasValue || companyId.Value == 0)
            {
                return GeneralResponse<CompanyDto>.FailResponse("Select Company");
            }

            var company = await GetByIdAsync(companyId??0);

            return GeneralResponse<CompanyDto>.SuccessResponse(new CompanyDto { CompanyId = company.CompanyId, Name = company.Name, IsActive = company.IsActive });

        }

        public async Task<GeneralResponse<CompanyDto>> GetCompanySettingAsync()
        {

            var CompanyId = await companyCache.Get();
            var res = await _context.Companies
                .AsNoTracking()
                .Where(e => e.CompanyId == CompanyId)
                .Select(e => new CompanyDto
                {
                    CompanyId = e.CompanyId,
                    Name = e.Name,
                })
                .FirstOrDefaultAsync();
            return GeneralResponse<CompanyDto>.SuccessResponse(res);
        }
        public async Task<GeneralResponse<CompanyDto>> DeleteCompanyAuthasync(int CompanyId)
        {
            // 1️⃣ Get existing Company auth (record الوحيد)
            var Company = await _context.Companies.FirstOrDefaultAsync(e => e.CompanyId == CompanyId);

            if (Company == null)
            {
                return GeneralResponse<CompanyDto>.FailResponse("Company configuration not found");
            }


           // Company.IsActive = false; // ⚠️ يفضل تكون encrypted

            var company = await DeleteAsync(CompanyId);
           
            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new CompanyDto
            {
                CompanyId = Company.CompanyId,
               Name = Company.Name,
               
            };

            return GeneralResponse<CompanyDto>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<CompanyDto>> ChangeActiveCompanyAuthasync(int CompanyId)
        {
            // 1️⃣ Get existing Company auth (record الوحيد)
            var Company = await _context.Companies.FirstOrDefaultAsync(e => e.CompanyId == CompanyId);

            if (Company == null)
            {
                return GeneralResponse<CompanyDto>.FailResponse("Company configuration not found");
            }


            // Company.IsActive = false; // ⚠️ يفضل تكون encrypted

            Company.IsActive = !Company.IsActive; // ⚠️ يفضل تكون encrypted

            // 3️⃣ Save changes
            await _context.SaveChangesAsync();

            // 4️⃣ Map to DTO
            var result = new CompanyDto
            {
                CompanyId = Company.CompanyId,
                Name = Company.Name,

            };

            return GeneralResponse<CompanyDto>.SuccessResponse(result);
        }

        // super admin
        public async Task<GeneralResponse<CompanyDto>> PutCompanyInCache(int CompanyId, string userId)
        {
            // 1️⃣ Get Company from DB
            var Company = await _context.Companies
                .AsNoTracking()
                .Where(e => e.CompanyId == CompanyId && e.IsActive)
                .Select(e => new CompanyDto
                {
                    CompanyId = e.CompanyId,
                   Name =e.Name
                })
                .FirstOrDefaultAsync();

            if (Company == null)
            {
                return GeneralResponse<CompanyDto>
                    .FailResponse("Company configuration not found or inactive");
            }

            // تحديث Claims
            await companyCache.SetCompanyToUserClaimAsync(CompanyId.ToString());

            // 2️⃣ Put in Cache
          //  companyCache.Set(CompanyId, Company);

            // 3️⃣ Return result
            return GeneralResponse<CompanyDto>.SuccessResponse(Company);
        }

        // admin
        //public async Task<GeneralResponse<CompanyDto>> PutCompanyInCacheToUsers(int? companyId, )
        //{
        //    // 1️⃣ Get User-Company relation
        //    //var userCompany = await _context.CompanyUsers
        //    //    .Include(us => us.Company)
        //    //    .ThenInclude(cs=>cs.Saps)
        //    //    .AsNoTracking()
        //    //    .FirstOrDefaultAsync(us =>
        //    //        us.UserId == userId);

        //    if (companyId == null)
        //    {
        //        return GeneralResponse<CompanyDto>
        //            .FailResponse("User is not linked to any Company");
        //    }


        //  //  var Company = userCompany.Company;
        //   // var CompanyId = Company.CompanyId;

        //    // 2️⃣ Map to DTO
        //    //var CompanyDto = new CompanyDto
        //    //{
        //    //    CompanyId = Company.CompanyId,
        //    //   Name = Company.Name,
        //    //    IsActive = Company.IsActive
        //    //};

        //    // 3️⃣ Update Claims (CurrentCompanyId)
        //    await UpdateUserClaimAsync(userId, CompanyId.ToString());

        //    // 4️⃣ Put Company config in Cache
        //   // companyCache.Set(CompanyId, CompanyDto);

        //    // 5️⃣ Return
        //    return GeneralResponse<CompanyDto>.SuccessResponse(CompanyDto);
        //}

        //public async Task UpdateUserClaimAsync(string userId, string claimValue)
        //{
        //    var user = await userManager.FindByIdAsync(userId);

        //    var existingClaims = await userManager.GetClaimsAsync(user);
        //  //  var existing = existingClaims.FirstOrDefault(c => c.Type == "CurrentCompanyId");

        //    if (existingClaims.Any())
        //        await userManager.RemoveClaimsAsync(user, existingClaims);

        //    await userManager.AddClaimAsync(user, new Claim("CurrentCompanyId", claimValue));
        //}

       
    }
}
