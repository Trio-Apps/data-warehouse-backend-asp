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

public class CountStockBatchRepository : BaseRepository<CountStockBatch>, ICountStockBatchRepository
{
    public CountStockBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<IEnumerable<CountStockBatchDTO>>> GetByCountStockItemIdAsync(int countStockItemId)
    {
        var res = await Query().Where(b => b.CountStockItemId == countStockItemId).ToListAsync();

        return GeneralResponse<IEnumerable<CountStockBatchDTO>>.SuccessResponse(
            res.Select(b => new CountStockBatchDTO
            {
                CountStockBatchId = b.CountStockBatchId,
                CountStockItemId = b.CountStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<CountStockBatchDTO>>> GetByCountStockItemIdWithPaginationAsync(int countStockItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.CountStockBatches
            .AsNoTracking()
            .Where(b => b.CountStockItemId == countStockItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new CountStockBatchDTO
            {
                CountStockBatchId = b.CountStockBatchId,
                CountStockItemId = b.CountStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<CountStockBatchDTO>>.SuccessResponse(
            new PagedResult<CountStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<CountStockBatchDTO>> AddByCountStockItemIdAsync(int countStockItemId, AddCountStockBatchDTO dto)
    {
        if (countStockItemId != dto.CountStockItemId)
            return GeneralResponse<CountStockBatchDTO>.FailResponse("Count Stock Item ID mismatch");

        var countStockItem = await _context.CountStockItems
            .FirstOrDefaultAsync(i => i.CountStockItemId == countStockItemId);

        if (countStockItem == null)
            return GeneralResponse<CountStockBatchDTO>.FailResponse("Count Stock Item not found");

        var mapping = new CountStockBatch
        {
            CountStockItemId = dto.CountStockItemId,
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new CountStockBatchDTO
        {
            CountStockBatchId = res.CountStockBatchId,
            CountStockItemId = res.CountStockItemId,
            Quantity = res.Quantity,
            Comment = res.Comment,
            BatchNumber = res.BatchNumber,
            ExpiryDate = res.ExpiryDate
        };

        return GeneralResponse<CountStockBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<CountStockBatchDTO>> UpdateCountStockBatchAsync(int countStockBatchId, UpdateCountStockBatchDTO dto)
    {
        var entity = await _context.CountStockBatches
            .FirstOrDefaultAsync(e => e.CountStockBatchId == dto.CountStockBatchId);

        if (entity == null)
            return GeneralResponse<CountStockBatchDTO>.FailResponse("Count Stock Batch not found");

        if (entity.CountStockBatchId != countStockBatchId)
            return GeneralResponse<CountStockBatchDTO>.FailResponse("ID mismatch");

        entity.Quantity = dto.Quantity;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new CountStockBatchDTO
        {
            CountStockBatchId = entity.CountStockBatchId,
            CountStockItemId = entity.CountStockItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<CountStockBatchDTO>.SuccessResponse(result);
    }
}

