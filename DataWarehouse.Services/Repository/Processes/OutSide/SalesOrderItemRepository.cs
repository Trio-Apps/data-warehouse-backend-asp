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
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesOrderItemRepository : BaseRepository<SalesOrderItem>, ISalesOrderItemRepository
{
    private readonly IBaseProcessesRepository<SalesOrderItem> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public SalesOrderItemRepository(IBaseProcessesRepository<SalesOrderItem> baseProcesses, ISapCache sapCache, DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<SalesOrderItemDTO>>> GetBySalesItemBySalesOrderIdAsync(int SalesOrderId)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdAsync<SalesOrder, SalesOrderItem, SalesOrderItemDTO>(
            orderId: SalesOrderId,
            orderIdSelector: x => x.SalesOrderId == SalesOrderId,
            orderSet: _context.SalesOrders,
            itemSet: _context.SalesOrderItems,
            itemFilter: soi => soi.SalesOrderId == SalesOrderId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: e => new SalesOrderItemDTO
            {
                ItemId = e.ItemId,
                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                Quantity = e.Quantity,
                SalesOrderItemId = e.SalesOrderItemId,
                SalesOrderId = e.SalesOrderId,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                ExecuteQuantity = _context.DeliveryNoteItems
                    .Where(d => d.SalesOrderItemId == e.SalesOrderItemId)
                    .Select(d => (decimal?)d.Quantity)
                    .Sum() ?? 0
            }
        );

        return res;
    }


    public async Task<GeneralResponse<PagedResult<SalesOrderItemDTO>>>
     GetBySalesItemBySalesOrderIdWithPaginationAsync(int SalesOrderId, string? status, int pageNumber, int pageSize)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdWithPaginationAsync<
            SalesOrder,
            SalesOrderItem,
            SalesOrderItemDTO,
            string,
            GeneralItemStatus>(
            orderId: SalesOrderId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            status: status,
            extraSelector: o => o.Status.ToString(),
            orderIdSelector: o => o.SalesOrderId == SalesOrderId,
            orderSet: _context.SalesOrders,
            itemSet: _context.SalesOrderItems,
            itemFilter: i => i.SalesOrderId == SalesOrderId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: e => new SalesOrderItemDTO
            {
                ItemId = e.ItemId,
                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                Quantity = e.Quantity,
                SalesOrderItemId = e.SalesOrderItemId,
                SalesOrderId = e.SalesOrderId,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                ExecuteQuantity = _context.DeliveryNoteItems
                    .Where(d => d.SalesOrderItemId == e.SalesOrderItemId)
                    .Select(d => (decimal?)d.Quantity)
                    .Sum() ?? 0
            },
            orderByDescSelector: x => x.SalesOrderItemId,
            itemStatusSelector: x => x.Status
        );

        if (!res.Success)
            return GeneralResponse<PagedResult<SalesOrderItemDTO>>.FailResponse(res.Message);

        return GeneralResponse<PagedResult<SalesOrderItemDTO>>.SuccessResponse(res.Data);
    }

 

    public async Task<GeneralResponse<SalesOrderItemDTO>> AddSalesItemBySalesOrderIdAsync(int SalesOrderid, bool isBarcode,
           DynamicBarcodesDto? barcodeDto,
           AddGeneralItemDto? dto)
    {

        var res =   await baseProcesses.AddOrderItemAsync<SalesOrder, SalesOrderItem>(
 SalesOrderid,
 ProcessType.Sales,
 isBarcode,
 barcodeDto,
 dto,
  x =>x.SalesOrderId == SalesOrderid,
 _context.SalesOrders,
 _context.SalesOrderItems);
       

        if (!res.Success)
            return GeneralResponse<SalesOrderItemDTO>.FailResponse(res.Message);

        var mapping = new SalesOrderItemDTO
        {
            SalesOrderId = res.Data.SalesOrderId,
            Quantity = res.Data.Quantity,
            Status = GetEnumString(res.Data.Status),
            ItemId = res.Data.ItemId,
            SalesOrderItemId = res.Data.SalesOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            VatPercent = res.Data.VatPercent,
            VatAmount = res.Data.VatAmount,
            LineTotalBeforeVat = res.Data.LineTotalBeforeVat,
            LineTotalAfterVat = res.Data.LineTotalAfterVat,
            ErrorMessage = res.Data.ErrorMessage
        };

        return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(mapping);

    }


    public async Task<GeneralResponse<SalesOrderItemDTO>> UpdateSalesItemAsync(int SalesItemId,
          UpdateGeneralItemDto dto)
    {

        var res = await baseProcesses.UpdateOrderItemAsync<SalesOrder, SalesOrderItem>(
       itemIdFromRoute: SalesItemId,
       processType: ProcessType.Sales,
       dto: dto,
       orderSet: _context.SalesOrders,
       itemSelector: x => x.SalesOrderItemId == SalesItemId, // أو x => x.SalesOrderItemId == SalesItemId
       itemSet: _context.SalesOrderItems
       );

        if (!res.Success)
            return GeneralResponse<SalesOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

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


    public async Task<GeneralResponse<SalesOrderItemDTO>> DeleteSalesItemAsync(int SalesItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<SalesOrder, SalesOrderItem>(
            itemIdFromRoute: SalesItemId,
            processType: ProcessType.Sales,
            orderSet: _context.SalesOrders,
            itemSelector: x => x.SalesOrderItemId == SalesItemId,
            itemSet: _context.SalesOrderItems
        );


        if (!res.Success)
            return GeneralResponse<SalesOrderItemDTO>.FailResponse(res.Message);


        var entity = res.Data;

        var dto = new SalesOrderItemDTO
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

        return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(dto);
    }


    // update
    //public async Task<GeneralResponse<SalesOrderItemDTO>> UpdateSalesItemAsync(int SalesItemId,
    //       UpdateSalesOrderItemDTO dto)
    //{
    //    var entity = await _context.SalesOrderItems.FirstOrDefaultAsync(e => e.SalesOrderItemId == dto.SalesOrderItemId);
    //    if (entity == null)
    //        return GeneralResponse<SalesOrderItemDTO>.FailResponse("id is not found");
    //    if (entity.SalesOrderItemId != SalesItemId)
    //    {
    //        return GeneralResponse<SalesOrderItemDTO>.FailResponse("id not equal Sales order id!");
    //    }

    //    var checkApprovalStatus = await GetProcessItem(entity.SalesOrderId, ProcessType.Sales);

    //    if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
    //        return GeneralResponse<SalesOrderItemDTO>.FailResponse("You cannot edit any item because its approval status is 'Approved' and all approval steps have been completed.");


    //    var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == entity.ItemId);

    //    if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
    //    {
    //        entity.Quantity = dto.Quantity.Value;
    //    }

    //    if (dto.UoMEntry > 0)
    //    {
    //        entity.UoMEntry = dto.UoMEntry;
    //    }

    //    await _context.SaveChangesAsync();

    //    var result = new SalesOrderItemDTO
    //    {
    //        SalesOrderId = entity.SalesOrderId,
    //        Quantity = entity.Quantity,
    //        Status = GetEnumString(entity.Status),
    //        ItemId = entity.ItemId,
    //        SalesOrderItemId = entity.SalesOrderItemId,
    //        BarCode = entity.BarCode,
    //        UoMEntry = entity.UoMEntry,
    //        ErrorMessage = entity.ErrorMessage,
    //        UnitPrice = entity.UnitPrice
    //    };

    //    return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(result);
    //}


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
    // create
    //public async Task<GeneralResponse<SalesOrderItemDTO>> AddSalesItemBySalesOrderIdAsync(int SalesOrderid, bool isBarcode,
    //       DynamicBarcodesDto? barcodeDto,
    //       AddSalesOrderItemDTO? dto)
    //{
    //    var model = new SalesOrderItem();
    //    var entity = await _context.SalesOrders.FirstOrDefaultAsync(e => e.SalesOrderId == SalesOrderid);


    //    if (entity == null)
    //        return GeneralResponse<SalesOrderItemDTO>.FailResponse("id is not found");

    //    var checkApprovalStatus = await GetProcessItem(entity.SalesOrderId, ProcessType.Sales);

    //    if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
    //        return GeneralResponse<SalesOrderItemDTO>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");


    //    if (isBarcode)
    //    {
    //        var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);
    //        var item = new ItemByBarCodeDto();
    //        if (isDynamic)
    //        {
    //            var resD = await barcodeOrder.GetItemByDynamicBarCodeAsync(entity.WarehouseId, barcodeDto);

    //            if (!resD.Success)
    //                return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);


    //            item = resD.Data;

    //            if (resD.Data == null)
    //                return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);
    //        }
    //        else
    //        {
    //            var resD = await barcodeOrder.GetItemByStaticBarCodeAsync(entity.WarehouseId, barcodeDto);
    //            if (!resD.Success)
    //                return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);


    //            item = resD.Data;
    //            if (resD.Data == null)
    //                return GeneralResponse<SalesOrderItemDTO>.FailResponse(resD.Message);
    //        }

    //        model = new SalesOrderItem()
    //        {
    //            Status = GeneralItemStatus.Planned,
    //            SalesOrderId = SalesOrderid,
    //            ItemId = item.Id,
    //            Quantity = item.Quantity,
    //            BarCode = item.Barcode,
    //            UnitPrice = item.Price,
    //            UoMEntry = item.UoMEntry
    //        };
    //    }
    //    else
    //    {
    //        var item = await _context.Items.FirstOrDefaultAsync(e => e.ItemId == dto.ItemId);
    //        model = new SalesOrderItem
    //        {
    //            Status = GeneralItemStatus.Planned,
    //            SalesOrderId = dto.SalesOrderId,
    //            ItemId = dto.ItemId,
    //            Quantity = dto.Quantity,
    //            BarCode = "",
    //            UnitPrice = item.SalesPrice,
    //            UoMEntry = dto.UoMEntry,
    //        };
    //    }

    //    var res = await AddAsync(model);
    //    await SaveChangesAsync();

    //    var modelfin = new SalesOrderItemDTO
    //    {
    //        SalesOrderId = res.SalesOrderId,
    //        Quantity = res.Quantity,
    //        Status = GetEnumString(res.Status),
    //        ItemId = res.ItemId,
    //        SalesOrderItemId = res.SalesOrderItemId,
    //        UoMEntry = res.UoMEntry,
    //        BarCode = res.BarCode,
    //        UnitPrice = res.UnitPrice,
    //VatPercent = res.VatPercent,
    //VatAmount = res.VatAmount,
    //LineTotalBeforeVat = res.LineTotalBeforeVat,
    //LineTotalAfterVat = res.LineTotalAfterVat,
    //        ErrorMessage = res.ErrorMessage
    //    };

    //    return GeneralResponse<SalesOrderItemDTO>.SuccessResponse(modelfin);
    //}

}
