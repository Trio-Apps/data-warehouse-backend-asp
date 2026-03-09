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
<<<<<<< HEAD
=======
    private const string CountingMode = "Counting";
    private const string PostingMode = "Posting";
>>>>>>> a523272dcfa9d2208665ee176cf81c9851798e63
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
<<<<<<< HEAD
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
=======
                CountStockId = cs.CountStockId,
                Status = cs.Status.ToString(),
              
                UserId = cs.UserId,
                WarehouseId = warehouseId,
                Comment = cs.Comment,
                CreatedAt = cs.CreatedAt,
                PostingDate = cs.PostingDate,
                Mode = GetModeFromDocType(cs.DocType)
>>>>>>> a523272dcfa9d2208665ee176cf81c9851798e63
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

<<<<<<< HEAD
=======
    public async Task<GeneralResponse<CountStockDTO>> AddCountStockByWarehouseIdAsync(string userId, AddCountStockDTO dto)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);
        if (warehouse == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Warehouse is not found");

        var normalizedMode = NormalizeMode(dto.Mode);

        using var tx = await _context.Database.BeginTransactionAsync();

        var mapping = new CountStock
        {
            Status = GeneralStatus.Draft,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            PostingDate = dto.PostingDate,
            DocType = GetDocTypeFromMode(normalizedMode),
        };

        try
        {
            var res = await AddAsync(mapping);
            await SaveChangesAsync();

            if (!dto.IsDraft)
            {
                try
                {
                    await approval.StartProcessAsync(
                        processType: ProcessType.Counting,
                        referenceId: res.CountStockId,
                        warehouseId: res.WarehouseId,
                        userId: userId
                    );
                }
                catch (Exception ex)
                {
                    return GeneralResponse<CountStockDTO>.FailResponse(ex.Message);
                }

                res.Status = GeneralStatus.Processing;
                await SaveChangesAsync();
            }

            await tx.CommitAsync();

            var model = new CountStockDTO
            {
                CountStockId = res.CountStockId,
                Status = res.Status.ToString(),
                UserId = res.UserId,
                WarehouseId = res.WarehouseId,
                Comment = res.Comment,
                CreatedAt = res.CreatedAt,
                PostingDate = res.PostingDate,
                Mode = GetModeFromDocType(res.DocType)
            };

            return GeneralResponse<CountStockDTO>.SuccessResponse(model);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return GeneralResponse<CountStockDTO>.FailResponse(ex.Message);
        }
    }

    public async Task<GeneralResponse<CountStockDTO>> UpdateCountStockAsync(string userId, int countStockId, UpdateCountStockDTO dto)
    {
        var entity = await _context.CountStocks.FirstOrDefaultAsync(e => e.CountStockId == countStockId);

        if (entity == null)
            return GeneralResponse<CountStockDTO>.FailResponse("not found");

        if (dto.CountStockId != countStockId)
        {
            return GeneralResponse<CountStockDTO>.FailResponse("id not equal count stock id!");
        }

        if (entity.Status != GeneralStatus.Draft)
            return GeneralResponse<CountStockDTO>.FailResponse("Only Draft count stocks can be edited.");

        entity.UserId = userId;
        entity.PostingDate = dto.PostingDate;
        entity.Comment = dto.Comment;

        if (!string.IsNullOrWhiteSpace(dto.Mode))
        {
            var normalizedMode = NormalizeMode(dto.Mode);
            entity.DocType = GetDocTypeFromMode(normalizedMode);
        }

        await _context.SaveChangesAsync();

        var result = new CountStockDTO
        {
            CountStockId = entity.CountStockId,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            PostingDate = entity.PostingDate,
            Mode = GetModeFromDocType(entity.DocType)
        };

        return GeneralResponse<CountStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<CountStockDTO>> SubmitCountStockAsync(string userId, int countStockId, string? note = null)
    {
        var entity = await _context.CountStocks
            .Include(x => x.CountStockItem)
                .ThenInclude(i => i.CountStockBatches)
            .FirstOrDefaultAsync(x => x.CountStockId == countStockId);

        if (entity == null)
            return GeneralResponse<CountStockDTO>.FailResponse("Count stock not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<CountStockDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.Status != GeneralStatus.Draft)
            return GeneralResponse<CountStockDTO>.FailResponse("Only Draft count stocks can be submitted.");

        if (entity.CountStockItem == null || !entity.CountStockItem.Any())
            return GeneralResponse<CountStockDTO>.FailResponse("Count stock must contain at least one item before submit.");

        if (entity.CountStockItem.Any(i => i.Quantity <= 0))
            return GeneralResponse<CountStockDTO>.FailResponse("Each item quantity must be greater than 0 before submit.");

        var itemIds = entity.CountStockItem
            .Select(x => x.ItemId)
            .Distinct()
            .ToList();

        var batchManagedItemIds = await _context.WarehouseItems
            .AsNoTracking()
            .Where(x => x.WarehouseId == entity.WarehouseId
                        && x.IsBatchManaged
                        && itemIds.Contains(x.ItemId))
            .Select(x => x.ItemId)
            .ToListAsync();

        foreach (var item in entity.CountStockItem.Where(i => batchManagedItemIds.Contains(i.ItemId)))
        {
            if (item.CountStockBatches == null || !item.CountStockBatches.Any())
                return GeneralResponse<CountStockDTO>.FailResponse($"Batch-managed item '{item.ItemId}' requires at least one batch before submit.");

            var totalBatchQty = item.CountStockBatches.Sum(x => x.Quantity);
            if (!AreEqual(totalBatchQty, item.Quantity))
                return GeneralResponse<CountStockDTO>.FailResponse($"Batch quantity total for item '{item.ItemId}' must equal counted quantity.");
        }

        if (string.IsNullOrWhiteSpace(entity.DocType))
            entity.DocType = GetDocTypeFromMode(CountingMode);

        try
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Counting,
                referenceId: entity.CountStockId,
                warehouseId: entity.WarehouseId,
                userId: userId
            );
        }
        catch (Exception ex)
        {
            return GeneralResponse<CountStockDTO>.FailResponse(ex.Message);
        }

        entity.Status = GeneralStatus.Processing;
        entity.UserId = userId;
        entity.Comment = MergeNote(entity.Comment, note);

        await _context.SaveChangesAsync();

        return GeneralResponse<CountStockDTO>.SuccessResponse(new CountStockDTO
        {
            CountStockId = entity.CountStockId,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            PostingDate = entity.PostingDate,
            Mode = GetModeFromDocType(entity.DocType),
            CountStockItems = entity.CountStockItem.Select(i => new CountStockItemDTO
            {
                CountStockItemId = i.CountStockItemId,
                CountStockId = i.CountStockId,
                ItemId = i.ItemId,
                Quantity = i.Quantity,
                Status = i.Status.ToString(),
                ErrorMessage = i.ErrorMessage,
                UoMEntry = i.UoMEntry,
                BarCode = i.BarCode,
                UnitPrice = i.UnitPrice,
                Comment = i.Comment
            }).ToList()
        });
    }

>>>>>>> a523272dcfa9d2208665ee176cf81c9851798e63
    public async Task<GeneralResponse<IEnumerable<CountStockDTO>>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            return GeneralResponse<IEnumerable<CountStockDTO>>.FailResponse("Not found any count to this status now!");

<<<<<<< HEAD
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
=======
        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(cs => cs.Status == statusEnum)
             .Select(cs => new CountStockDTO
             {
                 CountStockId = cs.CountStockId,
                 Status = cs.Status.ToString(),
                 
                 UserId = cs.UserId,
                 WarehouseId = cs.WarehouseId,
                 Comment = cs.Comment,
                 CreatedAt = cs.CreatedAt,
                 PostingDate = cs.PostingDate,
                 Mode = GetModeFromDocType(cs.DocType)
             }).ToListAsync();
>>>>>>> a523272dcfa9d2208665ee176cf81c9851798e63

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

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }

    private static bool AreEqual(decimal left, decimal right)
    {
        return Math.Abs(left - right) <= 0.000001m;
    }

    private static string? MergeNote(string? current, string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return current;

        var cleanNote = note.Trim();
        return string.IsNullOrWhiteSpace(current)
            ? cleanNote
            : $"{current}{Environment.NewLine}{cleanNote}";
    }

    private static string NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return CountingMode;

        if (string.Equals(mode, CountingMode, StringComparison.OrdinalIgnoreCase))
            return CountingMode;

        if (string.Equals(mode, PostingMode, StringComparison.OrdinalIgnoreCase))
            return PostingMode;

        return CountingMode;
    }

    private static string GetDocTypeFromMode(string? mode)
    {
        var normalized = NormalizeMode(mode);
        return normalized == PostingMode ? "InventoryPosting" : "InventoryCounting";
    }

    private static string GetModeFromDocType(string? docType)
    {
        if (string.Equals(docType, "InventoryPosting", StringComparison.OrdinalIgnoreCase))
            return PostingMode;

        return CountingMode;
    }
}
