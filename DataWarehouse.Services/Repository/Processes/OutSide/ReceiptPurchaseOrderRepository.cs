using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class ReceiptPurchaseOrderRepository : BaseRepository<ReceiptPurchaseOrder>, IReceiptPurchaseOrderRepository
{
    public ReceiptPurchaseOrderRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(rpo => rpo.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;


        var query = _context.ReceiptPurchaseOrders
            .AsNoTracking()
            .Where(rpo => rpo.WarehouseId == warehouseId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ReceiptPurchaseOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                ReceiptPurchaseOrderId = iw.ReceiptPurchaseOrderId,
                Status = iw.Status.ToString(),
                UserId = iw.UserId,
                WarehouseId = warehouseId,
                PurchaseOrderId = iw.PurchaseOrderId,
                SupplierId = iw.SupplierId
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>.SuccessResponse(
            new PagedResult<ReceiptPurchaseOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderByWarehouseIdAsync(string userId, AddReceiptPurchaseOrderDTO dto)
    {
        var purchaseOrder = await _context.PurchaseOrders.Include(p=>p.ReceiptPurchaseOrder).FirstOrDefaultAsync(p=>p.PurchaseOrderId == dto.PurchaseOrderId);

        if (purchaseOrder == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder is not found");

        if(purchaseOrder.ReceiptPurchaseOrder != null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder has receipt purchaseOrder already!");


        //var suppler = await _context.Suppliers.FirstOrDefaultAsync(p => p.SupplierId == dto.SupplierId);

        //if (suppler == null)
        //    return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("suppler is not found");


        var mapping = new ReceiptPurchaseOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = purchaseOrder.WarehouseId,
            Comment = dto.Comment,
            SupplierId = purchaseOrder.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            PurchaseOrderId = res.PurchaseOrderId,
            SupplierId = purchaseOrder.SupplierId,
            Comment = res.Comment
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> UpdateReceiptPurchaseOrderAsync(string userId, int receiptPurchaseOrderId, UpdateReceiptPurchaseOrderDTO dto)
    {
        var entity = await _context.ReceiptPurchaseOrders.FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (entity.ReceiptPurchaseOrderId != receiptPurchaseOrderId)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("id not equal receipt purchase order id!");
        }
        if (entity == null)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("not found");
        }

        entity.DueDate = dto.DueDate;
        entity.PostingDate = dto.PostingDate;
        entity.UserId = userId;
        entity.Comment = dto.Comment;
        entity.Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing;

       // entity.SupplierId = dto.SupplierId;

        await _context.SaveChangesAsync();

        var result = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            PurchaseOrderId = entity.PurchaseOrderId,
            SupplierId = entity.SupplierId,
            Comment = entity.Comment
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
    {
        var res = await Query().FirstOrDefaultAsync(rpo => rpo.PurchaseOrderId == purchaseOrderId);

        if (res == null)
           return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("Not Found");

        var mapping = new ReceiptPurchaseOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            PurchaseOrderId = res.PurchaseOrderId,
            SupplierId = res.SupplierId,
             Comment= res.Comment,
        };
        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(po => po.Status == statusEnum)
                .Select(p => new ReceiptPurchaseOrderDTO
                {
                    DueDate = p.DueDate,
                    PostingDate = p.PostingDate,
                    PurchaseOrderId = p.PurchaseOrderId,
                    ReceiptPurchaseOrderId = p.ReceiptPurchaseOrderId,
                    Comment = p.Comment,
                    SupplierId = p.SupplierId,    
                    Status = p.Status.ToString(),
                    // string = enum
                    UserId = p.UserId,

                    WarehouseId = p.WarehouseId,

                }).ToListAsync();

            return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.SuccessResponse(query);

        }
        return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.FailResponse("Not found any purchase to this status now!");
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(rpo => rpo.UserId == userId).ToListAsync();
    }

    public async Task<ReceiptPurchaseOrder?> GetWithItemsAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.ReceiptPurchaseOrderItems)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetWithItemsAndBatchesAsync(int receiptPurchaseOrderId)
    {
        var result = await _context.ReceiptPurchaseOrders
     .AsNoTracking()
     .Where(r => r.ReceiptPurchaseOrderId == receiptPurchaseOrderId)
     .Select(r => new ReceiptPurchaseOrderDTO
     {
         ReceiptPurchaseOrderId = r.ReceiptPurchaseOrderId,
         Status = r.Status.ToString(),
         PostingDate = r.PostingDate,
         DueDate = r.DueDate,
         UserId = r.UserId,
         Comment = r.Comment,
         PurchaseOrderId = r.PurchaseOrderId,
         WarehouseId = r.WarehouseId,
         SupplierId = r.SupplierId,
         SupplierName = r.Supplier.SupplierName,
         WarehouseCode = r.Warehouse.WarehouseCode,

         Items = r.ReceiptPurchaseOrderItems.Select(i => new ReceiptPurchaseOrderItemDTO
         {
             ReceiptPurchaseOrderItemId = i.ReceiptPurchaseOrderItemId,
             Quantity = i.Quantity,
             UoMEntry = i.UoMEntry,
             BarCode = i.BarCode,
             UnitPrice = i.UnitPrice,
             ErrorMessage = i.ErrorMessage,
             Comment = i.Comment,
             ReceiptPurchaseOrderId = i.ReceiptPurchaseOrderId,
             ItemId = i.ItemId,

             Batches = i.ReceiptPurchaseOrderBatches.Select(b => new ReceiptPurchaseOrderBatchDTO
             {
                 ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                 ReceiptPurchaseOrderItemId = b.ReceiptPurchaseOrderItemId,
                 Quantity = b.Quantity,
                 Comment = b.Comment,
                 BatchNumber = b.BatchNumber,
                 ExpiryDate = b.ExpiryDate
             }).ToList()
         }).ToList()
     })
     .FirstOrDefaultAsync();




        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }


    public async Task<ReceiptPurchaseOrder?> GetWithPurchaseOrderAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.PurchaseOrder)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<ReceiptPurchaseOrder?> GetWithWarehouseAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.Warehouse)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(rpo => rpo.CreatedAt >= startDate && rpo.CreatedAt <= endDate).ToListAsync();
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetPendingReceiptsAsync()
    {
        return await Query().Where(rpo => rpo.Status == GeneralStatus.Processing).ToListAsync();
    }
}
