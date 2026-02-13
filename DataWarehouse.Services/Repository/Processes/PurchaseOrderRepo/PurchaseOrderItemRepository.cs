using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.PurchaseOrderRepo;

public class PurchaseOrderItemRepository : BaseRepository<PurchaseOrderItem>, IPurchaseOrderItemRepository
{
    private readonly IBaseProcessesRepository<PurchaseOrderItem> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IBarCodeOrdersRepository barcodeOrder;

    public PurchaseOrderItemRepository(IBaseProcessesRepository<PurchaseOrderItem> baseProcesses, ISapCache sapCache, DataWarehouseDbContext context, IBarCodeOrdersRepository barcodeOrder) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.sapCache = sapCache;
        this.barcodeOrder = barcodeOrder;
    }

    public async Task<GeneralResponse<IEnumerable<PurchaseOrderItemDTO>>> GetByPurchaseItemByPurchaseOrderIdAsync(int PurchaseOrderId)
    {

        var res = await Query().Where(poi => poi.PurchaseOrderId == PurchaseOrderId).Select(
            e => new PurchaseOrderItemDTO
            {
                ItemId = e.ItemId,

                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                Quantity = e.Quantity,
                PurchaseOrderItemId = e.PurchaseOrderItemId,
                PurchaseOrderId = e.PurchaseOrderId,
                 BarCode = e.BarCode,
                  UnitPrice = e.UnitPrice,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName
        }).ToListAsync();

        return GeneralResponse<IEnumerable<PurchaseOrderItemDTO>>.SuccessResponse(res);
    }
    public async Task<GeneralWithTwoGenericResponse<PagedResult<PurchaseOrderItemDTO>, string>>
 GetByPurchaseItemByPurchaseOrderIdWithPaginationAsync(int purchaseOrderId, string? status, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var purchase = await _context.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId);

        if (purchase == null)
            return GeneralWithTwoGenericResponse<PagedResult<PurchaseOrderItemDTO>, string>
                .FailResponse("purchase not found");

        var query = _context.PurchaseOrderItems
            .AsNoTracking()
            .Where(b => b.PurchaseOrderId == purchaseOrderId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<GeneralItemStatus>(status, true, out var statusEnum))
                return GeneralWithTwoGenericResponse<PagedResult<PurchaseOrderItemDTO>, string>
                    .FailResponse("Invalid status");

            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.PurchaseOrderItemId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new PurchaseOrderItemDTO
            {
                ItemId = e.ItemId,
                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                Quantity = e.Quantity,
                PurchaseOrderItemId = e.PurchaseOrderItemId,
                PurchaseOrderId = e.PurchaseOrderId,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
            })
            .ToListAsync();

        return GeneralWithTwoGenericResponse<PagedResult<PurchaseOrderItemDTO>, string>
            .SuccessResponse(new PagedResult<PurchaseOrderItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            }, purchase.Status.ToString());
    }


    // create
    public async Task<GeneralResponse<PurchaseOrderItemDTO>> AddPurchaseItemByPurchaseOrderIdAsync(int PurchaseOrderId, bool isBarcode,
           DynamicBarcodesDto? barcodeDto,

           AddGeneralItemDto? dto)
    {

        var res = await baseProcesses.AddOrderItemAsync<PurchaseOrder, PurchaseOrderItem>(
  PurchaseOrderId,
  ProcessType.Purchase,
  isBarcode,
  barcodeDto,
  dto,
   x => x.PurchaseOrderId == PurchaseOrderId,
  _context.PurchaseOrders,
  _context.PurchaseOrderItems);


        if (!res.Success)
            return GeneralResponse<PurchaseOrderItemDTO>.FailResponse(res.Message);

        var modelfin = new PurchaseOrderItemDTO
        {
            PurchaseOrderId = res.Data.PurchaseOrderId,
            Quantity = res.Data.Quantity,
            Status = GetEnumString(res.Data.Status),
            ItemId = res.Data.ItemId,
            PurchaseOrderItemId = res.Data.PurchaseOrderItemId,

        };

        return GeneralResponse<PurchaseOrderItemDTO>.SuccessResponse(modelfin);
    }

    // update
    public async Task<GeneralResponse<PurchaseOrderItemDTO>> UpdatePurchaseItemAsync(int PurchaseItemId,
           UpdateGeneralItemDto dto)
    {

        var res = await baseProcesses.UpdateOrderItemAsync<PurchaseOrderItem>(
        itemIdFromRoute: PurchaseItemId,
        processType: ProcessType.Purchase,
        dto: dto,
        itemSelector: x => x.PurchaseOrderItemId == PurchaseItemId, // أو x => x.SalesOrderItemId == SalesItemId
        itemSet: _context.PurchaseOrderItems
    );

        if (!res.Success)
            return GeneralResponse<PurchaseOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        // 4️⃣ Map to DTO
        var result = new PurchaseOrderItemDTO
        {

            PurchaseOrderId = entity.PurchaseOrderId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            PurchaseOrderItemId = entity.PurchaseOrderItemId,
             BarCode = entity.BarCode,
              UoMEntry = entity.UoMEntry
              ,
               ErrorMessage = entity.ErrorMessage
                , 
                UnitPrice = entity.UnitPrice    

        };

        return GeneralResponse<PurchaseOrderItemDTO>.SuccessResponse(result);
    }
    public async Task<GeneralResponse<PurchaseOrderItemDTO>> DeletePurchaseItemAsync(int PurchaseItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<PurchaseOrderItem>(
            itemIdFromRoute: PurchaseItemId,
            processType: ProcessType.Sales,
            itemSelector: x => x.PurchaseOrderItemId == PurchaseItemId,
            itemSet: _context.PurchaseOrderItems
        );


        if (!res.Success)
            return GeneralResponse<PurchaseOrderItemDTO>.FailResponse(res.Message);


        var entity = res.Data;

        // 4️⃣ Map to DTO
        var result = new PurchaseOrderItemDTO
        {

            PurchaseOrderId = entity.PurchaseOrderId,
            Quantity = entity.Quantity,
            Status = GetEnumString(entity.Status),
            ItemId = entity.ItemId,
            PurchaseOrderItemId = entity.PurchaseOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry
              ,
            ErrorMessage = entity.ErrorMessage
                ,
            UnitPrice = entity.UnitPrice

        };

        return GeneralResponse<PurchaseOrderItemDTO>.SuccessResponse(result);
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
            case GeneralItemStatus.Closed:
                return "Closed";
            case GeneralItemStatus.Failed:
                return "Failed";
            default:
                return "Unknown";
        }
    }



}
