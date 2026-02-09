using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class CountStockItemRepository : BaseRepository<CountStockItem>, ICountStockItemRepository
{
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public CountStockItemRepository(ISapCache sapCache, DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<IEnumerable<CountStockItemDTO>> GetByCountStockItemByCountStockIdAsync(int CountStockId)
    {
        var res = await Query().Where(csi => csi.CountStockId == CountStockId).ToListAsync();

        return res.Select(e => new CountStockItemDTO
        {
            ItemId = e.ItemId,
            CountStockItemId = e.CountStockItemId,
            CountStockId = e.CountStockId,
            Quantity = e.Quantity,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            Comment = e.Comment
        });
    }

    public async Task<GeneralResponse<PagedResult<CountStockItemDTO>>>
     GetByCountStockItemByCountStockIdWithPaginationAsync(int CountStockId, string? status, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.CountStockItems.AsNoTracking().Where(csi => csi.CountStockId == CountStockId);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = Enum.Parse<GeneralItemStatus>(status, ignoreCase: true);
            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new CountStockItemDTO
        {
            ItemId = e.ItemId,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            Quantity = e.Quantity,
            CountStockItemId = e.CountStockItemId,
            CountStockId = e.CountStockId,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            Comment = e.Comment
        }).ToList();

        return GeneralResponse<PagedResult<CountStockItemDTO>>.SuccessResponse(new PagedResult<CountStockItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    // create
    public async Task<GeneralResponse<CountStockItemDTO>> AddCountStockItemByCountStockIdAsync(int CountStockid, bool isBarcode,
           DynamicBarcodesDto? barcodeDto,
           AddCountStockItemDTO? dto)
    {
        var model = new CountStockItem();
        var entity = await _context.CountStocks.FirstOrDefaultAsync(e => e.CountStockId == CountStockid);

        if (entity == null)
            return GeneralResponse<CountStockItemDTO>.FailResponse("id is not found");

        if (isBarcode)
        {
            var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);
            var item = new ItemByBarCodeDto();
            if (isDynamic)
            {
                var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

                if (!resD.Success)
                    return GeneralResponse<CountStockItemDTO>.FailResponse(resD.Message);

                item = resD.Data;

                if (resD.Data == null)
                    return GeneralResponse<CountStockItemDTO>.FailResponse(resD.Message);
            }
            else
            {
                var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
                if (!resD.Success)
                    return GeneralResponse<CountStockItemDTO>.FailResponse(resD.Message);

                item = resD.Data;
                if (resD.Data == null)
                    return GeneralResponse<CountStockItemDTO>.FailResponse(resD.Message);
            }

            model = new CountStockItem()
            {
                Status = GeneralItemStatus.Planned,
                CountStockId = CountStockid,
                ItemId = item.Id,
                Quantity = item.Quantity,
                BarCode = item.Barcode,
                UnitPrice = item.Price,
                UoMEntry = item.UoMEntry
            };
        }
        else
        {
            var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == dto.ItemId);
            model = new CountStockItem
            {
                Status = GeneralItemStatus.Planned,
                CountStockId = dto.CountStockId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                BarCode = "",
                UnitPrice = item.SalesPrice,
                UoMEntry = dto.UoMEntry,
            };
        }

        var res = await AddAsync(model);
        await SaveChangesAsync();

        var modelfin = new CountStockItemDTO
        {
            CountStockId = res.CountStockId,
            Quantity = res.Quantity,
            Status = GetEnumString(res.Status),
            ItemId = res.ItemId,
            CountStockItemId = res.CountStockItemId,
            UoMEntry = res.UoMEntry,
            BarCode = res.BarCode,
            UnitPrice = res.UnitPrice,
            ErrorMessage = res.ErrorMessage,
            Comment = res.Comment
        };

        return GeneralResponse<CountStockItemDTO>.SuccessResponse(modelfin);
    }

    // update
    public async Task<GeneralResponse<CountStockItemDTO>> UpdateCountStockItemAsync(int CountStockItemId,
           UpdateCountStockItemDTO dto)
    {
        var entity = await _context.CountStockItems.FirstOrDefaultAsync(e => e.CountStockItemId == dto.CountStockItemId);
        if (entity == null)
            return GeneralResponse<CountStockItemDTO>.FailResponse("id is not found");
        if (entity.CountStockItemId != CountStockItemId)
        {
            return GeneralResponse<CountStockItemDTO>.FailResponse("id not equal Count stock item id!");
        }

        var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == entity.ItemId);

        if (dto.Quantity.HasValue && dto.Quantity.Value >= 0)
        {
            entity.Quantity = dto.Quantity.Value;
        }

        if (dto.UoMEntry > 0)
        {
            entity.UoMEntry = dto.UoMEntry;
        }

        await _context.SaveChangesAsync();

        var result = new CountStockItemDTO
        {
            CountStockId = entity.CountStockId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            CountStockItemId = entity.CountStockItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice,
            Comment = entity.Comment
        };

        return GeneralResponse<CountStockItemDTO>.SuccessResponse(result);
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

    private string GetEnumString(GeneralItemStatus status)
    {
        switch (status)
        {
            case GeneralItemStatus.Draft:
                return "Draft";
            case GeneralItemStatus.Planned:
                return "Planned";
            case GeneralItemStatus.Released:
                return "Released";
            case GeneralItemStatus.Received:
                return "Received";
            case GeneralItemStatus.Closed:
                return "Closed";
            case GeneralItemStatus.Failed:
                return "Failed";
            default:
                return "Unknown";
        }
    }
}
