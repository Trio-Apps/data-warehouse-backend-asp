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
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class ReceiptPurchaseOrderItemRepository : BaseRepository<ReceiptPurchaseOrderItem>, IReceiptPurchaseOrderItemRepository
{
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public ReceiptPurchaseOrderItemRepository(ISapCache sapCache,DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderItemDTO>>> GetByReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(int ReceiptPurchaseOrderId)
    {
        var res = await Query().Where(rpoi => rpoi.ReceiptPurchaseOrderId == ReceiptPurchaseOrderId).Select(e => new ReceiptPurchaseOrderItemDTO
        {
            ItemId = e.ItemId,
            ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
            ReceiptPurchaseOrderId = e.ReceiptPurchaseOrderId,
            Quantity = e.Quantity,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            ErrorMessage = e.ErrorMessage,
            UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
            Comment = e.Comment, 
            ItemCode = e.Item.ItemCode,
           ItemName = e.Item.ItemName,
           IsBatches = e.Item.BatchNumbers
        }).ToListAsync();

        return GeneralResponse<IEnumerable<ReceiptPurchaseOrderItemDTO>>.SuccessResponse(res);    
    }

    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderItemDTO>>>
        GetByReceiptPurchaseItemByReceiptPurchaseOrderIdWithPaginationAsync(int ReceiptPurchaseOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;



        var query = _context.ReceiptPurchaseOrderItems.AsNoTracking().Where(b => b.ReceiptPurchaseOrderId == ReceiptPurchaseOrderId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new ReceiptPurchaseOrderItemDTO
        {
            ItemId = e.ItemId,
            ErrorMessage = e.ErrorMessage,
            Quantity = e.Quantity,
            ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
            ReceiptPurchaseOrderId = e.ReceiptPurchaseOrderId,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
            Comment = e.Comment,
            Item = e.Item,
            UnitPrice = e.UnitPrice
        }).ToList();


        return GeneralResponse<PagedResult<ReceiptPurchaseOrderItemDTO>>.SuccessResponse(new PagedResult<ReceiptPurchaseOrderItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> AddReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(int ReceiptPurchaseOrderid, 
        bool isBarcode
         , DynamicBarcodesDto? barcodeDto,
          AddReceiptPurchaseOrderItemDTO? dto)
    {
        var model = new ReceiptPurchaseOrderItem();
        var entity = await _context.ReceiptPurchaseOrders.FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderId == ReceiptPurchaseOrderid);

        if (entity == null)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse("id is not found");
       

        
        if (isBarcode)
        {
            var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);

            var item = new ItemByBarCodeDto();
            if (isDynamic)
            {
                var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

                if (!resD.Success)
                    return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(resD.Message);

                item = resD.Data;

                if (resD.Data == null)
                    return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(resD.Message);
            }
            else
            {
                var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
                if (!resD.Success)
                    return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(resD.Message);

                item = resD.Data;
                if (resD.Data == null)
                    return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(resD.Message);
            }

            model = new ReceiptPurchaseOrderItem()
            {
                ReceiptPurchaseOrderId = ReceiptPurchaseOrderid,
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
            model = new ReceiptPurchaseOrderItem
            {
                ReceiptPurchaseOrderId = dto.ReceiptPurchaseOrderId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                BarCode = "",
                UnitPrice = item.PurchasePrice,
                UoMEntry = dto.UoMEntry

            };
        }

        var itemFound = await CheckItemFound(model.ItemId, ReceiptPurchaseOrderid);

        if (itemFound)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse("this item is found already in order");

        var res = await AddAsync(model);
        await SaveChangesAsync();

        var modelfin = new ReceiptPurchaseOrderItemDTO
        {
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            Quantity = res.Quantity,
            ItemId = res.ItemId,
            ReceiptPurchaseOrderItemId = res.ReceiptPurchaseOrderItemId,
            UoMEntry = res.UoMEntry,
            BarCode = res.BarCode,
            UnitPrice = res.UnitPrice,
            ErrorMessage = res.ErrorMessage
        };

        return GeneralResponse<ReceiptPurchaseOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> UpdateReceiptPurchaseItemAsync(int ReceiptPurchaseItemId,
        UpdateReceiptPurchaseOrderItemDTO dto)
    {
        var entity = await _context.ReceiptPurchaseOrderItems.FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderItemId == ReceiptPurchaseItemId);
        if (entity == null)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse("id is not found");
        if (entity.ReceiptPurchaseOrderItemId != ReceiptPurchaseItemId)
        {
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse("id not equal Receipt purchase order id!");
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

        var result = new ReceiptPurchaseOrderItemDTO
        {
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<ReceiptPurchaseOrderItemDTO>.SuccessResponse(result);
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

    private async Task<bool> CheckItemFound(int itemId, int receiptOrderId)
    {
        var items = await GetByReceiptPurchaseItemByReceiptPurchaseOrderIdAsync(receiptOrderId);

        if (items == null || items.Data == null)
            return false;

        return items.Data.Any(e => e.ItemId == itemId);
    }


    public async Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId)
    {
        return await Query().Where(rpoi => rpoi.ReceiptPurchaseOrderId == receiptPurchaseOrderId).ToListAsync();
    }

    public async Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(rpoi => rpoi.ItemId == itemId).ToListAsync();
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithReceiptPurchaseOrderAsync(int receiptPurchaseOrderItemId)
    {
        return await QueryIncluding(false, rpoi => rpoi.ReceiptPurchaseOrder)
            .FirstOrDefaultAsync(rpoi => rpoi.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId);
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithItemAsync(int receiptPurchaseOrderItemId)
    {
        return await QueryIncluding(false, rpoi => rpoi.Item)
            .FirstOrDefaultAsync(rpoi => rpoi.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId);
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithCommentAsync(int receiptPurchaseOrderItemId)
    {
        return await QueryIncluding(false, rpoi => rpoi.Comment)
            .FirstOrDefaultAsync(rpoi => rpoi.ReceiptPurchaseOrderItemId == receiptPurchaseOrderItemId);
    }
}
