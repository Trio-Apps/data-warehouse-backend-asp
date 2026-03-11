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

namespace DataWarehouse.Services.Repository.Processes;

public class TransferredRequestItemRepository : BaseRepository<TransferredRequestItem>, ITransferredRequestItemRepository
{
    private readonly IBaseProcessesRepository<TransferredRequestItem> baseProcesses;

    public TransferredRequestItemRepository(
        IBaseProcessesRepository<TransferredRequestItem> baseProcesses,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<TransferredRequestItemDTO>>> GetByTransferredRequestItemByTransferredRequestIdAsync(int transferredRequestId)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdAsync<TransferredRequest, TransferredRequestItem, TransferredRequestItemDTO>(
            orderId: transferredRequestId,
            orderIdSelector: x => x.TransferredRequestId == transferredRequestId,
            orderSet: _context.TransferredRequests,
            itemSet: _context.TransferredRequestItems,
            itemFilter: x => x.TransferredRequestId == transferredRequestId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: e => new TransferredRequestItemDTO
            {
                TransferredRequestItemId = e.TransferredRequestItemId,
                TransferredRequestId = e.TransferredRequestId,
                ItemId = e.ItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                Comment = e.Comment,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault()
            });

        return res;
    }

    public async Task<GeneralResponse<PagedResult<TransferredRequestItemDTO>>> GetByTransferredRequestItemByTransferredRequestIdWithPaginationAsync(
        int transferredRequestId, string? status, int pageNumber, int pageSize)
    {
        var res = await baseProcesses.GetOrderItemsByOrderIdWithPaginationAsync<
            TransferredRequest,
            TransferredRequestItem,
            TransferredRequestItemDTO,
            string,
            GeneralItemStatus>(
            orderId: transferredRequestId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            status: status,
            extraSelector: o => o.Status.ToString(),
            orderIdSelector: o => o.TransferredRequestId == transferredRequestId,
            orderSet: _context.TransferredRequests,
            itemSet: _context.TransferredRequestItems,
            itemFilter: i => i.TransferredRequestId == transferredRequestId,
            include: q => q
                .Include(x => x.Item)
                .ThenInclude(i => i.ItemUomGroups),
            selector: e => new TransferredRequestItemDTO
            {
                TransferredRequestItemId = e.TransferredRequestItemId,
                TransferredRequestId = e.TransferredRequestId,
                ItemId = e.ItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                Comment = e.Comment,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault()
            },
            orderByDescSelector: x => x.TransferredRequestItemId,
            itemStatusSelector: x => x.Status);

        if (!res.Success)
            return GeneralResponse<PagedResult<TransferredRequestItemDTO>>.FailResponse(res.Message);

        return GeneralResponse<PagedResult<TransferredRequestItemDTO>>.SuccessResponse(res.Data);
    }

    public async Task<GeneralResponse<TransferredRequestItemDTO>> AddTransferredRequestItemByTransferredRequestIdAsync(
        int transferredRequestId,
        bool isBarcode,
        DynamicBarcodesDto? dynamicDto,
        AddGeneralItemDto? dto)
    {
      
        var res = await baseProcesses.AddOrderItemAsync<TransferredRequest, TransferredRequestItem>(
            transferredRequestId,
            ProcessType.TransferredRequest,
            isBarcode,
            dynamicDto,
            dto,
            x => x.TransferredRequestId == transferredRequestId,
            _context.TransferredRequests,
            _context.TransferredRequestItems);

        if (!res.Success)
            return GeneralResponse<TransferredRequestItemDTO>.FailResponse(res.Message);

        var mapping = new TransferredRequestItemDTO
        {
            TransferredRequestItemId = res.Data.TransferredRequestItemId,
            TransferredRequestId = res.Data.TransferredRequestId,
            ItemId = res.Data.ItemId,
            Quantity = res.Data.Quantity,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            ErrorMessage = res.Data.ErrorMessage,
            Status = GetEnumString(res.Data.Status),
            Comment = res.Data.Comment
        };

        return GeneralResponse<TransferredRequestItemDTO>.SuccessResponse(mapping);
    }

    public async Task<GeneralResponse<TransferredRequestItemDTO>> UpdateTransferredRequestItemAsync(
        int transferredRequestItemId,
        UpdateGeneralItemDto dto)
    {
       

        var res = await baseProcesses.UpdateOrderItemAsync<TransferredRequest, TransferredRequestItem>(
            itemIdFromRoute: transferredRequestItemId,
            processType: ProcessType.TransferredRequest,
            dto: dto,
            orderSet: _context.TransferredRequests,
            itemSelector: x => x.TransferredRequestItemId == transferredRequestItemId,
            itemSet: _context.TransferredRequestItems);

        if (!res.Success)
            return GeneralResponse<TransferredRequestItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new TransferredRequestItemDTO
        {
            TransferredRequestItemId = entity.TransferredRequestItemId,
            TransferredRequestId = entity.TransferredRequestId,
            ItemId = entity.ItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            Status = GetEnumString(entity.Status),
            Comment = entity.Comment
        };

        return GeneralResponse<TransferredRequestItemDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<TransferredRequestItemDTO>> DeleteTransferredRequestItemAsync(int transferredRequestItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<TransferredRequest, TransferredRequestItem>(
            itemIdFromRoute: transferredRequestItemId,
            processType: ProcessType.TransferredRequest,
            orderSet: _context.TransferredRequests,
            itemSelector: x => x.TransferredRequestItemId == transferredRequestItemId,
            itemSet: _context.TransferredRequestItems);

        if (!res.Success)
            return GeneralResponse<TransferredRequestItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var dto = new TransferredRequestItemDTO
        {
            TransferredRequestItemId = entity.TransferredRequestItemId,
            TransferredRequestId = entity.TransferredRequestId,
            ItemId = entity.ItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            Status = GetEnumString(entity.Status),
            Comment = entity.Comment
        };

        return GeneralResponse<TransferredRequestItemDTO>.SuccessResponse(dto);
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
