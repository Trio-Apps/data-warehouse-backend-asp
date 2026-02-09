using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.BarCode;

public class BarCodeSettingRepository : BaseRepository<BarCodeSetting>, IBarCodeSettingRepository
{
    private readonly ICompanyCache companyCache;

    public BarCodeSettingRepository(DataWarehouseDbContext context,ICompanyCache companyCache) : base(context)
    {
        this.companyCache = companyCache;
    }

    public async Task<GeneralResponse<PagedResult<BarCodeSettingDto>>> GetBarCodeSettingAsync(
     int skip,
     int pageSize)
    {
        var companyId = await companyCache.Get();
        var res = await PaginationWithFilterAsync(e=>e.CompanyId == companyId,skip, pageSize);
        
        var data = res.Data.Select(e => new BarCodeSettingDto
        {
            BarCodeSettingId = e.BarCodeSettingId,
            TotalLength = e.TotalLength,
            StartsWith = e.StartsWith,
            SapStartPosition = e.SapStartPosition,
            SapLength = e.SapLength,
            QuantityStartPosition = e.QuantityStartPosition,
            QuantityLength = e.QuantityLength,
            IgnoreLastDigit = e.IgnoreLastDigit,
            DefaultUom = e.DefaultUom
        }).ToList();


        return GeneralResponse<PagedResult<BarCodeSettingDto>>.SuccessResponse(
            new PagedResult<BarCodeSettingDto>
            {
                Data = data,
                PageNumber = res.PageNumber,
                PageSize = res.PageSize,
                TotalRecords = res.TotalRecords
            });
   
    }

    public async Task<GeneralResponse<BarCodeSettingDto>> AddBarCodeSettingAsync(AddBarCodeSettingDto dto)
    {
        var companyId = await companyCache.Get();
        var model = new BarCodeSetting
        {
            TotalLength = dto.TotalLength,
            StartsWith = dto.StartsWith,
            SapStartPosition = dto.SapStartPosition,
            SapLength = dto.SapLength,
            QuantityStartPosition = dto.QuantityStartPosition,
            QuantityLength = dto.QuantityLength,
            IgnoreLastDigit = dto.IgnoreLastDigit,
            DefaultUom = dto.DefaultUom,
            CreatedDate = DateTime.UtcNow,
            CompanyId = companyId??0
        };

       var res =  await AddAsync(model);
        await SaveChangesAsync();

        var result = new BarCodeSettingDto
        {
            BarCodeSettingId = res.BarCodeSettingId,
            TotalLength = res.TotalLength,
            StartsWith = res.StartsWith,
            SapStartPosition = res.SapStartPosition,
            SapLength = res.SapLength,
            QuantityStartPosition = res.QuantityStartPosition,
            QuantityLength = res.QuantityLength,
            IgnoreLastDigit = res.IgnoreLastDigit,
            DefaultUom = res.DefaultUom,
             CreatedDate = DateTime.UtcNow,
            
        };

        return GeneralResponse<BarCodeSettingDto>.SuccessResponse(result);
    }


    public async Task<GeneralResponse<BarCodeSettingDto>> UpdateBarCodeSettingAsync(
     UpdateBarCodeSettingDto dto)
    {
        var entity = await GetByIdAsync(dto.BarCodeSettingId);

        if (entity == null)
            return GeneralResponse<BarCodeSettingDto>
                .FailResponse("Barcode setting not found");

        // Partial Update (Tracking شغال)
        if (dto.TotalLength.HasValue)
            entity.TotalLength = dto.TotalLength.Value;

        if (dto.StartsWith != null)
            entity.StartsWith = dto.StartsWith;

        if (dto.SapStartPosition.HasValue)
            entity.SapStartPosition = dto.SapStartPosition.Value;

        if (dto.SapLength.HasValue)
            entity.SapLength = dto.SapLength.Value;

        if (dto.QuantityStartPosition.HasValue)
            entity.QuantityStartPosition = dto.QuantityStartPosition.Value;

        if (dto.QuantityLength.HasValue)
            entity.QuantityLength = dto.QuantityLength.Value;

        if (dto.IgnoreLastDigit.HasValue)
            entity.IgnoreLastDigit = dto.IgnoreLastDigit.Value;

        if (dto.DefaultUom != null)
            entity.DefaultUom = dto.DefaultUom;

        entity.ModifiedDate = DateTime.UtcNow;

        await SaveChangesAsync(); // 👈 EF هيعمل UPDATE لوحده

        return GeneralResponse<BarCodeSettingDto>.SuccessResponse(
            new BarCodeSettingDto
            {
                BarCodeSettingId = entity.BarCodeSettingId,
                TotalLength = entity.TotalLength,
                StartsWith = entity.StartsWith,
                SapStartPosition = entity.SapStartPosition,
                SapLength = entity.SapLength,
                QuantityStartPosition = entity.QuantityStartPosition,
                QuantityLength = entity.QuantityLength,
                IgnoreLastDigit = entity.IgnoreLastDigit,
                DefaultUom = entity.DefaultUom,
                 ModifiedDate = entity.ModifiedDate,
                  CreatedDate = entity.CreatedDate, 
            });
    }

    public async Task<GeneralResponse<BarCodeSettingDto>> DeleteBarCodeSettingAsync(int id)
    {
        var entity = await GetByIdAsync(id);

        if (entity == null)
            return GeneralResponse<BarCodeSettingDto>
                .FailResponse("Barcode setting not found");

        var res = await DeleteAsync(id);
        await SaveChangesAsync();
        var result = new BarCodeSettingDto
        {
            BarCodeSettingId = res.BarCodeSettingId,
            TotalLength = res.TotalLength,
            StartsWith = res.StartsWith,
            SapStartPosition = res.SapStartPosition,
            SapLength = res.SapLength,
            QuantityStartPosition = res.QuantityStartPosition,
            QuantityLength = res.QuantityLength,
            IgnoreLastDigit = res.IgnoreLastDigit,
            DefaultUom = res.DefaultUom
        };
        return GeneralResponse<BarCodeSettingDto>.SuccessResponse(result);

    }
}

