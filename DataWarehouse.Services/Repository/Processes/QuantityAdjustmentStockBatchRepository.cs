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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class QuantityAdjustmentStockBatchRepository : BaseRepository<QuantityAdjustmentStockBatch>, IQuantityAdjustmentStockBatchRepository
{
    private readonly IBaseProcessesRepository<QuantityAdjustmentStockBatch> baseProcesses;

    public QuantityAdjustmentStockBatchRepository(IBaseProcessesRepository<QuantityAdjustmentStockBatch> baseProcesses, DataWarehouseDbContext context)
        : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemIdAsync(int quantityAdjustmentStockItemId)
    {
        var res = await baseProcesses.GetOrderBatchesAsync<QuantityAdjustmentStockItem, QuantityAdjustmentStockBatch, QuantityAdjustmentStockBatchDTO>(
            orderItemId: quantityAdjustmentStockItemId,
            orderItemSelector: i => i.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            orderItemSet: _context.QuantityAdjustmentStockItems,
            batchItemSelector: b => b.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            batchSet: _context.QuantityAdjustmentStockBatches,
            map: b => new QuantityAdjustmentStockBatchDTO
            {
                QuantityAdjustmentStockBatchId = b.QuantityAdjustmentStockBatchId,
                QuantityAdjustmentStockItemId = b.QuantityAdjustmentStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<QuantityAdjustmentStockBatchDTO>>> GetByQuantityAdjustmentStockItemIdWithPaginationAsync(
        int quantityAdjustmentStockItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.QuantityAdjustmentStockBatches
            .AsNoTracking()
            .Where(b => b.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new QuantityAdjustmentStockBatchDTO
            {
                QuantityAdjustmentStockBatchId = b.QuantityAdjustmentStockBatchId,
                QuantityAdjustmentStockItemId = b.QuantityAdjustmentStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<QuantityAdjustmentStockBatchDTO>>.SuccessResponse(
            new PagedResult<QuantityAdjustmentStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> AddByQuantityAdjustmentStockItemIdAsync(int quantityAdjustmentStockItemId, GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem, QuantityAdjustmentStockBatch>(
            orderItemId: quantityAdjustmentStockItemId,
            processType: ProcessType.QuantityAdjustment,
            dto: dto,
            orderSet: _context.QuantityAdjustmentStocks,
            orderItemSelector: i => i.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            orderItemSet: _context.QuantityAdjustmentStockItems,
            batchItemSelector: b => b.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            batchSet: _context.QuantityAdjustmentStockBatches);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        return GeneralResponse<QuantityAdjustmentStockBatchDTO>.SuccessResponse(new QuantityAdjustmentStockBatchDTO
        {
            QuantityAdjustmentStockBatchId = saved.QuantityAdjustmentStockBatchId,
            QuantityAdjustmentStockItemId = saved.QuantityAdjustmentStockItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> UpdateQuantityAdjustmentStockBatchAsync(
        int quantityAdjustmentStockBatchId, UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem, QuantityAdjustmentStockBatch>(
            batchId: quantityAdjustmentStockBatchId,
            processType: ProcessType.QuantityAdjustment,
            dto: dto,
            orderSet: _context.QuantityAdjustmentStocks,
            batchSet: _context.QuantityAdjustmentStockBatches,
            orderItemSet: _context.QuantityAdjustmentStockItems,
            batchIdSelector: x => x.QuantityAdjustmentStockBatchId,
            orderItemIdSelector: x => x.QuantityAdjustmentStockItemId,
            orderItemIdForItemSelector: x => x.QuantityAdjustmentStockItemId);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<QuantityAdjustmentStockBatchDTO>.SuccessResponse(new QuantityAdjustmentStockBatchDTO
        {
            QuantityAdjustmentStockBatchId = entity.QuantityAdjustmentStockBatchId,
            QuantityAdjustmentStockItemId = entity.QuantityAdjustmentStockItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockBatchDTO>> DeleteQuantityAdjustmentStockBatchAsync(int quantityAdjustmentStockBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem, QuantityAdjustmentStockBatch>(
            batchIdFromRoute: quantityAdjustmentStockBatchId,
            processType: ProcessType.QuantityAdjustment,
            orderSet: _context.QuantityAdjustmentStocks,
            batchSet: _context.QuantityAdjustmentStockBatches,
            orderItemSet: _context.QuantityAdjustmentStockItems,
            batchIdSelector: b => b.QuantityAdjustmentStockBatchId,
            batchOrderItemIdSelector: b => b.QuantityAdjustmentStockItemId,
            orderItemPkSelector: i => i.QuantityAdjustmentStockItemId,
            orderIdSelector: i => i.QuantityAdjustmentStockId);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<QuantityAdjustmentStockBatchDTO>.SuccessResponse(new QuantityAdjustmentStockBatchDTO
        {
            QuantityAdjustmentStockBatchId = entity.QuantityAdjustmentStockBatchId,
            QuantityAdjustmentStockItemId = entity.QuantityAdjustmentStockItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        });
    }
}
