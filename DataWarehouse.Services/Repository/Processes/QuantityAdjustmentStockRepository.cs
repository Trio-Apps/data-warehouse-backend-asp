using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
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
using System.Threading;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class QuantityAdjustmentStockRepository : BaseRepository<QuantityAdjustmentStock>, IQuantityAdjustmentStockRepository
{
    private readonly IBaseProcessesRepository<QuantityAdjustmentStock> baseProcesses;
    private readonly IApprovalRepository approval;

    public QuantityAdjustmentStockRepository(
        IBaseProcessesRepository<QuantityAdjustmentStock> baseProcesses,
        IApprovalRepository approval,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
    }

    public async Task<IEnumerable<QuantityAdjustmentStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query()
            .Where(x => x.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.QuantityAdjustmentStocks
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.QuantityAdjustment);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(x => x.QuantityAdjustmentStockId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                Order = x,
                HasProgress = processQuery.Any(p => p.ReferenceId == x.QuantityAdjustmentStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == x.QuantityAdjustmentStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new QuantityAdjustmentStockDTO
            {
                QuantityAdjustmentStockId = x.Order.QuantityAdjustmentStockId,
                DueDate = x.Order.DueDate,
                PostingDate = x.Order.PostingDate ?? default,
                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                CreatedAt = x.Order.CreatedAt,
                ItemCount = x.Order.QuantityAdjustmentStockItems.Count(),
                Approval = x.HasProgress,
                ErrorMessage = x.Order.ErrorMessage,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>.SuccessResponse(
            new PagedResult<QuantityAdjustmentStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId, string userId, DateTime? postingDate, DateTime? dueDate, string? status,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.QuantityAdjustmentStocks
            .AsNoTracking()
            .Include(x => x.QuantityAdjustmentStockItems)
            .Include(x => x.Warehouse)
            .Where(x => x.WarehouseId == warehouseId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GeneralStatus>(status, true, out var statusEnum))
        {
            query = query.Where(x => x.Status == statusEnum);
        }

        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(x => x.PostingDate.HasValue && x.PostingDate.Value.Date == postDate);
        }

        if (dueDate.HasValue)
        {
            var targetDueDate = dueDate.Value.Date;
            query = query.Where(x => x.DueDate.Date == targetDueDate);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.QuantityAdjustment);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                Order = x,
                HasProgress = processQuery.Any(p => p.ReferenceId == x.QuantityAdjustmentStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == x.QuantityAdjustmentStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new QuantityAdjustmentStockDTO
            {
                QuantityAdjustmentStockId = x.Order.QuantityAdjustmentStockId,
                DueDate = x.Order.DueDate,
                PostingDate = x.Order.PostingDate ?? default,
                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                ErrorMessage = x.Order.ErrorMessage,

                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                CreatedAt = x.Order.CreatedAt,
                ItemCount = x.Order.QuantityAdjustmentStockItems.Count(),
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<QuantityAdjustmentStockDTO>>.SuccessResponse(
            new PagedResult<QuantityAdjustmentStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockDTO>> GetWithWarehouseAsync(
        int quantityAdjustmentStockId, string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuantityAdjustmentStocks
            .Include(x => x.Warehouse)
            .Include(x => x.QuantityAdjustmentStockItems)
            .FirstOrDefaultAsync(x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse("this QuantityAdjustmentStockId is not found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.QuantityAdjustment, entity.QuantityAdjustmentStockId);

        var result = new QuantityAdjustmentStockDTO
        {
            QuantityAdjustmentStockId = entity.QuantityAdjustmentStockId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate ?? default,
            Status = entity.Status.ToString(),
            Comment = entity.Comment,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse.WarehouseCode,
            CreatedAt = entity.CreatedAt,
            ItemCount = entity.QuantityAdjustmentStockItems.Count(),
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,
            Reason = approvalModel.Reason
        };


        return GeneralResponse<QuantityAdjustmentStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockDTO>> AddQuantityAdjustmentStockByWarehouseIdAsync(
        string userId, AddQuantityAdjustmentStockDTO dto, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId, cancellationToken);

        if (warehouse == null)
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse("Warehouse is not found");

        var entity = new QuantityAdjustmentStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment
        };

        var saved = await AddAsync(entity);
        await SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.QuantityAdjustment,
                referenceId: saved.QuantityAdjustmentStockId,
                warehouseId: saved.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<QuantityAdjustmentStockDTO>.SuccessResponse(new QuantityAdjustmentStockDTO
        {
            QuantityAdjustmentStockId = saved.QuantityAdjustmentStockId,
            DueDate = saved.DueDate,
            PostingDate = saved.PostingDate ?? default,
            Status = saved.Status.ToString(),
            UserId = saved.UserId,
            WarehouseId = saved.WarehouseId,
            Comment = saved.Comment,
            CreatedAt = saved.CreatedAt
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockDTO>> UpdateQuantityAdjustmentStockAsync(
        string userId, int quantityAdjustmentStockId, UpdateQuantityAdjustmentStockDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuantityAdjustmentStocks
            .FirstOrDefaultAsync(x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse("not found");

        if (dto.QuantityAdjustmentStockId > 0 && dto.QuantityAdjustmentStockId != quantityAdjustmentStockId)
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse("id not equal quantity adjustment stock id!");

        var checkApprovalStatus = await approval.GetProcessItem(entity.QuantityAdjustmentStockId, ProcessType.QuantityAdjustment, cancellationToken);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse(
                "You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        if (dto.PostingDate.HasValue && entity.PostingDate != dto.PostingDate.Value)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue && entity.DueDate != dto.DueDate.Value)
            entity.DueDate = dto.DueDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Comment) && entity.Comment != dto.Comment)
            entity.Comment = dto.Comment;

        if (entity.UserId != userId)
            entity.UserId = userId;

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.QuantityAdjustment,
                referenceId: entity.QuantityAdjustmentStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<QuantityAdjustmentStockDTO>.SuccessResponse(new QuantityAdjustmentStockDTO
        {
            QuantityAdjustmentStockId = entity.QuantityAdjustmentStockId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate ?? default,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockDTO>> DeleteQuantityAdjustmentStockAsync(
        int quantityAdjustmentStockId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuantityAdjustmentStocks
            .FirstOrDefaultAsync(x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(entity.QuantityAdjustmentStockId, ProcessType.QuantityAdjustment, cancellationToken);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<QuantityAdjustmentStockDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        var result = new QuantityAdjustmentStockDTO
        {
            QuantityAdjustmentStockId = entity.QuantityAdjustmentStockId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate ?? default,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt
        };

        _context.QuantityAdjustmentStocks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<QuantityAdjustmentStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int quantityAdjustmentStockId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<QuantityAdjustmentStock>(
            quantityAdjustmentStockId,
            ProcessType.QuantityAdjustment,
            x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            _context.QuantityAdjustmentStocks
        );
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetQuantityAdjustmentStockStatus()
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
            Message = "Quantity adjustment stock statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockDTO>>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            return GeneralResponse<IEnumerable<QuantityAdjustmentStockDTO>>.FailResponse("Not found any quantity adjustment stock to this status now!");

        var data = await Query()
            .Where(x => x.Status == statusEnum)
            .Select(x => new QuantityAdjustmentStockDTO
            {
                QuantityAdjustmentStockId = x.QuantityAdjustmentStockId,
                DueDate = x.DueDate,
                PostingDate = x.PostingDate ?? default,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                WarehouseId = x.WarehouseId,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<QuantityAdjustmentStockDTO>>.SuccessResponse(data);
    }

    public async Task<QuantityAdjustmentStock?> GetWithItemsAsync(int quantityAdjustmentStockId)
    {
        return await QueryIncluding(false, x => x.QuantityAdjustmentStockItems)
            .FirstOrDefaultAsync(x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId);
    }
}
