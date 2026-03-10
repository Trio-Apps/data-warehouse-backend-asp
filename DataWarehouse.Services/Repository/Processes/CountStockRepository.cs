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

namespace DataWarehouse.Services.Repository.Processes;

public class CountStockRepository : BaseRepository<CountStock>, ICountStockRepository
{
    private readonly IApprovalRepository approval;

    public CountStockRepository(
        IApprovalRepository approval,
        DataWarehouseDbContext context) : base(context)
    {
        this.approval = approval;
    }

    public async Task<IEnumerable<CountStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query()
            .Where(x => x.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<CountStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.CountStocks
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .OrderByDescending(x => x.CountStockId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CountStockDTO
            {
                CountStockId = x.CountStockId,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                PostingDate = x.PostingDate,
                WarehouseId = x.WarehouseId,
                CountStockItems = null
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
        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(w => w.WarehouseId == dto.WarehouseId);

        if (!warehouseExists)
            return GeneralResponse<CountStockDTO>.FailResponse("Warehouse is not found");

        var entity = new CountStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment
        };

        await _context.CountStocks.AddAsync(entity);
        await _context.SaveChangesAsync();

        if (!dto.IsDraft)
        {
            try
            {
                await approval.StartProcessAsync(
                    processType: ProcessType.Counting,
                    referenceId: entity.CountStockId,
                    warehouseId: entity.WarehouseId,
                    userId: userId);
            }
            catch (Exception ex)
            {
                return GeneralResponse<CountStockDTO>.FailResponse(ex.Message);
            }
        }

        return GeneralResponse<CountStockDTO>.SuccessResponse(MapToDto(entity));
    }

    public async Task<GeneralResponse<CountStockDTO>> UpdateCountStockAsync(string userId, int countStockId, UpdateCountStockDTO dto)
    {
        var entity = await _context.CountStocks
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

        if (entity == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Count stock not found");

        if (dto.CountStockId > 0 && dto.CountStockId != countStockId)
            return GeneralResponse<CountStockDTO>.FailResponse("ID mismatch");

        var process = await approval.GetProcessItem(entity.CountStockId, ProcessType.Counting);
        if (process != null && process.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<CountStockDTO>.FailResponse(
                "You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        if (entity.PostingDate != dto.PostingDate)
            entity.PostingDate = dto.PostingDate;

        if (entity.Comment != dto.Comment)
            entity.Comment = dto.Comment;

        if (!string.IsNullOrWhiteSpace(userId) && entity.UserId != userId)
            entity.UserId = userId;

        await _context.SaveChangesAsync();

        return GeneralResponse<CountStockDTO>.SuccessResponse(MapToDto(entity));
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
        if (!Enum.TryParse<GeneralStatus>(status, true, out var statusEnum))
            return GeneralResponse<IEnumerable<CountStockDTO>>.FailResponse("Not found any count stock to this status now!");

        var data = await Query()
            .Where(x => x.Status == statusEnum)
            .Select(x => new CountStockDTO
            {
                CountStockId = x.CountStockId,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                PostingDate = x.PostingDate,
                WarehouseId = x.WarehouseId
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
            .Where(x => x.Status == GeneralStatus.Processing)
            .ToListAsync();
    }

    public async Task<IEnumerable<CountStock>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query()
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .ToListAsync();
    }

    public async Task<GeneralResponse<CountStockDTO>> SubmitCountStockAsync(string userId, int countStockId, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return GeneralResponse<CountStockDTO>.FailResponse("User ID not found in token.");

        var entity = await _context.CountStocks
            .Include(x => x.CountStockItem)
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

        if (entity == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Count stock not found.");

        if (entity.Status != GeneralStatus.Draft)
            return GeneralResponse<CountStockDTO>.FailResponse("Only Draft orders can be submitted.");

        if (entity.CountStockItem == null || !entity.CountStockItem.Any())
            return GeneralResponse<CountStockDTO>.FailResponse("Count stock must contain at least one item before submit.");

        try
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Counting,
                referenceId: entity.CountStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }
        catch (Exception ex)
        {
            return GeneralResponse<CountStockDTO>.FailResponse(ex.Message);
        }

        entity.Status = GeneralStatus.Processing;
        entity.UserId = userId;

        if (!string.IsNullOrWhiteSpace(note))
        {
            var cleanNote = note.Trim();
            entity.Comment = string.IsNullOrWhiteSpace(entity.Comment)
                ? cleanNote
                : $"{entity.Comment}{Environment.NewLine}{cleanNote}";
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<CountStockDTO>.SuccessResponse(MapToDto(entity, includeItems: true));
    }

    private static CountStockDTO MapToDto(CountStock entity, bool includeItems = false)
    {
        return new CountStockDTO
        {
            CountStockId = entity.CountStockId,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            PostingDate = entity.PostingDate,
            WarehouseId = entity.WarehouseId,
            CountStockItems = includeItems
                ? entity.CountStockItem.Select(item => new CountStockItemDTO
                {
                    CountStockItemId = item.CountStockItemId,
                    Quantity = item.Quantity,
                    Status = item.Status.ToString(),
                    ErrorMessage = item.ErrorMessage,
                    UoMEntry = item.UoMEntry,
                    BarCode = item.BarCode,
                    UnitPrice = item.UnitPrice,
                    Comment = item.Comment,
                    CountStockId = item.CountStockId,
                    ItemId = item.ItemId,
                    IsBatchManaged = item.Item?.BatchNumbers ?? false
                }).ToList()
                : null
        };
    }
}
