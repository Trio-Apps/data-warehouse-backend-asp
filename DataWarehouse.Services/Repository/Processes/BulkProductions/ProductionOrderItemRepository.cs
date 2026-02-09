using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderItemRepository : BaseRepository<ProductionOrderItem>, IProductionOrderItemRepository
{
    private readonly ISapCache sapCache;

    public ProductionOrderItemRepository(ISapCache sapCache, DataWarehouseDbContext context) : base(context)
    {
        this.sapCache = sapCache;
    }

    public async Task<IEnumerable<ProductionOrderItemDTO>> GetByProductionItemByProductionOrderIdAsync(int productionOrderId)
    {

        var res = await Query().Where(poi => poi.ProductionOrderId == productionOrderId).ToListAsync();
        
        return res.Select(e=> new ProductionOrderItemDTO { ItemId = e.ItemId, AbsoluteEntry =e.AbsoluteEntry,Status = GetEnumString(e.Status),  ErrorMessage = e.ErrorMessage
        ,PlannedQuantity=e.PlannedQuantity, ProducedQuantity = e.ProducedQuantity??0, ProductionOrderItemId =e.ProductionOrderItemId,ProductionOrderId =e.ProductionOrderId, ProcessedAt =e.ProcessedAt});
    }
    public async Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>>
     GetByProductionItemByProductionOrderIdWithPaginationAsync(int productionOrderId,string? status, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ProductionOrderItems.AsNoTracking().Where(b => b.ProductionOrderId == productionOrderId);

        // 🔹 Filtering

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = Enum.Parse<GeneralItemStatus>(status, ignoreCase: true);
            query = query.Where(iw => iw.Status == statusEnum);
        }



        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new ProductionOrderItemDTO
        {
            ItemId = e.ItemId,
            AbsoluteEntry = e.AbsoluteEntry,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            PlannedQuantity = e.PlannedQuantity,
            ProducedQuantity = e.ProducedQuantity ?? 0,
            ProductionOrderItemId = e.ProductionOrderItemId,
            ProductionOrderId = e.ProductionOrderId,
            ProcessedAt = e.ProcessedAt
        }).ToList();


        return GeneralResponse<PagedResult<ProductionOrderItemDTO>>.SuccessResponse(new PagedResult<ProductionOrderItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });


        
    }
    // create
    public async Task<GeneralResponse<ProductionOrderItemDTO>> AddProductionItemByProductionOrderIdAsync(int productionOrderid,
           AddProductionOrderItemDTO dto)
    {
        var sapId = await sapCache.Get();


        var mapping = new ProductionOrderItem
        {
           Status = GeneralItemStatus.Planned,
           ProductionOrderId = dto.ProductionOrderId,
           ItemId = dto.ItemId,
           CreatedAt = DateTime.UtcNow,
           PlannedQuantity = dto.PlannedQuantity,
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ProductionOrderItemDTO
        {
            ProductionOrderId = res.ProductionOrderId,
             PlannedQuantity=res.PlannedQuantity,
              Status = GetEnumString(res.Status),
               ItemId = res.ItemId,
                ProductionOrderItemId=res.ProductionOrderItemId,
         
        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(model);
    }

    // update
    public async Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync(int productionItemId,bool? isRecevied, UpdateProductionOrderItemDTO dto)
    {

        // 1️⃣ Get existing Company auth (record الوحيد)
        var entity = await _context.ProductionOrderItems.FirstOrDefaultAsync(e => e.ProductionOrderItemId == dto.ProductionOrderItemId);

        if (entity.ProductionOrderItemId != productionItemId)
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("id not equal production order id!");
        }
        if (entity == null)
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("not found");
        }
        if(entity.Status != GeneralItemStatus.Released)
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production must be released to be edited!..");

        }

        // 2️⃣ Update fields

        entity.PlannedQuantity = dto.PlannedQuantity;
        entity.ProducedQuantity = dto.ProducedQuantity;

        if (isRecevied == true)
            entity.Status = GeneralItemStatus.Received;
        // Company.IsActive = dto.IsActive;

        // 3️⃣ Save changes
        await _context.SaveChangesAsync();

        // 4️⃣ Map to DTO
        var result = new ProductionOrderItemDTO
        {
            ItemId = entity.ItemId,
            AbsoluteEntry = entity.AbsoluteEntry,
            Status = GetEnumString(entity.Status),
            ErrorMessage = entity.ErrorMessage
        ,
            PlannedQuantity = entity.PlannedQuantity,
            ProducedQuantity = entity.ProducedQuantity ?? 0,
            ProductionOrderItemId = entity.ProductionOrderItemId,
            ProductionOrderId = entity.ProductionOrderId,
            ProcessedAt = entity.ProcessedAt,

        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(result);
    }


    
    private string GetEnumString(GeneralItemStatus status)
    {
        switch (status)
        {
            case GeneralItemStatus.Draft:
                return "Draft";
            case GeneralItemStatus.Planned:
                return "Planned";
            case GeneralItemStatus.Released:
                return "Released";
            case GeneralItemStatus.Closed:
                return "Closed";
            case GeneralItemStatus.Failed:
                return "Failed";
            default:
                return "Unknown";
        }
    }



}

