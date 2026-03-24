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

public class TransferredStockBatchRepository : BaseRepository<TransferredStockBatch>, ITransferredStockBatchRepository
{
    private readonly IBaseProcessesRepository<TransferredStockBatch> baseProcesses;

    public TransferredStockBatchRepository(
        IBaseProcessesRepository<TransferredStockBatch> baseProcesses,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<TransferredStockBatchDTO>>> GetByTransferredItemIdAsync(int transferredItemId)
    {
        var res = await Query()
            .Where(b => b.TransferredItemId == transferredItemId)
            .ToListAsync();

        return GeneralResponse<IEnumerable<TransferredStockBatchDTO>>.SuccessResponse(
            res.Select(b => new TransferredStockBatchDTO
            {
                TransferredStockBatchId = b.TransferredStockBatchId,
                TransferredItemId = b.TransferredItemId,
                Quantity = b.Quantity,
                ReceivedQuantity = b.ReceivedQuantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<TransferredStockBatchDTO>>> GetByTransferredItemIdWithPaginationAsync(
        int transferredItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredStockBatches
            .AsNoTracking()
            .Where(b => b.TransferredItemId == transferredItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TransferredStockBatchDTO
            {
                TransferredStockBatchId = b.TransferredStockBatchId,
                TransferredItemId = b.TransferredItemId,
                Quantity = b.Quantity,
                ReceivedQuantity = b.ReceivedQuantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<TransferredStockBatchDTO>>.SuccessResponse(
            new PagedResult<TransferredStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<TransferredStockBatchDTO>> AddByTransferredItemIdAsync(
        int transferredItemId, GeneralBatchDto dto)
    {
      

        var res = await baseProcesses.AddOrderBatchAsync<TransferredStock, TransferredItem, TransferredStockBatch>(
            orderItemId: transferredItemId,
            processType: ProcessType.Transferred,
            dto: dto,
            orderSet: _context.TransferredStocks,
            orderItemSelector: i => i.TransferredItemId == transferredItemId,
            orderItemSet: _context.TransferredItems,
            batchItemSelector: b => b.TransferredItemId == transferredItemId,
            batchSet: _context.TransferredStockBatches
        );

        if (!res.Success)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new TransferredStockBatchDTO
        {
            TransferredStockBatchId = saved.TransferredStockBatchId,
            TransferredItemId = saved.TransferredItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<TransferredStockBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<TransferredStockBatchDTO>> UpdateTransferredStockBatchAsync(
        int transferredStockBatchId, UpdateGeneralBatchDto dto)
    {
      

        var res = await baseProcesses.UpdateOrderBatchAsync<TransferredStock, TransferredItem, TransferredStockBatch>(
            batchId: transferredStockBatchId,
            processType: ProcessType.Transferred,
            dto: dto,
            orderSet: _context.TransferredStocks,
            batchSet: _context.TransferredStockBatches,
            orderItemSet: _context.TransferredItems,
            batchIdSelector: x => x.TransferredStockBatchId,
            orderItemIdSelector: x => x.TransferredItemId,
            orderItemIdForItemSelector: x => x.TransferredItemId
        );

        if (!res.Success)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<TransferredStockBatchDTO>.SuccessResponse(new TransferredStockBatchDTO
        {
            TransferredStockBatchId = entity.TransferredStockBatchId,
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        });
    }

    public async Task<GeneralResponse<TransferredStockBatchDTO>> DeleteTransferredStockBatchAsync(int transferredStockBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<TransferredStock, TransferredItem, TransferredStockBatch>(
            batchIdFromRoute: transferredStockBatchId,
            processType: ProcessType.Transferred,
            orderSet: _context.TransferredStocks,
            batchSet: _context.TransferredStockBatches,
            orderItemSet: _context.TransferredItems,
            batchIdSelector: b => b.TransferredStockBatchId,
            batchOrderItemIdSelector: b => b.TransferredItemId,
            orderItemPkSelector: i => i.TransferredItemId,
            orderIdSelector: i => i.TransferredStockId
        );

        if (!res.Success)
            return GeneralResponse<TransferredStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new TransferredStockBatchDTO
        {
            TransferredStockBatchId = entity.TransferredStockBatchId,
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<TransferredStockBatchDTO>.SuccessResponse(result);
    }
}
