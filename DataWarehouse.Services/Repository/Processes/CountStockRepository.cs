using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class CountStockRepository : BaseRepository<CountStock>, ICountStockRepository
{
    private readonly IApprovalRepository approval;

    public CountStockRepository(IApprovalRepository approval, DataWarehouseDbContext context) : base(context)
    {
        this.approval = approval;
    }

    public async Task<IEnumerable<CountStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query()
            .Where(x => x.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<CountStockDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.CountStocks
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Counting);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.CountStockId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                Order = x,
                HasProgress = processQuery.Any(p => p.ReferenceId == x.CountStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == x.CountStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new CountStockDTO
            {
                CountStockId = x.Order.CountStockId,
                Status = x.Order.Status.ToString(),
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                Comment = x.Order.Comment,
                CreatedAt = x.Order.CreatedAt,
                PostingDate = x.Order.PostingDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<CountStockDTO>>.SuccessResponse(
            new PagedResult<CountStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<CountStockDTO>> AddCountStockByWarehouseIdAsync(string userId, AddCountStockDTO dto)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);

        if (warehouse == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Warehouse is not found");

        var entity = new CountStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            PostingDate = dto.PostingDate
        };

        var saved = await AddAsync(entity);
        await SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Counting,
                referenceId: saved.CountStockId,
                warehouseId: saved.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<CountStockDTO>.SuccessResponse(new CountStockDTO
        {
            CountStockId = saved.CountStockId,
            Status = saved.Status.ToString(),
            UserId = saved.UserId,
            WarehouseId = saved.WarehouseId,
            Comment = saved.Comment,
            CreatedAt = saved.CreatedAt,
            PostingDate = saved.PostingDate
        });
    }

    public async Task<GeneralResponse<CountStockDTO>> UpdateCountStockAsync(string userId, int countStockId, UpdateCountStockDTO dto)
    {
        var entity = await _context.CountStocks
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

        if (entity == null)
            return GeneralResponse<CountStockDTO>.FailResponse("not found");

        if (dto.CountStockId > 0 && entity.CountStockId != dto.CountStockId)
            return GeneralResponse<CountStockDTO>.FailResponse("id not equal count stock id!");

        var checkApprovalStatus = await approval.GetProcessItem(entity.CountStockId, ProcessType.Counting);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<CountStockDTO>.FailResponse(
                "You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        entity.UserId = userId;
        entity.PostingDate = dto.PostingDate;
        entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        return GeneralResponse<CountStockDTO>.SuccessResponse(new CountStockDTO
        {
            CountStockId = entity.CountStockId,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            PostingDate = entity.PostingDate
        });
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetCountStockStatus()
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
            Message = "Count stock statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<GeneralResponse<IEnumerable<CountStockDTO>>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            return GeneralResponse<IEnumerable<CountStockDTO>>.FailResponse("Not found any count to this status now!");

        var data = await Query()
            .Where(x => x.Status == statusEnum)
            .Select(x => new CountStockDTO
            {
                CountStockId = x.CountStockId,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                WarehouseId = x.WarehouseId,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                PostingDate = x.PostingDate
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<CountStockDTO>>.SuccessResponse(data);
    }

    public async Task<IEnumerable<CountStock>> GetByUserIdAsync(string userId)
    {
        return await Query()
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<CountStock?> GetWithItemsAsync(int countStockId)
    {
        return await QueryIncluding(false, x => x.CountStockItem)
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);
    }

    public async Task<CountStock?> GetWithWarehouseAsync(int countStockId)
    {
        return await QueryIncluding(false, x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);
    }

    public async Task<IEnumerable<CountStock>> GetPendingCountsAsync()
    {
        return await Query()
            .Where(x => x.Status == GeneralStatus.Draft || x.Status == GeneralStatus.Processing)
            .ToListAsync();
    }

    public async Task<IEnumerable<CountStock>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query()
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .ToListAsync();
    }
}
