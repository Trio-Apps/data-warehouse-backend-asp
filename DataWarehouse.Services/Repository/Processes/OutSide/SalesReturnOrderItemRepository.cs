using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
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
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

   
    public SalesReturnOrderItemRepository(
        IBaseProcessesRepository<SalesReturnOrderItem> baseProcesses,
        ISalesReturnOrderRepository salesReturn,
        ISapCache sapCache,
        IBarCodeOrdersRepository barcodeOrder,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.salesReturn = salesReturn;
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
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
        int? deliveryNoteItemId = null;

        var salesReturnOrder = await _context.SalesReturnOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SalesReturnOrderId == salesReturnOrderId);

        if (salesReturnOrder != null && salesReturnOrder.DeliveryNoteOrderId.HasValue)
        {
            var itemDataRes = await ResolveAddItemDataAsync(salesReturnOrder.WarehouseId, isBarcode, barcodeDto, dto);
            if (!itemDataRes.Success)
                return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse(itemDataRes.Message);

            var linkedDeliveryNoteItemResult = await GetAvailableDeliveryNoteItemAsync(
                salesReturnOrder.DeliveryNoteOrderId.Value,
                itemDataRes.Data.ItemId,
                itemDataRes.Data.UoMEntry,
                itemDataRes.Data.Quantity);

            if (linkedDeliveryNoteItemResult.IsMatchingDeliveryNoteItemFound && linkedDeliveryNoteItemResult.DeliveryNoteItem == null)
                return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this delivery note item.");

            if (linkedDeliveryNoteItemResult.DeliveryNoteItem != null)
            {
                deliveryNoteItemId = linkedDeliveryNoteItemResult.DeliveryNoteItem.DeliveryNoteItemId;
            }
        }

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

        if (deliveryNoteItemId.HasValue)
        {
            res.Data.DeliveryNoteItemId = deliveryNoteItemId.Value;
            await _context.SaveChangesAsync();
        }

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
            ErrorMessage = res.Data.ErrorMessage,
            DeliveryNoteItemId = res.Data.DeliveryNoteItemId
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnItemWithoutRefAsync(
    int salesReturnOrderItemId,
    UpdateGeneralItemDto dto)
    {
        var entityBeforeUpdate = await _context.SalesReturnOrderItems
            .Include(x => x.SalesReturnOrder)
            .FirstOrDefaultAsync(x => x.SalesReturnOrderItemId == salesReturnOrderItemId);

        if (entityBeforeUpdate == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("id is not found");

        if (dto.Quantity.HasValue
            && entityBeforeUpdate.SalesReturnOrder != null
            && entityBeforeUpdate.SalesReturnOrder.DeliveryNoteOrderId.HasValue)
        {
            var deliveryNoteOrderId = entityBeforeUpdate.SalesReturnOrder.DeliveryNoteOrderId.Value;
            DeliveryNoteItem? linkedDeliveryNoteItem = null;

            if (entityBeforeUpdate.DeliveryNoteItemId.HasValue)
            {
                linkedDeliveryNoteItem = await _context.DeliveryNoteItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DeliveryNoteItemId == entityBeforeUpdate.DeliveryNoteItemId.Value);

                if (linkedDeliveryNoteItem == null)
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("delivery note item is not found");

                var executedQuantity = await _context.SalesReturnOrderItems
                    .AsNoTracking()
                    .Where(x => x.DeliveryNoteItemId == linkedDeliveryNoteItem.DeliveryNoteItemId
                                && x.SalesReturnOrderItemId != salesReturnOrderItemId)
                    .Select(x => (decimal?)x.Quantity)
                    .SumAsync() ?? 0m;

                if (executedQuantity + dto.Quantity.Value > linkedDeliveryNoteItem.Quantity)
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this delivery note item.");
            }
            else
            {
                var linkedDeliveryNoteItemResult = await GetAvailableDeliveryNoteItemAsync(
                    deliveryNoteOrderId,
                    entityBeforeUpdate.ItemId,
                    entityBeforeUpdate.UoMEntry,
                    dto.Quantity.Value,
                    salesReturnOrderItemId);

                if (!linkedDeliveryNoteItemResult.IsMatchingDeliveryNoteItemFound)
                {
                    linkedDeliveryNoteItem = null;
                }
                else if (linkedDeliveryNoteItemResult.DeliveryNoteItem == null)
                {
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this delivery note item.");
                }
                else
                {
                    linkedDeliveryNoteItem = linkedDeliveryNoteItemResult.DeliveryNoteItem;
                }
            }

            if (linkedDeliveryNoteItem != null)
            {
                entityBeforeUpdate.DeliveryNoteItemId = linkedDeliveryNoteItem.DeliveryNoteItemId;
            }
        }

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

        if (entityBeforeUpdate.DeliveryNoteItemId.HasValue && res.Data.DeliveryNoteItemId != entityBeforeUpdate.DeliveryNoteItemId)
        {
            res.Data.DeliveryNoteItemId = entityBeforeUpdate.DeliveryNoteItemId;
            await _context.SaveChangesAsync();
        }

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
        var deliveryNoteOrder = await _context.DeliveryNoteOrders
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == DeliveryNoteOrderId);

        if (deliveryNoteOrder == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order not found");

        var salesReturnOrder = await _context.SalesReturnOrders
            .Where(s => s.DeliveryNoteOrderId == DeliveryNoteOrderId)
            .OrderByDescending(s => s.SalesReturnOrderId)
            .FirstOrDefaultAsync();

        if (salesReturnOrder == null)
        {
            if (deliveryNoteOrder.Status != GeneralStatus.Completed)
                return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("You can add Sales Return, if DeliveryNoteOrder status is completed only");

            salesReturnOrder = new SalesReturnOrder
            {
                Status = GeneralStatus.Processing,
                PostingDate = deliveryNoteOrder.PostingDate,
                DueDate = deliveryNoteOrder.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                WarehouseId = deliveryNoteOrder.WarehouseId,
                Comment = dto.Comment,
                CustomerId = deliveryNoteOrder.CustomerId,
                DeliveryNoteOrderId = DeliveryNoteOrderId
            };

            await _context.SalesReturnOrders.AddAsync(salesReturnOrder);
            await _context.SaveChangesAsync();
        }

        // Get deliveryNoteItem with its batches
        var deliveryNoteItem = await _context.DeliveryNoteItems
            .Include(soi => soi.DeliveryNoteBatches)
            .Include(soi => soi.Item)
            .FirstOrDefaultAsync(soi => soi.DeliveryNoteItemId == dto.DeliveryNoteItemId);

        if (deliveryNoteItem == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order Item not found");

        var totalReturned = await _context.SalesReturnOrderItems
            .AsNoTracking()
            .Where(sroi => sroi.DeliveryNoteItemId == dto.DeliveryNoteItemId)
            .Select(sroi => (decimal?)sroi.Quantity)
            .SumAsync() ?? 0m;

        // Validate quantity doesn't exceed delivery note item quantity
        if (totalReturned + dto.Quantity > deliveryNoteItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed delivery note item quantity");

        // Create SalesReturnOrderItem
        var salesReturnOrderItem = new SalesReturnOrderItem
        {
            SalesReturnOrderId = salesReturnOrder.SalesReturnOrderId,
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
            .Include(sroi => sroi.SalesReturnOrder)
            .FirstOrDefaultAsync(e => e.SalesReturnOrderItemId == salesReturnOrderItemId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Return Order Item not found");

        if (entity.SalesReturnOrderItemId != salesReturnOrderItemId)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("ID mismatch");

        if (dto.Quantity.HasValue)
        {
            if (entity.DeliveryNoteItemId.HasValue)
            {
                var linkedDeliveryNoteItem = await _context.DeliveryNoteItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DeliveryNoteItemId == entity.DeliveryNoteItemId.Value);

                if (linkedDeliveryNoteItem == null)
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Delivery note item is not found");

                var executedQuantity = await _context.SalesReturnOrderItems
                    .AsNoTracking()
                    .Where(x => x.DeliveryNoteItemId == linkedDeliveryNoteItem.DeliveryNoteItemId
                                && x.SalesReturnOrderItemId != salesReturnOrderItemId)
                    .Select(x => (decimal?)x.Quantity)
                    .SumAsync() ?? 0m;

                if (executedQuantity + dto.Quantity.Value > linkedDeliveryNoteItem.Quantity)
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed delivery note item quantity");
            }
            else if (entity.SalesReturnOrder != null && entity.SalesReturnOrder.DeliveryNoteOrderId.HasValue)
            {
                var linkedDeliveryNoteItemResult = await GetAvailableDeliveryNoteItemAsync(
                    entity.SalesReturnOrder.DeliveryNoteOrderId.Value,
                    entity.ItemId,
                    entity.UoMEntry,
                    dto.Quantity.Value,
                    salesReturnOrderItemId);

                if (!linkedDeliveryNoteItemResult.IsMatchingDeliveryNoteItemFound)
                {
                    // no matching base item, allow without linking
                }
                else if (linkedDeliveryNoteItemResult.DeliveryNoteItem == null)
                {
                    return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this delivery note item.");
                }
                else
                {
                    entity.DeliveryNoteItemId = linkedDeliveryNoteItemResult.DeliveryNoteItem.DeliveryNoteItemId;
                }
            }
        }

        // بعد تحديث الكمية:
        if (dto.Quantity.HasValue && dto.Quantity.Value > 0 && entity.DeliveryNoteItemId.HasValue)
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
                .FirstOrDefaultAsync(soi => soi.DeliveryNoteItemId == entity.DeliveryNoteItemId.Value);

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

    private async Task<bool> CheckDynamicCodeValidationLocal(string barCode)
    {
        var sapId = await sapCache.Get();

        var settings = await _context.BarCodeSettings
            .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
            .ToListAsync();

        foreach (var setting in settings)
        {
            if (barCode.Length != setting.TotalLength)
                continue;

            return true;
        }

        return false;
    }

    private async Task<(bool Success, string Message, (int ItemId, int UoMEntry, decimal Quantity) Data)> ResolveAddItemDataAsync(
        int warehouseId,
        bool isBarcode,
        DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto)
    {
        if (isBarcode)
        {
            if (barcodeDto == null)
                return (false, "Barcode required", default);

            var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);

            var res = isDynamic
                ? await barcodeOrder.GetItemByDynamicBarCodeAsync(warehouseId, barcodeDto)
                : await barcodeOrder.GetItemByStaticBarCodeAsync(warehouseId, barcodeDto);

            if (!res.Success || res.Data == null)
                return (false, res.Message, default);

            return (true, "", (res.Data.Id, res.Data.UoMEntry, res.Data.Quantity));
        }

        if (dto == null)
            return (false, "DTO required", default);

        return (true, "", (dto.ItemId, dto.UoMEntry, dto.Quantity));
    }

    private async Task<(DeliveryNoteItem? DeliveryNoteItem, bool IsMatchingDeliveryNoteItemFound)> GetAvailableDeliveryNoteItemAsync(
        int deliveryNoteOrderId,
        int itemId,
        int uoMEntry,
        decimal requestedQuantity,
        int? excludeSalesReturnOrderItemId = null)
    {
        var deliveryNoteItems = await _context.DeliveryNoteItems
            .AsNoTracking()
            .Where(x => x.DeliveryNoteOrderId == deliveryNoteOrderId
                        && x.ItemId == itemId
                        && x.UoMEntry == uoMEntry)
            .OrderBy(x => x.DeliveryNoteItemId)
            .ToListAsync();

        if (!deliveryNoteItems.Any())
            return (null, false);

        foreach (var deliveryNoteItem in deliveryNoteItems)
        {
            var executedQuantity = await _context.SalesReturnOrderItems
                .AsNoTracking()
                .Where(x => x.DeliveryNoteItemId == deliveryNoteItem.DeliveryNoteItemId
                            && (!excludeSalesReturnOrderItemId.HasValue || x.SalesReturnOrderItemId != excludeSalesReturnOrderItemId.Value))
                .Select(x => (decimal?)x.Quantity)
                .SumAsync() ?? 0m;

            if (executedQuantity + requestedQuantity <= deliveryNoteItem.Quantity)
                return (deliveryNoteItem, true);
        }

        return (null, true);
    }
}

