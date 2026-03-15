using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedStockBatchRepository : BaseRepository<ReceivedStockBatch>, IReceivedStockBatchRepository
{
    private readonly IBaseProcessesRepository<ReceivedStockBatch> baseProcesses;
    private readonly IApprovalRepository approval;

    public ReceivedStockBatchRepository(
        IBaseProcessesRepository<ReceivedStockBatch> baseProcesses,
        IApprovalRepository approval,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
        
    }

    public async Task<GeneralResponse<IEnumerable<ReceivedStockBatchDTO>>> GetByReceivedItemIdAsync(int receivedItemId)
    {
        var res = await Query()
            .Where(b => b.ReceivedItemId == receivedItemId)
            .ToListAsync();


        return GeneralResponse<IEnumerable<ReceivedStockBatchDTO>>.SuccessResponse(
            res.Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<ReceivedStockBatchDTO>>> GetByReceivedItemIdWithPaginationAsync(
        int receivedItemId,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedStockBatches
            .AsNoTracking()
            .Where(b => b.ReceivedItemId == receivedItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.ReceivedStockBatchId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceivedStockBatchDTO>>.SuccessResponse(
            new PagedResult<ReceivedStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }
  
    public async Task<GeneralResponse<ReceivedStockBatchDTO>> AddByReceivedItemIdAsync(
        int receivedItemId,
        GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<ReceivedStock, ReceivedItem, ReceivedStockBatch>(
            orderItemId: receivedItemId,
            processType: ProcessType.Received,
            dto: dto,
            orderSet: _context.ReceivedStocks,

            orderItemSelector: i => i.ReceivedItemId == receivedItemId,
            orderItemSet: _context.ReceivedItems,

            batchItemSelector: b => b.ReceivedItemId == receivedItemId,
            batchSet: _context.ReceivedStockBatches
        );

        if (!res.Success)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new ReceivedStockBatchDTO
        {
            ReceivedStockBatchId = saved.ReceivedStockBatchId,
            ReceivedItemId = saved.ReceivedItemId,
            TransferredStockBatchId = saved.TransferredStockBatchId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<ReceivedStockBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceivedStockBatchDTO>> UpdateReceivedStockBatchAsync(
        int receivedStockBatchId,
        UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<ReceivedStock, ReceivedItem, ReceivedStockBatch>(
            batchId: receivedStockBatchId,
            processType: ProcessType.Received,
            dto: dto,
            orderSet: _context.ReceivedStocks,

            batchSet: _context.ReceivedStockBatches,
            orderItemSet: _context.ReceivedItems,

            batchIdSelector: x => x.ReceivedStockBatchId,
            orderItemIdSelector: x => x.ReceivedItemId,
            orderItemIdForItemSelector: x => x.ReceivedItemId
        );

        if (!res.Success)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new ReceivedStockBatchDTO
        {
            ReceivedStockBatchId = entity.ReceivedStockBatchId,
            ReceivedItemId = entity.ReceivedItemId,
            TransferredStockBatchId = entity.TransferredStockBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<ReceivedStockBatchDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ReceivedStockBatchDTO>> DeleteReceivedStockBatchAsync(int receivedStockBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<ReceivedStock, ReceivedItem, ReceivedStockBatch>(
            batchIdFromRoute: receivedStockBatchId,
            processType: ProcessType.Received,
            orderSet: _context.ReceivedStocks,

            batchSet: _context.ReceivedStockBatches,
            orderItemSet: _context.ReceivedItems,

            batchIdSelector: b => b.ReceivedStockBatchId,
            batchOrderItemIdSelector: b => b.ReceivedItemId,
            orderItemPkSelector: i => i.ReceivedItemId,
            orderIdSelector: i => i.ReceivedStockId
        );

        if (!res.Success)
            return GeneralResponse<ReceivedStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new ReceivedStockBatchDTO
        {
            ReceivedStockBatchId = entity.ReceivedStockBatchId,
            ReceivedItemId = entity.ReceivedItemId,
            TransferredStockBatchId = entity.TransferredStockBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<ReceivedStockBatchDTO>.SuccessResponse(result);
    }

}
