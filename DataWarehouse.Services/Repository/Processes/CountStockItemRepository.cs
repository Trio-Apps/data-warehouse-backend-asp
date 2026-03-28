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

public class CountStockItemRepository : BaseRepository<CountStockItem>, ICountStockItemRepository
{
    private readonly IBaseProcessesRepository<CountStockItem> baseProcesses;

    public CountStockItemRepository(IBaseProcessesRepository<CountStockItem> baseProcesses, DataWarehouseDbContext context)
        : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<IEnumerable<CountStockItemDTO>> GetByCountStockItemByCountStockIdAsync(int countStockId)
    {
        return await Query()
            .Where(x => x.CountStockId == countStockId)
            .Select(x => new CountStockItemDTO
            {
                ItemId = x.ItemId,
                IsBatchManaged = _context.WarehouseItems
                    .Where(w => w.WarehouseId == x.CountStock.WarehouseId && w.ItemId == x.ItemId)
                    .Select(w => (bool?)w.IsBatchManaged)
                    .FirstOrDefault() ?? x.Item.BatchNumbers,
                CountStockItemId = x.CountStockItemId,
                CountStockId = x.CountStockId,
                Quantity = x.Quantity,
                Status = x.Status.ToString(),
                ErrorMessage = x.ErrorMessage,
                UoMEntry = x.UoMEntry,
                BarCode = x.BarCode,
                UnitPrice = x.UnitPrice,
                VatPercent = x.VatPercent,
                VatAmount = x.VatAmount,
                LineTotalBeforeVat = x.LineTotalBeforeVat,
                LineTotalAfterVat = x.LineTotalAfterVat,
                Comment = x.Comment
            })
            .ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<CountStockItemDTO>>> GetByCountStockItemByCountStockIdWithPaginationAsync(
        int countStockId, string? status, int pageNumber, int pageSize)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdWithPaginationAsync<
            CountStock,
            CountStockItem,
            CountStockItemDTO,
            string,
            GeneralItemStatus>(
            orderId: countStockId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            status: status,
            extraSelector: o => o.Status.ToString(),
            orderIdSelector: o => o.CountStockId == countStockId,
            orderSet: _context.CountStocks,
            itemSet: _context.CountStockItems,
            itemFilter: i => i.CountStockId == countStockId,
            include: null,
            selector: x => new CountStockItemDTO
            {
                ItemId = x.ItemId,
                IsBatchManaged = _context.WarehouseItems
                    .Where(w => w.WarehouseId == x.CountStock.WarehouseId && w.ItemId == x.ItemId)
                    .Select(w => (bool?)w.IsBatchManaged)
                    .FirstOrDefault() ?? x.Item.BatchNumbers,
                CountStockItemId = x.CountStockItemId,
                CountStockId = x.CountStockId,
                Quantity = x.Quantity,
                Status = x.Status.ToString(),
                ErrorMessage = x.ErrorMessage,
                UoMEntry = x.UoMEntry,
                BarCode = x.BarCode,
                UnitPrice = x.UnitPrice,
                VatPercent = x.VatPercent,
                VatAmount = x.VatAmount,
                LineTotalBeforeVat = x.LineTotalBeforeVat,
                LineTotalAfterVat = x.LineTotalAfterVat,
                Comment = x.Comment
            },
            orderByDescSelector: x => x.CountStockItemId,
            itemStatusSelector: x => x.Status);

        if (!res.Success)
            return GeneralResponse<PagedResult<CountStockItemDTO>>.FailResponse(res.Message);

        return GeneralResponse<PagedResult<CountStockItemDTO>>.SuccessResponse(res.Data);
    }

    public async Task<GeneralResponse<CountStockItemDTO>> AddCountStockItemByCountStockIdAsync(
        int countStockId,
        bool isBarcode,
        DynamicBarcodesDto? dynamicDto,
        AddCountStockItemDTO? dto)
    {
        var mappedDto = dto == null ? null : new AddGeneralItemDto
        {
            ItemId = dto.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = dto.UoMEntry
        };

        var res = await baseProcesses.AddOrderItemAsync<CountStock, CountStockItem>(
            orderId: countStockId,
            processType: ProcessType.Counting,
            isBarcode: isBarcode,
            barcodeDto: dynamicDto,
            dto: mappedDto,
            orderIdSelector: x => x.CountStockId == countStockId,
            orderSet: _context.CountStocks,
            itemSet: _context.CountStockItems);

        if (!res.Success)
            return GeneralResponse<CountStockItemDTO>.FailResponse(res.Message);

        var entity = res.Data;
        var isBatchManaged = await ResolveIsBatchManagedAsync(entity.CountStockId, entity.ItemId);

        return GeneralResponse<CountStockItemDTO>.SuccessResponse(new CountStockItemDTO
        {
            ItemId = entity.ItemId,
            IsBatchManaged = isBatchManaged,
            CountStockItemId = entity.CountStockItemId,
            CountStockId = entity.CountStockId,
            Quantity = entity.Quantity,
            Status = entity.Status.ToString(),
            ErrorMessage = entity.ErrorMessage,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            Comment = entity.Comment
        });
    }

    public async Task<GeneralResponse<CountStockItemDTO>> UpdateCountStockItemAsync(int countStockItemId, UpdateCountStockItemDTO dto)
    {
        var mappedDto = new UpdateGeneralItemDto
        {
            Quantity = dto.Quantity,
            UoMEntry = dto.UoMEntry
        };

        var res = await baseProcesses.UpdateOrderItemAsync<CountStock, CountStockItem>(
            itemIdFromRoute: countStockItemId,
            processType: ProcessType.Counting,
            dto: mappedDto,
            orderSet: _context.CountStocks,
            itemSelector: x => x.CountStockItemId == countStockItemId,
            itemSet: _context.CountStockItems);

        if (!res.Success)
            return GeneralResponse<CountStockItemDTO>.FailResponse(res.Message);

        var entity = res.Data;
        var isBatchManaged = await ResolveIsBatchManagedAsync(entity.CountStockId, entity.ItemId);

        return GeneralResponse<CountStockItemDTO>.SuccessResponse(new CountStockItemDTO
        {
            ItemId = entity.ItemId,
            IsBatchManaged = isBatchManaged,
            CountStockItemId = entity.CountStockItemId,
            CountStockId = entity.CountStockId,
            Quantity = entity.Quantity,
            Status = entity.Status.ToString(),
            ErrorMessage = entity.ErrorMessage,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            VatPercent = entity.VatPercent,
            VatAmount = entity.VatAmount,
            LineTotalBeforeVat = entity.LineTotalBeforeVat,
            LineTotalAfterVat = entity.LineTotalAfterVat,
            Comment = entity.Comment
        });
    }

    private async Task<bool> ResolveIsBatchManagedAsync(int countStockId, int itemId)
    {
        var warehouseId = await _context.CountStocks
            .AsNoTracking()
            .Where(c => c.CountStockId == countStockId)
            .Select(c => (int?)c.WarehouseId)
            .FirstOrDefaultAsync();

        if (warehouseId.HasValue)
        {
            var warehouseManaged = await _context.WarehouseItems
                .AsNoTracking()
                .Where(w => w.WarehouseId == warehouseId.Value && w.ItemId == itemId)
                .Select(w => (bool?)w.IsBatchManaged)
                .FirstOrDefaultAsync();

            if (warehouseManaged.HasValue)
                return warehouseManaged.Value;
        }

        return await _context.Items
            .AsNoTracking()
            .Where(i => i.ItemId == itemId)
            .Select(i => i.BatchNumbers)
            .FirstOrDefaultAsync();
    }
}
