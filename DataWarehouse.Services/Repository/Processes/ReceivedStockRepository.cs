using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedStockRepository : BaseRepository<ReceivedStock>, IReceivedStockRepository
{
    public ReceivedStockRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReceivedStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(rs => rs.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedStocks
            .AsNoTracking()
            .Where(rs => rs.WarehouseId == warehouseId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(rs => new ReceivedStockDTO
            {
                ReceivedStockId = rs.ReceivedStockId,
                DueDate = rs.DueDate,
                Status = rs.Status.ToString(),
                UserId = rs.UserId,
                WarehouseId = warehouseId,
                SourceWarehouseId = rs.SourceWarehouseId,
                TransferredStockId = rs.TransferredStockId,
                Comment = rs.Comment
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceivedStockDTO>>.SuccessResponse(
            new PagedResult<ReceivedStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockByTransferredStockIdAsync(string userId, AddReceivedStockDTO dto)
    {
        var transferredStock = await _context.TransferredStocks
            .Include(ts => ts.ReceivedStock)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == dto.TransferredStockId);

        if (transferredStock == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred Stock is not found");

        if (transferredStock.ReceivedStock != null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred Stock already has a Received Stock!");

        var receivedStock = new ReceivedStock
        {
            UserId = userId,
            WarehouseId = transferredStock.DistinationWarehouseId,
            SourceWarehouseId = transferredStock.WarehouseId,
            TransferredStockId = dto.TransferredStockId,
            DueDate = dto.DueDate,
            Comment = dto.Comment,
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(receivedStock);
        await SaveChangesAsync();

        var model = new ReceivedStockDTO
        {
            ReceivedStockId = res.ReceivedStockId,
            DueDate = res.DueDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SourceWarehouseId = res.SourceWarehouseId,
            TransferredStockId = res.TransferredStockId,
            Comment = res.Comment
        };

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> UpdateReceivedStockAsync(string userId, int receivedStockId, UpdateReceivedStockDTO dto)
    {
        var entity = await _context.ReceivedStocks.FirstOrDefaultAsync(e => e.ReceivedStockId == dto.ReceivedStockId);

        if (entity == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Received Stock not found");

        if (entity.ReceivedStockId != receivedStockId)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("ID mismatch");

        entity.UserId = userId;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new ReceivedStockDTO
        {
            ReceivedStockId = entity.ReceivedStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SourceWarehouseId = entity.SourceWarehouseId,
            TransferredStockId = entity.TransferredStockId,
            Comment = entity.Comment
        };

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> GetByTransferredStockIdAsync(int transferredStockId)
    {
        var res = await Query().FirstOrDefaultAsync(rs => rs.TransferredStockId == transferredStockId);

        if (res == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Not Found");

        var mapping = new ReceivedStockDTO
        {
            ReceivedStockId = res.ReceivedStockId,
            DueDate = res.DueDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SourceWarehouseId = res.SourceWarehouseId,
            TransferredStockId = res.TransferredStockId,
            Comment = res.Comment
        };

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(mapping);
    }

    public async Task<IEnumerable<ReceivedStock>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(rs => rs.UserId == userId).ToListAsync();
    }

    public async Task<ReceivedStock?> GetWithItemsAsync(int receivedStockId)
    {
        return await QueryIncluding(false, rs => rs.ReceivedItems)
            .FirstOrDefaultAsync(rs => rs.ReceivedStockId == receivedStockId);
    }

    public async Task<ReceivedStock?> GetWithTransferredStockAsync(int receivedStockId)
    {
        return await QueryIncluding(false, rs => rs.TransferredStock)
            .FirstOrDefaultAsync(rs => rs.ReceivedStockId == receivedStockId);
    }

    public async Task<ReceivedStock?> GetWithWarehouseAsync(int receivedStockId)
    {
        return await QueryIncluding(false, rs => rs.Warehouse, rs => rs.SourceWarehouse)
            .FirstOrDefaultAsync(rs => rs.ReceivedStockId == receivedStockId);
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> GetWithItemsAndBatchesAsync(int receivedStockId)
    {
        var result = await _context.ReceivedStocks
            .AsNoTracking()
            .Where(s => s.ReceivedStockId == receivedStockId)
            .Select(s => new ReceivedStockDTO
            {
                ReceivedStockId = s.ReceivedStockId,
                DueDate = s.DueDate,
                Status = s.Status.ToString(),
                UserId = s.UserId,
                WarehouseId = s.WarehouseId,
                SourceWarehouseId = s.SourceWarehouseId,
                TransferredStockId = s.TransferredStockId,
                Comment = s.Comment,
                Items = s.ReceivedItems.Select(i => new ReceivedItemDTO
                {
                    ReceivedItemId = i.ReceivedItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    ErrorMessage = i.ErrorMessage,
                    Status = i.Status.ToString(),
                    Comment = i.Comment,
                    ReceivedStockId = i.ReceivedStockId,
                    TransferredItemId = i.TransferredItemId,
                    ItemId = i.ItemId,
                    Batches = i.ReceivedStockBatches.Select(b => new ReceivedStockBatchDTO
                    {
                        ReceivedStockBatchId = b.ReceivedStockBatchId,
                        ReceivedItemId = b.ReceivedItemId,
                        TransferredStockBatchId = b.TransferredStockBatchId,
                        Quantity = b.Quantity,
                        Comment = b.Comment,
                        BatchNumber = b.BatchNumber,
                        ExpiryDate = b.ExpiryDate
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Not Found");

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(result);
    }
}
