using DataWarehouse.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Company
{
    public interface ICompanyCache
    {
        Task<int?> Get();
        Task SetCompanyToUserClaimAsync(string claimValue);
        Task<bool> CheckOnCompanyInClaimAsync();
        void Set(int companyId, CompanyDto company);
        void Clear(int companyId);
    }
}
