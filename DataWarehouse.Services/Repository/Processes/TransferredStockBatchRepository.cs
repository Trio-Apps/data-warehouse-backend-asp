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

public class TransferredStockBatchRepository : BaseRepository<TransferredStockBatch>, ITransferredStockBatchRepository
{
    public TransferredStockBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<IEnumerable<TransferredStockBatchDTO>>> GetByTransferredItemIdAsync(int transferredItemId)
    {
        var res = await Query().Where(b => b.TransferredItemId == transferredItemId).ToListAsync();

        return GeneralResponse<IEnumerable<TransferredStockBatchDTO>>.SuccessResponse(
            res.Select(b => new TransferredStockBatchDTO
            {
                TransferredStockBatchId = b.TransferredStockBatchId,
                TransferredItemId = b.TransferredItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<TransferredStockBatchDTO>>> GetByTransferredItemIdWithPaginationAsync(int transferredItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredStockBatches
            .AsNoTracking()
            .Where(b => b.TransferredItemId == transferredItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TransferredStockBatchDTO
            {
                TransferredStockBatchId = b.TransferredStockBatchId,
                TransferredItemId = b.TransferredItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<TransferredStockBatchDTO>>.SuccessResponse(
            new PagedResult<TransferredStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<TransferredStockBatchDTO>> AddByTransferredItemIdAsync(int transferredItemId, AddTransferredStockBatchDTO dto)
    {
        if (transferredItemId != dto.TransferredItemId)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse("Transferred Item ID mismatch");

        var transferredItem = await _context.TransferredItems
            .FirstOrDefaultAsync(i => i.TransferredItemId == transferredItemId);

        if (transferredItem == null)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse("Transferred Item not found");

        var mapping = new TransferredStockBatch
        {
            TransferredItemId = dto.TransferredItemId,
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new TransferredStockBatchDTO
        {
            TransferredStockBatchId = res.TransferredStockBatchId,
            TransferredItemId = res.TransferredItemId,
            Quantity = res.Quantity,
            Comment = res.Comment,
            BatchNumber = res.BatchNumber,
            ExpiryDate = res.ExpiryDate
        };

        return GeneralResponse<TransferredStockBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<TransferredStockBatchDTO>> UpdateTransferredStockBatchAsync(int transferredStockBatchId, UpdateTransferredStockBatchDTO dto)
    {
        var entity = await _context.TransferredStockBatches
            .FirstOrDefaultAsync(e => e.TransferredStockBatchId == dto.TransferredStockBatchId);

        if (entity == null)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse("Transferred Stock Batch not found");

        if (entity.TransferredStockBatchId != transferredStockBatchId)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse("ID mismatch");

        entity.Quantity = dto.Quantity;
        entity.Comment = dto.Comment;
        entity.ExpiryDate = dto.ExpiryDate;

        await _context.SaveChangesAsync();

        var result = new TransferredStockBatchDTO
        {
            TransferredStockBatchId = entity.TransferredStockBatchId,
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<TransferredStockBatchDTO>.SuccessResponse(result);
    }
}

