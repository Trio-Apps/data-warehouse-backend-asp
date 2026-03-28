using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedTransferredRepository : BaseRepository<TransferredStock>, IReceivedTransferredRepository
{
    private readonly IProcessItemIsProgressRepository progressRepository;
    private readonly ITransferredStockRepository transferredStockRepository;
    private readonly DataWarehouseDbContext _context;


    public ReceivedTransferredRepository(IProcessItemIsProgressRepository progressRepository, ITransferredStockRepository transferredStockRepository, DataWarehouseDbContext context) : base(context)
    {
        this.progressRepository = progressRepository;
        _context = context;
        this.transferredStockRepository = transferredStockRepository;
    }

    public async Task<GeneralResponse<PagedResult<TransferredStockDTO>>> GetByWarehouseIdAsDestinationWarehouseAndStatusAndDateWithPaginationForDashboardAsync(
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

        var query = _context.TransferredStocks
            .AsNoTracking()
            .Include(ts => ts.TransferredItems)
            .Include(ts => ts.Warehouse)
            .Include(ts => ts.DistinationWarehouse)
            .Where(ts => ts.DistinationWarehouseId == warehouseId);

        if (sourceWarehouseId.HasValue)
            query = query.Where(ts => ts.WarehouseId == sourceWarehouseId.Value);

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
                ReceivingStatus = x.Order.ReceivingStatus.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                DistinationWarehouseId = x.Order.DistinationWarehouseId,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                WarehouseName = x.Order.Warehouse.WarehouseName,
                DistinationWarehouseName = x.Order.DistinationWarehouse.WarehouseName,
                CreatedAt = x.Order.CreatedAt,
                PostingDate = x.Order.PostingDate,
                Reference = x.Order.Reference,
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

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> UpdateReceivedQuantitiesAsync(string userId, ReceiveTransferredStockDTO dto)
    {
        if (dto == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Request body is required");

        if (dto.TransferredStockId <= 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockId must be greater than 0");

        if (dto.Items == null || dto.Items.Count == 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Items are required");

        var hasDuplicateItems = dto.Items
            .GroupBy(i => i.TransferredItemId)
            .Any(g => g.Count() > 1);

        if (hasDuplicateItems)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Duplicate TransferredItemId found in items");

        var transferredStock = await _context.TransferredStocks
            .Include(ts => ts.TransferredItems)
                .ThenInclude(i => i.TransferredStockBatches)
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == dto.TransferredStockId);

        if (transferredStock == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

        if (transferredStock.ReceivingStatus == ReceivingStatus.Completed)
        {
            if (dto.IsDraft)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
        "This request cannot change it to draft because the receiving process is already completed.");


            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
        "This request cannot be processed because the receiving process is already completed.");



        }


        foreach (var itemDto in dto.Items)
        {
            if (itemDto.TransferredItemId <= 0)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredItemId must be greater than 0");

            if (itemDto.Quantity <= 0)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Quantity must be greater than zero");

            var item = transferredStock.TransferredItems
                .FirstOrDefault(i => i.TransferredItemId == itemDto.TransferredItemId);

            if (item == null)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse($"Transferred item not found: {itemDto.TransferredItemId}");

            if (itemDto.Quantity > item.Quantity)
                return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Received quantity cannot exceed transferred quantity");

            item.ReceivedQuantity = itemDto.Quantity;

            if (itemDto.Batches != null && itemDto.Batches.Count > 0)
            {
                if (item.TransferredStockBatches == null || item.TransferredStockBatches.Count == 0)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse($"No batches found for item: {itemDto.TransferredItemId}");

                var hasDuplicateBatches = itemDto.Batches
                    .GroupBy(b => b.TransferredStockBatchId)
                    .Any(g => g.Count() > 1);

                if (hasDuplicateBatches)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Duplicate TransferredStockBatchId found in batches");

                decimal totalBatchQty = 0;

                foreach (var batchDto in itemDto.Batches)
                {
                    if (batchDto.TransferredStockBatchId <= 0)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockBatchId must be greater than 0");

                    if (batchDto.Quantity <= 0)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Batch quantity must be greater than zero");

                    var batch = item.TransferredStockBatches
                        .FirstOrDefault(b => b.TransferredStockBatchId == batchDto.TransferredStockBatchId);

                    if (batch == null)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
                            $"Transferred stock batch not found: {batchDto.TransferredStockBatchId}");

                    if (batchDto.Quantity > batch.Quantity)
                        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Received batch quantity cannot exceed transferred batch quantity");

                    batch.ReceivedQuantity = batchDto.Quantity;
                    totalBatchQty += batchDto.Quantity;
                }

                if (totalBatchQty > itemDto.Quantity)
                    return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Total batch quantity cannot exceed item received quantity");
            }
        }

        transferredStock.ReceivingStatus = dto.IsDraft ? ReceivingStatus.Draft : ReceivingStatus.Completed;

        await _context.SaveChangesAsync();

        var transferredStockRepo = await transferredStockRepository
      .GetTransferredStockByIdAsync(userId, dto.TransferredStockId);



        if (dto.IsDraft)
        {

            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
           "Draft"
       );
        }

        if (transferredStockRepo.Data?.ApprovalStatus == ApprovalStatus.Approved.ToString() && !dto.IsDraft)
        {
            var result = await GetProcessItemIsProgressForTransferredStock(dto.TransferredStockId);
            return GeneralResponse<ProcessItemIsProgressDto>.SuccessResponse(
                result,
                "Received quantities updated successfully"
            );
        }

        // ❌ لو الشرط لم يتحقق
        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
            "Cannot process request. Approval status must be Approved and not a draft."
        );
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> CompleteReceivingStatusIfDraftAsync(string userId, int transferredStockId)
    {
        if (transferredStockId <= 0)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("TransferredStockId must be greater than 0");

        var transferredStockRepo = await transferredStockRepository.GetTransferredStockByIdAsync(userId, transferredStockId);

        if (transferredStockRepo.Data == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

        if (transferredStockRepo.Data.ReceivingStatus != ReceivingStatus.Draft.ToString())
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Receiving status is not draft");

        var entity = await _context.TransferredStocks
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);

        if (entity == null)
            return GeneralResponse<ProcessItemIsProgressDto>.FailResponse("Transferred stock not found");

       
        entity.ReceivingStatus = ReceivingStatus.Completed;
        await _context.SaveChangesAsync();


        if (transferredStockRepo.Data.ApprovalStatus == ApprovalStatus.Approved.ToString())
        {
            var result = await GetProcessItemIsProgressForTransferredStock(transferredStockId);

            return GeneralResponse<ProcessItemIsProgressDto>.SuccessResponse(
                result,
                "Received quantities updated successfully"
            );

        }


        // ❌ لو الشرط لم يتحقق
        return GeneralResponse<ProcessItemIsProgressDto>.FailResponse(
            "Cannot process request. Approval status must be Approved and not a draft."
        );
    }

    private async Task<ProcessItemIsProgressDto> GetProcessItemIsProgressForTransferredStock(int transferredStockId)
    {
        var res = await progressRepository.GetProcessItemIsProgressAsync(
            ProcessType.Transferred,
            transferredStockId);

        return res;
    }

}
