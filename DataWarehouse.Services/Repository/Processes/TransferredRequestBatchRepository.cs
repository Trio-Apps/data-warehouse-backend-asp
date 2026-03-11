using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class TransferredRequestBatchRepository : BaseRepository<TransferredRequestBatch>, ITransferredRequestBatchRepository
{
    private readonly IBaseProcessesRepository<TransferredRequestBatch> baseProcesses;

    public TransferredRequestBatchRepository(
        IBaseProcessesRepository<TransferredRequestBatch> baseProcesses,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<TransferredRequestBatchDTO>>> GetByTransferredRequestItemIdAsync(int transferredRequestItemId)
    {
        var res = await baseProcesses.GetOrderBatchesAsync<TransferredRequestItem, TransferredRequestBatch, TransferredRequestBatchDTO>(
            orderItemId: transferredRequestItemId,
            orderItemSelector: i => i.TransferredRequestItemId == transferredRequestItemId,
            orderItemSet: _context.TransferredRequestItems,
            batchItemSelector: b => b.TransferredRequestItemId == transferredRequestItemId,
            batchSet: _context.TransferredRequestBatches,
            map: b => new TransferredRequestBatchDTO
            {
                TransferredRequestBatchId = b.TransferredRequestBatchId,
                TransferredRequestItemId = b.TransferredRequestItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<TransferredRequestBatchDTO>>> GetByTransferredRequestItemIdWithPaginationAsync(
        int transferredRequestItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredRequestBatches
            .AsNoTracking()
            .Where(b => b.TransferredRequestItemId == transferredRequestItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TransferredRequestBatchDTO
            {
                TransferredRequestBatchId = b.TransferredRequestBatchId,
                TransferredRequestItemId = b.TransferredRequestItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<TransferredRequestBatchDTO>>.SuccessResponse(
            new PagedResult<TransferredRequestBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<TransferredRequestBatchDTO>> AddByTransferredRequestItemIdAsync(
        int transferredRequestItemId,
        GeneralBatchDto dto)
    {
       

        var res = await baseProcesses.AddOrderBatchAsync<TransferredRequest, TransferredRequestItem, TransferredRequestBatch>(
            orderItemId: transferredRequestItemId,
            processType: ProcessType.TransferredRequest,
            dto: dto,
            orderSet: _context.TransferredRequests,
            orderItemSelector: i => i.TransferredRequestItemId == transferredRequestItemId,
            orderItemSet: _context.TransferredRequestItems,
            batchItemSelector: b => b.TransferredRequestItemId == transferredRequestItemId,
            batchSet: _context.TransferredRequestBatches);

        if (!res.Success)
            return GeneralResponse<TransferredRequestBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new TransferredRequestBatchDTO
        {
            TransferredRequestBatchId = saved.TransferredRequestBatchId,
            TransferredRequestItemId = saved.TransferredRequestItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<TransferredRequestBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<TransferredRequestBatchDTO>> UpdateTransferredRequestBatchAsync(
        int transferredRequestBatchId,
        UpdateGeneralBatchDto dto)
    {
      

        var res = await baseProcesses.UpdateOrderBatchAsync<TransferredRequest, TransferredRequestItem, TransferredRequestBatch>(
            batchId: transferredRequestBatchId,
            processType: ProcessType.TransferredRequest,
            dto: dto,
            orderSet: _context.TransferredRequests,
            batchSet: _context.TransferredRequestBatches,
            orderItemSet: _context.TransferredRequestItems,
            batchIdSelector: x => x.TransferredRequestBatchId,
            orderItemIdSelector: x => x.TransferredRequestItemId,
            orderItemIdForItemSelector: x => x.TransferredRequestItemId);

        if (!res.Success)
            return GeneralResponse<TransferredRequestBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<TransferredRequestBatchDTO>.SuccessResponse(new TransferredRequestBatchDTO
        {
            TransferredRequestBatchId = entity.TransferredRequestBatchId,
            TransferredRequestItemId = entity.TransferredRequestItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        });
    }

    public async Task<GeneralResponse<TransferredRequestBatchDTO>> DeleteTransferredRequestBatchAsync
        (int transferredRequestBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<TransferredRequest, TransferredRequestItem, TransferredRequestBatch>(
            batchIdFromRoute: transferredRequestBatchId,
            processType: ProcessType.TransferredRequest,
            orderSet: _context.TransferredRequests,
            batchSet: _context.TransferredRequestBatches,
            orderItemSet: _context.TransferredRequestItems,
            batchIdSelector: b => b.TransferredRequestBatchId,
            batchOrderItemIdSelector: b => b.TransferredRequestItemId,
            orderItemPkSelector: i => i.TransferredRequestItemId,
            orderIdSelector: i => i.TransferredRequestId);

        if (!res.Success)
            return GeneralResponse<TransferredRequestBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var dto = new TransferredRequestBatchDTO
        {
            TransferredRequestBatchId = entity.TransferredRequestBatchId,
            TransferredRequestItemId = entity.TransferredRequestItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<TransferredRequestBatchDTO>.SuccessResponse(dto);
    }
}
