using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedItemRepository : BaseRepository<ReceivedItem>, IReceivedItemRepository
{
    private readonly IBaseProcessesRepository<ReceivedItem> baseProcesses;

    public ReceivedItemRepository(
        IBaseProcessesRepository<ReceivedItem> baseProcesses,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
    }

    public async Task<GeneralResponse<IEnumerable<ReceivedItemDTO>>> GetByReceivedStockIdAsync(int receivedStockId)
    {
        var res = await Query()
            .Where(ri => ri.ReceivedStockId == receivedStockId)
            .Select(e => new ReceivedItemDTO
            {
                ReceivedItemId = e.ReceivedItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                Comment = e.Comment,
                ReceivedStockId = e.ReceivedStockId,
                TransferredItemId = e.TransferredItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                IsBatches = e.Item.BatchNumbers,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return GeneralResponse<IEnumerable<ReceivedItemDTO>>.SuccessResponse(res);
    }

    public async Task<IEnumerable<ReceivedItemDTO>> GetByReceivedItemByReceivedStockIdAsync(int receivedStockId)
    {
        var res = await GetByReceivedStockIdAsync(receivedStockId);
        return res.Data ?? Enumerable.Empty<ReceivedItemDTO>();
    }

    public async Task<GeneralResponse<PagedResult<ReceivedItemDTO>>> GetByReceivedStockIdWithPaginationAsync(
        int receivedStockId,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedItems
            .AsNoTracking()
            .Where(ri => ri.ReceivedStockId == receivedStockId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.ReceivedItemId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ReceivedItemDTO
            {
                ReceivedItemId = e.ReceivedItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                Comment = e.Comment,
                ReceivedStockId = e.ReceivedStockId,
                TransferredItemId = e.TransferredItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                IsBatches = e.Item.BatchNumbers
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceivedItemDTO>>.SuccessResponse(
            new PagedResult<ReceivedItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<PagedResult<ReceivedItemDTO>>> GetByReceivedItemByReceivedStockIdWithPaginationAsync(
        int receivedStockId,
        int pageNumber,
        int pageSize)
    {
        return await GetByReceivedStockIdWithPaginationAsync(receivedStockId, pageNumber, pageSize);
    }

    public async Task<GeneralResponse<ReceivedItemDTO>> AddReceivedItemByReceivedStockIdWithoutRefAsync(
        int receivedStockId,
        bool isBarcode,
        DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<ReceivedStock, ReceivedItem>(
              orderId: receivedStockId,
              processType: ProcessType.Received,
              isBarcode: isBarcode,
              barcodeDto: barcodeDto,
              dto: dto,

              orderIdSelector: x => x.ReceivedStockId == receivedStockId,
              orderSet: _context.ReceivedStocks,
              itemSet: _context.ReceivedItems
          );

        if (!res.Success)
            return GeneralResponse<ReceivedItemDTO>.FailResponse(res.Message);

        var model = new ReceivedItemDTO
        {
            ReceivedItemId = res.Data.ReceivedItemId,
            Quantity = res.Data.Quantity,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            ErrorMessage = res.Data.ErrorMessage,
            Status = res.Data.Status.ToString(),
            Comment = res.Data.Comment,
            ReceivedStockId = res.Data.ReceivedStockId,
            TransferredItemId = res.Data.TransferredItemId,
            ItemId = res.Data.ItemId,
        };

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(model);

      
    }

    public async Task<GeneralResponse<ReceivedItemDTO>> UpdateReceivedItemWithoutRefAsync(
        int receivedItemId,
        UpdateGeneralItemDto dto)
    {
        var res = await baseProcesses.UpdateOrderItemAsync<ReceivedStock, ReceivedItem>(
                itemIdFromRoute: receivedItemId,
                processType: ProcessType.DeliveryNote,
                dto: dto,
                orderSet: _context.ReceivedStocks,
                itemSelector: x => x.ReceivedItemId == receivedItemId,
                itemSet: _context.ReceivedItems
            );

        if (!res.Success)
            return GeneralResponse<ReceivedItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new ReceivedItemDTO
        {
             ReceivedStockId = entity.ReceivedStockId,
            ReceivedItemId = entity.ReceivedItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            TransferredItemId = entity.TransferredItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ReceivedItemDTO>> AddReceivedItemByTransferredItemIdAsync(
     string userId,
     int transferredStockId,
     AddReceivedItemDTO dto)
    {
        // Validate ReceivedStock exists
        var receivedStock = await _context.ReceivedStocks
            .FirstOrDefaultAsync(r => r.TransferredStockId == transferredStockId);

        if (receivedStock == null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received Stock not found");

        // Validate TransferredItem exists + load batches + item
        var transferredItem = await _context.TransferredItems
            .Include(i => i.TransferredStockBatches)
            .Include(i => i.Item)
            .FirstOrDefaultAsync(i => i.TransferredItemId == dto.TransferredItemId);

        if (transferredItem == null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Transferred Item not found");

        // Prevent duplicates for the same TransferredItemId in the same ReceivedStock
        var exists = await _context.ReceivedItems
            .AnyAsync(x => x.ReceivedStockId == receivedStock.ReceivedStockId && x.TransferredItemId == dto.TransferredItemId);

        if (exists)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("This Transferred Item already exists in this Received Stock");

        // Validate quantity
        if (dto.Quantity <= 0)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Quantity must be greater than zero");

        if (dto.Quantity > transferredItem.Quantity)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received quantity cannot exceed transferred quantity");

        var receivedItem = new ReceivedItem
        {
            ReceivedStockId = receivedStock.ReceivedStockId,
            TransferredItemId = dto.TransferredItemId,
            ItemId = transferredItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = transferredItem.UoMEntry,
            BarCode = transferredItem.BarCode,
            UnitPrice = transferredItem.UnitPrice,

            // status like your planned logic
            Status = GeneralItemStatus.Planned,
            ErrorMessage = null,
            Comment = dto.Comment ?? transferredItem.Comment,
            LineNum = transferredItem.LineNum
        };

        var res = await AddAsync(receivedItem);
        await SaveChangesAsync();

        // Auto-copy batches
        if (transferredItem.TransferredStockBatches != null && transferredItem.TransferredStockBatches.Any())
        {
            var batchesToAdd = new List<ReceivedStockBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var transferredBatch in transferredItem.TransferredStockBatches.OrderBy(b => b.CreatedAt))
            {
                if (remainingQuantity <= 0)
                    break;

                decimal batchQuantity = remainingQuantity > transferredBatch.Quantity ? transferredBatch.Quantity : remainingQuantity;

                batchesToAdd.Add(new ReceivedStockBatch
                {
                    ReceivedItemId = res.ReceivedItemId,
                    TransferredStockBatchId = transferredBatch.TransferredStockBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = transferredBatch.BatchNumber,
                    ExpiryDate = transferredBatch.ExpiryDate,
                    Comment = dto.Comment ?? transferredBatch.Comment,
                    CreatedAt = DateTime.UtcNow
                });

                remainingQuantity -= batchQuantity;
            }

            if (batchesToAdd.Any())
            {
                await _context.ReceivedStockBatches.AddRangeAsync(batchesToAdd);
                await SaveChangesAsync();
            }
        }

        // Reload with batches
        var finalItem = await _context.ReceivedItems
            .Include(i => i.ReceivedStockBatches)
            .FirstOrDefaultAsync(i => i.ReceivedItemId == res.ReceivedItemId);

        var model = new ReceivedItemDTO
        {
            ReceivedItemId = finalItem!.ReceivedItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            ErrorMessage = finalItem.ErrorMessage,
            ReceivedStockId = finalItem.ReceivedStockId,
            TransferredItemId = finalItem.TransferredItemId,
            ItemId = finalItem.ItemId,
            Comment = finalItem.Comment,

            Batches = finalItem.ReceivedStockBatches?.Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(model);
    }
    public async Task<GeneralResponse<ReceivedItemDTO>> UpdateReceivedItemAsync(
       int receivedItemId,
       UpdateReceivedItemDTO dto)
    {
        var entity = await _context.ReceivedItems
            .Include(i => i.TransferredItem)
                .ThenInclude(ti => ti.TransferredStockBatches)
            .FirstOrDefaultAsync(e => e.ReceivedItemId == receivedItemId);

        if (entity == null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received Item not found");

        if (entity.ReceivedItemId != receivedItemId)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("ID mismatch");

        // Validate quantity with reference (TransferredItem)
        if (entity.TransferredItem != null && dto.Quantity.HasValue)
        {
            if (dto.Quantity.Value <= 0)
                return GeneralResponse<ReceivedItemDTO>.FailResponse("Quantity must be greater than zero");

            if (dto.Quantity.Value > entity.TransferredItem.Quantity)
                return GeneralResponse<ReceivedItemDTO>.FailResponse("Received quantity cannot exceed transferred quantity");
        }

        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
        {
            entity.Quantity = dto.Quantity.Value;

            // remove old batches
            var existingBatches = await _context.ReceivedStockBatches
                .Where(b => b.ReceivedItemId == entity.ReceivedItemId)
                .ToListAsync();

            if (existingBatches.Any())
            {
                _context.ReceivedStockBatches.RemoveRange(existingBatches);
                await _context.SaveChangesAsync();
            }

            // rebuild batches from TransferredStockBatches (if reference exists)
            if (entity.TransferredItem?.TransferredStockBatches != null && entity.TransferredItem.TransferredStockBatches.Any())
            {
                var newBatches = new List<ReceivedStockBatch>();
                decimal remainingQty = dto.Quantity.Value;

                foreach (var transferredBatch in entity.TransferredItem.TransferredStockBatches.OrderBy(b => b.CreatedAt))
                {
                    if (remainingQty <= 0)
                        break;

                    decimal batchQty = Math.Min(transferredBatch.Quantity, remainingQty);

                    newBatches.Add(new ReceivedStockBatch
                    {
                        ReceivedItemId = entity.ReceivedItemId,
                        TransferredStockBatchId = transferredBatch.TransferredStockBatchId,
                        Quantity = batchQty,
                        BatchNumber = transferredBatch.BatchNumber,
                        ExpiryDate = transferredBatch.ExpiryDate,
                        CreatedAt = DateTime.UtcNow,
                        Comment = dto.Comment
                    });

                    remainingQty -= batchQty;
                }

                if (newBatches.Any())
                {
                    await _context.ReceivedStockBatches.AddRangeAsync(newBatches);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // update optional comment on batches only (item comment field may not exist)
        await _context.SaveChangesAsync();

        var result = new ReceivedItemDTO
        {
            ReceivedItemId = entity.ReceivedItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            ReceivedStockId = entity.ReceivedStockId,
            TransferredItemId = entity.TransferredItemId,
            ItemId = entity.ItemId
        };

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(result);
    }
    public async Task<GeneralResponse<ReceivedItemDTO>> DeleteReceivedItemAsync(int receivedItemId)
    {
        var res = await baseProcesses.DeleteOrderItemAsync<ReceivedStock, ReceivedItem>(
            itemIdFromRoute: receivedItemId,
            processType: ProcessType.Received,
            orderSet: _context.ReceivedStocks,
            itemSelector: x => x.ReceivedItemId == receivedItemId,
            itemSet: _context.ReceivedItems);

        if (!res.Success)
            return GeneralResponse<ReceivedItemDTO>.FailResponse(res.Message);

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(MapItemToDto(res.Data));
    }

    public async Task<IEnumerable<ReceivedItem>> GetByReceivedStockIdEntitiesAsync(int receivedStockId)
    {
        return await Query()
            .Where(ri => ri.ReceivedStockId == receivedStockId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReceivedItem>> GetByItemIdAsync(int itemId)
    {
        return await Query()
            .Where(ri => ri.ItemId == itemId)
            .ToListAsync();
    }

    public async Task<ReceivedItem?> GetWithReceivedStockAsync(int receivedItemId)
    {
        return await QueryIncluding(false, ri => ri.ReceivedStock)
            .FirstOrDefaultAsync(ri => ri.ReceivedItemId == receivedItemId);
    }

    public async Task<ReceivedItem?> GetWithTransferredItemAsync(int receivedItemId)
    {
        return await QueryIncluding(false, ri => ri.TransferredItem)
            .FirstOrDefaultAsync(ri => ri.ReceivedItemId == receivedItemId);
    }

    public async Task<ReceivedItem?> GetWithItemAsync(int receivedItemId)
    {
        return await QueryIncluding(false, ri => ri.Item)
            .FirstOrDefaultAsync(ri => ri.ReceivedItemId == receivedItemId);
    }

    public async Task<ReceivedItem?> GetWithBatchesAsync(int receivedItemId)
    {
        return await QueryIncluding(false, ri => ri.ReceivedStockBatches)
            .FirstOrDefaultAsync(ri => ri.ReceivedItemId == receivedItemId);
    }

    private async Task<int> EnsureReceivedStockExistsAsync(string userId, TransferredStock transferredStock, string? comment)
    {
        if (transferredStock.ReceivedStock != null)
            return transferredStock.ReceivedStock.ReceivedStockId;

        var receivedStock = new ReceivedStock
        {
            UserId = userId,
            WarehouseId = transferredStock.DistinationWarehouseId,
            SourceWarehouseId = transferredStock.WarehouseId,
            TransferredStockId = transferredStock.TransferredStockId,
            DueDate = transferredStock.DueDate,
            Comment = comment,
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ReceivedStocks.AddAsync(receivedStock);
        await _context.SaveChangesAsync();

        return receivedStock.ReceivedStockId;
    }

    private static ReceivedItemDTO MapItemToDto(ReceivedItem entity)
    {
        return new ReceivedItemDTO
        {
            ReceivedItemId = entity.ReceivedItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString(),
            Comment = entity.Comment,
            ReceivedStockId = entity.ReceivedStockId,
            TransferredItemId = entity.TransferredItemId,
            ItemId = entity.ItemId,
            Batches = entity.ReceivedStockBatches?.Select(b => new ReceivedStockBatchDTO
            {
                ReceivedStockBatchId = b.ReceivedStockBatchId,
                ReceivedItemId = b.ReceivedItemId,
                TransferredStockBatchId = b.TransferredStockBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };
    }
}
