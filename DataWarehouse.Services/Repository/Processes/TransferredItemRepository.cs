using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class TransferredItemRepository : BaseRepository<TransferredItem>, ITransferredItemRepository
{
    private readonly IBaseProcessesRepository<TransferredItem> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public TransferredItemRepository(
        IBaseProcessesRepository<TransferredItem> baseProcesses,
        ISapCache sapCache,
        IBarCodeOrdersRepository barcodeOrder,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<TransferredItemDTO>>> GetByTransferredItemByTransferredStockIdAsync(int transferredStockId)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdAsync<
            TransferredStock,
            TransferredItem,
            TransferredItemDTO>(
            orderId: transferredStockId,
            orderIdSelector: x => x.TransferredStockId == transferredStockId,
            orderSet: _context.TransferredStocks,
            itemSet: _context.TransferredItems,
            itemFilter: x => x.TransferredStockId == transferredStockId,
            include: q => q
                .Include(x => x.Item)
                  .ThenInclude(i => i.ItemUomGroups)
                .Include(x => x.TransferredStockBatches)
,
            selector: e => new TransferredItemDTO
            {
                TransferredItemId = e.TransferredItemId,
                Quantity = e.Quantity,
                ReceivedQuantity = e.ReceivedQuantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                TransferredStockId = e.TransferredStockId,
                TransferredRequestItemId = e.TransferredRequestItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                BatchesCount = e.TransferredStockBatches.Count,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                Comment = e.Comment
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<TransferredItemDTO>>> GetByTransferredItemByTransferredStockIdWithPaginationAsync(
        int transferredStockId, string? status, int pageNumber, int pageSize)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdWithPaginationAsync<
            TransferredStock,
            TransferredItem,
            TransferredItemDTO,
            string,
            GeneralItemStatus>(
            orderId: transferredStockId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            status: status,
            extraSelector: o => o.Status.ToString(),
            orderIdSelector: o => o.TransferredStockId == transferredStockId,
            orderSet: _context.TransferredStocks,
            itemSet: _context.TransferredItems,
            itemFilter: i => i.TransferredStockId == transferredStockId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups)
                                .Include(x => x.TransferredStockBatches)
,
            selector: e => new TransferredItemDTO
            {
                TransferredItemId = e.TransferredItemId,
                Quantity = e.Quantity,
                ReceivedQuantity = e.ReceivedQuantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                TransferredStockId = e.TransferredStockId,
                TransferredRequestItemId = e.TransferredRequestItemId,
                BatchesCount = e.TransferredStockBatches.Count,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                Comment = e.Comment
            },
            orderByDescSelector: x => x.TransferredItemId,
            itemStatusSelector: x => x.Status);

        if (!res.Success)
            return GeneralResponse<PagedResult<TransferredItemDTO>>.FailResponse(res.Message);

        return GeneralResponse<PagedResult<TransferredItemDTO>>.SuccessResponse(res.Data);
    }
    public async Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredStockIdWithoutRefAsync(
        int transferredStockId,
        bool isBarcode,
        DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto)
    {
        int? transferredRequestItemId = null;

        var transferredStock = await _context.TransferredStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransferredStockId == transferredStockId);

        if (transferredStock != null && transferredStock.TransferredRequestId.HasValue)
        {
            var itemDataRes = await ResolveAddItemDataAsync(transferredStock.WarehouseId, isBarcode, barcodeDto, dto);
            if (!itemDataRes.Success)
                return GeneralResponse<TransferredItemDTO>.FailResponse(itemDataRes.Message);

            var linkedRequestItemResult = await GetAvailableTransferredRequestItemAsync(
                transferredStock.TransferredRequestId.Value,
                itemDataRes.Data.ItemId,
                itemDataRes.Data.UoMEntry,
                itemDataRes.Data.Quantity);

            if (linkedRequestItemResult.IsMatchingTransferredRequestItemFound
                && linkedRequestItemResult.TransferredRequestItem == null)
                return GeneralResponse<TransferredItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this transferred request item.");

            if (linkedRequestItemResult.TransferredRequestItem != null)
            {
                transferredRequestItemId = linkedRequestItemResult.TransferredRequestItem.TransferredRequestItemId;
            }
        }

        var res = await baseProcesses.AddOrderItemAsync<TransferredStock, TransferredItem>(
            orderId: transferredStockId,
            processType: ProcessType.Transferred,
            isBarcode: isBarcode,
            barcodeDto: barcodeDto,
            dto: dto,
            orderIdSelector: x => x.TransferredStockId == transferredStockId,
            orderSet: _context.TransferredStocks,
            itemSet: _context.TransferredItems
        );

        if (!res.Success)
            return GeneralResponse<TransferredItemDTO>.FailResponse(res.Message);

        if (transferredRequestItemId.HasValue)
        {
            res.Data.TransferredRequestItemId = transferredRequestItemId.Value;
            await _context.SaveChangesAsync();
        }

        var model = new TransferredItemDTO
        {
            TransferredStockId = res.Data.TransferredStockId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            TransferredItemId = res.Data.TransferredItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage,
            TransferredRequestItemId = res.Data.TransferredRequestItemId,
            Comment = res.Data.Comment
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(model);
    }


    public async Task<GeneralResponse<TransferredItemDTO>> UpdateTransferredItemWithoutRefAsync(
          int transferredItemId,
          UpdateGeneralItemDto dto)
    {
        var entityBeforeUpdate = await _context.TransferredItems
            .Include(x => x.TransferredStock)
            .FirstOrDefaultAsync(x => x.TransferredItemId == transferredItemId);

        if (entityBeforeUpdate == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("id is not found");

        if (dto.Quantity.HasValue
            && entityBeforeUpdate.TransferredStock != null
            && entityBeforeUpdate.TransferredStock.TransferredRequestId.HasValue)
        {
            var requestId = entityBeforeUpdate.TransferredStock.TransferredRequestId.Value;
            TransferredRequestItem? linkedRequestItem = null;

            if (entityBeforeUpdate.TransferredRequestItemId.HasValue)
            {
                linkedRequestItem = await _context.TransferredRequestItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TransferredRequestItemId == entityBeforeUpdate.TransferredRequestItemId.Value);

                if (linkedRequestItem == null)
                    return GeneralResponse<TransferredItemDTO>.FailResponse("transferred request item is not found");

                var executedQuantity = await _context.TransferredItems
                    .AsNoTracking()
                    .Where(x => x.TransferredRequestItemId == linkedRequestItem.TransferredRequestItemId
                                && x.TransferredItemId != transferredItemId)
                    .Select(x => (decimal?)x.Quantity)
                    .SumAsync() ?? 0m;

                if (executedQuantity + dto.Quantity.Value > linkedRequestItem.Quantity)
                    return GeneralResponse<TransferredItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this transferred request item.");
            }
            else
            {
                var linkedRequestItemResult = await GetAvailableTransferredRequestItemAsync(
                    requestId,
                    entityBeforeUpdate.ItemId,
                    entityBeforeUpdate.UoMEntry,
                    dto.Quantity.Value,
                    transferredItemId);

                if (!linkedRequestItemResult.IsMatchingTransferredRequestItemFound)
                {
                    linkedRequestItem = null;
                }
                else if (linkedRequestItemResult.TransferredRequestItem == null)
                {
                    return GeneralResponse<TransferredItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this transferred request item.");
                }
                else
                {
                    linkedRequestItem = linkedRequestItemResult.TransferredRequestItem;
                }
            }

            if (linkedRequestItem != null)
            {
                entityBeforeUpdate.TransferredRequestItemId = linkedRequestItem.TransferredRequestItemId;
            }
        }

        var res = await baseProcesses.UpdateOrderItemAsync<TransferredStock, TransferredItem>(
            itemIdFromRoute: transferredItemId,
            processType: ProcessType.Transferred,
            dto: dto,
            orderSet: _context.TransferredStocks,
            itemSelector: x => x.TransferredItemId == transferredItemId,
            itemSet: _context.TransferredItems
        );

        if (!res.Success)
            return GeneralResponse<TransferredItemDTO>.FailResponse(res.Message);

        if (entityBeforeUpdate.TransferredRequestItemId.HasValue
            && res.Data.TransferredRequestItemId != entityBeforeUpdate.TransferredRequestItemId)
        {
            res.Data.TransferredRequestItemId = entityBeforeUpdate.TransferredRequestItemId;
            await _context.SaveChangesAsync();
        }

        var entity = res.Data;

        var result = new TransferredItemDTO
        {
            TransferredStockId = entity.TransferredStockId,
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            TransferredRequestItemId = entity.TransferredRequestItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(result);
    }


 
    public async Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredRequestItemIdAsync(
           string userId,
           int transferredStockId,
           AddTransferredItemDTO dto)
    {
        var stock = await _context.TransferredStocks
            .FirstOrDefaultAsync(ts => ts.TransferredStockId == transferredStockId);

        if (stock == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("Transferred stock not found");

        var requestItem = await _context.TransferredRequestItems
            .Include(ri => ri.TransferredRequestBatches)
            .Include(ri => ri.Item)
            .FirstOrDefaultAsync(ri => ri.TransferredRequestItemId == dto.TransferredRequestItemId);

        if (requestItem == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("Transferred request item not found");


        var qty = dto.Quantity ?? requestItem.Quantity;
        if (qty <= 0)
            return GeneralResponse<TransferredItemDTO>.FailResponse("Quantity must be greater than zero");

        var executedQuantity = await _context.TransferredItems
            .Where(ti => ti.TransferredRequestItemId == dto.TransferredRequestItemId)
            .SumAsync(ti => (decimal?)ti.Quantity) ?? 0m;

        if (executedQuantity + qty > requestItem.Quantity)
            return GeneralResponse<TransferredItemDTO>.FailResponse("Transfer quantity cannot exceed request quantity");

        var item = new TransferredItem
        {
            TransferredStockId = transferredStockId,
            TransferredRequestItemId = dto.TransferredRequestItemId,
            ItemId = requestItem.ItemId,
            Quantity = qty,
            UoMEntry = requestItem.UoMEntry,
            BarCode = requestItem.BarCode,
            UnitPrice = requestItem.UnitPrice,
            VatPercent = requestItem.VatPercent,
            VatAmount = requestItem.VatAmount,
            LineTotalBeforeVat = requestItem.LineTotalBeforeVat,
            LineTotalAfterVat = requestItem.LineTotalAfterVat,
            Status = GeneralItemStatus.Planned,
            ErrorMessage = null,
            Comment = requestItem.Comment,
            LineNum = requestItem.LineNum
        };

        var res = await AddAsync(item);
        await SaveChangesAsync();

        if (requestItem.TransferredRequestBatches != null && requestItem.TransferredRequestBatches.Any())
        {
            var batchesToAdd = new List<TransferredStockBatch>();
            decimal remainingQuantity = qty;

            foreach (var requestBatch in requestItem.TransferredRequestBatches.OrderBy(b => b.CreatedAt))
            {
                if (remainingQuantity <= 0)
                    break;

                decimal batchQuantity = remainingQuantity > requestBatch.Quantity ? requestBatch.Quantity : remainingQuantity;

                batchesToAdd.Add(new TransferredStockBatch
                {
                    TransferredItemId = res.TransferredItemId,
                    TransferredRequestBatchId = requestBatch.TransferredRequestBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = requestBatch.BatchNumber,
                    ExpiryDate = requestBatch.ExpiryDate,
                    Comment = requestBatch.Comment,
                    CreatedAt = DateTime.UtcNow
                });

                remainingQuantity -= batchQuantity;
            }

            if (batchesToAdd.Any())
            {
                await _context.TransferredStockBatches.AddRangeAsync(batchesToAdd);
                await SaveChangesAsync();
            }
        }

        var finalItem = await _context.TransferredItems
            .Include(i => i.TransferredStockBatches)
            .FirstOrDefaultAsync(i => i.TransferredItemId == res.TransferredItemId);

        var model = new TransferredItemDTO
        {
            TransferredItemId = finalItem!.TransferredItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            VatPercent = finalItem.VatPercent,
            VatAmount = finalItem.VatAmount,
            LineTotalBeforeVat = finalItem.LineTotalBeforeVat,
            LineTotalAfterVat = finalItem.LineTotalAfterVat,
            ErrorMessage = finalItem.ErrorMessage,
            TransferredStockId = finalItem.TransferredStockId,
            TransferredRequestItemId = finalItem.TransferredRequestItemId,
            ItemId = finalItem.ItemId,
            Comment = finalItem.Comment,
            Status = finalItem.Status.ToString(),
            Batches = finalItem.TransferredStockBatches?.Select(b => new TransferredStockBatchDTO
            {
                TransferredStockBatchId = b.TransferredStockBatchId,
                TransferredItemId = b.TransferredItemId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<TransferredItemDTO>> UpdateTransferredItemAsync(
        int transferredItemId,
        UpdateTransferredItemDTO dto)
    {
        var entity = await _context.TransferredItems
            .Include(i => i.TransferredRequestItem)
                .ThenInclude(ri => ri.TransferredRequestBatches)
            .Include(i => i.TransferredStock)
            .FirstOrDefaultAsync(e => e.TransferredItemId == transferredItemId);

        if (entity == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("Transferred Item not found");

        if (entity.TransferredItemId != transferredItemId)
            return GeneralResponse<TransferredItemDTO>.FailResponse("ID mismatch");

        if (dto.Quantity.HasValue
            && entity.TransferredStock != null
            && entity.TransferredStock.TransferredRequestId.HasValue)
        {
            var requestId = entity.TransferredStock.TransferredRequestId.Value;
            TransferredRequestItem? linkedRequestItem = entity.TransferredRequestItem;

            if (entity.TransferredRequestItemId.HasValue)
            {
                if (linkedRequestItem == null)
                {
                    linkedRequestItem = await _context.TransferredRequestItems
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TransferredRequestItemId == entity.TransferredRequestItemId.Value);
                }

                if (linkedRequestItem == null)
                    return GeneralResponse<TransferredItemDTO>.FailResponse("transferred request item is not found");

                var executedQuantity = await _context.TransferredItems
                    .AsNoTracking()
                    .Where(x => x.TransferredRequestItemId == linkedRequestItem.TransferredRequestItemId
                                && x.TransferredItemId != transferredItemId)
                    .Select(x => (decimal?)x.Quantity)
                    .SumAsync() ?? 0m;

                if (executedQuantity + dto.Quantity.Value > linkedRequestItem.Quantity)
                    return GeneralResponse<TransferredItemDTO>.FailResponse("Transfer quantity cannot exceed request quantity");
            }
            else
            {
                var linkedRequestItemResult = await GetAvailableTransferredRequestItemAsync(
                    requestId,
                    entity.ItemId,
                    entity.UoMEntry,
                    dto.Quantity.Value,
                    transferredItemId);

                if (!linkedRequestItemResult.IsMatchingTransferredRequestItemFound)
                {
                    linkedRequestItem = null;
                }
                else if (linkedRequestItemResult.TransferredRequestItem == null)
                {
                    return GeneralResponse<TransferredItemDTO>.FailResponse("Transfer quantity cannot exceed request quantity");
                }
                else
                {
                    linkedRequestItem = linkedRequestItemResult.TransferredRequestItem;
                    entity.TransferredRequestItemId = linkedRequestItem.TransferredRequestItemId;
                }
            }
        }

        var mappedDto = new UpdateGeneralItemDto
        {
            Quantity = dto.Quantity,
            UoMEntry = dto.UoMEntry
        };

        var res = await baseProcesses.UpdateOrderItemAsync<TransferredStock, TransferredItem>(
            itemIdFromRoute: transferredItemId,
            processType: ProcessType.Transferred,
            dto: mappedDto,
            orderSet: _context.TransferredStocks,
            itemSelector: x => x.TransferredItemId == transferredItemId,
            itemSet: _context.TransferredItems);

        if (!res.Success)
            return GeneralResponse<TransferredItemDTO>.FailResponse(res.Message);

        if (dto.Quantity.HasValue && res.Data.TransferredRequestItemId.HasValue)
        {
            var existingBatches = await _context.TransferredStockBatches
                .Where(b => b.TransferredItemId == entity.TransferredItemId)
                .ToListAsync();

            if (existingBatches.Any())
            {
                _context.TransferredStockBatches.RemoveRange(existingBatches);
                await _context.SaveChangesAsync();
            }

            var requestItemForBatches = entity.TransferredRequestItem;
            if (requestItemForBatches == null)
            {
                requestItemForBatches = await _context.TransferredRequestItems
                    .Include(ri => ri.TransferredRequestBatches)
                    .FirstOrDefaultAsync(ri => ri.TransferredRequestItemId == entity.TransferredRequestItemId);
            }

            if (requestItemForBatches?.TransferredRequestBatches != null &&
                requestItemForBatches.TransferredRequestBatches.Any())
            {
                var newBatches = new List<TransferredStockBatch>();
                decimal remainingQty = entity.Quantity;

                foreach (var requestBatch in requestItemForBatches.TransferredRequestBatches.OrderBy(b => b.CreatedAt))
                {
                    if (remainingQty <= 0)
                        break;

                    decimal batchQty = Math.Min(requestBatch.Quantity, remainingQty);

                    newBatches.Add(new TransferredStockBatch
                    {
                        TransferredItemId = entity.TransferredItemId,
                        TransferredRequestBatchId = requestBatch.TransferredRequestBatchId,
                        Quantity = batchQty,
                        BatchNumber = requestBatch.BatchNumber,
                        ExpiryDate = requestBatch.ExpiryDate,
                        CreatedAt = DateTime.UtcNow,
                        Comment = dto.Quantity.HasValue ? entity.Comment : null
                    });

                    remainingQty -= batchQty;
                }

                if (newBatches.Any())
                {
                    await _context.TransferredStockBatches.AddRangeAsync(newBatches);
                    await _context.SaveChangesAsync();
                }
            }
        }

        var result = new TransferredItemDTO
        {
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString(),
            TransferredStockId = entity.TransferredStockId,
            TransferredRequestItemId = entity.TransferredRequestItemId,
            ItemId = entity.ItemId,
            Comment = entity.Comment
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(result);
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

    private async Task<(TransferredRequestItem? TransferredRequestItem, bool IsMatchingTransferredRequestItemFound)> GetAvailableTransferredRequestItemAsync(
        int transferredRequestId,
        int itemId,
        int uoMEntry,
        decimal requestedQuantity,
        int? excludeTransferredItemId = null)
    {
        var requestItems = await _context.TransferredRequestItems
            .AsNoTracking()
            .Where(x => x.TransferredRequestId == transferredRequestId
                        && x.ItemId == itemId
                        && x.UoMEntry == uoMEntry)
            .OrderBy(x => x.TransferredRequestItemId)
            .ToListAsync();

        if (!requestItems.Any())
            return (null, false);

        foreach (var requestItem in requestItems)
        {
            var executedQuantity = await _context.TransferredItems
                .AsNoTracking()
                .Where(x => x.TransferredRequestItemId == requestItem.TransferredRequestItemId
                            && (!excludeTransferredItemId.HasValue || x.TransferredItemId != excludeTransferredItemId.Value))
                .Select(x => (decimal?)x.Quantity)
                .SumAsync() ?? 0m;

            if (executedQuantity + requestedQuantity <= requestItem.Quantity)
                return (requestItem, true);
        }

        return (null, true);
    }

    public async Task<GeneralResponse<TransferredItemDTO>> DeleteTransferredItemAsync(int transferredItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<TransferredStock, TransferredItem>(
            itemIdFromRoute: transferredItemId,
            processType: ProcessType.Transferred,
            orderSet: _context.TransferredStocks,
            itemSelector: x => x.TransferredItemId == transferredItemId,
            itemSet: _context.TransferredItems);

        if (!res.Success)
            return GeneralResponse<TransferredItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var dto = new TransferredItemDTO
        {
            TransferredItemId = entity.TransferredItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString(),
            TransferredStockId = entity.TransferredStockId,
            TransferredRequestItemId = entity.TransferredRequestItemId,
            ItemId = entity.ItemId,
            Comment = entity.Comment
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(dto);
    }

    public async Task<IEnumerable<TransferredItem>> GetByTransferredStockIdEntitiesAsync(int transferredStockId)
    {
        return await Query()
            .Where(ti => ti.TransferredStockId == transferredStockId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransferredItem>> GetByItemIdAsync(int itemId)
    {
        return await Query()
            .Where(ti => ti.ItemId == itemId)
            .ToListAsync();
    }

    public async Task<TransferredItem?> GetWithTransferredStockAsync(int transferredItemId)
    {
        return await QueryIncluding(false, ti => ti.TransferredStock)
            .FirstOrDefaultAsync(ti => ti.TransferredItemId == transferredItemId);
    }

    public async Task<TransferredItem?> GetWithTransferredRequestItemAsync(int transferredItemId)
    {
        return await QueryIncluding(false, ti => ti.TransferredRequestItem)
            .FirstOrDefaultAsync(ti => ti.TransferredItemId == transferredItemId);
    }

    public async Task<TransferredItem?> GetWithItemAsync(int transferredItemId)
    {
        return await QueryIncluding(false, ti => ti.Item)
            .FirstOrDefaultAsync(ti => ti.TransferredItemId == transferredItemId);
    }

    public async Task<TransferredItem?> GetWithBatchesAsync(int transferredItemId)
    {
        return await QueryIncluding(false, ti => ti.TransferredStockBatches)
            .FirstOrDefaultAsync(ti => ti.TransferredItemId == transferredItemId);
    }
}
