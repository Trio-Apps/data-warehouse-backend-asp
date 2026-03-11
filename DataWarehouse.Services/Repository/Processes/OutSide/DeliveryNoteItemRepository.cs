using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace DataWarehouse.Services.Repository.Processes.OutSide
{



    public class DeliveryNoteItemRepository : BaseRepository<DeliveryNoteItem>, IDeliveryNoteItemRepository
    {
        private readonly IBaseProcessesRepository<DeliveryNoteItem> baseProcesses;
        private readonly IDeliveryNoteOrderRepository deliveryNote;

        public DeliveryNoteItemRepository(
            IBaseProcessesRepository<DeliveryNoteItem> baseProcesses,
            IDeliveryNoteOrderRepository deliveryNote,
            DataWarehouseDbContext context) : base(context)
        {
            this.baseProcesses = baseProcesses;
            this.deliveryNote = deliveryNote;
        }

        public async Task<GeneralResponse<IEnumerable<DeliveryNoteItemDTO>>> GetByDeliveryNoteOrderIdAsync(int deliveryNoteOrderId)
        {
            var res = await Query()
                .Where(dni => dni.DeliveryNoteOrderId == deliveryNoteOrderId)
                .Select(e => new DeliveryNoteItemDTO
                {
                    DeliveryNoteItemId = e.DeliveryNoteItemId,
                    Quantity = e.Quantity,
                    UoMEntry = e.UoMEntry,
                    BarCode = e.BarCode,
                    UnitPrice = e.UnitPrice,
                    ErrorMessage = e.ErrorMessage,
                    Status = e.Status.ToString(),
                    DeliveryNoteOrderId = e.DeliveryNoteOrderId,
                    SalesOrderItemId = e.SalesOrderItemId,
                    ItemId = e.ItemId,
                    ItemCode = e.Item.ItemCode,
                    ItemName = e.Item.ItemName,
                    UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry)!.UomCode,
                })
                .ToListAsync();

            return GeneralResponse<IEnumerable<DeliveryNoteItemDTO>>.SuccessResponse(res);
        }

        public async Task<GeneralResponse<PagedResult<DeliveryNoteItemDTO>>> GetByDeliveryNoteOrderIdWithPaginationAsync(
            int deliveryNoteOrderId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DeliveryNoteItems
                .AsNoTracking()
                .Where(dni => dni.DeliveryNoteOrderId == deliveryNoteOrderId);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.DeliveryNoteItemId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new DeliveryNoteItemDTO
                {
                    DeliveryNoteItemId = e.DeliveryNoteItemId,
                    Quantity = e.Quantity,
                    UoMEntry = e.UoMEntry,
                    BarCode = e.BarCode,
                    UnitPrice = e.UnitPrice,
                    ErrorMessage = e.ErrorMessage,
                    Status = e.Status.ToString(),
                    DeliveryNoteOrderId = e.DeliveryNoteOrderId,
                    SalesOrderItemId = e.SalesOrderItemId,
                    ItemId = e.ItemId
                })
                .ToListAsync();

            return GeneralResponse<PagedResult<DeliveryNoteItemDTO>>.SuccessResponse(
                new PagedResult<DeliveryNoteItemDTO>
                {
                    Data = data,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                });
        }

        public async Task<GeneralResponse<DeliveryNoteItemDTO>> AddDeliveryNoteItemByDeliveryNoteOrderIdWithoutRefAsync(
            int deliveryNoteOrderId,
            bool isBarcode,
            DynamicBarcodesDto? barcodeDto,
            AddGeneralItemDto? dto)
        {
            var res = await baseProcesses.AddOrderItemAsync<DeliveryNoteOrder, DeliveryNoteItem>(
                orderId: deliveryNoteOrderId,
                processType: ProcessType.DeliveryNote,
                isBarcode: isBarcode,
                barcodeDto: barcodeDto,
                dto: dto,
              
                orderIdSelector: x => x.DeliveryNoteOrderId == deliveryNoteOrderId,
                orderSet: _context.DeliveryNoteOrders,
                itemSet: _context.DeliveryNoteItems
            );

            if (!res.Success)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse(res.Message);

            var model = new DeliveryNoteItemDTO
            {
                DeliveryNoteOrderId = res.Data.DeliveryNoteOrderId,
                Quantity = res.Data.Quantity,
                ItemId = res.Data.ItemId,
                DeliveryNoteItemId = res.Data.DeliveryNoteItemId,
                UoMEntry = res.Data.UoMEntry,
                BarCode = res.Data.BarCode,
                UnitPrice = res.Data.UnitPrice,
                ErrorMessage = res.Data.ErrorMessage,
                SalesOrderItemId = res.Data.SalesOrderItemId
            };

            return GeneralResponse<DeliveryNoteItemDTO>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DeliveryNoteItemDTO>> UpdateDeliveryNoteItemWithoutRefAsync(
            int deliveryNoteItemId,
            UpdateGeneralItemDto dto)
        {
            var res = await baseProcesses.UpdateOrderItemAsync<DeliveryNoteOrder, DeliveryNoteItem>(
                itemIdFromRoute: deliveryNoteItemId,
                processType: ProcessType.DeliveryNote,
                dto: dto,
                orderSet: _context.DeliveryNoteOrders,
                itemSelector: x => x.DeliveryNoteItemId == deliveryNoteItemId,
                itemSet: _context.DeliveryNoteItems
            );

            if (!res.Success)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse(res.Message);

            var entity = res.Data;

            var result = new DeliveryNoteItemDTO
            {
                DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
                DeliveryNoteItemId = entity.DeliveryNoteItemId,
                Quantity = entity.Quantity,
                ItemId = entity.ItemId,
                SalesOrderItemId = entity.SalesOrderItemId,
                BarCode = entity.BarCode,
                UoMEntry = entity.UoMEntry,
                ErrorMessage = entity.ErrorMessage,
                UnitPrice = entity.UnitPrice
            };

            return GeneralResponse<DeliveryNoteItemDTO>.SuccessResponse(result);
        }

        /// <summary>
        /// Add DeliveryNoteItem based on SalesOrderItemId (reference)
        /// + auto-copy batches from SalesOrderBatches to DeliveryNoteBatches
        /// </summary>
        public async Task<GeneralResponse<DeliveryNoteItemDTO>> AddDeliveryNoteItemBySalesOrderItemIdAsync(
            string userId,
            int deliveryNoteOrderId,
            AddDeliveryNoteOrderItemDTO dto)
        {
            // Validate DeliveryNoteOrder exists
            var dnOrder = await _context.DeliveryNoteOrders
                .FirstOrDefaultAsync(d => d.DeliveryNoteOrderId == deliveryNoteOrderId);

            if (dnOrder == null)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Delivery Note Order not found");

            // Validate SalesOrderItem exists + load batches + item
            var salesOrderItem = await _context.SalesOrderItems
                .Include(i => i.SalesOrderBatches)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.SalesOrderItemId == dto.SalesOrderItemId);

            if (salesOrderItem == null)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Sales Order Item not found");

            // Prevent duplicates for the same SalesOrderItemId in the same DeliveryNoteOrder
            var exists = await _context.DeliveryNoteItems
                .AnyAsync(x => x.DeliveryNoteOrderId == deliveryNoteOrderId && x.SalesOrderItemId == dto.SalesOrderItemId);

            if (exists)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("This Sales Order Item already exists in this Delivery Note");

            // Validate quantity
            if (dto.Quantity <= 0)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Quantity must be greater than zero");

            if (dto.Quantity > salesOrderItem.Quantity)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Delivery quantity cannot exceed sales quantity");

            var dnItem = new DeliveryNoteItem
            {
                DeliveryNoteOrderId = deliveryNoteOrderId,
                SalesOrderItemId = dto.SalesOrderItemId,
                ItemId = salesOrderItem.ItemId,
                Quantity = dto.Quantity,
                UoMEntry = salesOrderItem.UoMEntry,
                BarCode = salesOrderItem.BarCode,
                UnitPrice = salesOrderItem.UnitPrice,

                // status like your planned logic
                Status = GeneralItemStatus.Planned,
                ErrorMessage = null
            };

            var res = await AddAsync(dnItem);
            await SaveChangesAsync();

            // Auto-copy batches
            if (salesOrderItem.SalesOrderBatches != null && salesOrderItem.SalesOrderBatches.Any())
            {
                var batchesToAdd = new List<DeliveryNoteBatch>();
                decimal remainingQuantity = dto.Quantity;

                foreach (var salesBatch in salesOrderItem.SalesOrderBatches.OrderBy(b => b.CreatedAt))
                {
                    if (remainingQuantity <= 0)
                        break;

                    decimal batchQuantity = remainingQuantity > salesBatch.Quantity ? salesBatch.Quantity : remainingQuantity;

                    batchesToAdd.Add(new DeliveryNoteBatch
                    {
                        DeliveryNoteItemId = res.DeliveryNoteItemId,
                        SalesOrderBatchId = salesBatch.SalesOrderBatchId,
                        Quantity = batchQuantity,
                        BatchNumber = salesBatch.BatchNumber,
                        ExpiryDate = salesBatch.ExpiryDate,
                        Comment = dto.Comment,
                        CreatedAt = DateTime.UtcNow
                    });

                    remainingQuantity -= batchQuantity;
                }

                if (batchesToAdd.Any())
                {
                    await _context.DeliveryNoteBatches.AddRangeAsync(batchesToAdd);
                    await SaveChangesAsync();
                }
            }

            // Reload with batches
            var finalItem = await _context.DeliveryNoteItems
                .Include(i => i.DeliveryNoteBatches)
                .FirstOrDefaultAsync(i => i.DeliveryNoteItemId == res.DeliveryNoteItemId);

            var model = new DeliveryNoteItemDTO
            {
                DeliveryNoteItemId = finalItem!.DeliveryNoteItemId,
                Quantity = finalItem.Quantity,
                UoMEntry = finalItem.UoMEntry,
                BarCode = finalItem.BarCode,
                UnitPrice = finalItem.UnitPrice,
                ErrorMessage = finalItem.ErrorMessage,
                DeliveryNoteOrderId = finalItem.DeliveryNoteOrderId,
                SalesOrderItemId = finalItem.SalesOrderItemId,
                ItemId = finalItem.ItemId,

                Batches = finalItem.DeliveryNoteBatches?.Select(b => new DeliveryNoteBatchDTO
                {
                    DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                    DeliveryNoteItemId = b.DeliveryNoteItemId,
                    SalesOrderBatchId = b.SalesOrderBatchId,
                    Quantity = b.Quantity,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate
                }).ToList()
            };

            return GeneralResponse<DeliveryNoteItemDTO>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DeliveryNoteItemDTO>> UpdateDeliveryNoteItemAsync(
            int deliveryNoteItemId,
            UpdateDeliveryNoteOrderItemDTO dto)
        {
            var entity = await _context.DeliveryNoteItems
                .Include(i => i.SalesOrderItem)
                    .ThenInclude(soi => soi.SalesOrderBatches)
                .FirstOrDefaultAsync(e => e.DeliveryNoteItemId == deliveryNoteItemId);

            if (entity == null)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Delivery Note Item not found");

            if (entity.DeliveryNoteItemId != deliveryNoteItemId)
                return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("ID mismatch");

            // Validate quantity with reference (SalesOrderItem)
            if (entity.SalesOrderItem != null && dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value <= 0)
                    return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Quantity must be greater than zero");

                //if (dto.Quantity.Value > entity.SalesOrderItem.Quantity)
                //    return GeneralResponse<DeliveryNoteItemDTO>.FailResponse("Delivery quantity cannot exceed sales quantity");
            }

            if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
            {
                entity.Quantity = dto.Quantity.Value;

                // remove old batches
                var existingBatches = await _context.DeliveryNoteBatches
                    .Where(b => b.DeliveryNoteItemId == entity.DeliveryNoteItemId)
                    .ToListAsync();

                if (existingBatches.Any())
                {
                    _context.DeliveryNoteBatches.RemoveRange(existingBatches);
                    await _context.SaveChangesAsync();
                }

                // rebuild batches from SalesOrderBatches (if reference exists)
                if (entity.SalesOrderItem?.SalesOrderBatches != null && entity.SalesOrderItem.SalesOrderBatches.Any())
                {
                    var newBatches = new List<DeliveryNoteBatch>();
                    decimal remainingQty = dto.Quantity.Value;

                    foreach (var salesBatch in entity.SalesOrderItem.SalesOrderBatches.OrderBy(b => b.CreatedAt))
                    {
                        if (remainingQty <= 0)
                            break;

                        decimal batchQty = Math.Min(salesBatch.Quantity, remainingQty);

                        newBatches.Add(new DeliveryNoteBatch
                        {
                            DeliveryNoteItemId = entity.DeliveryNoteItemId,
                            SalesOrderBatchId = salesBatch.SalesOrderBatchId,
                            Quantity = batchQty,
                            BatchNumber = salesBatch.BatchNumber,
                            ExpiryDate = salesBatch.ExpiryDate,
                            CreatedAt = DateTime.UtcNow,
                            Comment = dto.Comment
                        });

                        remainingQty -= batchQty;
                    }

                    if (newBatches.Any())
                    {
                        await _context.DeliveryNoteBatches.AddRangeAsync(newBatches);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // update optional comment on batches only (item comment field may not exist)
            await _context.SaveChangesAsync();

            var result = new DeliveryNoteItemDTO
            {
                DeliveryNoteItemId = entity.DeliveryNoteItemId,
                Quantity = entity.Quantity,
                UoMEntry = entity.UoMEntry,
                BarCode = entity.BarCode,
                UnitPrice = entity.UnitPrice,
                ErrorMessage = entity.ErrorMessage,
                DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
                SalesOrderItemId = entity.SalesOrderItemId,
                ItemId = entity.ItemId
            };

            return GeneralResponse<DeliveryNoteItemDTO>.SuccessResponse(result);
        }


        public async Task<IEnumerable<DeliveryNoteItem>> GetByDeliveryNoteOrderIdEntitiesAsync(int deliveryNoteOrderId)
        {
            return await Query()
                .Where(dni => dni.DeliveryNoteOrderId == deliveryNoteOrderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryNoteItem>> GetByItemIdAsync(int itemId)
        {
            return await Query()
                .Where(dni => dni.ItemId == itemId)
                .ToListAsync();
        }

        public async Task<DeliveryNoteItem?> GetWithDeliveryNoteOrderAsync(int deliveryNoteItemId)
        {
            return await QueryIncluding(false, dni => dni.DeliveryNoteOrder)
                .FirstOrDefaultAsync(dni => dni.DeliveryNoteItemId == deliveryNoteItemId);
        }

        public async Task<DeliveryNoteItem?> GetWithSalesOrderItemAsync(int deliveryNoteItemId)
        {
            return await QueryIncluding(false, dni => dni.SalesOrderItem)
                .FirstOrDefaultAsync(dni => dni.DeliveryNoteItemId == deliveryNoteItemId);
        }

        public async Task<DeliveryNoteItem?> GetWithItemAsync(int deliveryNoteItemId)
        {
            return await QueryIncluding(false, dni => dni.Item)
                .FirstOrDefaultAsync(dni => dni.DeliveryNoteItemId == deliveryNoteItemId);
        }

        public async Task<DeliveryNoteItem?> GetWithBatchesAsync(int deliveryNoteItemId)
        {
            return await QueryIncluding(false, dni => dni.DeliveryNoteBatches)
                .FirstOrDefaultAsync(dni => dni.DeliveryNoteItemId == deliveryNoteItemId);
        }

     
    }
}
