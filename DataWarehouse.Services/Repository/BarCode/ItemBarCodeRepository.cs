using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Services.Repository.Based;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.BarCode;

public class ItemBarCodeRepository : BaseRepository<ItemBarCode>, IItemBarCodeRepository
{
    private readonly ISapCache sapCache;

    public ItemBarCodeRepository(DataWarehouseDbContext context,ISapCache sapCache) : base(context)
    {
        this.sapCache = sapCache;
    }


    public async Task<GeneralResponse<BarCodeDto>> AddBarCodeForItem(
     int itemId,
     AddBarCodeDto dto)
    {
        var sapId = await sapCache.Get();
        var found = await ExistsByBarCodeAsync(dto.BarCode);
        if(found)
        {
            return GeneralResponse<BarCodeDto>.FailResponse("This barCode is found already");
        }
        var setting = await CheckCodeValidation(dto.BarCode);

        if (!setting)
            return GeneralResponse<BarCodeDto>
                .FailResponse("Barcode does not match any configuration");

        var checkOnUom = await ExistUomInItem(itemId, dto.UoMEntry);


        if (!checkOnUom)
            return GeneralResponse<BarCodeDto>
               .FailResponse("this uom doesn't valid to this item");

        var model = new ItemBarCode
        {
            ItemId = itemId,
            BarCode = dto.BarCode,
            UoMEntry = dto.UoMEntry,
            FreeText = dto.FreeText,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            SapId = sapId ?? 0,
            SapFlag = false,
            AbsEntry = 0,

        };

       var res =  await AddAsync(model);

        await SaveChangesAsync();
        var result = new BarCodeDto
        {
            BarCode = res.BarCode,
            ItemBarCodeId = res.ItemBarCodeId,
            UoMEntry = res.UoMEntry,
             FreeText= res.FreeText
            
        
        };

        return GeneralResponse<BarCodeDto>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<PagedResult<BarCodeDto>>> GetByItemIdAsync(int itemId,int pageNumber, int pageSize, string? barCode)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ItemBarCodes.Where(b => b.ItemId == itemId && b.IsActive);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(barCode))
        {
            query = query.Where(iw =>
                iw.BarCode.Contains(barCode));
        }

       

        var totalRecords = await query.CountAsync();

        var data = await query
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new BarCodeDto
            {
                BarCode = e.BarCode,
                FreeText = e.FreeText,
                ItemBarCodeId = e.ItemBarCodeId,
                UoMEntry = e.UoMEntry,
               UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
                

            })
            .ToListAsync();


        var mapping = new PagedResult<BarCodeDto>(){ 
           Data = data,
           PageNumber = pageNumber,
           PageSize = pageSize,
           TotalRecords = totalRecords
        
        };
       
        return GeneralResponse<PagedResult<BarCodeDto>>.SuccessResponse(mapping);
    
    }


    //

    public async Task<GeneralResponse<PagedResult<BarCodeDto>>> GetByItemIdOrNoAsync(int pageNumber, int pageSize, int? itemId, string? barCode)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ItemBarCodes.Where(b => b.IsActive);

        if (itemId != null)
        {
            query = query.Where(b => b.ItemId == itemId);
        }
        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(barCode))
        {
            query = query.Where(iw =>
                iw.BarCode.Contains(barCode));
        }

     

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new BarCodeDto
            {
                BarCode = e.BarCode,
                FreeText = e.FreeText,
                ItemBarCodeId = e.ItemBarCodeId,
                UoMEntry = e.UoMEntry,
                 ItemCode = e.Item.ItemCode,
                  ItemName  = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
            })
            .ToListAsync();


        var mapping = new PagedResult<BarCodeDto>()
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords

        };

        return GeneralResponse<PagedResult<BarCodeDto>>.SuccessResponse(mapping);

    }


    public async Task<GeneralResponse<BarCodeDto>> GetBarCodeByBarCodeIdAsync(int barCodeId)
    {
        var res = await GetByIdAsync(barCodeId);

        if (res == null && !res.IsActive)
        {
            return GeneralResponse<BarCodeDto>.FailResponse("this barCode is not found");
        }
        var mapping = new BarCodeDto
        {
            BarCode = res.BarCode,
            FreeText = res.FreeText,
            ItemBarCodeId = res.ItemBarCodeId,
           
            UoMEntry = res.UoMEntry
        };



        return GeneralResponse<BarCodeDto>.SuccessResponse(mapping);

    }

    public async Task<GeneralResponse<BarCodeDto>> GetWithItemAsync(int BarCodeId)
    {
        var res = await QueryIncluding(false, b => b.Item).Where(e => e.IsActive)
            .FirstOrDefaultAsync(b => b.ItemBarCodeId == BarCodeId);
        if(res == null)
        {
            return GeneralResponse<BarCodeDto>.FailResponse("the BarCode Not Found");
        }

        return GeneralResponse<BarCodeDto>.SuccessResponse( new BarCodeDto
        {
            BarCode = res.BarCode,
            
            FreeText = res.FreeText,
            ItemBarCodeId = res.ItemBarCodeId,
           
            UoMEntry = res.UoMEntry
        });

    }

    public async Task<bool> ExistsByBarCodeAsync(string barCode)
    {
        return await ExistsAsync(b => b.BarCode == barCode);
    }


    //
    public async Task<GeneralResponse<BarCodeDto>> UpdateBarCodeAsync(int barCodeId,
   UpdateBarCodeDto dto)
    {
        var entity = await GetByIdAsync(barCodeId);

        if (entity == null)
            return GeneralResponse<BarCodeDto>
                .FailResponse("Barcode setting not found");

        // Partial Update (Tracking شغال)
        if (dto.BarCode != null)
            entity.BarCode = dto.BarCode;

        if (dto.UoMEntry.HasValue)
            entity.UoMEntry = dto.UoMEntry.Value;

        if (dto.FreeText != null)
            entity.FreeText = dto.FreeText;



        var setting = await CheckCodeValidation(dto.BarCode);

        if (!setting)
            return GeneralResponse<BarCodeDto>
                .FailResponse("Barcode does not match any configuration");

         entity.IsActive = false;

        if (!entity.SapFlag)
            await DeleteAsync(entity.ItemBarCodeId);


        var model = new AddBarCodeDto
        {
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            FreeText = entity.FreeText,
        };

        return await AddBarCodeForItem(entity.ItemId, model);

    }


    public async Task<GeneralResponse<BarCodeDto>> DeleteBarCodeAsync(int barCodeId)
    {
        var entity = await GetByIdAsync(barCodeId);


        if (entity == null)
            return GeneralResponse<BarCodeDto>
                .FailResponse("Barcode setting not found");

        entity.IsActive = false;

        if (!entity.SapFlag)
            await DeleteAsync(entity.ItemBarCodeId);

        await SaveChangesAsync();

        var mapping = new BarCodeDto
        {
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry
        };
     
        return GeneralResponse<BarCodeDto>.SuccessResponse(mapping);

    }


    public async Task<GeneralResponse<List<ItemUomGroupDto>>> GetItemUomGroupAsync(int itemId)
    {
        var item = await _context.Items.FindAsync(itemId);
        if(item == null)
                return GeneralResponse<List<ItemUomGroupDto>>
                    .FailResponse("this item is not found");


        var data = await _context.Items
            .AsNoTracking()
            .Where(i => i.ItemId == itemId)
            .SelectMany(i => i.ItemUomGroups)
            .Select(e => new ItemUomGroupDto
            {
                BaseQty = e.BaseQty,
                UomCode = e.UomCode,
                UomEntry = e.UomEntry
            })
            .ToListAsync();

        if (!data.Any())
            return GeneralResponse<List<ItemUomGroupDto>>
                .FailResponse("No UoM Groups found for this item");

        return GeneralResponse<List<ItemUomGroupDto>>
            .SuccessResponse(data);
    }

    #region helper
    private async Task<bool> CheckCodeValidation(string barCode)
    {
        if (string.IsNullOrWhiteSpace(barCode))
            return false;

        var settings = await _context.BarCodeSettings.ToListAsync();

        foreach (var setting in settings)
        {
            var code = barCode;


            if (!string.IsNullOrEmpty(setting.StartsWith) &&
                 !code.StartsWith(setting.StartsWith)) continue;


          
            if (code.Length != setting.SapLength)
                    continue;
            

                return true;
        }

        return false;
    }
    private bool IsValidSegment(string code, int start, int length)
    {
        if (start + length > code.Length)
            return false;

        return true;
    }
    private async Task<bool> ExistUomInItem(int itemId, int uomEntry)
    {
        bool hasUom = await _context.ItemUomGroups
    .AnyAsync(u =>
        u.ItemId == itemId &&
        u.UomEntry == uomEntry );

        return hasUom;
    }



  




    //

    #endregion



}

