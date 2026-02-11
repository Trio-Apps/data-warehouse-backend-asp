using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
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

public class SalesReturnOrderBatchRepository : BaseRepository<SalesReturnOrderBatch>, ISalesReturnOrderBatchRepository
{
    public SalesReturnOrderBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<IEnumerable<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId)
    {
        var res = await Query().Where(b => b.SalesReturnOrderItemId == salesReturnOrderItemId).ToListAsync();

        return GeneralResponse<IEnumerable<SalesReturnOrderBatchDTO>>.SuccessResponse(
            res.Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                SalesOrderBatchId = b.SalesOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdWithPaginationAsync(int salesReturnOrderItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrderBatches
            .AsNoTracking()
            .Where(b => b.SalesReturnOrderItemId == salesReturnOrderItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                SalesOrderBatchId = b.SalesOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesReturnOrderBatchDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<SalesReturnOrderBatchDTO>> AddBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId, AddSalesReturnOrderBatchDTO dto)
    {
        if (salesReturnOrderItemId != dto.SalesReturnOrderItemId)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Sales Return Order Item ID mismatch");

        var salesReturnOrderItem = await _context.SalesReturnOrderItems
            .Include(sroi => sroi.SalesOrderItem)
            .FirstOrDefaultAsync(i => i.SalesReturnOrderItemId == salesReturnOrderItemId);

         


        if (salesReturnOrderItem == null)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Sales Return Order Item not found");

        var checkApprovalStatus = await GetProcessItem(salesReturnOrderItem.SalesReturnOrderId, ProcessType.SalesReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("You cannot add any batch to return item because its approval status is 'Approved' and all approval steps have been completed.");


        // Validate SalesOrderBatch exists
        var salesOrderBatch = await _context.SalesOrderBatches
            .FirstOrDefaultAsync(b => b.SalesOrderBatchId == dto.SalesOrderBatchId);

        if (salesOrderBatch == null)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Sales Order Batch not found");

        // Validate that the sales batch belongs to the same sales item
        if (salesOrderBatch.SalesOrderItemId != salesReturnOrderItem.SalesOrderItemId)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Sales Order Batch does not belong to the same Sales Order Item");

        // Check if this batch already exists for this return item
        var existingBatch = await _context.SalesReturnOrderBatches
            .FirstOrDefaultAsync(b => b.SalesReturnOrderItemId == salesReturnOrderItemId && 
                                      b.SalesOrderBatchId == dto.SalesOrderBatchId);

        if (existingBatch != null)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("This batch already exists for this return item");

        // Validate quantity doesn't exceed sales batch quantity
        if (dto.Quantity > salesOrderBatch.Quantity)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Return batch quantity cannot exceed sales batch quantity");

        var mapping = new SalesReturnOrderBatch
        {
            SalesReturnOrderItemId = dto.SalesReturnOrderItemId,
            SalesOrderBatchId = dto.SalesOrderBatchId,
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = salesOrderBatch.BatchNumber,
            ExpiryDate = salesOrderBatch.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new SalesReturnOrderBatchDTO
        {
            SalesReturnOrderBatchId = res.SalesReturnOrderBatchId,
            SalesReturnOrderItemId = res.SalesReturnOrderItemId,
            SalesOrderBatchId = res.SalesOrderBatchId,
            Quantity = res.Quantity,
            Comment = res.Comment,
            BatchNumber = res.BatchNumber,
            ExpiryDate = res.ExpiryDate
        };

        return GeneralResponse<SalesReturnOrderBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderBatchDTO>> UpdateSalesReturnOrderBatchAsync(int salesReturnOrderBatchId, UpdateSalesReturnOrderBatchDTO dto)
    {
        var entity = await _context.SalesReturnOrderBatches
            .Include(x=>x.SalesReturnOrderItem)
            .Include(b => b.SalesOrderBatch)
            .FirstOrDefaultAsync(e => e.SalesReturnOrderBatchId == dto.SalesReturnOrderBatchId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Sales Return Order Batch not found");

        if (entity.SalesReturnOrderBatchId != salesReturnOrderBatchId)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("ID mismatch");


        var checkApprovalStatus = await GetProcessItem(entity.SalesReturnOrderItem.SalesReturnOrderId, ProcessType.SalesReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("You cannot edit on any batch to return item because its approval status is 'Approved' and all approval steps have been completed.");


        // Validate quantity doesn't exceed sales batch quantity
        if (dto.Quantity > entity.SalesOrderBatch.Quantity)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse("Return batch quantity cannot exceed sales batch quantity");

        entity.Quantity = dto.Quantity?? entity.Quantity;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new SalesReturnOrderBatchDTO
        {
            SalesReturnOrderBatchId = entity.SalesReturnOrderBatchId,
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            SalesOrderBatchId = entity.SalesOrderBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<SalesReturnOrderBatchDTO>.SuccessResponse(result);
    }
}

