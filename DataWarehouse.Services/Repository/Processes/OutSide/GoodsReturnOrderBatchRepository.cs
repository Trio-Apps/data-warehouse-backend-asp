using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class GoodsReturnOrderBatchRepository : BaseRepository<GoodsReturnOrderBatch>, IGoodsReturnOrderBatchRepository
{
    private readonly IBaseProcessesRepository<GoodsReturnOrderBatch> baseProcesses;

    public GoodsReturnOrderBatchRepository(IBaseProcessesRepository<GoodsReturnOrderBatch> baseProcesses, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

  
    public async Task<GeneralResponse<PagedResult<GoodsReturnOrderBatchDTO>>> GetByGoodsReturnOrderItemIdWithPaginationAsync(int goodsReturnOrderItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.GoodsReturnOrderBatches
            .AsNoTracking()
            .Where(b => b.GoodsReturnOrderItemId == goodsReturnOrderItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new GoodsReturnOrderBatchDTO
            {
                GoodsReturnOrderBatchId = b.GoodsReturnOrderBatchId,
                GoodsReturnOrderItemId = b.GoodsReturnOrderItemId,
                ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();


        return GeneralResponse<PagedResult<GoodsReturnOrderBatchDTO>>.SuccessResponse(
            new PagedResult<GoodsReturnOrderBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }
    public async Task<GeneralResponse<IEnumerable<GoodsReturnOrderBatchDTO>>> GetByGoodsReturnOrderItemIdAsync(
     int goodsReturnOrderItemId)
    {
        var res = await baseProcesses.GetOrderBatchesAsync<
            GoodsReturnOrderItem,
            GoodsReturnOrderBatch,
            GoodsReturnOrderBatchDTO>(
            orderItemId: goodsReturnOrderItemId,
            orderItemSelector: i => i.GoodsReturnOrderItemId == goodsReturnOrderItemId,
            orderItemSet: _context.GoodsReturnOrderItems,

            batchItemSelector: b => b.GoodsReturnOrderItemId == goodsReturnOrderItemId,
            batchSet: _context.GoodsReturnOrderBatches,

            map: b => new GoodsReturnOrderBatchDTO
            {
                GoodsReturnOrderBatchId = b.GoodsReturnOrderBatchId,
                GoodsReturnOrderItemId = b.GoodsReturnOrderItemId,
                ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }
        );

        return res;
    }
    public async Task<GeneralResponse<GoodsReturnOrderBatchDTO>> AddByGoodsReturnOrderItemIdAsync(int goodsReturnOrderItemId, GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<GoodsReturnOrder, GoodsReturnOrderItem, GoodsReturnOrderBatch>(
            orderItemId: goodsReturnOrderItemId,
            processType: ProcessType.GoodsReturn,
            dto: dto,
            orderSet: _context.GoodsReturnOrders,
            // ✅ لازم predicate على العمود الحقيقي
            orderItemSelector: i => i.GoodsReturnOrderItemId == goodsReturnOrderItemId,
            orderItemSet: _context.GoodsReturnOrderItems,
            // ✅ لازم predicate على العمود الحقيقي
            batchItemSelector: b => b.GoodsReturnOrderItemId == goodsReturnOrderItemId,
            batchSet: _context.GoodsReturnOrderBatches
        );

        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new GoodsReturnOrderBatchDTO
        {
            GoodsReturnOrderBatchId = saved.GoodsReturnOrderBatchId,
            GoodsReturnOrderItemId = saved.GoodsReturnOrderItemId,
            ReceiptPurchaseOrderBatchId = saved.ReceiptPurchaseOrderBatchId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<GoodsReturnOrderBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<GoodsReturnOrderBatchDTO>> UpdateGoodsReturnOrderBatchAsync(int goodsReturnOrderBatchId, UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<GoodsReturnOrder, GoodsReturnOrderItem, GoodsReturnOrderBatch>(
             batchId: goodsReturnOrderBatchId,
             processType: ProcessType.GoodsReturn,
             dto: dto,
             orderSet: _context.GoodsReturnOrders,

             batchSet: _context.GoodsReturnOrderBatches,
             orderItemSet: _context.GoodsReturnOrderItems,

             batchIdSelector: x => x.GoodsReturnOrderBatchId,
             orderItemIdSelector: x => x.GoodsReturnOrderItemId,
             orderItemIdForItemSelector: x => x.GoodsReturnOrderItemId
         );

        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new GoodsReturnOrderBatchDTO
        {
            GoodsReturnOrderBatchId = entity.GoodsReturnOrderBatchId,
            GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
            ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<GoodsReturnOrderBatchDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<GoodsReturnOrderBatchDTO>> DeleteGoodsReturnOrderBatchAsync(int goodsReturnOrderBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<GoodsReturnOrder, GoodsReturnOrderItem, GoodsReturnOrderBatch>(
            batchIdFromRoute: goodsReturnOrderBatchId,
            processType: ProcessType.GoodsReturn,
            orderSet: _context.GoodsReturnOrders,

            batchSet: _context.GoodsReturnOrderBatches,
            orderItemSet: _context.GoodsReturnOrderItems,

            batchIdSelector: b => b.GoodsReturnOrderBatchId,
            batchOrderItemIdSelector: b => b.GoodsReturnOrderItemId,  // FK الحقيقي
            orderItemPkSelector: i => i.GoodsReturnOrderItemId,       // PK الحقيقي للـ item
            orderIdSelector: i => i.GoodsReturnOrderId                // OrderId الحقيقي (SalesOrderId)
        );

        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new GoodsReturnOrderBatchDTO
        {
            GoodsReturnOrderBatchId = entity.GoodsReturnOrderBatchId,
            GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
            ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<GoodsReturnOrderBatchDTO>.SuccessResponse(result);
    }

  
    //public async Task<GeneralResponse<GoodsReturnOrderBatchDTO>> AddByGoodsReturnOrderItemIdAsync(int goodsReturnOrderItemId, AddGoodsReturnOrderBatchDTO dto)
    //{
    //    if (goodsReturnOrderItemId != dto.GoodsReturnOrderItemId)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Goods Return Order Item ID mismatch");

    //    var goodsReturnOrderItem = await _context.GoodsReturnOrderItems
    //        .Include(groi => groi.ReceiptPurchaseOrderItem)
    //        .FirstOrDefaultAsync(i => i.GoodsReturnOrderItemId == goodsReturnOrderItemId);

    //    if (goodsReturnOrderItem == null)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Goods Return Order Item not found");

    //    // Validate ReceiptPurchaseOrderBatch exists
    //    var receiptPurchaseOrderBatch = await _context.ReceiptPurchaseOrderBatches
    //        .FirstOrDefaultAsync(b => b.ReceiptPurchaseOrderBatchId == dto.ReceiptPurchaseOrderBatchId);

    //    if (receiptPurchaseOrderBatch == null)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Receipt Purchase Order Batch not found");

    //    // Validate that the receipt batch belongs to the same receipt item
    //    if (receiptPurchaseOrderBatch.ReceiptPurchaseOrderItemId != goodsReturnOrderItem.ReceiptPurchaseOrderItemId)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Receipt Purchase Order Batch does not belong to the same Receipt Purchase Order Item");

    //    // Check if this batch already exists for this return item
    //    var existingBatch = await _context.GoodsReturnOrderBatches
    //        .FirstOrDefaultAsync(b => b.GoodsReturnOrderItemId == goodsReturnOrderItemId &&
    //                                  b.ReceiptPurchaseOrderBatchId == dto.ReceiptPurchaseOrderBatchId);

    //    if (existingBatch != null)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("This batch already exists for this return item");

    //    // Validate quantity doesn't exceed receipt batch quantity
    //    if (dto.Quantity > receiptPurchaseOrderBatch.Quantity)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Return batch quantity cannot exceed receipt batch quantity");

    //    var mapping = new GoodsReturnOrderBatch
    //    {
    //        GoodsReturnOrderItemId = dto.GoodsReturnOrderItemId,
    //        ReceiptPurchaseOrderBatchId = dto.ReceiptPurchaseOrderBatchId,
    //        Quantity = dto.Quantity,
    //        Comment = dto.Comment,
    //        BatchNumber = receiptPurchaseOrderBatch.BatchNumber,
    //        ExpiryDate = receiptPurchaseOrderBatch.ExpiryDate,
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    var res = await AddAsync(mapping);
    //    await SaveChangesAsync();

    //    var model = new GoodsReturnOrderBatchDTO
    //    {
    //        GoodsReturnOrderBatchId = res.GoodsReturnOrderBatchId,
    //        GoodsReturnOrderItemId = res.GoodsReturnOrderItemId,
    //        ReceiptPurchaseOrderBatchId = res.ReceiptPurchaseOrderBatchId,
    //        Quantity = res.Quantity,
    //        Comment = res.Comment,
    //        BatchNumber = res.BatchNumber,
    //        ExpiryDate = res.ExpiryDate
    //    };

    //    return GeneralResponse<GoodsReturnOrderBatchDTO>.SuccessResponse(model);
    //}

    //public async Task<GeneralResponse<GoodsReturnOrderBatchDTO>> UpdateGoodsReturnOrderBatchAsync(int goodsReturnOrderBatchId, UpdateGoodsReturnOrderBatchDTO dto)
    //{
    //    var entity = await _context.GoodsReturnOrderBatches
    //        .Include(b => b.ReceiptPurchaseOrderBatch)
    //        .FirstOrDefaultAsync(e => e.GoodsReturnOrderBatchId == dto.GoodsReturnOrderBatchId);

    //    if (entity == null)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Goods Return Order Batch not found");

    //    if (entity.GoodsReturnOrderBatchId != goodsReturnOrderBatchId)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("ID mismatch");

    //    // Validate quantity doesn't exceed receipt batch quantity
    //    if (dto.Quantity > entity.ReceiptPurchaseOrderBatch.Quantity)
    //        return GeneralResponse<GoodsReturnOrderBatchDTO>.FailResponse("Return batch quantity cannot exceed receipt batch quantity");

    //    entity.Quantity = dto.Quantity;
    //    entity.Comment = dto.Comment;

    //    await _context.SaveChangesAsync();

    //    var result = new GoodsReturnOrderBatchDTO
    //    {
    //        GoodsReturnOrderBatchId = entity.GoodsReturnOrderBatchId,
    //        GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
    //        ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
    //        Quantity = entity.Quantity,
    //        Comment = entity.Comment,
    //        BatchNumber = entity.BatchNumber,
    //        ExpiryDate = entity.ExpiryDate
    //    };

    //    return GeneralResponse<GoodsReturnOrderBatchDTO>.SuccessResponse(result);
    //}

}

