using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.PurchaseOrderRepo
{
    public class FinishedGoodPurchaseOrderRepository : BaseRepository<WarehouseItem>, IFinishedGoodPurchaseOrderRepository
    {
        private readonly ISapCache sapCache;

        public FinishedGoodPurchaseOrderRepository(ISapCache sapCache, DataWarehouseDbContext context) : base(context)
        {
            this.sapCache = sapCache;
        }

        //public async Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetByWarehouseIdAsync(int warehouseId)
        //{
        //    return GeneralResponse<IEnumerable<WarehouseItemDto>>.SuccessResponse(await Query()
        //        .Where(iw => iw.FinishedGood && !iw.HasActiveBOM && iw.WarehouseId == warehouseId)
        //        .Select(wi => new WarehouseItemDto
        //        {
        //            WarehouseItemId = wi.WarehouseItemId,
        //            ItemId = wi.ItemId,
        //            WarehouseId = wi.WarehouseId,
        //            ItemName = wi.Item.ItemName,
        //            ItemCode = wi.Item.ItemCode,
        //            WarehouseCode = wi.Warehouse.WarehouseCode,
        //            InStock = wi.InStock,
        //            MinStock = wi.MinStock
        //        })
        //        .ToListAsync());
        //}
        public async Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetByWarehouseIdAsync(int warehouseId)
        {
            var sapId = await sapCache.Get();
            // 1) هات بيانات المستودع مرة واحدة
            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.WarehouseId == warehouseId)
                .Select(w => new { w.WarehouseId, w.WarehouseCode })
                .SingleOrDefaultAsync();

            if (warehouse == null)
                return GeneralResponse<IEnumerable<WarehouseItemDto>>.SuccessResponse(Enumerable.Empty<WarehouseItemDto>());

            // 2) Left join Items مع WarehouseItems (لنفس المستودع فقط)
            var query =
                from i in _context.Items.AsNoTracking().Where(it=>it.SapId == sapId)
                join wi in _context.WarehouseItems.AsNoTracking()
                        .Where(x => x.WarehouseId == warehouseId )
                    on i.ItemId equals wi.ItemId into wiGroup
                from wi in wiGroup.DefaultIfEmpty()
                where wi == null && i.ProcurementType == "bom_Buy" && i.PurchaseItem && i.Valid
                select new WarehouseItemDto
                {
                    WarehouseItemId = 0,              // مفيش row
                    ItemId = i.ItemId,
                    WarehouseId = warehouse.WarehouseId,
                    ItemName = i.ItemName,
                    ItemCode = i.ItemCode,

                    WarehouseCode = warehouse.WarehouseCode,

                    InStock = 0,
                    MinStock = 0
                };

            var result = await query.ToListAsync();
            return GeneralResponse<IEnumerable<WarehouseItemDto>>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<PagedResult<WarehouseItemDto>>> GetByWarehouseIdAsync(
    int warehouseId,
    string? search = null,
    int pageNumber = 1,
    int pageSize = 20)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 20;

            var sapId = await sapCache.Get();

            // 1) هات بيانات المستودع مرة واحدة
            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.WarehouseId == warehouseId)
                .Select(w => new { w.WarehouseId, w.WarehouseCode })
                .SingleOrDefaultAsync();

            if (warehouse == null)
            {
                return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(new PagedResult<WarehouseItemDto>
                {
                    Data = Array.Empty<WarehouseItemDto>(),
                     TotalRecords = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }

            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            // 2) Left join Items مع WarehouseItems (لنفس المستودع فقط)
            var query =
                from i in _context.Items.AsNoTracking().Where(it => it.SapId == sapId)
                join wi in _context.WarehouseItems.AsNoTracking()
                        .Where(x => x.WarehouseId == warehouseId)
                    on i.ItemId equals wi.ItemId into wiGroup
                from wi in wiGroup.DefaultIfEmpty()
                where wi == null
                      && i.PurchaseItem
                      && i.Valid
                      && (
                            search == null
                            || (i.ItemCode != null && EF.Functions.Like(i.ItemCode, $"%{search}%"))
                            || (i.ItemName != null && EF.Functions.Like(i.ItemName, $"%{search}%"))
                         )
                select new WarehouseItemDto
                {
                    WarehouseItemId = 0,
                    ItemId = i.ItemId,
                    WarehouseId = warehouse.WarehouseId,
                    ItemName = i.ItemName,
                    ItemCode = i.ItemCode,
                    WarehouseCode = warehouse.WarehouseCode,
                    InStock = 0,
                    MinStock = 0
                };

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.ItemName)
                .ThenBy(x => x.ItemCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<WarehouseItemDto>
            {
                Data = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(result);
        }
     
        
        public async Task<IEnumerable<WarehouseItem>> GetByItemIdAsync(int itemId)
        {
            return await Query().Where(fgi => fgi.ItemId == itemId).ToListAsync();
        }

        public async Task<WarehouseItem?> GetByItemAndWarehouseAsync(int itemId, int warehouseId)
        {
            return await Query().FirstOrDefaultAsync(fgi => fgi.ItemId == itemId && fgi.WarehouseId == warehouseId);
        }

        public async Task<GeneralResponse<PagedResult<WarehouseItemDto>>>
        GetFinishedGoodBomItemsByWarehouseIdAsync(
            int warehouseId,
            int? itemId,
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.WarehouseItems
                .AsNoTracking()
                .Where(iw => iw.FinishedGood && !iw.HasActiveBOM && iw.WarehouseId == warehouseId);

            // 🔹 Filtering


            if (itemId != null)
            {
                query = query.Where(f => f.ItemId == itemId);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                //  .OrderBy(iw => iw.Item.ItemName) // مهم جدًا مع Pagination
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(wi => new WarehouseItemDto
                {
                    WarehouseItemId = wi.WarehouseItemId,
                    ItemId = wi.ItemId,
                    WarehouseId = wi.WarehouseId,
                    ItemName = wi.Item.ItemName,
                    ItemCode = wi.Item.ItemCode,
                    WarehouseCode = wi.Warehouse.WarehouseCode,
                    InStock = wi.InStock,
                    MinStock = wi.MinStock

                })
                .ToListAsync();

            return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(
                new PagedResult<WarehouseItemDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    Data = data
                });
        }


        public async Task<WarehouseItem?> GetWithItemAsync(int finishedGoodItemId)
        {
            return await QueryIncluding(false, fgi => fgi.Item)
                .FirstOrDefaultAsync(fgi => fgi.WarehouseItemId == finishedGoodItemId);
        }

        public async Task<WarehouseItem?> GetWithWarehouseAsync(int finishedGoodItemId)
        {
            return await QueryIncluding(false, fgi => fgi.Warehouse)
                .FirstOrDefaultAsync(fgi => fgi.WarehouseItemId == finishedGoodItemId);
        }

        public async Task<IEnumerable<WarehouseItem>> GetActiveItemsAsync()
        {
            return await Query().Where(fgi => fgi.IsActive).ToListAsync();
        }
    }
}
