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
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;


namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class ReceiptPurchaseOrderItemRepository : BaseRepository<ReceiptPurchaseOrderItem>, IReceiptPurchaseOrderItemRepository
{
    private readonly IBaseProcessesRepository<ReceiptPurchaseOrderItem> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public ReceiptPurchaseOrderItemRepository(IBaseProcessesRepository<ReceiptPurchaseOrderItem> baseProcesses, ISapCache sapCache,DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.baseProcesses = baseProcesses;
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
            VatPercent = e.VatPercent,
            VatAmount = e.VatAmount,
            LineTotalBeforeVat = e.LineTotalBeforeVat,
            LineTotalAfterVat = e.LineTotalAfterVat,
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
          AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<ReceiptPurchaseOrder, ReceiptPurchaseOrderItem>(
    ReceiptPurchaseOrderid,
    ProcessType.Receipt,
    isBarcode,
    barcodeDto,
    dto,
     x => x.ReceiptPurchaseOrderId == ReceiptPurchaseOrderid,
    _context.ReceiptPurchaseOrders,
    _context.ReceiptPurchaseOrderItems);


        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(res.Message);
        

        var modelfin = new ReceiptPurchaseOrderItemDTO
        {
            ReceiptPurchaseOrderId = res.Data.ReceiptPurchaseOrderId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            ReceiptPurchaseOrderItemId = res.Data.ReceiptPurchaseOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage
        };

        return GeneralResponse<ReceiptPurchaseOrderItemDTO>.SuccessResponse(modelfin);
    }


    public async Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> UpdateReceiptPurchaseItemAsync(int ReceiptPurchaseItemId,
        UpdateGeneralItemDto dto)
    {

        var res = await baseProcesses.UpdateOrderItemAsync<ReceiptPurchaseOrder, ReceiptPurchaseOrderItem>(
       itemIdFromRoute: ReceiptPurchaseItemId,
       processType: ProcessType.Receipt,
       dto: dto,
       orderSet: _context.ReceiptPurchaseOrders,
       itemSelector: x => x.ReceiptPurchaseOrderItemId == ReceiptPurchaseItemId,
       itemSet: _context.ReceiptPurchaseOrderItems
   );

        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

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

    public async Task<GeneralResponse<ReceiptPurchaseOrderItemDTO>> DeleteReceiptPurchaseItemAsync(int ReceiptPurchaseItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<ReceiptPurchaseOrder, ReceiptPurchaseOrderItem>(
            itemIdFromRoute: ReceiptPurchaseItemId,
            processType: ProcessType.Receipt,
            orderSet: _context.ReceiptPurchaseOrders,
            itemSelector: x => x.ReceiptPurchaseOrderItemId == ReceiptPurchaseItemId,
            itemSet: _context.ReceiptPurchaseOrderItems
        );


        if (!res.Success)
            return GeneralResponse<ReceiptPurchaseOrderItemDTO>.FailResponse(res.Message);


        var entity = res.Data;

        var dto = new ReceiptPurchaseOrderItemDTO
        {
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<ReceiptPurchaseOrderItemDTO>.SuccessResponse(dto);
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
