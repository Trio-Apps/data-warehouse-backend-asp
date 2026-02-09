using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesOrderBatchRepository : BaseRepository<SalesOrderBatch>, ISalesOrderBatchRepository
{
    public SalesOrderBatchRepository(DataWarehouseDbContext context) : base(context)
    {
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
     AddSalesOrderBatchDTO dto)
    {
        if (salesOrderItemId != dto.SalesOrderItemId)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item ID mismatch");

        var salesOrderItem = await _context.SalesOrderItems
            .FirstOrDefaultAsync(i => i.SalesOrderItemId == salesOrderItemId);

        if (salesOrderItem == null)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item not found");

        // 🔍 1. مجموع الباتشات الحالية
        var existingBatchesTotalQuantity = await _context.SalesOrderBatches
            .Where(b => b.SalesOrderItemId == salesOrderItemId)
            .SumAsync(b => b.Quantity);

        // 🔍 2. الكمية بعد الإضافة
        var totalAfterAdd = existingBatchesTotalQuantity + dto.Quantity;

        // 🛑 3. التحقق
        if (totalAfterAdd > salesOrderItem.Quantity)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse(
                "Total batch quantities exceed sales order item quantity");

        // ✅ 4. الإضافة
        var mapping = new SalesOrderBatch
        {
            SalesOrderItemId = dto.SalesOrderItemId,
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new SalesOrderBatchDTO
        {
            SalesOrderBatchId = res.SalesOrderBatchId,
            SalesOrderItemId = res.SalesOrderItemId,
            Quantity = res.Quantity,
            Comment = res.Comment,
            BatchNumber = res.BatchNumber,
            ExpiryDate = res.ExpiryDate
        };

        return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesOrderBatchDTO>> UpdateSalesOrderBatchAsync(
     int salesOrderBatchId,
     UpdateSalesOrderBatchDTO dto)
    {
        if (salesOrderBatchId != dto.SalesOrderBatchId)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse("ID mismatch");

        var entity = await _context.SalesOrderBatches
            .FirstOrDefaultAsync(e => e.SalesOrderBatchId == salesOrderBatchId);

        if (entity == null)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Batch not found");

        // 🔍 1. جلب SalesOrderItem
        var salesOrderItem = await _context.SalesOrderItems
            .FirstOrDefaultAsync(i => i.SalesOrderItemId == entity.SalesOrderItemId);

        if (salesOrderItem == null)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse("Sales Order Item not found");

        // 🔍 2. مجموع الباتشات الأخرى
        var otherBatchesTotalQuantity = await _context.SalesOrderBatches
            .Where(b =>
                b.SalesOrderItemId == entity.SalesOrderItemId &&
                b.SalesOrderBatchId != entity.SalesOrderBatchId)
            .SumAsync(b => b.Quantity);

        // 🔍 3. الكمية بعد التعديل
        var totalAfterUpdate = otherBatchesTotalQuantity + dto.Quantity;

        // 🛑 4. التحقق
        if (totalAfterUpdate > salesOrderItem.Quantity)
            return GeneralResponse<SalesOrderBatchDTO>.FailResponse(
                "Total batch quantities exceed sales order item quantity");

        // ✅ 5. التعديل
        entity.Quantity = dto.Quantity;
        entity.Comment = dto.Comment;
        entity.ExpiryDate = dto.ExpiryDate;

        await _context.SaveChangesAsync();

        var result = new SalesOrderBatchDTO
        {
            SalesOrderBatchId = entity.SalesOrderBatchId,
            SalesOrderItemId = entity.SalesOrderItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<SalesOrderBatchDTO>.SuccessResponse(result);
    }

}

