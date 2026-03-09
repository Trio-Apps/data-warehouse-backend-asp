using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionReceiptRepository : BaseRepository<ProductionReceipt>, IProductionReceiptRepository
{
    private readonly ISapCache sapCache;

    public ProductionReceiptRepository(ISapCache sapCache, DataWarehouseDbContext context) : base(context)
    {
        this.sapCache = sapCache;
    }

    public async Task<GeneralResponse<IEnumerable<ProductionReceiptDTO>>> GetByProductionOrderItemIdAsync(int productionOrderItemId)
    {
        var res = await Query().Where(pr => pr.ProductionOrderItemId == productionOrderItemId).ToListAsync();

        return GeneralResponse < IEnumerable < ProductionReceiptDTO >>.SuccessResponse( res.Select(pr => new ProductionReceiptDTO { ProductionReceiptId = pr.ProductionOrderItemId, ProducedQuantity = pr.ProducedQuantity, 
            BatchNumber =pr.BatchNumber, Comment =pr.Comment,
            ExpiryDate=pr.ExpiryDate, ProductionOrderItemId =pr.ProductionOrderItemId }));
    }
    
    public async Task<GeneralResponse<ProductionReceiptDTO>>
        AddByProductionItemIdAsync(int productionOrderItemId,AddProductionReceiptDTO pr)
    {

        var sapId = await sapCache.Get();

        if (productionOrderItemId != pr.ProductionOrderItemId)
            return GeneralResponse<ProductionReceiptDTO>.FailResponse("id doesn't equal ProductionOrderItemId ");


        var productionOrderItem = await _context.ProductionOrderItems.FirstOrDefaultAsync(e=>e.ProductionOrderItemId == pr.ProductionOrderItemId);

        //if (productionOrderItem.Status != ProductionItemStatus.Released)
        //{
        //    return GeneralResponse<ProductionReceiptDTO>.FailResponse("Production must be released to add batch!..");

        //}
        bool flag = await CheckItemHasBatchsAsync(productionOrderItem.ItemId);
        if (!flag)
            return GeneralResponse<ProductionReceiptDTO>.FailResponse("this order you can't add any batch for it.");

        var mapping = new ProductionReceipt
        {

            ProducedQuantity = pr.ProducedQuantity,
            Comment = pr.Comment,
            ExpiryDate = pr.ExpiryDate,
            ProductionOrderItemId = pr.ProductionOrderItemId,
            BatchNumber = Guid.NewGuid().ToString()

        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ProductionReceiptDTO
        {
            ProductionReceiptId = res.ProductionOrderItemId,
            ProducedQuantity = res.ProducedQuantity,
            BatchNumber = res.BatchNumber,
            Comment = res.Comment,
            ExpiryDate = res.ExpiryDate,
            ProductionOrderItemId = res.ProductionOrderItemId

        };


        return GeneralResponse<ProductionReceiptDTO>.SuccessResponse(model);
    }

    public async Task<bool> CheckItemHasBatchsAsync(int itemId)
    {
        var item = await _context.Items.FirstOrDefaultAsync(e=>e.ItemId == itemId);
        return  item.BatchNumbers;
    }

    public async Task<GeneralResponse<PagedResult<ProductionReceiptDTO>>> GetByProductionItemIdWithPaginationAsync(int productionItemId, int skip, int pageSize)

    {
        var res = await PaginationWithFilterAsync(e => e.ProductionOrderItemId == productionItemId, skip, pageSize);

        var data = res.Data.Select(e => new ProductionReceiptDTO
        {
            ProductionReceiptId = e.ProductionOrderItemId,
            ProducedQuantity = e.ProducedQuantity,
            BatchNumber = e.BatchNumber,
            Comment = e.Comment,
            ExpiryDate = e.ExpiryDate,
            ProductionOrderItemId = e.ProductionOrderItemId
        }).ToList();


        return GeneralResponse<PagedResult<ProductionReceiptDTO>>.SuccessResponse(new PagedResult<ProductionReceiptDTO>
        {
            Data = data,
            PageNumber = res.PageNumber,
            PageSize = res.PageSize,
            TotalRecords = res.TotalRecords
        });

    }



    public async Task<GeneralResponse<ProductionReceiptDTO>> UpdateProductionReceiptAsync(int productionReceiptId, UpdateProductionReceiptDTO dto)
    {
        // 1️⃣ Get existing Company auth (record الوحيد)
        var entity = await _context.ProductionReceipts.FirstOrDefaultAsync(e => e.ProductionReceiptId == dto.ProductionReceiptId);
        if (entity == null)
        {
            return GeneralResponse<ProductionReceiptDTO>.FailResponse("not found");
        }
        if (entity.ProductionReceiptId != productionReceiptId)
        {
            return GeneralResponse<ProductionReceiptDTO>.FailResponse("id not equal batch id!");
        }
       

        // 2️⃣ Update fields


      
        entity.ProducedQuantity = dto.ProducedQuantity;
        entity.Comment = dto.Comment;
         entity.ExpiryDate = dto.ExpiryDate;
      

        // Company.IsActive = dto.IsActive;

        // 3️⃣ Save changes
        await _context.SaveChangesAsync();

        // 4️⃣ Map to DTO
        var result = new ProductionReceiptDTO
        {
            ProductionReceiptId = entity.ProductionOrderItemId,
            ProducedQuantity = entity.ProducedQuantity,
            BatchNumber = entity.BatchNumber,
            Comment = entity.Comment,
            ExpiryDate = entity.ExpiryDate,
            ProductionOrderItemId = entity.ProductionOrderItemId

        };

        return GeneralResponse<ProductionReceiptDTO>.SuccessResponse(result);
    }



    //public async Task<decimal> GetTotalQuantityByProductionOrderItemIdAsync(int productionOrderItemId)
    //{
    //    return await Query()
    //        .Where(pr => pr.ProductionOrderItemId == productionOrderItemId);
    //        //.SumAsync(pr => pr.Quantity);
    //}
}

