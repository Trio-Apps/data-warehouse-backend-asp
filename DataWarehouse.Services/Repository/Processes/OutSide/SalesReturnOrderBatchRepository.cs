using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesReturnOrderBatchRepository : BaseRepository<SalesReturnOrderBatch>, ISalesReturnOrderBatchRepository
{
    private readonly IBaseProcessesRepository<SalesReturnOrderBatch> baseProcesses;

    public SalesReturnOrderBatchRepository(IBaseProcessesRepository<SalesReturnOrderBatch> baseProcesses, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId)
    {
        var res = await Query().Where(b => b.SalesReturnOrderItemId == salesReturnOrderItemId).ToListAsync();

        return GeneralResponse<IEnumerable<SalesReturnOrderBatchDTO>>.SuccessResponse(
            res.Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }));
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderBatchDTO>>> GetBySalesReturnOrderItemIdWithPaginationAsync(int salesReturnOrderItemId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrderBatches
            .AsNoTracking()
            .Where(b => b.SalesReturnOrderItemId == salesReturnOrderItemId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesReturnOrderBatchDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderBatchDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<SalesReturnOrderBatchDTO>> AddBySalesReturnOrderItemIdAsync(int salesReturnOrderItemId,
        GeneralBatchDto dto)
    {
        var res = await baseProcesses.AddOrderBatchAsync<SalesReturnOrder, SalesReturnOrderItem, SalesReturnOrderBatch>(
            orderItemId: salesReturnOrderItemId,
            processType: ProcessType.SalesReturn,
            dto: dto,
            orderSet: _context.SalesReturnOrders,

            // ✅ لازم predicate على العمود الحقيقي
            orderItemSelector: i => i.SalesReturnOrderItemId == salesReturnOrderItemId,
            orderItemSet: _context.SalesReturnOrderItems,

            // ✅ لازم predicate على العمود الحقيقي
            batchItemSelector: b => b.SalesReturnOrderItemId == salesReturnOrderItemId,
            batchSet: _context.SalesReturnOrderBatches
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse(res.Message);

        var saved = res.Data;

        var model = new SalesReturnOrderBatchDTO
        {
            SalesReturnOrderBatchId = saved.SalesReturnOrderBatchId,
            SalesReturnOrderItemId = saved.SalesReturnOrderItemId,
            DeliveryNoteBatchId = saved.DeliveryNoteBatchId,
            Quantity = saved.Quantity,
            Comment = saved.Comment,
            BatchNumber = saved.BatchNumber,
            ExpiryDate = saved.ExpiryDate
        };

        return GeneralResponse<SalesReturnOrderBatchDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderBatchDTO>> UpdateSalesReturnOrderBatchAsync(
        int salesReturnOrderBatchId, UpdateGeneralBatchDto dto)
    {
        var res = await baseProcesses.UpdateOrderBatchAsync<SalesReturnOrder, SalesReturnOrderItem, SalesReturnOrderBatch>(
            batchId: salesReturnOrderBatchId,
            processType: ProcessType.SalesReturn,
            dto: dto,
            orderSet: _context.SalesReturnOrders,

            batchSet: _context.SalesReturnOrderBatches,
            orderItemSet: _context.SalesReturnOrderItems,

            batchIdSelector: x => x.SalesReturnOrderBatchId,
            orderItemIdSelector: x => x.SalesReturnOrderItemId,
            orderItemIdForItemSelector: x => x.SalesReturnOrderItemId
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new SalesReturnOrderBatchDTO
        {
            SalesReturnOrderBatchId = entity.SalesReturnOrderBatchId,
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            DeliveryNoteBatchId = entity.DeliveryNoteBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<SalesReturnOrderBatchDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<SalesReturnOrderBatchDTO>> DeleteSalesReturnOrderBatchAsync(
        int salesReturnOrderBatchId)
    {
        var res = await baseProcesses.DeleteOrderBatchAsync<SalesReturnOrder, SalesReturnOrderItem, SalesReturnOrderBatch>(
            batchIdFromRoute: salesReturnOrderBatchId,
            processType: ProcessType.SalesReturn,
            orderSet: _context.SalesReturnOrders,

            batchSet: _context.SalesReturnOrderBatches,
            orderItemSet: _context.SalesReturnOrderItems,

            batchIdSelector: b => b.SalesReturnOrderBatchId,
            batchOrderItemIdSelector: b => b.SalesReturnOrderItemId,  // FK الحقيقي
            orderItemPkSelector: i => i.SalesReturnOrderItemId,       // PK الحقيقي للـ item
            orderIdSelector: i => i.SalesReturnOrderId                // OrderId الحقيقي
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderBatchDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new SalesReturnOrderBatchDTO
        {
            SalesReturnOrderBatchId = entity.SalesReturnOrderBatchId,
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            DeliveryNoteBatchId = entity.DeliveryNoteBatchId,
            Quantity = entity.Quantity,
            Comment = entity.Comment,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };

        return GeneralResponse<SalesReturnOrderBatchDTO>.SuccessResponse(result);
    }


}

