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

public class SalesOrderBatchRepository : BaseRepository<SalesOrderBatch>, ISalesOrderBatchRepository
{
    private readonly IBaseProcessesRepository<SalesOrderBatch> baseProcesses;

    public SalesOrderBatchRepository(IBaseProcessesRepository<SalesOrderBatch> baseProcesses, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<SalesOrderBatchDTO>>> GetBySalesOrderItemIdAsync(int salesOrderItemId)
    {
        var res = await Query().Where(b => b.SalesOrderItemId == salesOrderItemId).ToListAsync();

        return GeneralResponse<IEnumerable<SalesOrderBatchDTO>>.SuccessResponse(
            res.Select(b => new SalesOrderBatchDTO
            {
                SalesOrderBatchId = b.SalesOrderBatchId,
                SalesOrderItemId = b.SalesOrderItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<SalesOrderBatchDTO>>> GetBySalesOrderItemIdWithPaginationAsync(int salesOrderItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesOrderBatches
            .AsNoTracking()
            .Where(b => b.SalesOrderItemId == salesOrderItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new SalesOrderBatchDTO
            {
                SalesOrderBatchId = b.SalesOrderBatchId,
                SalesOrderItemId = b.SalesOrderItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesOrderBatchDTO>>.SuccessResponse(
            new PagedResult<SalesOrderBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<SalesOrderBatchDTO>> AddBySalesOrderItemIdAsync(
      int salesOrderItemId,
      GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<SalesOrderItem, SalesOrderBatch>(
            orderItemId: salesOrderItemId,
            processType: ProcessType.Sales,
            dto: dto,
            // ✅ لازم predicate على العمود الحقيقي
            orderItemSelector: i => i.SalesOrderItemId == salesOrderItemId,
            orderItemSet: _context.SalesOrderItems,

            // ✅ لازم predicate على العمود الحقيقي
            batchItemSelector: b => b.SalesOrderItemId == salesOrderItemId,
            batchSet: _context.SalesOrderBatches
        );

        if (!res.Success)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new SalesOrderBatchDTO
        {
            SalesOrderBatchId = saved.SalesOrderBatchId,
            SalesOrderItemId = saved.SalesOrderItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesOrderBatchDTO>> UpdateSalesOrderBatchAsync(
       int salesOrderBatchId,
       UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<SalesOrderItem, SalesOrderBatch>(
            batchId: salesOrderBatchId,
            processType: ProcessType.Sales,
            dto: dto,

            batchSet: _context.SalesOrderBatches,
            orderItemSet: _context.SalesOrderItems,

            batchIdSelector: x => x.SalesOrderBatchId,
            orderItemIdSelector: x => x.SalesOrderItemId,
            orderItemIdForItemSelector: x => x.SalesOrderItemId
        );

        if (!res.Success)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse(res.Message);

        var e = res.Data;

        return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(new SalesOrderBatchDTO
        {
            SalesOrderBatchId = e.SalesOrderBatchId,
            SalesOrderItemId = e.SalesOrderItemId,
            Quantity = e.Quantity,
            Comment = e.Comment,
            BatchNumber = e.BatchNumber,
            ExpiryDate = e.ExpiryDate
        });
    }


    public async Task<GeneralResponse<SalesOrderBatchDTO>> DeleteSalesOrderBatchAsync(int salesOrderBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<SalesOrderItem, SalesOrderBatch>(
            batchIdFromRoute: salesOrderBatchId,
            processType: ProcessType.Sales,

            batchSet: _context.SalesOrderBatches,
            orderItemSet: _context.SalesOrderItems,

            batchIdSelector: b => b.SalesOrderBatchId,
            batchOrderItemIdSelector: b => b.SalesOrderItemId,  // FK الحقيقي
            orderItemPkSelector: i => i.SalesOrderItemId,       // PK الحقيقي للـ item
            orderIdSelector: i => i.SalesOrderId                // OrderId الحقيقي (SalesOrderId)
        );

        if (!res.Success)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse(res.Message);

        var e = res.Data;

        var dto = new SalesOrderBatchDTO
        {
            SalesOrderBatchId = e.SalesOrderBatchId,
            SalesOrderItemId = e.SalesOrderItemId,
            Quantity = e.Quantity,
            Comment = e.Comment,
            BatchNumber = e.BatchNumber,
            ExpiryDate = e.ExpiryDate
        };

        return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(dto);
    }


    //public async Task<GeneralResponse<SalesOrderBatchDTO>> AddBySalesOrderItemIdAsync(
    // int salesOrderItemId,
    // AddSalesOrderBatchDTO dto)
    //{
    //    if (salesOrderItemId != dto.SalesOrderItemId)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item ID mismatch");

    //    var salesOrderItem = await _context.SalesOrderItems
    //        .FirstOrDefaultAsync(i => i.SalesOrderItemId == salesOrderItemId);

    //    if (salesOrderItem == null)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item not found");

    //    var checkApprovalStatus = await GetProcessItem(salesOrderItem.SalesOrderId, ProcessType.Sales);

    //    if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("You cannot add any batch because its approval status is 'Approved' and all approval steps have been completed.");


    //    // 🔍 1. مجموع الباتشات الحالية
    //    var existingBatchesTotalQuantity = await _context.SalesOrderBatches
    //        .Where(b => b.SalesOrderItemId == salesOrderItemId)
    //        .SumAsync(b => b.Quantity);

    //    // 🔍 2. الكمية بعد الإضافة
    //    var totalAfterAdd = existingBatchesTotalQuantity + dto.Quantity;

    //    // 🛑 3. التحقق
    //    if (totalAfterAdd > salesOrderItem.Quantity)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse(
    //            "Total batch quantities exceed sales order item quantity");

    //    // ✅ 4. الإضافة
    //    var mapping = new SalesOrderBatch
    //    {
    //        SalesOrderItemId = dto.SalesOrderItemId,
    //        Quantity = dto.Quantity,
    //        Comment = dto.Comment,
    //        ExpiryDate = dto.ExpiryDate,
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    var res = await AddAsync(mapping);
    //    await SaveChangesAsync();

    //    var model = new SalesOrderBatchDTO
    //    {
    //        SalesOrderBatchId = res.SalesOrderBatchId,
    //        SalesOrderItemId = res.SalesOrderItemId,
    //        Quantity = res.Quantity,
    //        Comment = res.Comment,
    //        BatchNumber = res.BatchNumber,
    //        ExpiryDate = res.ExpiryDate
    //    };

    //    return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(model);
    //}

    //public async Task<GeneralResponse<SalesOrderBatchDTO>> UpdateSalesOrderBatchAsync(
    // int salesOrderBatchId,
    // UpdateGeneralBatchDto dto)
    //{


    //    var entity = await _context.SalesOrderBatches
    //        .FirstOrDefaultAsync(e => e.SalesOrderBatchId == salesOrderBatchId);

    //    if (entity == null)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Batch not found");

    //    // 🔍 1. جلب SalesOrderItem
    //    var salesOrderItem = await _context.SalesOrderItems
    //        .FirstOrDefaultAsync(i => i.SalesOrderItemId == entity.SalesOrderItemId);

    //    if (salesOrderItem == null)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item not found");

    //    var checkApprovalStatus = await GetProcessItem(salesOrderItem.SalesOrderId, ProcessType.Sales);

    //    if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse("You cannot edit any batch because its approval status is 'Approved' and all approval steps have been completed.");


    //    // 🔍 2. مجموع الباتشات الأخرى
    //    var otherBatchesTotalQuantity = await _context.SalesOrderBatches
    //        .Where(b =>
    //            b.SalesOrderItemId == entity.SalesOrderItemId &&
    //            b.SalesOrderBatchId != entity.SalesOrderBatchId)
    //        .SumAsync(b => b.Quantity);

    //    // 🔍 3. الكمية بعد التعديل
    //    var totalAfterUpdate = otherBatchesTotalQuantity + dto.Quantity;

    //    // 🛑 4. التحقق
    //    if (totalAfterUpdate > salesOrderItem.Quantity)
    //        return GeneralResponse<SalesOrderBatchDTO>.FailResponse(
    //            "Total batch quantities exceed sales order item quantity");

    //    // ✅ 5. التعديل

    //    entity.Quantity = dto.Quantity??entity.Quantity;
    //    entity.Comment = dto.Comment??dto.Comment;
    //    entity.ExpiryDate = dto.ExpiryDate??dto.ExpiryDate;

    //    await _context.SaveChangesAsync();

    //    var result = new SalesOrderBatchDTO
    //    {
    //        SalesOrderBatchId = entity.SalesOrderBatchId,
    //        SalesOrderItemId = entity.SalesOrderItemId,
    //        Quantity = entity.Quantity,
    //        Comment = entity.Comment,
    //        BatchNumber = entity.BatchNumber,
    //        ExpiryDate = entity.ExpiryDate
    //    };

    //    return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(result);
    //}

}

