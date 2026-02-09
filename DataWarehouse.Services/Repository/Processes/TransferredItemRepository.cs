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

public class TransferredItemRepository : BaseRepository<TransferredItem>, ITransferredItemRepository
{
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public TransferredItemRepository(ISapCache sapCache, DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<IEnumerable<TransferredItemDTO>> GetByTransferredItemByTransferredStockIdAsync(int TransferredStockId)
    {
        var res = await Query().Where(ti => ti.TransferredStockId == TransferredStockId).ToListAsync();

        return res.Select(e => new TransferredItemDTO
        {
            ItemId = e.ItemId,
            TransferredItemId = e.TransferredItemId,
            TransferredStockId = e.TransferredStockId,
            Quantity = e.Quantity,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            Comment = e.Comment
        });
    }

    public async Task<GeneralResponse<PagedResult<TransferredItemDTO>>>
     GetByTransferredItemByTransferredStockIdWithPaginationAsync(int TransferredStockId, string? status, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.TransferredItems.AsNoTracking().Where(b => b.TransferredStockId == TransferredStockId);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = Enum.Parse<GeneralItemStatus>(status, ignoreCase: true);
            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new TransferredItemDTO
        {
            ItemId = e.ItemId,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            Quantity = e.Quantity,
            TransferredItemId = e.TransferredItemId,
            TransferredStockId = e.TransferredStockId,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            Comment = e.Comment
        }).ToList();

        return GeneralResponse<PagedResult<TransferredItemDTO>>.SuccessResponse(new PagedResult<TransferredItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    // create
    public async Task<GeneralResponse<TransferredItemDTO>> AddTransferredItemByTransferredStockIdAsync(int TransferredStockid, bool isBarcode,
           DynamicBarcodesDto? barcodeDto,
           AddTransferredItemDTO? dto)
    {
        var model = new TransferredItem();
        var entity = await _context.TransferredStocks.FirstOrDefaultAsync(e => e.TransferredStockId == TransferredStockid);

        if (entity == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("id is not found");

        if (isBarcode)
        {
            var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);
            var item = new ItemByBarCodeDto();
            if (isDynamic)
            {
                var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

                if (!resD.Success)
                    return GeneralResponse<TransferredItemDTO>.FailResponse(resD.Message);

                item = resD.Data;

                if (resD.Data == null)
                    return GeneralResponse<TransferredItemDTO>.FailResponse(resD.Message);
            }
            else
            {
                var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
                if (!resD.Success)
                    return GeneralResponse<TransferredItemDTO>.FailResponse(resD.Message);

                item = resD.Data;
                if (resD.Data == null)
                    return GeneralResponse<TransferredItemDTO>.FailResponse(resD.Message);
            }

            model = new TransferredItem()
            {
                Status = GeneralItemStatus.Planned,
                TransferredStockId = TransferredStockid,
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
            model = new TransferredItem
            {
                Status = GeneralItemStatus.Planned,
                TransferredStockId = dto.TransferredStockId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                BarCode = "",
                UnitPrice = item.SalesPrice,
                UoMEntry = dto.UoMEntry,
            };
        }

        var res = await AddAsync(model);
        await SaveChangesAsync();

        var modelfin = new TransferredItemDTO
        {
            TransferredStockId = res.TransferredStockId,
            Quantity = res.Quantity,
            Status = GetEnumString(res.Status),
            ItemId = res.ItemId,
            TransferredItemId = res.TransferredItemId,
            UoMEntry = res.UoMEntry,
            BarCode = res.BarCode,
            UnitPrice = res.UnitPrice,
            ErrorMessage = res.ErrorMessage,
            Comment = res.Comment
        };

        return GeneralResponse<TransferredItemDTO>.SuccessResponse(modelfin);
    }

    // update
    public async Task<GeneralResponse<TransferredItemDTO>> UpdateTransferredItemAsync(int TransferredItemId,
           UpdateTransferredItemDTO dto)
    {
        var entity = await _context.TransferredItems.FirstOrDefaultAsync(e => e.TransferredItemId == dto.TransferredItemId);
        if (entity == null)
            return GeneralResponse<TransferredItemDTO>.FailResponse("id is not found");
        if (entity.TransferredItemId != TransferredItemId)
        {
            return GeneralResponse<TransferredItemDTO>.FailResponse("id not equal Transferred item id!");
        }

        var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == entity.ItemId);

        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
        {
            entity.Quantity = dto.Quantity.Value;
        }

        if (dto.UoMEntry > 0)
        {
            entity.UoMEntry = dto.UoMEntry;
        }

        await _context.SaveChangesAsync();

        var result = new TransferredItemDTO
        {
            TransferredStockId = entity.TransferredStockId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            TransferredItemId = entity.TransferredItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice,
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
