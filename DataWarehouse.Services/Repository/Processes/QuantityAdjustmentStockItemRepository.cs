using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class QuantityAdjustmentStockItemRepository : BaseRepository<QuantityAdjustmentStockItem>, IQuantityAdjustmentStockItemRepository
{
    private readonly IBaseProcessesRepository<QuantityAdjustmentStockItem> baseProcesses;

    public QuantityAdjustmentStockItemRepository(IBaseProcessesRepository<QuantityAdjustmentStockItem> baseProcesses, DataWarehouseDbContext context)
        : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(int quantityAdjustmentStockId)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem, QuantityAdjustmentStockItemDTO>(
            orderId: quantityAdjustmentStockId,
            orderIdSelector: x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            orderSet: _context.QuantityAdjustmentStocks,
            itemSet: _context.QuantityAdjustmentStockItems,
            itemFilter: x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: x => new QuantityAdjustmentStockItemDTO
            {
                QuantityAdjustmentStockItemId = x.QuantityAdjustmentStockItemId,
                QuantityAdjustmentStockId = x.QuantityAdjustmentStockId,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity,
                UoMEntry = x.UoMEntry,
                UnitName = x.Item.ItemUomGroups
                    .Where(i => i.UomEntry == x.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                BarCode = x.BarCode,
                UnitPrice = x.UnitPrice,
                VatPercent = x.VatPercent,
                VatAmount = x.VatAmount,
                LineTotalBeforeVat = x.LineTotalBeforeVat,
                LineTotalAfterVat = x.LineTotalAfterVat,
                ErrorMessage = x.ErrorMessage,
                Status = x.Status.ToString()
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<QuantityAdjustmentStockItemDTO>>> GetByQuantityAdjustmentStockItemByQuantityAdjustmentStockIdWithPaginationAsync(
        int quantityAdjustmentStockId, string? status, int pageNumber, int pageSize)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdWithPaginationAsync<
            QuantityAdjustmentStock,
            QuantityAdjustmentStockItem,
            QuantityAdjustmentStockItemDTO,
            string,
            GeneralItemStatus>(
            orderId: quantityAdjustmentStockId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            status: status,
            extraSelector: o => o.Status.ToString(),
            orderIdSelector: o => o.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            orderSet: _context.QuantityAdjustmentStocks,
            itemSet: _context.QuantityAdjustmentStockItems,
            itemFilter: i => i.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: x => new QuantityAdjustmentStockItemDTO
            {
                QuantityAdjustmentStockItemId = x.QuantityAdjustmentStockItemId,
                QuantityAdjustmentStockId = x.QuantityAdjustmentStockId,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                Quantity = x.Quantity,
                UoMEntry = x.UoMEntry,
                UnitName = x.Item.ItemUomGroups
                    .Where(i => i.UomEntry == x.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                BarCode = x.BarCode,
                UnitPrice = x.UnitPrice,
                VatPercent = x.VatPercent,
                VatAmount = x.VatAmount,
                LineTotalBeforeVat = x.LineTotalBeforeVat,
                LineTotalAfterVat = x.LineTotalAfterVat,
                ErrorMessage = x.ErrorMessage,
                Status = x.Status.ToString()
            },
            orderByDescSelector: x => x.QuantityAdjustmentStockItemId,
            itemStatusSelector: x => x.Status);

        if (!res.Success)
            return GeneralResponse<PagedResult<QuantityAdjustmentStockItemDTO>>.FailResponse(res.Message);

        return GeneralResponse<PagedResult<QuantityAdjustmentStockItemDTO>>.SuccessResponse(res.Data);
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> AddQuantityAdjustmentStockItemByQuantityAdjustmentStockIdAsync(
        int quantityAdjustmentStockId, bool isBarcode, DynamicBarcodesDto? dynamicDto, AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem>(
            orderId: quantityAdjustmentStockId,
            processType: ProcessType.QuantityAdjustment,
            isBarcode: isBarcode,
            barcodeDto: dynamicDto,
            dto: dto,
            orderIdSelector: x => x.QuantityAdjustmentStockId == quantityAdjustmentStockId,
            orderSet: _context.QuantityAdjustmentStocks,
            itemSet: _context.QuantityAdjustmentStockItems);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockItemDTO>.FailResponse(res.Message);

        return GeneralResponse<QuantityAdjustmentStockItemDTO>.SuccessResponse(new QuantityAdjustmentStockItemDTO
        {
            QuantityAdjustmentStockItemId = res.Data.QuantityAdjustmentStockItemId,
            QuantityAdjustmentStockId = res.Data.QuantityAdjustmentStockId,
            ItemId = res.Data.ItemId,
            Quantity = res.Data.Quantity,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage,
            Status = res.Data.Status.ToString()
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> UpdateQuantityAdjustmentStockItemAsync(
        int quantityAdjustmentStockItemId, UpdateGeneralItemDto dto)
    {
        var res = await baseProcesses.UpdateOrderItemAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem>(
            itemIdFromRoute: quantityAdjustmentStockItemId,
            processType: ProcessType.QuantityAdjustment,
            dto: dto,
            orderSet: _context.QuantityAdjustmentStocks,
            itemSelector: x => x.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            itemSet: _context.QuantityAdjustmentStockItems);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<QuantityAdjustmentStockItemDTO>.SuccessResponse(new QuantityAdjustmentStockItemDTO
        {
            QuantityAdjustmentStockItemId = entity.QuantityAdjustmentStockItemId,
            QuantityAdjustmentStockId = entity.QuantityAdjustmentStockId,
            ItemId = entity.ItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString()
        });
    }

    public async Task<GeneralResponse<QuantityAdjustmentStockItemDTO>> DeleteQuantityAdjustmentStockItemAsync(int quantityAdjustmentStockItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<QuantityAdjustmentStock, QuantityAdjustmentStockItem>(
            itemIdFromRoute: quantityAdjustmentStockItemId,
            processType: ProcessType.QuantityAdjustment,
            orderSet: _context.QuantityAdjustmentStocks,
            itemSelector: x => x.QuantityAdjustmentStockItemId == quantityAdjustmentStockItemId,
            itemSet: _context.QuantityAdjustmentStockItems);

        if (!res.Success)
            return GeneralResponse<QuantityAdjustmentStockItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        return GeneralResponse<QuantityAdjustmentStockItemDTO>.SuccessResponse(new QuantityAdjustmentStockItemDTO
        {
            QuantityAdjustmentStockItemId = entity.QuantityAdjustmentStockItemId,
            QuantityAdjustmentStockId = entity.QuantityAdjustmentStockId,
            ItemId = entity.ItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString()
        });
    }
}
