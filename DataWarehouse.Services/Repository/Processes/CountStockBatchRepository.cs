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

public class CountStockBatchRepository : BaseRepository<CountStockBatch>, ICountStockBatchRepository
{
    private readonly IBaseProcessesRepository<CountStockBatch> baseProcesses;

    public CountStockBatchRepository(
        IBaseProcessesRepository<CountStockBatch> baseProcesses,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<CountStockBatchDTO>>> GetByCountStockItemIdAsync(int countStockItemId)
    {
        var res = await baseProcesses.GetOrderBatchesAsync<CountStockItem, CountStockBatch, CountStockBatchDTO>(
            orderItemId: countStockItemId,
            orderItemSelector: i => i.CountStockItemId == countStockItemId,
            orderItemSet: _context.CountStockItems,
            batchItemSelector: b => b.CountStockItemId == countStockItemId,
            batchSet: _context.CountStockBatches,
            map: b => new CountStockBatchDTO
            {
                CountStockBatchId = b.CountStockBatchId,
                CountStockItemId = b.CountStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<CountStockBatchDTO>>> GetByCountStockItemIdWithPaginationAsync(
        int countStockItemId,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.CountStockBatches
            .AsNoTracking()
            .Where(b => b.CountStockItemId == countStockItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new CountStockBatchDTO
            {
                CountStockBatchId = b.CountStockBatchId,
                CountStockItemId = b.CountStockItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<CountStockBatchDTO>>.SuccessResponse(
            new PagedResult<CountStockBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<CountStockBatchDTO>> AddByCountStockItemIdAsync(int countStockItemId, AddCountStockBatchDTO dto)
    {
        var mappedDto = new GeneralBatchDto
        {
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = dto.BatchNumber,
            ExpiryDate = dto.ExpiryDate
        };

        var res = await baseProcesses.AddOrderBatchAsync<CountStock, CountStockItem, CountStockBatch>(
            orderItemId: countStockItemId,
            processType: ProcessType.Counting,
            dto: mappedDto,
            orderSet: _context.CountStocks,
            orderItemSelector: i => i.CountStockItemId == countStockItemId,
            orderItemSet: _context.CountStockItems,
            batchItemSelector: b => b.CountStockItemId == countStockItemId,
            batchSet: _context.CountStockBatches);

        if (!res.Success)
            return GeneralResponse<CountStockBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        return GeneralResponse<CountStockBatchDTO>.SuccessResponse(new CountStockBatchDTO
        {
            CountStockBatchId = saved.CountStockBatchId,
            CountStockItemId = saved.CountStockItemId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        });
    }

    public async Task<GeneralResponse<CountStockBatchDTO>> UpdateCountStockBatchAsync(int countStockBatchId, UpdateCountStockBatchDTO dto)
    {
        var mappedDto = new UpdateGeneralBatchDto
        {
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = dto.BatchNumber,
            ExpiryDate = dto.ExpiryDate
        };

        var res = await baseProcesses.UpdateOrderBatchAsync<CountStock, CountStockItem, CountStockBatch>(
            batchId: countStockBatchId,
            processType: ProcessType.Counting,
            dto: mappedDto,
            orderSet: _context.CountStocks,
            batchSet: _context.CountStockBatches,
            orderItemSet: _context.CountStockItems,
            batchIdSelector: x => x.CountStockBatchId,
            orderItemIdSelector: x => x.CountStockItemId,
            orderItemIdForItemSelector: x => x.CountStockItemId);

        if (!res.Success)
            return GeneralResponse<CountStockBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<CountStockBatchDTO>.SuccessResponse(new CountStockBatchDTO
        {
            CountStockBatchId = entity.CountStockBatchId,
            CountStockItemId = entity.CountStockItemId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        });
    }
}
