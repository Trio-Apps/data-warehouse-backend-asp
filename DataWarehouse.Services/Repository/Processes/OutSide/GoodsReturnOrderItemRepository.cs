using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class GoodsReturnOrderItemRepository : BaseRepository<GoodsReturnOrderItem>, IGoodsReturnOrderItemRepository
{
    private readonly IBaseProcessesRepository<GoodsReturnOrderItem> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public GoodsReturnOrderItemRepository(
        IBaseProcessesRepository<GoodsReturnOrderItem> baseProcesses,
        ISapCache sapCache,
        DataWarehouseDbContext context,
        IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdAsync(int goodsReturnOrderId)
    {
        var res = await Query()
            .Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId).Select(e => new GoodsReturnOrderItemDTO
            {
                GoodsReturnOrderItemId = e.GoodsReturnOrderItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                ErrorMessage = e.ErrorMessage,
                Comment = e.Comment,
                GoodsReturnOrderId = e.GoodsReturnOrderId,
                ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,

            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<GoodsReturnOrderItemDTO>>.SuccessResponse(res);
    }

    public async Task<GeneralResponse<PagedResult<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdWithPaginationAsync(int goodsReturnOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.GoodsReturnOrderItems
            .AsNoTracking()
            .Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderItemId = e.GoodsReturnOrderItemId,
            Quantity = e.Quantity,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            VatPercent = e.VatPercent,
            VatAmount = e.VatAmount,
            LineTotalBeforeVat = e.LineTotalBeforeVat,
            LineTotalAfterVat = e.LineTotalAfterVat,
            ErrorMessage = e.ErrorMessage,
            Comment = e.Comment,
            GoodsReturnOrderId = e.GoodsReturnOrderId,
            ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
            ItemId = e.ItemId
        }).ToList();

        return GeneralResponse<PagedResult<GoodsReturnOrderItemDTO>>.SuccessResponse(
            new PagedResult<GoodsReturnOrderItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> AddGoodsReturnItemByGoodsReturnOrderIdWithoutRefAsync(int goodsReturnOrderId,
      bool isBarcode
       , DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto)
    {
        int? receiptPurchaseOrderItemId = null;

        var goodsReturnOrder = await _context.GoodsReturnOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GoodsReturnOrderId == goodsReturnOrderId);

        if (goodsReturnOrder != null && goodsReturnOrder.ReceiptPurchaseOrderId.HasValue)
        {
            var itemDataRes = await ResolveAddItemDataAsync(goodsReturnOrder.WarehouseId, isBarcode, barcodeDto, dto);
            if (!itemDataRes.Success)
                return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse(itemDataRes.Message);

            var linkedReceiptPurchaseOrderItemResult = await GetAvailableReceiptPurchaseOrderItemAsync(
                goodsReturnOrder.ReceiptPurchaseOrderId.Value,
                itemDataRes.Data.ItemId,
                itemDataRes.Data.UoMEntry,
                itemDataRes.Data.Quantity);

            if (linkedReceiptPurchaseOrderItemResult.IsMatchingReceiptPurchaseOrderItemFound
                && linkedReceiptPurchaseOrderItemResult.ReceiptPurchaseOrderItem == null)
                return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this receipt purchase order item.");

            if (linkedReceiptPurchaseOrderItemResult.ReceiptPurchaseOrderItem != null)
            {
                receiptPurchaseOrderItemId = linkedReceiptPurchaseOrderItemResult.ReceiptPurchaseOrderItem.ReceiptPurchaseOrderItemId;
            }
        }

        var res = await baseProcesses.AddOrderItemAsync<GoodsReturnOrder, GoodsReturnOrderItem>(
    goodsReturnOrderId,
    ProcessType.GoodsReturn,
    isBarcode,
    barcodeDto,
    dto,
     x => x.GoodsReturnOrderId == goodsReturnOrderId,
    _context.GoodsReturnOrders,
    _context.GoodsReturnOrderItems);


        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse(res.Message);

        if (receiptPurchaseOrderItemId.HasValue)
        {
            res.Data.ReceiptPurchaseOrderItemId = receiptPurchaseOrderItemId.Value;
            await _context.SaveChangesAsync();
        }

        var modelfin = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderId = res.Data.GoodsReturnOrderId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            GoodsReturnOrderItemId = res.Data.GoodsReturnOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage,
            ReceiptPurchaseOrderItemId = res.Data.ReceiptPurchaseOrderItemId
        };

        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> UpdateGoodsReturnItemWithoutRefAsync(int goodsReturnOrderItemId,
        UpdateGeneralItemDto dto)
    {
        var entityBeforeUpdate = await _context.GoodsReturnOrderItems
            .Include(x => x.GoodsReturnOrder)
            .FirstOrDefaultAsync(x => x.GoodsReturnOrderItemId == goodsReturnOrderItemId);

        if (entityBeforeUpdate == null)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("id is not found");

        if (dto.Quantity.HasValue
            && entityBeforeUpdate.GoodsReturnOrder != null
            && entityBeforeUpdate.GoodsReturnOrder.ReceiptPurchaseOrderId.HasValue)
        {
            var receiptPurchaseOrderId = entityBeforeUpdate.GoodsReturnOrder.ReceiptPurchaseOrderId.Value;
            ReceiptPurchaseOrderItem? linkedReceiptPurchaseOrderItem = null;

            if (entityBeforeUpdate.ReceiptPurchaseOrderItemId.HasValue)
            {
                linkedReceiptPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ReceiptPurchaseOrderItemId == entityBeforeUpdate.ReceiptPurchaseOrderItemId.Value);

                if (linkedReceiptPurchaseOrderItem == null)
                    return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("receipt purchase order item is not found");

                var executedQuantity = await _context.GoodsReturnOrderItems
                    .AsNoTracking()
                    .Where(x => x.ReceiptPurchaseOrderItemId == linkedReceiptPurchaseOrderItem.ReceiptPurchaseOrderItemId
                                && x.GoodsReturnOrderItemId != goodsReturnOrderItemId)
                    .Select(x => (decimal?)x.Quantity)
                    .SumAsync() ?? 0m;

                if (executedQuantity + dto.Quantity.Value > linkedReceiptPurchaseOrderItem.Quantity)
                    return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this receipt purchase order item.");
            }
            else
            {
                var linkedReceiptPurchaseOrderItemResult = await GetAvailableReceiptPurchaseOrderItemAsync(
                    receiptPurchaseOrderId,
                    entityBeforeUpdate.ItemId,
                    entityBeforeUpdate.UoMEntry,
                    dto.Quantity.Value,
                    goodsReturnOrderItemId);

                if (!linkedReceiptPurchaseOrderItemResult.IsMatchingReceiptPurchaseOrderItemFound)
                {
                    linkedReceiptPurchaseOrderItem = null;
                }
                else if (linkedReceiptPurchaseOrderItemResult.ReceiptPurchaseOrderItem == null)
                {
                    return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Quantity exceeds the remaining allowed quantity for this receipt purchase order item.");
                }
                else
                {
                    linkedReceiptPurchaseOrderItem = linkedReceiptPurchaseOrderItemResult.ReceiptPurchaseOrderItem;
                }
            }

            if (linkedReceiptPurchaseOrderItem != null)
            {
                entityBeforeUpdate.ReceiptPurchaseOrderItemId = linkedReceiptPurchaseOrderItem.ReceiptPurchaseOrderItemId;
            }
        }

        var res = await baseProcesses.UpdateOrderItemAsync<GoodsReturnOrder, GoodsReturnOrderItem>(
       itemIdFromRoute: goodsReturnOrderItemId,
       processType: ProcessType.GoodsReturn,
       dto: dto,
       orderSet: _context.GoodsReturnOrders,
       itemSelector: x => x.GoodsReturnOrderItemId == goodsReturnOrderItemId, // أو x => x.SalesOrderItemId == SalesItemId
       itemSet: _context.GoodsReturnOrderItems
   );

        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse(res.Message);

        if (entityBeforeUpdate.ReceiptPurchaseOrderItemId.HasValue
            && res.Data.ReceiptPurchaseOrderItemId != entityBeforeUpdate.ReceiptPurchaseOrderItemId)
        {
            res.Data.ReceiptPurchaseOrderItemId = entityBeforeUpdate.ReceiptPurchaseOrderItemId;
            await _context.SaveChangesAsync();
        }

        var entity = res.Data;

        var result = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(result);
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

    private async Task<(ReceiptPurchaseOrderItem? ReceiptPurchaseOrderItem, bool IsMatchingReceiptPurchaseOrderItemFound)> GetAvailableReceiptPurchaseOrderItemAsync(
        int receiptPurchaseOrderId,
        int itemId,
        int uoMEntry,
        decimal requestedQuantity,
        int? excludeGoodsReturnOrderItemId = null)
    {
        var receiptPurchaseOrderItems = await _context.ReceiptPurchaseOrderItems
            .AsNoTracking()
            .Where(x => x.ReceiptPurchaseOrderId == receiptPurchaseOrderId
                        && x.ItemId == itemId
                        && x.UoMEntry == uoMEntry)
            .OrderBy(x => x.ReceiptPurchaseOrderItemId)
            .ToListAsync();

        if (!receiptPurchaseOrderItems.Any())
            return (null, false);

        foreach (var receiptPurchaseOrderItem in receiptPurchaseOrderItems)
        {
            var executedQuantity = await _context.GoodsReturnOrderItems
                .AsNoTracking()
                .Where(x => x.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItem.ReceiptPurchaseOrderItemId
                            && (!excludeGoodsReturnOrderItemId.HasValue || x.GoodsReturnOrderItemId != excludeGoodsReturnOrderItemId.Value))
                .Select(x => (decimal?)x.Quantity)
                .SumAsync() ?? 0m;

            if (executedQuantity + requestedQuantity <= receiptPurchaseOrderItem.Quantity)
                return (receiptPurchaseOrderItem, true);
        }

        return (null, true);
    }


    public async Task<IEnumerable<GoodsReturnOrderItem>> GetByGoodsReturnOrderIdEntitiesAsync(int goodsReturnOrderId)
    {
        return await Query().Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId).ToListAsync();
    }


    public async Task<IEnumerable<GoodsReturnOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(groi => groi.ItemId == itemId).ToListAsync();
    }

    public async Task<GoodsReturnOrderItem?> GetWithGoodsReturnOrderAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.GoodsReturnOrder)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }

    public async Task<GoodsReturnOrderItem?> GetWithItemAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.Item)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }
}

