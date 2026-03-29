using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Services.Processes;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedStockRepository : BaseRepository<ReceivedStock>, IReceivedStockRepository
{
    private readonly IBaseProcessesRepository<ReceivedStock> baseProcesses;
    private readonly IApprovalRepository approval;
    private readonly IAppNotificationTrigger notificationTrigger;
    private readonly ReasonValidationService reasonValidationService;

    public ReceivedStockRepository(
        IBaseProcessesRepository<ReceivedStock> baseProcesses,
        IApprovalRepository approval,
        IAppNotificationTrigger notificationTrigger,
        ReasonValidationService reasonValidationService,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
        this.notificationTrigger = notificationTrigger;
        this.reasonValidationService = reasonValidationService;
    }




    #region Received
    public async Task<IEnumerable<ReceivedStock>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query()
            .Where(rs => rs.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdWithPaginationAsync(
        int warehouseId,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedStocks
            .AsNoTracking()
            .Where(rs => rs.WarehouseId == warehouseId)
            .OrderByDescending(rs => rs.CreatedAt);

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
                WarehouseId = rs.WarehouseId,
                SourceWarehouseId = rs.SourceWarehouseId,
                TransferredStockId = rs.TransferredStockId,
                Comment = rs.Comment,
                WarehouseCode = rs.Warehouse.WarehouseCode,
                SourceWarehouseName = rs.SourceWarehouse.WarehouseName,
                CreatedAt = rs.CreatedAt,
                ItemCount = rs.ReceivedItems.Count(),
                ReasonId = rs.ReasonId,
                ReasonName = rs.Reason != null ? rs.Reason.Name : null
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

    public async Task<GeneralResponse<PagedResult<ReceivedStockDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
        int warehouseId,
        string userId,
        int? sourceWarehouseId,
        DateTime? postingDate,
        DateTime? dueDate,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedStocks
            .AsNoTracking()
            .Include(rs => rs.ReceivedItems)
            .Include(rs => rs.Warehouse)
            .Include(rs => rs.SourceWarehouse)
            .Where(rs => rs.WarehouseId == warehouseId);

        if (sourceWarehouseId.HasValue)
            query = query.Where(rs => rs.SourceWarehouseId == sourceWarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            query = query.Where(rs => rs.Status == statusEnum);

        if (postingDate.HasValue)
        {
            var post = postingDate.Value.Date;
            query = query.Where(rs => rs.CreatedAt.Date == post);
        }

        if (dueDate.HasValue)
        {
            var due = dueDate.Value.Date;
            query = query.Where(rs => rs.DueDate.Date == due);
        }

        query = query.OrderByDescending(rs => rs.CreatedAt);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Received);

        var totalRecords = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(rs => new
            {
                Order = rs,
                HasProgress = processQuery.Any(p => p.ReferenceId == rs.ReceivedStockId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == rs.ReceivedStockId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new ReceivedStockDTO
            {
                ReceivedStockId = x.Order.ReceivedStockId,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                SourceWarehouseId = x.Order.SourceWarehouseId,
                TransferredStockId = x.Order.TransferredStockId,
                Comment = x.Order.Comment,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                SourceWarehouseName = x.Order.SourceWarehouse.WarehouseName,
                CreatedAt = x.Order.CreatedAt,
                ItemCount = x.Order.ReceivedItems.Count(),
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,
                ReasonId = x.Order.ReasonId,
                ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<ReceivedStockDTO>>.SuccessResponse(
            new PagedResult<ReceivedStockDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> GetReceivedStockByIdAsync(
        string userId,
        int receivedStockId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.ReceivedStocks
            .Include(rs => rs.Warehouse)
            .Include(rs => rs.SourceWarehouse)
            .Include(rs => rs.TransferredStock)
            .Include(rs => rs.ReceivedItems)
            .FirstOrDefaultAsync(rs => rs.ReceivedStockId == receivedStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Received, entity.ReceivedStockId);

        var model = MapStockToDto(entity);
        model.CanApprove = approvalModel.CanApprove;
        model.ProcessApprovalId = approvalModel.ProcessApprovalId;
        model.ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId;
        model.Approval = approvalModel.hasProgress;
        model.ApprovalStatus = approvalModel.ApprovalStatus;
        model.Reason = approvalModel.Reason;

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(model);
    }



    public async Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockByTransferredStockIdAsync(
        string userId,
        AddReceivedStockDTO dto)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Received);
        var transferredStock = await _context.TransferredStocks
            .Include(ts => ts.ReceivedStock)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == dto.TransferredStockId);

        if (transferredStock == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred stock not found");

        if (transferredStock.ReceivedStock != null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred stock already has a received stock");

        var entity = new ReceivedStock
        {
            UserId = userId,
            WarehouseId = transferredStock.DistinationWarehouseId,
            SourceWarehouseId = transferredStock.WarehouseId,
            TransferredStockId = dto.TransferredStockId,
            DueDate = dto.DueDate,
            Comment = dto.Comment,
            ReasonId = dto.ReasonId,
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ReceivedStocks.AddAsync(entity);
        await _context.SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.Received, entity.ReceivedStockId, userId, dto.IsDraft);

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Received,
                referenceId: entity.ReceivedStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(MapStockToDto(entity));
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockWitoutRefAsync(
      string userId,
      AddReceivedStockWithoutRefDTO dto)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Received);
        var entity = new ReceivedStock
        {
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            SourceWarehouseId = dto.SourceWarehouseId,
            DueDate = dto.DueDate,
            Comment = dto.Comment,
            ReasonId = dto.ReasonId,
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ReceivedStocks.AddAsync(entity);
        await _context.SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.Received, entity.ReceivedStockId, userId, dto.IsDraft);

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Received,
                referenceId: entity.ReceivedStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(MapStockToDto(entity));
    }



    public async Task<GeneralResponse<ReceivedStockDTO>> AddReceivedStockAndItemsByTransferredStockIdAsync(
        string userId,
        AddReceivedStockDTO dto)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Received);
        var transferredStock = await _context.TransferredStocks
            .Include(ts => ts.ReceivedStock)
            .Include(ts => ts.TransferredItems)
                .ThenInclude(ti => ti.TransferredStockBatches)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == dto.TransferredStockId);

        if (transferredStock == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred stock not found");

        if (transferredStock.ReceivedStock != null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred stock already has a received stock");

        if (transferredStock.TransferredItems == null || !transferredStock.TransferredItems.Any())
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Transferred stock has no items");




        var entity = new ReceivedStock
        {
            UserId = userId,
            WarehouseId = transferredStock.DistinationWarehouseId,
            SourceWarehouseId = transferredStock.WarehouseId,
            TransferredStockId = dto.TransferredStockId,
            DueDate = dto.DueDate,
            Comment = dto.Comment,
            ReasonId = dto.ReasonId,
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            ReceivedItems = BuildReceivedItemsFromTransferredItems(transferredStock.TransferredItems)
        };

        await _context.ReceivedStocks.AddAsync(entity);
        await _context.SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.Received, entity.ReceivedStockId, userId, dto.IsDraft);

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Received,
                referenceId: entity.ReceivedStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(MapStockToDto(entity));
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> UpdateReceivedStockAsync(
        string userId,
        int receivedStockId,
        UpdateReceivedStockDTO dto)
    {
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Received);
        var entity = await _context.ReceivedStocks
            .FirstOrDefaultAsync(e => e.ReceivedStockId == receivedStockId);

        if (entity == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Received stock not found");

        if (dto.ReceivedStockId != receivedStockId)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("ID mismatch");

        var checkApprovalStatus = await approval.GetProcessItem(entity.ReceivedStockId, ProcessType.Received);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<ReceivedStockDTO>.FailResponse(
                "You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        entity.UserId = userId;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Comment))
            entity.Comment = dto.Comment;

        entity.ReasonId = dto.ReasonId;

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Received,
                referenceId: entity.ReceivedStockId,
                warehouseId: entity.WarehouseId,
                userId: userId);

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(MapStockToDto(entity));
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> DuplicateReceivedStockAsync(
        string userId,
        int receivedStockId,
        CancellationToken cancellationToken = default)
    {
        var source = await _context.ReceivedStocks
            .AsNoTracking()
            .Include(x => x.ReceivedItems)
                .ThenInclude(x => x.ReceivedStockBatches)
            .FirstOrDefaultAsync(x => x.ReceivedStockId == receivedStockId, cancellationToken);
        if (source == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Not Found");
        var clone = OrderDuplicationHelper.Clone(source, userId);
        await _context.ReceivedStocks.AddAsync(clone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.Received, clone.ReceivedStockId, userId, true);
        return await GetReceivedStockByIdAsync(userId, clone.ReceivedStockId, cancellationToken);
    }    public async Task<GeneralResponse<ReceivedStockDTO>> DeleteReceivedStockAsync(
        int receivedStockId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.ReceivedStocks
            .FirstOrDefaultAsync(e => e.ReceivedStockId == receivedStockId, cancellationToken);

        if (entity == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(entity.ReceivedStockId, ProcessType.Received, cancellationToken);
        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
        {
            return GeneralResponse<ReceivedStockDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");
        }

        var result = MapStockToDto(entity);

        _context.ReceivedStocks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int receivedStockId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<ReceivedStock>(
            receivedStockId,
            ProcessType.Received,
            x => x.ReceivedStockId == receivedStockId,
            _context.ReceivedStocks
        );
    }

    public async Task<GeneralResponse<ReceivedStockDTO>> GetByTransferredStockIdAsync(int transferredStockId)
    {
        var res = await _context.ReceivedStocks
            .AsNoTracking()
            .Include(rs => rs.Warehouse)
            .Include(rs => rs.SourceWarehouse)
            .Include(rs => rs.ReceivedItems)
            .FirstOrDefaultAsync(rs => rs.TransferredStockId == transferredStockId);

        if (res == null)
            return GeneralResponse<ReceivedStockDTO>.FailResponse("Not Found");

        return GeneralResponse<ReceivedStockDTO>.SuccessResponse(MapStockToDto(res));
    }

    public async Task<IEnumerable<ReceivedStock>> GetByUserIdAsync(string userId)
    {
        return await Query()
            .Where(rs => rs.UserId == userId)
            .ToListAsync();
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
                WarehouseCode = s.Warehouse.WarehouseCode,
                SourceWarehouseName = s.SourceWarehouse.WarehouseName,
                CreatedAt = s.CreatedAt,
                ItemCount = s.ReceivedItems.Count(),
                ReasonId = s.ReasonId,
                ReasonName = s.Reason != null ? s.Reason.Name : null,
                Items = s.ReceivedItems.Select(i => new ReceivedItemDTO
                {
                    ReceivedItemId = i.ReceivedItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    VatPercent = i.VatPercent,
                    VatAmount = i.VatAmount,
                    LineTotalBeforeVat = i.LineTotalBeforeVat,
                    LineTotalAfterVat = i.LineTotalAfterVat,
                    ErrorMessage = i.ErrorMessage,
                    Status = i.Status.ToString(),
                    Comment = i.Comment,
                    ReceivedStockId = i.ReceivedStockId,
                    TransferredItemId = i.TransferredItemId,
                    ItemId = i.ItemId,
                    ItemCode = i.Item.ItemCode,
                    ItemName = i.Item.ItemName,
                    UnitName = i.Item.ItemUomGroups
                        .Where(u => u.UomEntry == i.UoMEntry)
                        .Select(u => u.UomCode)
                        .FirstOrDefault(),
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

    private static List<ReceivedItem> BuildReceivedItemsFromTransferredItems(IEnumerable<TransferredItem> transferredItems)
    {
        return transferredItems.Select(transferredItem =>
        {
            var batches = new List<ReceivedStockBatch>();
            decimal remainingQuantity = transferredItem.Quantity;

            foreach (var transferredBatch in transferredItem.TransferredStockBatches.OrderBy(b => b.CreatedAt))
            {
                if (remainingQuantity <= 0)
                    break;

                var batchQuantity = Math.Min(transferredBatch.Quantity, remainingQuantity);

                batches.Add(new ReceivedStockBatch
                {
                    TransferredStockBatchId = transferredBatch.TransferredStockBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = transferredBatch.BatchNumber,
                    ExpiryDate = transferredBatch.ExpiryDate,
                    Comment = transferredBatch.Comment,
                    CreatedAt = DateTime.UtcNow
                });

                remainingQuantity -= batchQuantity;
            }

            return new ReceivedItem
            {
                TransferredItemId = transferredItem.TransferredItemId,
                ItemId = transferredItem.ItemId,
                Quantity = transferredItem.Quantity,
                UoMEntry = transferredItem.UoMEntry,
                BarCode = transferredItem.BarCode,
                UnitPrice = transferredItem.UnitPrice,
                VatPercent = transferredItem.VatPercent,
                VatAmount = transferredItem.VatAmount,
                LineTotalBeforeVat = transferredItem.LineTotalBeforeVat,
                LineTotalAfterVat = transferredItem.LineTotalAfterVat,
                Status = GeneralItemStatus.Planned,
                ErrorMessage = null,
                Comment = transferredItem.Comment,
                LineNum = transferredItem.LineNum,
                ReceivedStockBatches = batches
            };
        }).ToList();
    }

    private static ReceivedStockDTO MapStockToDto(ReceivedStock stock)
    {
        return new ReceivedStockDTO
        {
            ReceivedStockId = stock.ReceivedStockId,
            DueDate = stock.DueDate,
            Status = stock.Status.ToString(),
            UserId = stock.UserId,
            WarehouseId = stock.WarehouseId,
            SourceWarehouseId = stock.SourceWarehouseId,
            TransferredStockId = stock.TransferredStockId,
            Comment = stock.Comment,
            WarehouseCode = stock.Warehouse?.WarehouseCode,
            SourceWarehouseName = stock.SourceWarehouse?.WarehouseName,
            CreatedAt = stock.CreatedAt,
            ItemCount = stock.ReceivedItems?.Count,
            ReasonId = stock.ReasonId,
            ReasonName = stock.Reason != null ? stock.Reason.Name : null
        };
    }
    #endregion
}

