using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ReceivedItemRepository : BaseRepository<ReceivedItem>, IReceivedItemRepository
{
    private readonly IReceivedStockRepository received;
    public ReceivedItemRepository(IReceivedStockRepository received, DataWarehouseDbContext context) : base(context)
    {
        this.received = received;
    }

    public async Task<IEnumerable<ReceivedItemDTO>> GetByReceivedItemByReceivedStockIdAsync(int ReceivedStockId)
    {
        var res = await Query()
            .Where(ri => ri.ReceivedStockId == ReceivedStockId)
            .ToListAsync();

        return res.Select(e => new ReceivedItemDTO
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
            ItemId = e.ItemId
        });
    }

    public async Task<GeneralResponse<PagedResult<ReceivedItemDTO>>> GetByReceivedItemByReceivedStockIdWithPaginationAsync(int ReceivedStockId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceivedItems
            .AsNoTracking()
            .Where(ri => ri.ReceivedStockId == ReceivedStockId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new ReceivedItemDTO
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
            ItemId = e.ItemId
        }).ToList();

        return GeneralResponse<PagedResult<ReceivedItemDTO>>.SuccessResponse(
            new PagedResult<ReceivedItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<ReceivedItemDTO>> AddReceivedItemByTransferredItemIdAsync(
        string userId,
        int transferredStockid,
        AddReceivedItemDTO dto)
    {
       


        var receivedStock = await _context.TransferredStocks
           .FirstOrDefaultAsync(gro => gro.TransferredStockId == transferredStockid);


        if (receivedStock.ReceivedStock == null)
        {
            var modelGood = new AddReceivedStockDTO
            {
                 TransferredStockId = transferredStockid,
                Comment = dto.Comment,
            };
            var addReceivedOrder = await received.AddReceivedStockByTransferredStockIdAsync(userId, modelGood);
        }




         receivedStock = await _context.TransferredStocks
           .FirstOrDefaultAsync(gro => gro.TransferredStockId == transferredStockid);



        // Get TransferredItem with its batches
        var transferredItem = await _context.TransferredItems
            .Include(ti => ti.TransferredStockBatches)
            .Include(ti => ti.Item)
            .FirstOrDefaultAsync(ti => ti.TransferredItemId == dto.TransferredItemId);

        if (transferredItem == null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Transferred Item not found");

        // Check if this transferred item already has a received item
        var existingReceivedItem = await _context.ReceivedItems
            .FirstOrDefaultAsync(ri => ri.TransferredItemId == dto.TransferredItemId);

        if (existingReceivedItem != null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("This Transferred Item already has a received item");

        // Validate quantity doesn't exceed transferred item quantity
        if (dto.Quantity > transferredItem.Quantity)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received quantity cannot exceed transferred quantity");

        // Create ReceivedItem
        var receivedItem = new ReceivedItem
        {
            ReceivedStockId = receivedStock.ReceivedStock.ReceivedStockId,
            TransferredItemId = dto.TransferredItemId,
            ItemId = transferredItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = transferredItem.UoMEntry,
            BarCode = transferredItem.BarCode,
            UnitPrice = transferredItem.UnitPrice,
            Status = GeneralItemStatus.Planned,
            Comment = dto.Comment
        };

        var res = await AddAsync(receivedItem);
        await SaveChangesAsync();

        // Automatically add batches from TransferredStockBatch
        if (transferredItem.TransferredStockBatches != null && transferredItem.TransferredStockBatches.Any())
        {
            var batchesToAdd = new List<ReceivedStockBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var transferredBatch in transferredItem.TransferredStockBatches.OrderBy(b => b.CreatedAt))
            {
                if (remainingQuantity <= 0)
                    break;

                decimal batchQuantity = remainingQuantity > transferredBatch.Quantity ? transferredBatch.Quantity : remainingQuantity;

                var receivedBatch = new ReceivedStockBatch
                {
                    ReceivedItemId = res.ReceivedItemId,
                    TransferredStockBatchId = transferredBatch.TransferredStockBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = transferredBatch.BatchNumber,
                    ExpiryDate = transferredBatch.ExpiryDate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                batchesToAdd.Add(receivedBatch);
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
            .Include(ri => ri.ReceivedStockBatches)
            .FirstOrDefaultAsync(ri => ri.ReceivedItemId == res.ReceivedItemId);

        var model = new ReceivedItemDTO
        {
            ReceivedItemId = finalItem.ReceivedItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            ErrorMessage = finalItem.ErrorMessage,
            Status = finalItem.Status.ToString(),
            Comment = finalItem.Comment,
            ReceivedStockId = finalItem.ReceivedStockId,
            TransferredItemId = finalItem.TransferredItemId,
            ItemId = finalItem.ItemId,
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
        int ReceivedItemId,
        UpdateReceivedItemDTO dto)
    {
        var entity = await _context.ReceivedItems
            .Include(ri => ri.TransferredItem)
            .FirstOrDefaultAsync(e => e.ReceivedItemId == ReceivedItemId);

        if (entity == null)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received Item not found");

        if (entity.ReceivedItemId != ReceivedItemId)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed transferred item quantity
        if (dto.Quantity.HasValue && dto.Quantity.Value > entity.TransferredItem.Quantity)
            return GeneralResponse<ReceivedItemDTO>.FailResponse("Received quantity cannot exceed transferred quantity");

        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
            entity.Quantity = dto.Quantity.Value;

        if (!string.IsNullOrWhiteSpace(dto.Comment))
            entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new ReceivedItemDTO
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
            ItemId = entity.ItemId
        };

        return GeneralResponse<ReceivedItemDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<ReceivedItem>> GetByReceivedStockIdEntitiesAsync(int receivedStockId)
    {
        return await Query().Where(ri => ri.ReceivedStockId == receivedStockId).ToListAsync();
    }

    public async Task<IEnumerable<ReceivedItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(ri => ri.ItemId == itemId).ToListAsync();
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
}
