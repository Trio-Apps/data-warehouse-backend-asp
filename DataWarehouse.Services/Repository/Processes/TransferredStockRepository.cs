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

public class TransferredStockRepository : BaseRepository<TransferredStock>, ITransferredStockRepository
{
    public TransferredStockRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransferredStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(ts => ts.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Warehouses.Where(e => e.WarehouseId == warehouseId)
            .AsNoTracking()
            .SelectMany(e => e.TransferredStocks);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ts => new TransferredStockDTO
            {
                TransferredStockId = ts.TransferredStockId,
                DueDate = ts.DueDate,
                Status = ts.Status.ToString(),
                UserId = ts.UserId,
                WarehouseId = warehouseId,
                DistinationWarehouseId = ts.DistinationWarehouseId,
                Comment = ts.Comment
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<TransferredStockDTO>>.SuccessResponse(
            new PagedResult<TransferredStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetTransferredStockStatus()
    {
        var statuses = Enum.GetValues(typeof(GeneralStatus))
            .Cast<GeneralStatus>()
            .Select(s => new NameStatus
            {
                Id = (int)s,
                Name = s.ToString()
            })
            .ToList();

        return await Task.FromResult(new GeneralResponse<List<NameStatus>>
        {
            Success = true,
            Message = "Transferred stock statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByWarehouseIdAsync(string userId, AddTransferredStockDTO dto)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);
        if (warehouse == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Warehouse is not found");

        var destinationWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.DistinationWarehouseId);
        if (destinationWarehouse == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Destination warehouse is not found");

        var mapping = new TransferredStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            DistinationWarehouseId = dto.DistinationWarehouseId,
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new TransferredStockDTO
        {
            TransferredStockId = res.TransferredStockId,
            DueDate = res.DueDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            DistinationWarehouseId = res.DistinationWarehouseId,
            Comment = res.Comment
        };

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<TransferredStockDTO>> UpdateTransferredStockAsync(string userId, int transferredStockId, UpdateTransferredStockDTO dto)
    {
        var entity = await _context.TransferredStocks.FirstOrDefaultAsync(e => e.TransferredStockId == dto.TransferredStockId);

        if (entity.TransferredStockId != transferredStockId)
        {
            return GeneralResponse<TransferredStockDTO>.FailResponse("id not equal transferred stock id!");
        }
        if (entity == null)
        {
            return GeneralResponse<TransferredStockDTO>.FailResponse("not found");
        }

        var destinationWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.DistinationWarehouseId);
        if (destinationWarehouse == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Destination warehouse is not found");

        entity.DueDate = dto.DueDate;
        entity.UserId = userId;
        entity.DistinationWarehouseId = dto.DistinationWarehouseId;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment
        };

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<TransferredStock>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId)
    {
        return await Query().Where(ts => ts.DistinationWarehouseId == destinationWarehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<TransferredStockDTO>>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(ts => ts.Status == statusEnum)
                .Select(ts => new TransferredStockDTO
                {
                    TransferredStockId = ts.TransferredStockId,
                    DueDate = ts.DueDate,
                    Status = ts.Status.ToString(),
                    UserId = ts.UserId,
                    WarehouseId = ts.WarehouseId,
                    DistinationWarehouseId = ts.DistinationWarehouseId,
                    Comment = ts.Comment
                }).ToListAsync();

            return GeneralResponse<IEnumerable<TransferredStockDTO>>.SuccessResponse(query);
        }
        return GeneralResponse<IEnumerable<TransferredStockDTO>>.FailResponse("Not found any transferred stock to this status now!");
    }

    public async Task<IEnumerable<TransferredStock>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(ts => ts.UserId == userId).ToListAsync();
    }

    public async Task<TransferredStock?> GetWithItemsAsync(int transferredStockId)
    {
        return await QueryIncluding(false, ts => ts.TransferredItems)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);
    }

    public async Task<TransferredStock?> GetWithWarehousesAsync(int transferredStockId)
    {
        return await QueryIncluding(false, ts => ts.Warehouse, ts => ts.DistinationWarehouse)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);
    }

    public async Task<IEnumerable<TransferredStock>> GetPendingTransfersAsync()
    {
        return await Query().Where(ts => ts.Status == GeneralStatus.Draft || ts.Status == GeneralStatus.Processing).ToListAsync();
    }

    public async Task<IEnumerable<TransferredStock>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(ts => ts.CreatedAt >= startDate && ts.CreatedAt <= endDate).ToListAsync();
    }
}
