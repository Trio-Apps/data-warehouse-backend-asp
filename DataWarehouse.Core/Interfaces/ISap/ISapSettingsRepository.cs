using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities;
using DataWarehouse.Domain.Entities.Processes;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.ISap
{
    public interface ISapSettingsRepository 
    {
        Task<GeneralResponse<SapDto>> AddSapAuthasync(string userId, AddSapDto dto);
        // update only 
        Task<GeneralResponse<SapDto>> UpdateSapAuthasync(string userId, UpdateSapDto dto);
        Task<GeneralResponse<IEnumerable<SapDto>>> GetAllSapAsync(string userId,IList<string> roles);
        Task<GeneralResponse<IEnumerable<SapByCompanyIdDto>>> GetSapsByCompanyId(int companyId);
        Task<GeneralResponse<PagedResult<SapDto>>> GetSapsAsync(string userId, IList<string> roles,
                 int skip,
                 int pageSize,
                 int? companyIdDto,
                 string? userName,
                 string? sapName);
        Task<GeneralResponse<SapDto>> GetSapSettingAsync();
        Task<GeneralResponse<SapDto>> GetCurruntCompany();
        Task<GeneralResponse<SapDto>> ChangeActiveCompanyAuthasync(int sapId);
        Task<GeneralResponse<SapDto>> PutSapInCache(int sapId, string userId);
        Task<GeneralResponse<SapDto>> PutSapInCacheToUsers(string userId);
       // Task<GeneralResponse<SapDto>> PutSapInCacheToEmployees(string userId);
        Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetYourWarehousesToEmployees(string userId);
       // Task UpdateUserClaimAsync(string userId, string claimType, string claimValue);
       

    }

}
