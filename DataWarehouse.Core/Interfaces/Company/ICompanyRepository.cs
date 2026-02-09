using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Company
{
    public interface ICompanyRepository
    {
        Task<GeneralResponse<CompanyDto>> AddCompanyAuthasync(AddCompanyDto dto);
        // update only 
        Task<GeneralResponse<CompanyDto>> UpdateCompanyAuthasync(UpdateCompanyDto dto);
        Task<GeneralResponse<IEnumerable<CompanyDto>>> GetAllCompanyAsync();
        Task<GeneralResponse<CompanyDto>> GetCompanySettingAsync();
        Task<GeneralResponse<PagedResult<CompanyDto>>> GetCompaniesAsync(
   int skip,
   int pageSize);
        Task<GeneralResponse<CompanyDto>> GetCurruntCompany();
        Task<GeneralResponse<CompanyDto>> DeleteCompanyAuthasync(int companyId);
        Task<GeneralResponse<CompanyDto>> ChangeActiveCompanyAuthasync(int CompanyId);
        Task<GeneralResponse<CompanyDto>> PutCompanyInCache(int companyId, string userId);
       // Task<GeneralResponse<CompanyDto>> PutCompanyInCacheToUsers(string userId);
       // Task UpdateUserClaimAsync(string userId, string claimValue);
    }
}
