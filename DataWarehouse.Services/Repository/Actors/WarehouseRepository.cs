using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Actors;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    private readonly IAuthServices authServices;
    private readonly IRoleServices roleServices;
    private readonly ISapSettingsRepository sapRepo;
    private readonly ICompanyCache companyCache;
    private readonly ISapCache sapCache;

    public WarehouseRepository(IAuthServices authServices, DataWarehouseDbContext context, IRoleServices roleServices, ISapSettingsRepository sapRepo, ICompanyCache companyCache, ISapCache sapCache) : base(context)
    {
        this.authServices = authServices;
        this.roleServices = roleServices;
        this.sapRepo = sapRepo;
        this.companyCache = companyCache;
        this.sapCache = sapCache;
    }

    public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehouses(string userId, IList<string> roleNames)
    {

        var roles = await roleServices.GetUserRolesAsync(userId);


        var checkRole = await authServices.GetLoginContextAsync(userId, roles);

        var sapId = await sapCache.Get();

        List<WarehouseDTO> res;
        if (checkRole.IsGlobal)
        {
            var companyId = await companyCache.Get();

            if (sapId == null)
            {

                var sap = await _context.Saps.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();
                if (sap == null)
                    return GeneralResponse<IEnumerable<WarehouseDTO>>.FailResponse("Not Found Any Warehouses In This Company");

                await sapCache.UpdateSapUserClaimAsync(sap.SapId.ToString());
                sapId = sap.SapId;
            }

            res = await _context.Warehouses.Where(x => x.SapId == sapId).Select(d =>

            new WarehouseDTO
            {
                SapId = d.SapId,
                WarehouseId = d.WarehouseId,
                WarehouseName = d.WarehouseName,
            }).ToListAsync();
        }
       
        else
        {

            res = await _context.UserWarehouses.AsNoTracking().Where(uw=> uw.UserId == userId && uw.Warehouse.SapId==sapId).Select(d =>

            new WarehouseDTO
            {
                SapId = d.Warehouse.SapId,
                WarehouseId = d.WarehouseId,
                WarehouseName = d.Warehouse.WarehouseName,
            }).ToListAsync();



        }

           

        return  GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
    }
    public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetAllWarehousesForEmployeeAsync(string userId)
    {
        var sapId = await sapCache.Get();
        var res = _context.UserWarehouses.AsNoTracking().Where(x => x.UserId == userId).Select(d =>
        new WarehouseDTO
        {
            WarehouseId = d.WarehouseId,
            WarehouseName = d.Warehouse.WarehouseName
        });

        return GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
    }
   
    public async Task<int?> GetSap()
    {
      

        return await sapCache.Get();
    }
    

   public async Task<GeneralResponse<IEnumerable<WarehouseDTO>>> GetSapByIdAsync(int sapId)
    {

        var res = await _context.Warehouses.AsNoTracking().Where(w => w.SapId == sapId)
            .Select(e => new WarehouseDTO
            {
                WarehouseId = e.WarehouseId,
                WarehouseName = e.WarehouseName,
                SapId = e.SapId
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<WarehouseDTO>>.SuccessResponse(res);
      
    }

    public async Task<Warehouse?> GetByNameAsync(string warehouseCode)
    {
        return await Query().FirstOrDefaultAsync(w => w.WarehouseCode == warehouseCode);
    }


    public async Task<Warehouse?> GetWithUserWarehousesAsync(int warehouseId)
    {
        return await QueryIncluding(false, w => w.UserWarehouses)
            .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);
    }

    // by warehouse id only 

    public async Task<GeneralResponse<IEnumerable<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
       int warehouseId)
    {

      

        var query = await _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.WarehouseId == warehouseId)
            .Select(iw => new ItemResponseDTO
            {
                ItemId = iw.Item.ItemId,
                ItemCode = iw.Item.ItemCode,
                ItemName = iw.Item.ItemName,
                PurchasePrice = iw.Item.PurchasePrice,
                SalesPrice = iw.Item.SalesPrice,
                UoM = iw.Item.UoM,
                UpdateDate = iw.Item.UpdateDate,
                WarehouseCode = iw.WarehouseCode,
                InStock = iw.InStock
            }).ToListAsync();

        return GeneralResponse<IEnumerable<ItemResponseDTO>>.SuccessResponse(query);

    }



    public async Task<GeneralResponse<PagedResult<ItemResponseDTO>>> GetItemsOfWarehouseAsync(
        int warehouseId,
         int pageNumber,
    int pageSize)
    {

        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
    pageSize   = pageSize   <= 0 ? 10 : pageSize;

    var query = _context.WarehouseItems
        .AsNoTracking()
        .Where(iw => iw.WarehouseId == warehouseId)
        .Select(iw => new ItemResponseDTO
        {
            ItemId        = iw.Item.ItemId,
            ItemCode      = iw.Item.ItemCode,
            ItemName      = iw.Item.ItemName,
            PurchasePrice = iw.Item.PurchasePrice,
            SalesPrice    = iw.Item.SalesPrice,
            UoM           = iw.Item.UoM,
            UpdateDate    = iw.Item.UpdateDate,
            WarehouseCode = iw.WarehouseCode,
            InStock       = iw.InStock
        });

    var totalRecords = await query.CountAsync();

    var data = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

        return GeneralResponse<PagedResult<ItemResponseDTO>>.SuccessResponse(new PagedResult<ItemResponseDTO>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Data = data
        });

    }


    //GetItemsByWarehouseIdWithItemCodeAndName
    public async Task<GeneralResponse<PagedResult<ItemResponseDTO>>>
      GetItemsByWarehouseIdWithItemCodeAndName(
          int warehouseId,
          string? itemCode,
          string? itemName,
          int pageNumber,
          int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.WarehouseItems
            .AsNoTracking()
            .Where(iw => iw.WarehouseId == warehouseId);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(itemCode))
        {
           
            query = query.Where(iw =>
                iw.Item.ItemCode.Contains(itemCode));
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            query = query.Where(iw =>
        iw.Item.ItemName.Contains(itemName));
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(iw => iw.Item.ItemName) // مهم جدًا مع Pagination
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ItemResponseDTO
            {
                ItemId = iw.Item.ItemId,
                ItemCode = iw.Item.ItemCode,
                ItemName = iw.Item.ItemName,
                PurchasePrice = iw.Item.PurchasePrice,
                SalesPrice = iw.Item.SalesPrice,
                UoM = iw.Item.UoM,
                UpdateDate = iw.Item.UpdateDate,
                WarehouseCode = iw.WarehouseCode,
                InStock = iw.InStock
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ItemResponseDTO>>.SuccessResponse(
            new PagedResult<ItemResponseDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<bool> ExistsByNameAsync(string warehouseName)
    {
        return await ExistsAsync(w => w.WarehouseName == warehouseName);
    }
}
