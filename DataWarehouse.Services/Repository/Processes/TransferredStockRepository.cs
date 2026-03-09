using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class TransferredStockRepository : BaseRepository<TransferredStock>, ITransferredStockRepository
{
    private readonly IBaseProcessesRepository<TransferredStock> baseProcesses;
    private readonly IApprovalRepository approval;
    private readonly DataWarehouseDbContext _context;

    public TransferredStockRepository(IBaseProcessesRepository<TransferredStock> baseProcesses, IApprovalRepository approval, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
        _context = context;
    }


    public async Task<IEnumerable<TransferredStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query()
            .Where(ts => ts.WarehouseId == warehouseId)
            .ToListAsync();
    }


    public async Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredStocks
            .AsNoTracking()
            .Where(ts => ts.WarehouseId == warehouseId)
            .OrderByDescending(ts => ts.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ts => new TransferredStockDTO
            {
                TransferredStockId = ts.TransferredStockId,
                UserId = ts.UserId,
                WarehouseId = ts.WarehouseId,
                DistinationWarehouseId = ts.DistinationWarehouseId,
                WarehouseCode = ts.Warehouse.WarehouseCode,
                DistinationWarehouseName = ts.DistinationWarehouse.WarehouseName,
                DueDate = ts.DueDate,
                Status = ts.Status.ToString(),
                Comment = ts.Comment,
                CreatedAt = ts.CreatedAt,
                ErrorMessage = ts.ErrorMessage,

                ItemCount = ts.TransferredItems.Count(),
                TransferredRequestId = ts.TransferredRequestId,
                IsReceived = ts.ReceivedStock != null,
                ReceivedStockId = ts.ReceivedStock != null ? ts.ReceivedStock.ReceivedStockId : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<TransferredStockDTO>>.SuccessResponse(
            new PagedResult<TransferredStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? destinationWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredStocks
            .AsNoTracking()
            .Include(ts => ts.TransferredItems)
            .Include(ts => ts.Warehouse)
            .Include(ts => ts.DistinationWarehouse)
            .Include(ts => ts.ReceivedStock)
            .Where(ts => ts.WarehouseId == warehouseId);

        if (destinationWarehouseId.HasValue)
            query = query.Where(ts => ts.DistinationWarehouseId == destinationWarehouseId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            query = query.Where(ts => ts.Status == statusEnum);

        if (postingDate.HasValue)
        {
            var createdDate = postingDate.Value.Date;
            query = query.Where(ts => ts.CreatedAt.Date == createdDate);
        }

        if (dueDate.HasValue)
        {
            var due = dueDate.Value.Date;
            query = query.Where(ts => ts.DueDate.Date == due);
        }

        query = query.OrderByDescending(ts => ts.CreatedAt);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Transferred);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ts => new
            {
                Order = ts,
                HasProgress = processQuery.Any(p => p.ReferenceId == ts.TransferredStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == ts.TransferredStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new TransferredStockDTO
            {
                TransferredStockId = x.Order.TransferredStockId,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                DistinationWarehouseId = x.Order.DistinationWarehouseId,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                DistinationWarehouseName = x.Order.DistinationWarehouse.WarehouseName,
                CreatedAt = x.Order.CreatedAt,
                ErrorMessage = x.Order.ErrorMessage,

                ItemCount = x.Order.TransferredItems.Count(),
                TransferredRequestId = x.Order.TransferredRequestId,
                IsReceived = x.Order.ReceivedStock != null,
                ReceivedStockId = x.Order.ReceivedStock != null ? x.Order.ReceivedStock.ReceivedStockId : null,
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<TransferredStockDTO>>.SuccessResponse(
            new PagedResult<TransferredStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAsDestinationWarehouseAndStatusAndDateWithPaginationForDashboardAsync(
      int warehouseId,
      string userId,
      int? transferredWarehouseId,
      DateTime? postingDate,
      DateTime? dueDate,
      string? status,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredStocks
            .AsNoTracking()
            .Include(ts => ts.TransferredItems)
            .Include(ts => ts.Warehouse)
            .Include(ts => ts.DistinationWarehouse)
            .Include(ts => ts.ReceivedStock)
            .Where(ts => ts.DistinationWarehouseId == warehouseId);

        if (transferredWarehouseId.HasValue)
            query = query.Where(ts => ts.WarehouseId == transferredWarehouseId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            query = query.Where(ts => ts.Status == statusEnum);

        if (postingDate.HasValue)
        {
            var createdDate = postingDate.Value.Date;
            query = query.Where(ts => ts.CreatedAt.Date == createdDate);
        }

        if (dueDate.HasValue)
        {
            var due = dueDate.Value.Date;
            query = query.Where(ts => ts.DueDate.Date == due);
        }

        query = query.OrderByDescending(ts => ts.CreatedAt);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Transferred);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ts => new
            {
                Order = ts,
                HasProgress = processQuery.Any(p => p.ReferenceId == ts.TransferredStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == ts.TransferredStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new TransferredStockDTO
            {
                TransferredStockId = x.Order.TransferredStockId,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                DistinationWarehouseId = x.Order.DistinationWarehouseId,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                DistinationWarehouseName = x.Order.DistinationWarehouse.WarehouseName,
                CreatedAt = x.Order.CreatedAt,
                ItemCount = x.Order.TransferredItems.Count(),
                TransferredRequestId = x.Order.TransferredRequestId,
                IsReceived = x.Order.ReceivedStock != null,
                ReceivedStockId = x.Order.ReceivedStock != null ? x.Order.ReceivedStock.ReceivedStockId : null,
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<TransferredStockDTO>>.SuccessResponse(
            new PagedResult<TransferredStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<GeneralResponse<TransferredStockDTO>> GetTransferredStockByIdAsync(
        string userId, int transferredStockId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TransferredStocks
            .Include(ts => ts.Warehouse)
            .Include(ts => ts.DistinationWarehouse)
            .Include(ts => ts.TransferredRequest)
            .Include(ts => ts.ReceivedStock)
            .Include(ts => ts.TransferredItems)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Transferred, entity.TransferredStockId);

        var result = new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            Comment = entity.Comment,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            WarehouseCode = entity.Warehouse.WarehouseCode,
            DistinationWarehouseName = entity.DistinationWarehouse.WarehouseName,
            CreatedAt = entity.CreatedAt,
            ItemCount = entity.TransferredItems.Count,
            TransferredRequestId = entity.TransferredRequestId,
            IsReceived = entity.ReceivedStock != null,
            ReceivedStockId = entity.ReceivedStock != null ? entity.ReceivedStock.ReceivedStockId : null,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,
            Reason = approvalModel.Reason
        };

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockAndItemsByTransferredRequestIdAsync(
      string userId,
      AddTransferredStockDTO dto)
    {
        var request = await _context.TransferredRequests
            .Include(tr => tr.TransferredStock)
            .Include(tr => tr.TransferredRequestItems)
                .ThenInclude(i => i.TransferredRequestBatches)
            .FirstOrDefaultAsync(tr => tr.TransferredRequestId == dto.TransferredRequestId);

        if (request == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Transferred request is not found");

        if (request.TransferredStock != null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Transferred request already has a transferred stock");

        if (request.TransferredRequestItems == null || !request.TransferredRequestItems.Any())
            return GeneralResponse<TransferredStockDTO>.FailResponse("Transferred request has no items");

        var entity = new TransferredStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = request.WarehouseId,
            DistinationWarehouseId = request.DistinationWarehouseId,
            Comment = dto.Comment,
            TransferredRequestId = request.TransferredRequestId,
            TransferredItems = request.TransferredRequestItems.Select(ri => new TransferredItem
            {
                TransferredRequestItemId = ri.TransferredRequestItemId,
                ItemId = ri.ItemId,
                Quantity = ri.Quantity,
                UoMEntry = ri.UoMEntry,
                BarCode = ri.BarCode,
                UnitPrice = ri.UnitPrice,
                Status = GeneralItemStatus.Planned,
                ErrorMessage = null,
                Comment = ri.Comment,
                LineNum = ri.LineNum,
                TransferredStockBatches = ri.TransferredRequestBatches.Select(rb => new TransferredStockBatch
                {
                    TransferredRequestBatchId = rb.TransferredRequestBatchId,
                    Quantity = rb.Quantity,
                    BatchNumber = rb.BatchNumber,
                    ExpiryDate = rb.ExpiryDate,
                    Comment = rb.Comment,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            }).ToList()
        };

        await _context.TransferredStocks.AddAsync(entity);
        await _context.SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Transferred,
                referenceId: entity.TransferredStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            TransferredRequestId = entity.TransferredRequestId
        });
    }

    public async Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByWarehouseIdWithoutRefAsync
        (string userId, AddTransferredStockWithoutRefDTO dto)
    {
        var sourceWarehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);
        if (sourceWarehouse == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Warehouse is not found");

        var destinationWarehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == dto.DistinationWarehouseId);
        if (destinationWarehouse == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Destination warehouse is not found");

        var entity = new TransferredStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            DistinationWarehouseId = dto.DistinationWarehouseId,
            Comment = dto.Comment,
            TransferredRequestId = null
        };

        var res = await AddAsync(entity);
        await SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Transferred,
                referenceId: res.TransferredStockId,
                warehouseId: res.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(new TransferredStockDTO
        {
            TransferredStockId = res.TransferredStockId,
            DueDate = res.DueDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            DistinationWarehouseId = res.DistinationWarehouseId,
            Comment = res.Comment,
            CreatedAt = res.CreatedAt
        });
    }

    public async Task<GeneralResponse<TransferredStockDTO>> AddTransferredStockByTransferredRequestIdAsync(
         string userId, AddTransferredStockDTO dto)
    {
        var request = await _context.TransferredRequests
          
           .FirstOrDefaultAsync(tr => tr.TransferredRequestId == dto.TransferredRequestId);

        if (request == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Transferred request is not found");

        //if (salesOrder.DeliveryNoteOrder != null)
        //    return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order already has a Delivery Note Order!");

        var entity = new TransferredStock
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            DistinationWarehouseId = dto.DistinationWarehouseId,
            Comment = dto.Comment,
            TransferredRequestId = null
        };

        var res = await AddAsync(entity);
        await SaveChangesAsync();

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Transferred,
                referenceId: res.TransferredStockId,
                warehouseId: res.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(new TransferredStockDTO
        {
            TransferredStockId = res.TransferredStockId,
            DueDate = res.DueDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            DistinationWarehouseId = res.DistinationWarehouseId,
            Comment = res.Comment,
            CreatedAt = res.CreatedAt
        });
    }


    public async Task<GeneralResponse<TransferredStockDTO>> UpdateTransferredStockAsync(
        string userId, int transferredStockId, UpdateTransferredStockDTO dto)
    {
        var entity = await _context.TransferredStocks
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);

        if (entity == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Transferred stock not found");

        if (dto.TransferredStockId > 0 && entity.TransferredStockId != dto.TransferredStockId)
            return GeneralResponse<TransferredStockDTO>.FailResponse("ID mismatch");

        if (entity.TransferredRequestId != null &&
            dto.DistinationWarehouseId.HasValue &&
            dto.DistinationWarehouseId.Value != entity.DistinationWarehouseId)
        {
            return GeneralResponse<TransferredStockDTO>.FailResponse(
                "You cannot edit destination warehouse, because transfer is based on transferred request.");
        }

        var checkApprovalStatus = await approval.GetProcessItem(entity.TransferredStockId, ProcessType.Transferred);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<TransferredStockDTO>.FailResponse(
                "You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        entity.UserId = userId;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.DistinationWarehouseId.HasValue && entity.TransferredRequestId == null)
        {
            var destinationExists = await _context.Warehouses
                .AnyAsync(w => w.WarehouseId == dto.DistinationWarehouseId.Value);

            if (!destinationExists)
                return GeneralResponse<TransferredStockDTO>.FailResponse("Destination warehouse is not found");

            entity.DistinationWarehouseId = dto.DistinationWarehouseId.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Comment))
            entity.Comment = dto.Comment;

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Transferred,
                referenceId: entity.TransferredStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            TransferredRequestId = entity.TransferredRequestId
        });
    }

    public async Task<GeneralResponse<TransferredStockDTO>> DeleteTransferredStockAsync(
        int transferredStockId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TransferredStocks
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(entity.TransferredStockId, ProcessType.Transferred, cancellationToken);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<TransferredStockDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        var result = new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            TransferredRequestId = entity.TransferredRequestId
        };

        _context.TransferredStocks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int transferredStockId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<TransferredStock>(
            transferredStockId,
            ProcessType.Transferred,
            x => x.TransferredStockId == transferredStockId,
            _context.TransferredStocks
        );
    }

    public async Task<GeneralResponse<TransferredStockDTO>> GetByTransferredRequestIdAsync(int transferredRequestId)
    {
        var entity = await _context.TransferredStocks
            .AsNoTracking()
            .Include(ts => ts.TransferredRequest)
            .Include(ts => ts.Warehouse)
            .Include(ts => ts.DistinationWarehouse)
            .Include(ts => ts.TransferredItems)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.ItemUomGroups)
            .Include(ts => ts.TransferredItems)
                .ThenInclude(i => i.TransferredStockBatches)
            .FirstOrDefaultAsync(ts => ts.TransferredRequestId == transferredRequestId);

        if (entity == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Not Found");

        var mapping = new TransferredStockDTO
        {
            TransferredStockId = entity.TransferredStockId,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            DistinationWarehouseId = entity.DistinationWarehouseId,
            DueDate = entity.DueDate,
            Status = entity.Status.ToString(),
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            WarehouseCode = entity.Warehouse?.WarehouseCode,
            DistinationWarehouseName = entity.DistinationWarehouse?.WarehouseName,
            TransferredRequestId = entity.TransferredRequestId,
            IsReceived = entity.ReceivedStock != null,
            ReceivedStockId = entity.ReceivedStock != null ? entity.ReceivedStock.ReceivedStockId : null,
            TransferredItems = entity.TransferredItems.Select(i => new TransferredItemDTO
            {
                TransferredItemId = i.TransferredItemId,
                TransferredStockId = i.TransferredStockId,
                TransferredRequestItemId = i.TransferredRequestItemId,
                ItemId = i.ItemId,
                ItemCode = i.Item.ItemCode,
                ItemName = i.Item.ItemName,
                Quantity = i.Quantity,
                UoMEntry = i.UoMEntry,
                UnitName = i.Item.ItemUomGroups
                    .Where(u => u.UomEntry == i.UoMEntry)
                    .Select(u => u.UomCode)
                    .FirstOrDefault(),
                BarCode = i.BarCode,
                UnitPrice = i.UnitPrice,
                ErrorMessage = i.ErrorMessage,
                Status = i.Status.ToString(),
                Comment = i.Comment,
                Batches = i.TransferredStockBatches.Select(b => new TransferredStockBatchDTO
                {
                    TransferredStockBatchId = b.TransferredStockBatchId,
                    TransferredItemId = b.TransferredItemId,
                    Quantity = b.Quantity,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate
                }).ToList()
            }).ToList()
        };

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(mapping);
    }

    public async Task<GeneralResponse<TransferredStockDTO>> GetWithItemsAndBatchesAsync(int transferredStockId)
    {
        var result = await _context.TransferredStocks
            .AsNoTracking()
            .Where(ts => ts.TransferredStockId == transferredStockId)
            .Select(ts => new TransferredStockDTO
            {
                TransferredStockId = ts.TransferredStockId,
                UserId = ts.UserId,
                WarehouseId = ts.WarehouseId,
                DistinationWarehouseId = ts.DistinationWarehouseId,
                WarehouseCode = ts.Warehouse.WarehouseCode,
                DistinationWarehouseName = ts.DistinationWarehouse.WarehouseName,
                DueDate = ts.DueDate,
                Status = ts.Status.ToString(),
                Comment = ts.Comment,
                CreatedAt = ts.CreatedAt,
                TransferredRequestId = ts.TransferredRequestId,
                IsReceived = ts.ReceivedStock != null,
                ReceivedStockId = ts.ReceivedStock != null ? ts.ReceivedStock.ReceivedStockId : null,
                TransferredItems = ts.TransferredItems.Select(i => new TransferredItemDTO
                {
                    TransferredItemId = i.TransferredItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    ErrorMessage = i.ErrorMessage,
                    Status = i.Status.ToString(),
                    TransferredStockId = i.TransferredStockId,
                    TransferredRequestItemId = i.TransferredRequestItemId,
                    ItemId = i.ItemId,
                    ItemCode = i.Item.ItemCode,
                    ItemName = i.Item.ItemName,
                    UnitName = i.Item.ItemUomGroups
                        .Where(u => u.UomEntry == i.UoMEntry)
                        .Select(u => u.UomCode)
                        .FirstOrDefault(),
                    Batches = i.TransferredStockBatches.Select(b => new TransferredStockBatchDTO
                    {
                        TransferredStockBatchId = b.TransferredStockBatchId,
                        TransferredItemId = b.TransferredItemId,
                        Quantity = b.Quantity,
                        Comment = b.Comment,
                        BatchNumber = b.BatchNumber,
                        ExpiryDate = b.ExpiryDate
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return GeneralResponse<TransferredStockDTO>.FailResponse("Not Found");

        return GeneralResponse<TransferredStockDTO>.SuccessResponse(result);
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

    public async Task<IEnumerable<TransferredStock>> GetByDestinationWarehouseIdAsync(int destinationWarehouseId)
    {
        return await Query()
            .Where(ts => ts.DistinationWarehouseId == destinationWarehouseId)
            .ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<TransferredStockDTO>>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            return GeneralResponse<IEnumerable<TransferredStockDTO>>.FailResponse("Not found any transferred stock to this status now!");

        var query = await Query()
            .Where(ts => ts.Status == statusEnum)
            .Select(ts => new TransferredStockDTO
            {
                TransferredStockId = ts.TransferredStockId,
                DueDate = ts.DueDate,
                Status = ts.Status.ToString(),
                UserId = ts.UserId,
                WarehouseId = ts.WarehouseId,
                DistinationWarehouseId = ts.DistinationWarehouseId,
                Comment = ts.Comment,
                CreatedAt = ts.CreatedAt,
                TransferredRequestId = ts.TransferredRequestId,
                IsReceived = ts.ReceivedStock != null,
                ReceivedStockId = ts.ReceivedStock != null ? ts.ReceivedStock.ReceivedStockId : null
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<TransferredStockDTO>>.SuccessResponse(query);
    }

    public async Task<IEnumerable<TransferredStock>> GetByUserIdAsync(string userId)
    {
        return await Query()
            .Where(ts => ts.UserId == userId)
            .ToListAsync();
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
        return await Query()
            .Where(ts => ts.Status == GeneralStatus.Draft || ts.Status == GeneralStatus.Processing)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransferredStock>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query()
            .Where(ts => ts.CreatedAt >= startDate && ts.CreatedAt <= endDate)
            .ToListAsync();
    }
}
