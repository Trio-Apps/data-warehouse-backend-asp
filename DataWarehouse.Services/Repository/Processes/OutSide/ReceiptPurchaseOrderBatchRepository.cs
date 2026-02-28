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

public class ReceiptPurchaseOrderBatchRepository : BaseRepository<ReceiptPurchaseOrderBatch>, IReceiptPurchaseOrderBatchRepository
{
    private readonly IBaseProcessesRepository<ReceiptPurchaseOrderBatch> baseProcesses;

    public ReceiptPurchaseOrderBatchRepository(IBaseProcessesRepository<ReceiptPurchaseOrderBatch> baseProcesses, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    //public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderBatchDTO>>> GetByReceiptPurchaseOrderItemIdAsync(int receiptPurchaseOrderItemId)
    //{
    //    var res = await Query().Where(b => b.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId).ToListAsync();

    //    return GeneralResponse<IEnumerable<ReceiptPurchaseOrderBatchDTO>>.SuccessResponse(
    //        res.Select(b => new ReceiptPurchaseOrderBatchDTO
    //        {
    //            ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
    //            ReceiptPurchaseOrderItemId = b.ReceiptPurchaseOrderItemId,
    //            Quantity = b.Quantity,
    //            Comment = b.Comment,
    //            BatchNumber = b.BatchNumber,
    //            ExpiryDate = b.ExpiryDate
    //        }));
    //}
    public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderBatchDTO>>> GetByReceiptPurchaseOrderItemIdAsync(
    int receiptPurchaseOrderItemId)
    {
        var res = await baseProcesses.GetOrderBatchesAsync<ReceiptPurchaseOrderItem, ReceiptPurchaseOrderBatch, ReceiptPurchaseOrderBatchDTO>(
            orderItemId: receiptPurchaseOrderItemId,
            orderItemSelector: i => i.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId,
            orderItemSet: _context.ReceiptPurchaseOrderItems,

            batchItemSelector: b => b.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId,
            batchSet: _context.ReceiptPurchaseOrderBatches,

            map: b => new ReceiptPurchaseOrderBatchDTO
            {
                ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                ReceiptPurchaseOrderItemId = b.ReceiptPurchaseOrderItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }
        );

        return res;
    }


    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderBatchDTO>>> GetByReceiptPurchaseOrderItemIdWithPaginationAsync(int receiptPurchaseOrderItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceiptPurchaseOrderBatches
            .AsNoTracking()
            .Where(b => b.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new ReceiptPurchaseOrderBatchDTO
            {
                ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                ReceiptPurchaseOrderItemId = b.ReceiptPurchaseOrderItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceiptPurchaseOrderBatchDTO>>.SuccessResponse(
            new PagedResult<ReceiptPurchaseOrderBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> AddByReceiptPurchaseOrderItemIdAsync(int receiptPurchaseOrderItemId, GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<ReceiptPurchaseOrderItem, ReceiptPurchaseOrderBatch>(
            orderItemId: receiptPurchaseOrderItemId,
            processType: ProcessType.Receipt,
            dto: dto,
            // ✅ لازم predicate على العمود الحقيقي
            orderItemSelector: i => i.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId,
            orderItemSet: _context.ReceiptPurchaseOrderItems,

            // ✅ لازم predicate على العمود الحقيقي
            batchItemSelector: b => b.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId,
            batchSet: _context.ReceiptPurchaseOrderBatches
        );

        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new ReceiptPurchaseOrderBatchDTO
        {
            ReceiptPurchaseOrderBatchId = saved.ReceiptPurchaseOrderBatchId,
            ReceiptPurchaseOrderItemId = saved.ReceiptPurchaseOrderItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.SuccessResponse(model);
    }
    public async Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> UpdateReceiptPurchaseOrderBatchAsync(
    int receiptPurchaseOrderBatchId,
    UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<ReceiptPurchaseOrderItem, ReceiptPurchaseOrderBatch>(
            batchId: receiptPurchaseOrderBatchId,
            processType: ProcessType.Receipt,
            dto: dto,

            batchSet: _context.ReceiptPurchaseOrderBatches,
            orderItemSet: _context.ReceiptPurchaseOrderItems,

            batchIdSelector: x => x.ReceiptPurchaseOrderBatchId,
            orderItemIdSelector: x => x.ReceiptPurchaseOrderItemId,
            orderItemIdForItemSelector: x => x.ReceiptPurchaseOrderItemId
        );

        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new ReceiptPurchaseOrderBatchDTO
        {
            ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.SuccessResponse(result);
    }


    public async Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> DeleteReceiptPurchaseOrderBatchAsync(
  int receiptPurchaseOrderBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<
            ReceiptPurchaseOrderItem,
            ReceiptPurchaseOrderBatch>(
            batchIdFromRoute: receiptPurchaseOrderBatchId,
            processType: ProcessType.Receipt,

            batchSet: _context.ReceiptPurchaseOrderBatches,
            orderItemSet: _context.ReceiptPurchaseOrderItems,

            batchIdSelector: b => b.ReceiptPurchaseOrderBatchId,
            batchOrderItemIdSelector: b => b.ReceiptPurchaseOrderItemId, // FK الحقيقي
            orderItemPkSelector: i => i.ReceiptPurchaseOrderItemId,      // PK الحقيقي للـ item
            orderIdSelector: i => i.ReceiptPurchaseOrderId               // OrderId الحقيقي
        );

        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new ReceiptPurchaseOrderBatchDTO
        {
            ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.SuccessResponse(result);
    }


    //public async Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> UpdateReceiptPurchaseOrderBatchAsync(
    // int receiptPurchaseOrderBatchId,
    // UpdateReceiptPurchaseOrderBatchDTO dto)
    //{
    //    if (receiptPurchaseOrderBatchId != dto.ReceiptPurchaseOrderBatchId)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("ID mismatch");

    //    var entity = await _context.ReceiptPurchaseOrderBatches
    //        .FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderBatchId == receiptPurchaseOrderBatchId);

    //    if (entity == null)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("Receipt Purchase Order Batch not found");

    //    // 🔍 1. جلب الـ Parent Item
    //    var receiptPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
    //        .FirstOrDefaultAsync(i => i.ReceiptPurchaseOrderItemId == entity.ReceiptPurchaseOrderItemId);

    //    if (receiptPurchaseOrderItem == null)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("Receipt Purchase Order Item not found");

    //    // 🔍 2. حساب مجموع الباتشات الأخرى (بدون الحالي)
    //    var otherBatchesTotalQuantity = await _context.ReceiptPurchaseOrderBatches
    //        .Where(b =>
    //            b.ReceiptPurchaseOrderItemId == entity.ReceiptPurchaseOrderItemId &&
    //            b.ReceiptPurchaseOrderBatchId != entity.ReceiptPurchaseOrderBatchId)
    //        .SumAsync(b => b.Quantity);

    //    // 🔍 3. حساب الإجمالي بعد التعديل
    //    var totalAfterUpdate = otherBatchesTotalQuantity + dto.Quantity;

    //    // 🛑 4. التحقق
    //    if (totalAfterUpdate > receiptPurchaseOrderItem.Quantity)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse(
    //            "Total batch quantities exceed item quantity");

    //    // ✅ 5. التعديل
    //    entity.Quantity = dto.Quantity;
    //    entity.Comment = dto.Comment;
    //    entity.ExpiryDate = dto.ExpiryDate;

    //    await _context.SaveChangesAsync();

    //    // 🎯 6. الإرجاع
    //    var result = new ReceiptPurchaseOrderBatchDTO
    //    {
    //        ReceiptPurchaseOrderBatchId = entity.ReceiptPurchaseOrderBatchId,
    //        ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
    //        Quantity = entity.Quantity,
    //        Comment = entity.Comment,
    //        BatchNumber = entity.BatchNumber,
    //        ExpiryDate = entity.ExpiryDate
    //    };

    //    return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.SuccessResponse(result);
    //}

    //public async Task<GeneralResponse<ReceiptPurchaseOrderBatchDTO>> AddByReceiptPurchaseOrderItemIdAsync(int receiptPurchaseOrderItemId, AddReceiptPurchaseOrderBatchDTO dto)
    //{
    //    if (receiptPurchaseOrderItemId != dto.ReceiptPurchaseOrderItemId)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("Receipt Purchase Order Item ID mismatch");

    //    var receiptPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
    //        .FirstOrDefaultAsync(i => i.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId);

    //    if (receiptPurchaseOrderItem == null)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("Receipt Purchase Order Item not found");

    //    // 🔍 1. حساب مجموع كميات الباتشات الحالية
    //    var existingBatchesTotalQuantity = await _context.ReceiptPurchaseOrderBatches
    //        .Where(b => b.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId)
    //        .SumAsync(b => b.Quantity);

    //    // 🔍 2. حساب الكمية بعد الإضافة
    //    var totalAfterAdd = existingBatchesTotalQuantity + dto.Quantity;

    //    // 🛑 3. التحقق
    //    if (totalAfterAdd > receiptPurchaseOrderItem.Quantity)
    //        return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.FailResponse("Total batch quantities exceed item quantity");

    //    // ✅ 4. الإضافة
    //    var mapping = new ReceiptPurchaseOrderBatch
    //    {
    //        ReceiptPurchaseOrderItemId = dto.ReceiptPurchaseOrderItemId,
    //        Quantity = dto.Quantity,
    //        Comment = dto.Comment,
    //        ExpiryDate = dto.ExpiryDate,
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    var res = await AddAsync(mapping);
    //    await SaveChangesAsync();

    //    var model = new ReceiptPurchaseOrderBatchDTO
    //    {
    //        ReceiptPurchaseOrderBatchId = res.ReceiptPurchaseOrderBatchId,
    //        ReceiptPurchaseOrderItemId = res.ReceiptPurchaseOrderItemId,
    //        Quantity = res.Quantity,
    //        Comment = res.Comment,
    //        BatchNumber = res.BatchNumber,
    //        ExpiryDate = res.ExpiryDate
    //    };

    //    return GeneralResponse<ReceiptPurchaseOrderBatchDTO>.SuccessResponse(model);
    //}



}

