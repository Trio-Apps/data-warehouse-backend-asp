using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedStockBatchRepository : BaseRepository<ReceivedStockBatch>, IReceivedStockBatchRepository
{
    public ReceivedStockBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<IEnumerable<ReceivedStockBatchDTO>>> GetByReceivedItemIdAsync(int receivedItemId)
    {
        var res = await Query().Where(b => b.ReceivedItemId == receivedItemId).ToListAsync();

        return GeneralResponse<IEnumerable<ReceivedStockBatchDTO>>.SuccessResponse(
            res.Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<ReceivedStockBatchDTO>>> GetByReceivedItemIdWithPaginationAsync(int receivedItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedStockBatches
            .AsNoTracking()
            .Where(b => b.ReceivedItemId == receivedItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceivedStockBatchDTO>>.SuccessResponse(
            new PagedResult<ReceivedStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<ReceivedStockBatchDTO>> AddByReceivedItemIdAsync(int receivedItemId, AddReceivedStockBatchDTO dto)
    {
        if (receivedItemId != dto.ReceivedItemId)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Received Item ID mismatch");

        var receivedItem = await _context.ReceivedItems
            .Include(ri => ri.TransferredItem)
            .FirstOrDefaultAsync(i => i.ReceivedItemId == receivedItemId);

        if (receivedItem == null)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Received Item not found");

        // Validate TransferredStockBatch exists
        var transferredStockBatch = await _context.TransferredStockBatches
            .FirstOrDefaultAsync(b => b.TransferredStockBatchId == dto.TransferredStockBatchId);

        if (transferredStockBatch == null)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Transferred Stock Batch not found");

        // Validate that the transferred batch belongs to the same transferred item
        if (transferredStockBatch.TransferredItemId != receivedItem.TransferredItemId)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Transferred Stock Batch does not belong to the same Transferred Item");

        // Check if this batch already exists for this received item
        var existingBatch = await _context.ReceivedStockBatches
            .FirstOrDefaultAsync(b => b.ReceivedItemId == receivedItemId &&
                                      b.TransferredStockBatchId == dto.TransferredStockBatchId);

        if (existingBatch != null)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("This batch already exists for this received item");

        // Validate quantity doesn't exceed transferred batch quantity
        if (dto.Quantity > transferredStockBatch.Quantity)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Received batch quantity cannot exceed transferred batch quantity");

        var mapping = new ReceivedStockBatch
        {
            ReceivedItemId = dto.ReceivedItemId,
            TransferredStockBatchId = dto.TransferredStockBatchId,
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = transferredStockBatch.BatchNumber,
            ExpiryDate = transferredStockBatch.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ReceivedStockBatchDTO
        {
            ReceivedStockBatchId = res.ReceivedStockBatchId,
            ReceivedItemId = res.ReceivedItemId,
            TransferredStockBatchId = res.TransferredStockBatchId,
            Quantity = res.Quantity,
            Comment = res.Comment,
            BatchNumber = res.BatchNumber,
            ExpiryDate = res.ExpiryDate
        };

        return GeneralResponse<ReceivedStockBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceivedStockBatchDTO>> UpdateReceivedStockBatchAsync(int receivedStockBatchId, UpdateReceivedStockBatchDTO dto)
    {
        var entity = await _context.ReceivedStockBatches
            .Include(b => b.TransferredStockBatch)
            .FirstOrDefaultAsync(e => e.ReceivedStockBatchId == dto.ReceivedStockBatchId);

        if (entity == null)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Received Stock Batch not found");

        if (entity.ReceivedStockBatchId != receivedStockBatchId)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed transferred batch quantity
        if (dto.Quantity > entity.TransferredStockBatch.Quantity)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse("Received batch quantity cannot exceed transferred batch quantity");

        entity.Quantity = dto.Quantity;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new ReceivedStockBatchDTO
        {
            ReceivedStockBatchId = entity.ReceivedStockBatchId,
            ReceivedItemId = entity.ReceivedItemId,
            TransferredStockBatchId = entity.TransferredStockBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<ReceivedStockBatchDTO>.SuccessResponse(result);
    }
}

