using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Services.Processes;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class TransferredRequestOrderRepository : BaseRepository<TransferredRequest>, ITransferredRequestOrderRepository
{
    private readonly IBaseProcessesRepository<TransferredRequest> baseProcesses;
    private readonly IApprovalRepository approval;
    private readonly ISapCache sapCache;
    private readonly ReasonValidationService reasonValidationService;

    public TransferredRequestOrderRepository(
        IBaseProcessesRepository<TransferredRequest> baseProcesses,
        IApprovalRepository approval,
        ISapCache sapCache,
        ReasonValidationService reasonValidationService,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
        this.sapCache = sapCache;
        this.reasonValidationService = reasonValidationService;
    }
    public async Task<GeneralResponse<PagedResult<WarehouseItemDto>>> GetByWarehouseIdAsync(
  int warehouseId,
  string? search = null,
  int pageNumber = 1,
  int pageSize = 20)
    {
        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 20;

        var sapId = await sapCache.Get();

        // 1) ??? ?????? ???????? ??? ?????
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.WarehouseId == warehouseId)
            .Select(w => new { w.WarehouseId, w.WarehouseCode })
            .SingleOrDefaultAsync();

        if (warehouse == null)
        {
            return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(new PagedResult<WarehouseItemDto>
            {
                Data = Array.Empty<WarehouseItemDto>(),
                TotalRecords = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        // 2) Left join Items ?? WarehouseItems (???? ???????? ???)
        var query =
      from i in _context.Items.AsNoTracking().Where(it => it.SapId == sapId)
      join wi in _context.WarehouseItems.AsNoTracking()
              .Where(x => x.WarehouseId == warehouseId)
          on i.ItemId equals wi.ItemId
      where i.InventoryItem
            && i.Valid
            && (
                  search == null
                  || (i.ItemCode != null && EF.Functions.Like(i.ItemCode, $"%{search}%"))
                  || (i.ItemName != null && EF.Functions.Like(i.ItemName, $"%{search}%"))
               )
      select new WarehouseItemDto
      {
          WarehouseItemId = wi.WarehouseItemId,
          ItemId = i.ItemId,
          WarehouseId = wi.WarehouseId,
          ItemName = i.ItemName,
          ItemCode = i.ItemCode,
          WarehouseCode = warehouse.WarehouseCode,
          InStock = wi.InStock,
          MinStock = wi.MinStock
      };

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.ItemName)
            .ThenBy(x => x.ItemCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<WarehouseItemDto>
        {
            Data = items,
            TotalRecords = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return GeneralResponse<PagedResult<WarehouseItemDto>>.SuccessResponse(result);
    }

    public async Task<IEnumerable<TransferredRequest>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(x => x.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<TransferredRequestDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredRequests
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.TransferredRequest);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(x => x.TransferredRequestId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new
            {
                Order = iw,
                HasProgress = processQuery.Any(p => p.ReferenceId == iw.TransferredRequestId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == iw.TransferredRequestId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new TransferredRequestDTO
            {
                TransferredRequestId = x.Order.TransferredRequestId,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                DistinationWarehouseId = x.Order.DistinationWarehouseId,
                Comment = x.Order.Comment,
                CreatedAt = x.Order.CreatedAt,
                ErrorMessage = x.Order.ErrorMessage,

                ItemCount = x.Order.TransferredRequestItems.Count(),
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,
                ReasonId = x.Order.ReasonId,
                ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,
                TransferredStockId = x.Order.TransferredStock != null ? x.Order.TransferredStock.TransferredStockId : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<TransferredRequestDTO>>.SuccessResponse(
            new PagedResult<TransferredRequestDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PagedResult<TransferredRequestDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? destinationWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? liveStatus,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredRequests
            .AsNoTracking()
            .Include(x => x.TransferredRequestItems)
            .Include(x => x.Warehouse)
            .Include(x => x.DistinationWarehouse)
            .Include(x => x.TransferredStock)
            .Where(x => x.WarehouseId == warehouseId);

        if (destinationWarehouseId.HasValue)
            query = query.Where(x => x.DistinationWarehouseId == destinationWarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            query = query.Where(x => x.Status == statusEnum);

        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(x => x.CreatedAt.Date == postDate);
        }

        if (dueDate.HasValue)
        {
            var dueDateValue = dueDate.Value.Date;
            query = query.Where(x => x.DueDate.Date == dueDateValue);
        }

        if (!string.IsNullOrWhiteSpace(liveStatus))
        {
            if (liveStatus.Equals("not-transferred", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.TransferredStock == null);
            else
                query = query.Where(x => x.TransferredStock != null);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.TransferredRequest);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new
            {
                Order = iw,
                HasProgress = processQuery.Any(p => p.ReferenceId == iw.TransferredRequestId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == iw.TransferredRequestId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new TransferredRequestDTO
            {
                TransferredRequestId = x.Order.TransferredRequestId,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                DistinationWarehouseId = x.Order.DistinationWarehouseId,
                Comment = x.Order.Comment,
                CreatedAt = x.Order.CreatedAt,
                ErrorMessage = x.Order.ErrorMessage,
                ItemCount = x.Order.TransferredRequestItems.Count(),
                WarehouseName = x.Order.Warehouse.WarehouseName,
                DistinationWarehouseName = x.Order.DistinationWarehouse.WarehouseName,
                TransferredStockId = x.Order.TransferredStock != null ? x.Order.TransferredStock.TransferredStockId : null,
                ReasonId = x.Order.ReasonId,
                ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<TransferredRequestDTO>>.SuccessResponse(
            new PagedResult<TransferredRequestDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<TransferredRequestDTO>> GetWithWarehousesAndApprovalAsync(
        int transferredRequestId, string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TransferredRequests
            .Include(x => x.Warehouse)
            .Include(x => x.DistinationWarehouse)
            .Include(x => x.TransferredStock)
            .Include(x => x.TransferredRequestItems)
            .FirstOrDefaultAsync(x => x.TransferredRequestId == transferredRequestId, cancellationToken);

        if (entity == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("this TransferredRequestId is not found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.TransferredRequest, entity.TransferredRequestId);

        return GeneralResponse<TransferredRequestDTO>.SuccessResponse(new TransferredRequestDTO
        {
            TransferredRequestId = entity.TransferredRequestId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            ItemCount = entity.TransferredRequestItems.Count,
            WarehouseName = entity.Warehouse.WarehouseName,
            DistinationWarehouseName = entity.DistinationWarehouse.WarehouseName,
            TransferredStockId = entity.TransferredStock?.TransferredStockId,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Reason = approvalModel.Reason,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus
        });
    }

    public async Task<GeneralResponse<TransferredRequestDTO>> AddTransferredRequestByWarehouseIdAsync(
        string userId, AddTransferredRequestDTO dto, CancellationToken cancellationToken = default)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.TransferredRequest);
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId, cancellationToken);
        if (warehouse == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("Warehouse is not found");

        var destinationWarehouse = await _context.Warehouses.FirstOrDefaultAsync(
            w => w.WarehouseId == dto.DistinationWarehouseId,
            cancellationToken);
        if (destinationWarehouse == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("Destination warehouse is not found");

        var entity = new TransferredRequest
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            DistinationWarehouseId = dto.DistinationWarehouseId,
            Comment = dto.Comment,
            ReasonId = dto.ReasonId
        };

        var result = await AddAsync(entity);
        await SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.TransferredRequest,
                referenceId: result.TransferredRequestId,
                warehouseId: result.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<TransferredRequestDTO>.SuccessResponse(new TransferredRequestDTO
        {
            TransferredRequestId = result.TransferredRequestId,
            DueDate = result.DueDate,
            Status = result.Status.ToString(),
            UserId = result.UserId,
            WarehouseId = result.WarehouseId,
            DistinationWarehouseId = result.DistinationWarehouseId,
            Comment = result.Comment,
            CreatedAt = result.CreatedAt,
            ReasonId = result.ReasonId,
            ReasonName = result.Reason != null ? result.Reason.Name : null
        });
    }

    public async Task<GeneralResponse<TransferredRequestDTO>> UpdateTransferredRequestAsync(
        string userId, int transferredRequestId, UpdateTransferredRequestDTO dto, CancellationToken cancellationToken = default)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.TransferredRequest);
        var entity = await _context.TransferredRequests
            .FirstOrDefaultAsync(x => x.TransferredRequestId == dto.TransferredRequestId, cancellationToken);

        if (entity == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("not found");

        if (entity.TransferredRequestId != transferredRequestId)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("id not equal transferred request id");

        var checkApprovalStatus = await approval.GetProcessItem(entity.TransferredRequestId, ProcessType.TransferredRequest, cancellationToken);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<TransferredRequestDTO>.FailResponse(
                "You cannot edit this request because its approval status is 'Approved' and all approval steps have been completed.");
        }

        if (dto.DueDate.HasValue && entity.DueDate != dto.DueDate.Value)
        {
            entity.DueDate = dto.DueDate.Value;
        }

        if (entity.DistinationWarehouseId != dto.DistinationWarehouseId)
        {
            var destinationExists = await _context.Warehouses
                .AnyAsync(w => w.WarehouseId == dto.DistinationWarehouseId, cancellationToken);

            if (!destinationExists)
                return GeneralResponse<TransferredRequestDTO>.FailResponse("Destination warehouse is not found");

            entity.DistinationWarehouseId = dto.DistinationWarehouseId;
        }

        if (!string.IsNullOrWhiteSpace(dto.Comment) && entity.Comment != dto.Comment)
            entity.Comment = dto.Comment;

        if (entity.UserId != userId)
            entity.UserId = userId;

        entity.ReasonId = dto.ReasonId;

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.TransferredRequest,
                referenceId: entity.TransferredRequestId,
                warehouseId: entity.WarehouseId,
                userId: userId);

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<TransferredRequestDTO>.SuccessResponse(new TransferredRequestDTO
        {
            TransferredRequestId = entity.TransferredRequestId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        });
    }

    public async Task<GeneralResponse<TransferredRequestDTO>> DuplicateTransferredRequestAsync(
        string userId,
        int transferredRequestId,
        CancellationToken cancellationToken = default)
    {
        var source = await _context.TransferredRequests
            .AsNoTracking()
            .Include(x => x.TransferredRequestItems)
                .ThenInclude(x => x.TransferredRequestBatches)
            .FirstOrDefaultAsync(x => x.TransferredRequestId == transferredRequestId, cancellationToken);
        if (source == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("not found");
        var clone = OrderDuplicationHelper.Clone(source, userId);
        await _context.TransferredRequests.AddAsync(clone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return await GetWithWarehousesAndApprovalAsync(clone.TransferredRequestId, userId, cancellationToken);
    }    public async Task<GeneralResponse<TransferredRequestDTO>> DeleteTransferredRequestAsync(
        int transferredRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.TransferredRequests
            .FirstOrDefaultAsync(x => x.TransferredRequestId == transferredRequestId, cancellationToken);

        if (entity == null)
            return GeneralResponse<TransferredRequestDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(
            entity.TransferredRequestId,
            ProcessType.TransferredRequest,
            cancellationToken);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<TransferredRequestDTO>.FailResponse(
                "You cannot delete this request because its approval status is 'Approved' and all approval steps have been completed.");
        }

        var result = new TransferredRequestDTO
        {
            TransferredRequestId = entity.TransferredRequestId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        };

        _context.TransferredRequests.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<TransferredRequestDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int transferredRequestId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<TransferredRequest>(
            transferredRequestId,
            ProcessType.TransferredRequest,
            x => x.TransferredRequestId == transferredRequestId,
            _context.TransferredRequests
        );
    }

    public async Task<GeneralResponse<IEnumerable<WarehouseItemDto>>> GetItemsByWarehouseIdAsync(int warehouseId)
    {
        var sapId = await sapCache.Get();

        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.WarehouseId == warehouseId)
            .Select(w => new { w.WarehouseId, w.WarehouseCode })
            .SingleOrDefaultAsync();

        if (warehouse == null)
            return GeneralResponse<IEnumerable<WarehouseItemDto>>
                .SuccessResponse(Enumerable.Empty<WarehouseItemDto>());

        var result = await (
            from i in _context.Items.AsNoTracking().Where(it => it.SapId == sapId)
            join wi in _context.WarehouseItems.AsNoTracking().Where(x => x.WarehouseId == warehouseId)
                on i.ItemId equals wi.ItemId into wiGroup
            from wi in wiGroup.DefaultIfEmpty()
            select new WarehouseItemDto
            {
                WarehouseItemId = wi != null ? wi.WarehouseItemId : 0,
                ItemId = i.ItemId,
                WarehouseId = warehouse.WarehouseId,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                WarehouseCode = warehouse.WarehouseCode,
                InStock = wi != null ? wi.InStock : 0,
                MinStock = wi != null ? wi.MinStock : 0
            }).ToListAsync();

        return GeneralResponse<IEnumerable<WarehouseItemDto>>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetTransferredRequestStatus()
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
            Message = "Transferred request statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<IEnumerable<TransferredRequest>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId)
    {
        return await Query().Where(x => x.DistinationWarehouseId == destinationWarehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<TransferredRequestDTO>>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            return GeneralResponse<IEnumerable<TransferredRequestDTO>>.FailResponse("Not found any transferred request to this status now!");

        var data = await Query().Where(x => x.Status == statusEnum)
            .Select(x => new TransferredRequestDTO
            {
                TransferredRequestId = x.TransferredRequestId,
                DueDate = x.DueDate,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                WarehouseId = x.WarehouseId,
                DistinationWarehouseId = x.DistinationWarehouseId,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                ReasonId = x.ReasonId,
                ReasonName = x.Reason != null ? x.Reason.Name : null,
                TransferredStockId = x.TransferredStock != null ? x.TransferredStock.TransferredStockId : null
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<TransferredRequestDTO>>.SuccessResponse(data);
    }

    public async Task<IEnumerable<TransferredRequest>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<TransferredRequest?> GetWithItemsAsync(int transferredRequestId)
    {
        return await QueryIncluding(false, x => x.TransferredRequestItems)
            .FirstOrDefaultAsync(x => x.TransferredRequestId == transferredRequestId);
    }

    public async Task<TransferredRequest?> GetWithWarehousesAsync(int transferredRequestId)
    {
        return await QueryIncluding(false, x => x.Warehouse, x => x.DistinationWarehouse)
            .FirstOrDefaultAsync(x => x.TransferredRequestId == transferredRequestId);
    }

    public async Task<IEnumerable<TransferredRequest>> GetPendingRequestsAsync()
    {
        return await Query().Where(x => x.Status == GeneralStatus.Draft || x.Status == GeneralStatus.Processing).ToListAsync();
    }

    public async Task<IEnumerable<TransferredRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToListAsync();
    }
}

