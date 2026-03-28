using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
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

public class SalesReturnOrderItemRepository : BaseRepository<SalesReturnOrderItem>, ISalesReturnOrderItemRepository
{
    private readonly IBaseProcessesRepository<SalesReturnOrderItem> baseProcesses;
    private readonly ISalesReturnOrderRepository salesReturn;

   
    public SalesReturnOrderItemRepository(IBaseProcessesRepository<SalesReturnOrderItem> baseProcesses, ISalesReturnOrderRepository salesReturn, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.salesReturn = salesReturn;
    }

    public async Task<GeneralResponse<IEnumerable<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdAsync(int salesReturnOrderId)
    {
        var res = await Query()
            .Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId)
            .Select(e => new SalesReturnOrderItemDTO
            {
                SalesReturnOrderItemId = e.SalesReturnOrderItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                SalesReturnOrderId = e.SalesReturnOrderId,
                DeliveryNoteItemId = e.DeliveryNoteItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,

            })
            .ToListAsync();

        return GeneralResponse < IEnumerable < SalesReturnOrderItemDTO >>.SuccessResponse( res);
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdWithPaginationAsync(int salesReturnOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrderItems
            .AsNoTracking()
            .Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = e.SalesReturnOrderItemId,
            Quantity = e.Quantity,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            VatPercent = e.VatPercent,
            VatAmount = e.VatAmount,
            LineTotalBeforeVat = e.LineTotalBeforeVat,
            LineTotalAfterVat = e.LineTotalAfterVat,
            ErrorMessage = e.ErrorMessage,
            Status = e.Status.ToString(),
            SalesReturnOrderId = e.SalesReturnOrderId,
            DeliveryNoteItemId = e.DeliveryNoteItemId,
            ItemId = e.ItemId
        }).ToList();

        return GeneralResponse<PagedResult<SalesReturnOrderItemDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    //
    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnItemBySalesReturnOrderIdWithoutRefAsync(
    int salesReturnOrderId,
    bool isBarcode,
    DynamicBarcodesDto? barcodeDto,
    AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<SalesReturnOrder, SalesReturnOrderItem>(
            salesReturnOrderId,
            ProcessType.SalesReturn,
            isBarcode,
            barcodeDto,
            dto,
            x => x.SalesReturnOrderId == salesReturnOrderId,
            _context.SalesReturnOrders,
            _context.SalesReturnOrderItems
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse(res.Message);

        var modelfin = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderId = res.Data.SalesReturnOrderId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            SalesReturnOrderItemId = res.Data.SalesReturnOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnItemWithoutRefAsync(
    int salesReturnOrderItemId,
    UpdateGeneralItemDto dto)
    {
        var res = await baseProcesses.UpdateOrderItemAsync<SalesReturnOrder, SalesReturnOrderItem>(
            itemIdFromRoute: salesReturnOrderItemId,
            processType: ProcessType.SalesReturn,
            dto: dto,
            orderSet: _context.SalesReturnOrders,
            itemSelector: x => x.SalesReturnOrderItemId == salesReturnOrderItemId,
            itemSet: _context.SalesReturnOrderItems
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderId = entity.SalesReturnOrderId,
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            DeliveryNoteItemId = entity.DeliveryNoteItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(result);
    }


    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnOrderItemByDeliveryNoteItemIdAsync(
    string userId,
    int DeliveryNoteOrderId,
    AddSalesReturnOrderItemDTO dto)
    {
        // Validate SalesOrder exists
        var salesOrder = await _context.DeliveryNoteOrders
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == DeliveryNoteOrderId);

        if (salesOrder == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order not found");

        // لو مفيش SalesReturnOrder، اعمله
        if (salesOrder.SalesReturnOrder == null)
        {
            var modelReturnOrder = new AddSalesReturnOrderDTO
            {
                DeliveryNoteOrderId = DeliveryNoteOrderId,
                Comment = dto.Comment
            };

            await salesReturn.AddSalesReturnOrderAsync(userId, modelReturnOrder);
        }

        // reload
        salesOrder = await _context.DeliveryNoteOrders
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == DeliveryNoteOrderId);

        // Get deliveryNoteItem with its batches
        var deliveryNoteItem = await _context.DeliveryNoteItems
            .Include(soi => soi.DeliveryNoteBatches)
            .Include(soi => soi.Item)
            .FirstOrDefaultAsync(soi => soi.DeliveryNoteItemId == dto.DeliveryNoteItemId);

        if (deliveryNoteItem == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order Item not found");

        // Check if this sales item already has a return item
        var existingReturnItem = await _context.SalesReturnOrderItems
            .FirstOrDefaultAsync(sroi => sroi.DeliveryNoteItemId == dto.DeliveryNoteItemId);

        if (existingReturnItem != null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("This Sales Order Item already has a return item");

        // Validate quantity doesn't exceed sales item quantity
        if (dto.Quantity > deliveryNoteItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed sales quantity");

        // Create SalesReturnOrderItem
        var salesReturnOrderItem = new SalesReturnOrderItem
        {
            SalesReturnOrderId = salesOrder.SalesReturnOrder.SalesReturnOrderId,
            DeliveryNoteItemId = dto.DeliveryNoteItemId,
            ItemId = deliveryNoteItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = deliveryNoteItem.UoMEntry,
            BarCode = deliveryNoteItem.BarCode,
            UnitPrice = deliveryNoteItem.UnitPrice,
        VatPercent = deliveryNoteItem.VatPercent,
        VatAmount = deliveryNoteItem.VatAmount,
        LineTotalBeforeVat = deliveryNoteItem.LineTotalBeforeVat,
        LineTotalAfterVat = deliveryNoteItem.LineTotalAfterVat,
        };

        var res = await AddAsync(salesReturnOrderItem);
        await SaveChangesAsync();

        // Automatically add batches from SalesOrderBatch (نفس لوجيك GoodsReturn)
        if (deliveryNoteItem.DeliveryNoteBatches != null && deliveryNoteItem.DeliveryNoteBatches.Any())
        {
            var batchesToAdd = new List<SalesReturnOrderBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var salesBatch in deliveryNoteItem.DeliveryNoteBatches.OrderBy(b => b.CreatedAt))
            {
                decimal batchQuantity = remainingQuantity > salesBatch.Quantity ? salesBatch.Quantity : remainingQuantity;

                if (remainingQuantity <= 0)
                    batchQuantity = 0;

                var returnBatch = new SalesReturnOrderBatch
                {
                    SalesReturnOrderItemId = res.SalesReturnOrderItemId,
                    DeliveryNoteBatchId = salesBatch.DeliveryNoteBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = salesBatch.BatchNumber,
                    ExpiryDate = salesBatch.ExpiryDate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                batchesToAdd.Add(returnBatch);
                remainingQuantity -= batchQuantity;
            }

            if (batchesToAdd.Any())
            {
                await _context.SalesReturnOrderBatches.AddRangeAsync(batchesToAdd);
                await SaveChangesAsync();
            }
        }

        // Reload with batches
        var finalItem = await _context.SalesReturnOrderItems
            .Include(sroi => sroi.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == res.SalesReturnOrderItemId);

        var model = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = finalItem.SalesReturnOrderItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            VatPercent = finalItem.VatPercent,
            VatAmount = finalItem.VatAmount,
            LineTotalBeforeVat = finalItem.LineTotalBeforeVat,
            LineTotalAfterVat = finalItem.LineTotalAfterVat,
            ErrorMessage = finalItem.ErrorMessage,
            SalesReturnOrderId = finalItem.SalesReturnOrderId,
            DeliveryNoteItemId = finalItem.DeliveryNoteItemId,
            ItemId = finalItem.ItemId,
            Batches = finalItem.SalesReturnOrderBatches?.Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(model);
    }


    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnOrderItemAsync(
    int salesReturnOrderItemId,
    UpdateSalesReturnOrderItemDTO dto)
    {
        var entity = await _context.SalesReturnOrderItems
            .Include(sroi => sroi.DeliveryNoteItem)
            .FirstOrDefaultAsync(e => e.SalesReturnOrderItemId == salesReturnOrderItemId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Return Order Item not found");

        if (entity.SalesReturnOrderItemId != salesReturnOrderItemId)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed sales item quantity
        if (dto.Quantity.HasValue && dto.Quantity.Value > entity.DeliveryNoteItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed sales quantity");

        // بعد تحديث الكمية:
        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
        {
            entity.Quantity = dto.Quantity.Value;

            // 🧹 احذف الباتشات القديمة
            var existingBatches = await _context.SalesReturnOrderBatches
                .Where(b => b.SalesReturnOrderItemId == entity.SalesReturnOrderItemId)
                .ToListAsync();

            if (existingBatches.Any())
            {
                _context.SalesReturnOrderBatches.RemoveRange(existingBatches);
                await _context.SaveChangesAsync();
            }

            // 🔄 رجّع deliveryNoteItem وباتشاته
            var deliveryNoteItem = await _context.DeliveryNoteItems
                .Include(soi => soi.DeliveryNoteBatches)
                .FirstOrDefaultAsync(soi => soi.DeliveryNoteItemId == entity.DeliveryNoteItemId);

            if (deliveryNoteItem == null)
                return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order Item not found");

            // ✅ إعادة بناء الباتشات
            var newBatches = new List<SalesReturnOrderBatch>();
            decimal remainingQty = dto.Quantity.Value;

            foreach (var salesBatch in deliveryNoteItem.DeliveryNoteBatches.OrderBy(b => b.CreatedAt))
            {
                decimal batchQty = Math.Min(salesBatch.Quantity, remainingQty);

                if (batchQty <= 0)
                    batchQty = 0;

                newBatches.Add(new SalesReturnOrderBatch
                {
                    SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
                    DeliveryNoteBatchId = salesBatch.DeliveryNoteBatchId,
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
                await _context.SalesReturnOrderBatches.AddRangeAsync(newBatches);
                await _context.SaveChangesAsync();
            }
        }

       

        await _context.SaveChangesAsync();

        var result = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            ErrorMessage = entity.ErrorMessage,
            SalesReturnOrderId = entity.SalesReturnOrderId,
            DeliveryNoteItemId = entity.DeliveryNoteItemId,
            ItemId = entity.ItemId
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(result);
    }
    public async Task<IEnumerable<SalesReturnOrderItem>> GetBySalesReturnOrderIdEntitiesAsync(int salesReturnOrderId)
    {
        return await Query().Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId).ToListAsync();
    }


    public async Task<IEnumerable<SalesReturnOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(sroi => sroi.ItemId == itemId).ToListAsync();
    }

    public async Task<SalesReturnOrderItem?> GetWithSalesReturnOrderAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.SalesReturnOrder)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithDeliveryNoteItemAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.DeliveryNoteItem)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithItemAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.Item)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithBatchesAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }
}

