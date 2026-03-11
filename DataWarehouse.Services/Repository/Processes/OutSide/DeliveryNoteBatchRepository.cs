using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide
{
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
    using Polly;

    public class DeliveryNoteBatchRepository : BaseRepository<DeliveryNoteBatch>, IDeliveryNoteBatchRepository
    {
        private readonly IBaseProcessesRepository<DeliveryNoteBatch> baseProcesses;

        public DeliveryNoteBatchRepository(
            IBaseProcessesRepository<DeliveryNoteBatch> baseProcesses,
            DataWarehouseDbContext context) : base(context)
        {
            this.baseProcesses = baseProcesses;
        }

        public async Task<GeneralResponse<IEnumerable<DeliveryNoteBatchDTO>>> GetByDeliveryNoteItemIdAsync(int deliveryNoteItemId)
        {
            var res = await Query()
                .Where(b => b.DeliveryNoteItemId == deliveryNoteItemId)
                .ToListAsync();

            return GeneralResponse<IEnumerable<DeliveryNoteBatchDTO>>.SuccessResponse(
                res.Select(b => new DeliveryNoteBatchDTO
                {
                    DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                    DeliveryNoteItemId = b.DeliveryNoteItemId,
                    SalesOrderBatchId = b.SalesOrderBatchId,
                    Quantity = b.Quantity,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate
                }));
        }

        public async Task<GeneralResponse<PagedResult<DeliveryNoteBatchDTO>>> GetByDeliveryNoteItemIdWithPaginationAsync(
            int deliveryNoteItemId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DeliveryNoteBatches
                .AsNoTracking()
                .Where(b => b.DeliveryNoteItemId == deliveryNoteItemId);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new DeliveryNoteBatchDTO
                {
                    DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                    DeliveryNoteItemId = b.DeliveryNoteItemId,
                    SalesOrderBatchId = b.SalesOrderBatchId,
                    Quantity = b.Quantity,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate
                })
                .ToListAsync();

            return GeneralResponse<PagedResult<DeliveryNoteBatchDTO>>.SuccessResponse(
                new PagedResult<DeliveryNoteBatchDTO>
                {
                    Data = data,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                });
        }

        public async Task<GeneralResponse<DeliveryNoteBatchDTO>> AddByDeliveryNoteItemIdAsync(
            int deliveryNoteItemId,
            GeneralBatchDto dto)
        {
            var res = await baseProcesses.AddOrderBatchAsync<DeliveryNoteOrder, DeliveryNoteItem, DeliveryNoteBatch>(
                orderItemId: deliveryNoteItemId,
                processType: ProcessType.DeliveryNote,
                dto: dto,
                orderSet: _context.DeliveryNoteOrders,

                // ✅ predicate على الـ PK الحقيقي للـ item
                orderItemSelector: i => i.DeliveryNoteItemId == deliveryNoteItemId,
                orderItemSet: _context.DeliveryNoteItems,

                // ✅ predicate على الـ FK الحقيقي داخل batch
                batchItemSelector: b => b.DeliveryNoteItemId == deliveryNoteItemId,
                batchSet: _context.DeliveryNoteBatches
            );

            if (!res.Success)
                return GeneralResponse<DeliveryNoteBatchDTO>.FailResponse(res.Message);

            var saved = res.Data;

            var model = new DeliveryNoteBatchDTO
            {
                DeliveryNoteBatchId = saved.DeliveryNoteBatchId,
                DeliveryNoteItemId = saved.DeliveryNoteItemId,
                SalesOrderBatchId = saved.SalesOrderBatchId,
                Quantity = saved.Quantity,
                Comment = saved.Comment,
                BatchNumber = saved.BatchNumber,
                ExpiryDate = saved.ExpiryDate
            };

            return GeneralResponse<DeliveryNoteBatchDTO>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DeliveryNoteBatchDTO>> UpdateDeliveryNoteBatchAsync(
            int deliveryNoteBatchId,
            UpdateGeneralBatchDto dto)
        {
            var res = await baseProcesses.UpdateOrderBatchAsync<DeliveryNoteOrder, DeliveryNoteItem, DeliveryNoteBatch>(
                batchId: deliveryNoteBatchId,
                processType: ProcessType.DeliveryNote,
                dto: dto,
                orderSet: _context.DeliveryNoteOrders,

                batchSet: _context.DeliveryNoteBatches,
                orderItemSet: _context.DeliveryNoteItems,

                batchIdSelector: x => x.DeliveryNoteBatchId,
                orderItemIdSelector: x => x.DeliveryNoteItemId,
                orderItemIdForItemSelector: x => x.DeliveryNoteItemId
            );

            if (!res.Success)
                return GeneralResponse<DeliveryNoteBatchDTO>.FailResponse(res.Message);

            var entity = res.Data;

            var result = new DeliveryNoteBatchDTO
            {
                DeliveryNoteBatchId = entity.DeliveryNoteBatchId,
                DeliveryNoteItemId = entity.DeliveryNoteItemId,
                SalesOrderBatchId = entity.SalesOrderBatchId,
                Quantity = entity.Quantity,
                Comment = entity.Comment,
                BatchNumber = entity.BatchNumber,
                ExpiryDate = entity.ExpiryDate
            };

            return GeneralResponse<DeliveryNoteBatchDTO>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<DeliveryNoteBatchDTO>> DeleteDeliveryNoteBatchAsync(int deliveryNoteBatchId)
        {
            var res = await baseProcesses.DeleteOrderBatchAsync<DeliveryNoteOrder, DeliveryNoteItem, DeliveryNoteBatch>(
                batchIdFromRoute: deliveryNoteBatchId,
                processType: ProcessType.DeliveryNote,
                orderSet: _context.DeliveryNoteOrders,

                batchSet: _context.DeliveryNoteBatches,
                orderItemSet: _context.DeliveryNoteItems,

                batchIdSelector: b => b.DeliveryNoteBatchId,
                batchOrderItemIdSelector: b => b.DeliveryNoteItemId,  // FK الحقيقي
                orderItemPkSelector: i => i.DeliveryNoteItemId,       // PK الحقيقي للـ item
                orderIdSelector: i => i.DeliveryNoteOrderId           // OrderId الحقيقي
            );

            if (!res.Success)
                return GeneralResponse<DeliveryNoteBatchDTO>.FailResponse(res.Message);

            var entity = res.Data;

            var result = new DeliveryNoteBatchDTO
            {
                DeliveryNoteBatchId = entity.DeliveryNoteBatchId,
                DeliveryNoteItemId = entity.DeliveryNoteItemId,
                SalesOrderBatchId = entity.SalesOrderBatchId,
                Quantity = entity.Quantity,
                Comment = entity.Comment,
                BatchNumber = entity.BatchNumber,
                ExpiryDate = entity.ExpiryDate
            };

            return GeneralResponse<DeliveryNoteBatchDTO>.SuccessResponse(result);
        }
    }
}
