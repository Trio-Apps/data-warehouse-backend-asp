using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesOrderItemRepository : BaseRepository<SalesOrderItem>, ISalesOrderItemRepository
{
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public SalesOrderItemRepository(ISapCache sapCache, DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<SalesOrderItemDTO>>> GetBySalesItemBySalesOrderIdAsync(int SalesOrderId)
    {
        var salesOrder = await _context.SalesOrders.FirstOrDefaultAsync(s=>s.SalesOrderId == SalesOrderId) ;
      
        if (salesOrder == null)
            return GeneralResponse<IEnumerable<SalesOrderItemDTO>>.FailResponse("sales order id is not found");
        var res = await Query().Include(e=>e.Item).Where(soi => soi.SalesOrderId == SalesOrderId).Select(e => new SalesOrderItemDTO
        {
            ItemId = e.ItemId,
            SalesOrderItemId = e.SalesOrderItemId,
            SalesOrderId = e.SalesOrderId,
            Quantity = e.Quantity,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
            ItemCode = e.Item.ItemCode,
            ItemName = e.Item.ItemName,

        }).ToListAsync();

       

        return GeneralResponse<IEnumerable<SalesOrderItemDTO>>.SuccessResponse(res);
    
    }


    public async Task<GeneralResponse<PagedResult<SalesOrderItemDTO>>>
     GetBySalesItemBySalesOrderIdWithPaginationAsync(int SalesOrderId, string? status, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesOrderItems.AsNoTracking().Where(b => b.SalesOrderId == SalesOrderId);

        // 🔹 Filtering
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = Enum.Parse<GeneralItemStatus>(status, ignoreCase: true);
            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new SalesOrderItemDTO
        {
            ItemId = e.ItemId,
            Status = e.Status.ToString(),
            ErrorMessage = e.ErrorMessage,
            Quantity = e.Quantity,
            SalesOrderItemId = e.SalesOrderItemId,
            SalesOrderId = e.SalesOrderId,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice
        }).ToList();

        return GeneralResponse<PagedResult<SalesOrderItemDTO>>.SuccessResponse(new PagedResult<SalesOrderItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    // create
    public async Task<GeneralResponse<SalesOrderItemDTO>> AddSalesItemBySalesOrderIdAsync(int SalesOrderid, bool isBarcode,
           DynamicBarcodesDto? barcodeDto,
           AddSalesOrderItemDTO? dto)
    {
        var model = new SalesOrderItem();
        var entity = await _context.SalesOrders.FirstOrDefaultAsync(e => e.SalesOrderId == SalesOrderid);

        if (entity == null)
            return GeneralResponse<SalesOrderItemDTO>.FailResponse("id is not found");

        if (isBarcode)
        {
            var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);
            var item = new ItemByBarCodeDto();
            if (isDynamic)
            {
                var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

                if (!resD.Success)
                    return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);

                item = resD.Data;

                if (resD.Data == null)
                    return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);
            }
            else
            {
                var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
                if (!resD.Success)
                    return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);

                item = resD.Data;
                if (resD.Data == null)
                    return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);
            }

            model = new SalesOrderItem()
            {
                Status = GeneralItemStatus.Planned,
                SalesOrderId = SalesOrderid,
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
            model = new SalesOrderItem
            {
                Status = GeneralItemStatus.Planned,
                SalesOrderId = dto.SalesOrderId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                BarCode = "",
                UnitPrice = item.SalesPrice,
                UoMEntry = dto.UoMEntry,
            };
        }

        var res = await AddAsync(model);
        await SaveChangesAsync();

        var modelfin = new SalesOrderItemDTO
        {
            SalesOrderId = res.SalesOrderId,
            Quantity = res.Quantity,
            Status = GetEnumString(res.Status),
            ItemId = res.ItemId,
            SalesOrderItemId = res.SalesOrderItemId,
            UoMEntry = res.UoMEntry,
            BarCode = res.BarCode,
            UnitPrice = res.UnitPrice,
            ErrorMessage = res.ErrorMessage
        };

        return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(modelfin);
    }

    // update
    public async Task<GeneralResponse<SalesOrderItemDTO>> UpdateSalesItemAsync(int SalesItemId,
           UpdateSalesOrderItemDTO dto)
    {
        var entity = await _context.SalesOrderItems.FirstOrDefaultAsync(e => e.SalesOrderItemId == dto.SalesOrderItemId);
        if (entity == null)
            return GeneralResponse<SalesOrderItemDTO>.FailResponse("id is not found");
        if (entity.SalesOrderItemId != SalesItemId)
        {
            return GeneralResponse<SalesOrderItemDTO>.FailResponse("id not equal Sales order id!");
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

        var result = new SalesOrderItemDTO
        {
            SalesOrderId = entity.SalesOrderId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            SalesOrderItemId = entity.SalesOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(result);
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
