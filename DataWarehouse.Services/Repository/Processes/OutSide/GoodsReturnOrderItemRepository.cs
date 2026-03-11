using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class GoodsReturnOrderItemRepository : BaseRepository<GoodsReturnOrderItem>, IGoodsReturnOrderItemRepository
{
    private readonly IBaseProcessesRepository<GoodsReturnOrderItem> baseProcesses;
    private readonly IGoodsReturnOrderRepository goodsReturn;

    public GoodsReturnOrderItemRepository(IBaseProcessesRepository<GoodsReturnOrderItem> baseProcesses, IGoodsReturnOrderRepository goodsReturn, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.goodsReturn = goodsReturn;
    }

    public async Task<GeneralResponse<IEnumerable<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdAsync(int goodsReturnOrderId)
    {
        var res = await Query()
            .Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId).Select(e => new GoodsReturnOrderItemDTO
            {
                GoodsReturnOrderItemId = e.GoodsReturnOrderItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Comment = e.Comment,
                GoodsReturnOrderId = e.GoodsReturnOrderId,
                ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,

            })
            .ToListAsync();

        return GeneralResponse < IEnumerable < GoodsReturnOrderItemDTO >>.SuccessResponse(res);
    }

    public async Task<GeneralResponse<PagedResult<GoodsReturnOrderItemDTO>>> GetByGoodsReturnOrderIdWithPaginationAsync(int goodsReturnOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.GoodsReturnOrderItems
            .AsNoTracking()
            .Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderItemId = e.GoodsReturnOrderItemId,
            Quantity = e.Quantity,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            ErrorMessage = e.ErrorMessage,
            Comment = e.Comment,
            GoodsReturnOrderId = e.GoodsReturnOrderId,
            ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
            ItemId = e.ItemId
        }).ToList();

        return GeneralResponse<PagedResult<GoodsReturnOrderItemDTO>>.SuccessResponse(
            new PagedResult<GoodsReturnOrderItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> AddGoodsReturnItemByGoodsReturnOrderIdWithoutRefAsync(int goodsReturnOrderId,
      bool isBarcode
       , DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<GoodsReturnOrder, GoodsReturnOrderItem>(
    goodsReturnOrderId,
    ProcessType.GoodsReturn,
    isBarcode,
    barcodeDto,
    dto,
     x => x.GoodsReturnOrderId == goodsReturnOrderId,
    _context.GoodsReturnOrders,
    _context.GoodsReturnOrderItems);


        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse(res.Message);


        var modelfin = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderId = res.Data.GoodsReturnOrderId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            GoodsReturnOrderItemId = res.Data.GoodsReturnOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            ErrorMessage = res.Data.ErrorMessage
        };

        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> UpdateGoodsReturnItemWithoutRefAsync(int goodsReturnOrderItemId,
        UpdateGeneralItemDto dto)
    {

        var res = await baseProcesses.UpdateOrderItemAsync<GoodsReturnOrder, GoodsReturnOrderItem>(
       itemIdFromRoute: goodsReturnOrderItemId,
       processType: ProcessType.GoodsReturn,
       dto: dto,
       orderSet: _context.GoodsReturnOrders,
       itemSelector: x => x.GoodsReturnOrderItemId == goodsReturnOrderItemId, // أو x => x.SalesOrderItemId == SalesItemId
       itemSet: _context.GoodsReturnOrderItems
   );

        if (!res.Success)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(result);
    }

    // reference
    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> AddGoodsReturnOrderItemByReceiptPurchaseOrderItemIdAsync(string userId,
        int receiptOrderId,
        AddGoodsReturnOrderItemDTO dto)
    {
        // Validate GoodsReturnOrder exists
        var goodsReturnOrder = await _context.ReceiptPurchaseOrders
            .FirstOrDefaultAsync(gro => gro.ReceiptPurchaseOrderId == receiptOrderId);

        if (goodsReturnOrder == null)
        {
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Receipt Purchase Order not found");
        }

        if (goodsReturnOrder.GoodsReturnOrder == null)
        {
            var modelGood = new AddGoodsReturnOrderModel
            {
                ReceiptPurchaseOrderId = receiptOrderId,
                Comment = dto.Comment,
                
            };
            var addGoodOrder = await goodsReturn.AddGoodsReturnOrderByReceiptPurchaseOrderIdAsync(userId, modelGood);
        }



         goodsReturnOrder = await _context.ReceiptPurchaseOrders
            .FirstOrDefaultAsync(gro => gro.ReceiptPurchaseOrderId == receiptOrderId);


        // Get ReceiptPurchaseOrderItem with its batches
        var receiptPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
            .Include(rpoi => rpoi.ReceiptPurchaseOrderBatches)
            .Include(rpoi => rpoi.Item)
            .FirstOrDefaultAsync(rpoi => rpoi.ReceiptPurchaseOrderItemId == dto.ReceiptPurchaseOrderItemId);

        if (receiptPurchaseOrderItem == null)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Receipt Purchase Order Item not found");

        // Check if this receipt item already has a return item
        var existingReturnItem = await _context.GoodsReturnOrderItems
            .FirstOrDefaultAsync(groi => groi.ReceiptPurchaseOrderItemId == dto.ReceiptPurchaseOrderItemId);

        if (existingReturnItem != null)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("This Receipt Purchase Order Item already has a return item");

        // Validate quantity doesn't exceed receipt item quantity
        if (dto.Quantity > receiptPurchaseOrderItem.Quantity)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed receipt quantity");

        // Create GoodsReturnOrderItem
        var goodsReturnOrderItem = new GoodsReturnOrderItem
        {
            GoodsReturnOrderId = goodsReturnOrder.GoodsReturnOrder.GoodsReturnOrderId,
            ReceiptPurchaseOrderItemId = dto.ReceiptPurchaseOrderItemId,
            ItemId = receiptPurchaseOrderItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = receiptPurchaseOrderItem.UoMEntry,
            BarCode = receiptPurchaseOrderItem.BarCode,
            UnitPrice = receiptPurchaseOrderItem.UnitPrice,
            Comment = dto.Comment
        };

        var res = await AddAsync(goodsReturnOrderItem);
        await SaveChangesAsync();

        // Automatically add batches from ReceiptPurchaseOrderBatch
        if (receiptPurchaseOrderItem.ReceiptPurchaseOrderBatches != null && receiptPurchaseOrderItem.ReceiptPurchaseOrderBatches.Any())
        {
            var batchesToAdd = new List<GoodsReturnOrderBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var receiptBatch in receiptPurchaseOrderItem.ReceiptPurchaseOrderBatches.OrderBy(b => b.CreatedAt))
            {
                //if (remainingQuantity <= 0)
                //    break;

                decimal batchQuantity = remainingQuantity > receiptBatch.Quantity ? receiptBatch.Quantity : remainingQuantity;

                if (remainingQuantity <= 0)
                    batchQuantity = 0;

                 var returnBatch = new GoodsReturnOrderBatch
                {
                    GoodsReturnOrderItemId = res.GoodsReturnOrderItemId,
                    ReceiptPurchaseOrderBatchId = receiptBatch.ReceiptPurchaseOrderBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = receiptBatch.BatchNumber,
                    ExpiryDate = receiptBatch.ExpiryDate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                batchesToAdd.Add(returnBatch);
                remainingQuantity -= batchQuantity;
            }

            if (batchesToAdd.Any())
            {
                await _context.GoodsReturnOrderBatches.AddRangeAsync(batchesToAdd);
                await SaveChangesAsync();
            }
        
        }

        // Reload with batches
        var finalItem = await _context.GoodsReturnOrderItems
            .Include(groi => groi.GoodsReturnOrderBatches)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == res.GoodsReturnOrderItemId);

        var model = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderItemId = finalItem.GoodsReturnOrderItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            ErrorMessage = finalItem.ErrorMessage,
            Comment = finalItem.Comment,
            GoodsReturnOrderId = finalItem.GoodsReturnOrderId,
            ReceiptPurchaseOrderItemId = finalItem.ReceiptPurchaseOrderItemId,
            ItemId = finalItem.ItemId,
            Batches = finalItem.GoodsReturnOrderBatches?.Select(b => new GoodsReturnOrderBatchDTO
            {
                GoodsReturnOrderBatchId = b.GoodsReturnOrderBatchId,
                GoodsReturnOrderItemId = b.GoodsReturnOrderItemId,
                ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };


        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<GoodsReturnOrderItemDTO>> UpdateGoodsReturnOrderItemAsync(
        int goodsReturnOrderItemId,
        UpdateGoodsReturnOrderItemDTO dto)
    {
        var entity = await _context.GoodsReturnOrderItems
            .Include(groi => groi.ReceiptPurchaseOrderItem)
            .FirstOrDefaultAsync(e => e.GoodsReturnOrderItemId == goodsReturnOrderItemId);

        if (entity == null)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Goods Return Order Item not found");

        if (entity.GoodsReturnOrderItemId != goodsReturnOrderItemId)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed receipt item quantity
        if (dto.Quantity.HasValue && dto.Quantity.Value > entity.ReceiptPurchaseOrderItem.Quantity)
            return GeneralResponse<GoodsReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed receipt quantity");

        // بعد تحديث الكمية:
        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
        {
            entity.Quantity = dto.Quantity.Value;

            // 🧹 احذف الباتشات القديمة
            var existingBatches = await _context.GoodsReturnOrderBatches
                .Where(b => b.GoodsReturnOrderItemId == entity.GoodsReturnOrderItemId)
                .ToListAsync();

            if (existingBatches.Any())
            {
                _context.GoodsReturnOrderBatches.RemoveRange(existingBatches);
                await _context.SaveChangesAsync();
            }

            // 🔄 رجّع ReceiptPurchaseOrderItem وباتشاته
            var receiptPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
                .Include(rpoi => rpoi.ReceiptPurchaseOrderBatches)
                .FirstOrDefaultAsync(r => r.ReceiptPurchaseOrderItemId == entity.ReceiptPurchaseOrderItemId);

            // ✅ إعادة بناء الباتشات
            var newBatches = new List<GoodsReturnOrderBatch>();
            decimal remainingQty = dto.Quantity.Value;

            foreach (var receiptBatch in receiptPurchaseOrderItem.ReceiptPurchaseOrderBatches.OrderBy(b => b.CreatedAt))
            {
                //if (remainingQty <= 0)
                //    break;

                decimal batchQty = Math.Min(receiptBatch.Quantity, remainingQty);

                if (batchQty <= 0)
                    batchQty = 0;

                newBatches.Add(new GoodsReturnOrderBatch
                {
                    GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
                    ReceiptPurchaseOrderBatchId = receiptBatch.ReceiptPurchaseOrderBatchId,
                    Quantity = batchQty,
                    BatchNumber = receiptBatch.BatchNumber,
                    ExpiryDate = receiptBatch.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    Comment = dto.Comment
                });

                remainingQty -= batchQty;
            }

            if (newBatches.Any())
            {
                await _context.GoodsReturnOrderBatches.AddRangeAsync(newBatches);
                await _context.SaveChangesAsync();
            }
        }


        if (!string.IsNullOrEmpty(dto.Comment))
            entity.Comment = dto.Comment;

        await _context.SaveChangesAsync();

        var result = new GoodsReturnOrderItemDTO
        {
            GoodsReturnOrderItemId = entity.GoodsReturnOrderItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            Comment = entity.Comment,
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            ReceiptPurchaseOrderItemId = entity.ReceiptPurchaseOrderItemId,
            ItemId = entity.ItemId
        };

        return GeneralResponse<GoodsReturnOrderItemDTO>.SuccessResponse(result);
    }


    public async Task<IEnumerable<GoodsReturnOrderItem>> GetByGoodsReturnOrderIdEntitiesAsync(int goodsReturnOrderId)
    {
        return await Query().Where(groi => groi.GoodsReturnOrderId == goodsReturnOrderId).ToListAsync();
    }


    public async Task<IEnumerable<GoodsReturnOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(groi => groi.ItemId == itemId).ToListAsync();
    }

    public async Task<GoodsReturnOrderItem?> GetWithGoodsReturnOrderAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.GoodsReturnOrder)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }

    public async Task<GoodsReturnOrderItem?> GetWithReceiptPurchaseOrderItemAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.ReceiptPurchaseOrderItem)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }

    public async Task<GoodsReturnOrderItem?> GetWithItemAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.Item)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }

    public async Task<GoodsReturnOrderItem?> GetWithBatchesAsync(int goodsReturnOrderItemId)
    {
        return await QueryIncluding(false, groi => groi.GoodsReturnOrderBatches)
            .FirstOrDefaultAsync(groi => groi.GoodsReturnOrderItemId == goodsReturnOrderItemId);
    }
}

