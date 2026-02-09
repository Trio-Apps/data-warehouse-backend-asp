using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class GoodsReturnOrderRepository : BaseRepository<GoodsReturnOrder>, IGoodsReturnOrderRepository
{
    public GoodsReturnOrderRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GoodsReturnOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(gro => gro.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<GoodsReturnOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.GoodsReturnOrders
            .AsNoTracking()
            .Where(gro => gro.WarehouseId == warehouseId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(gro => new GoodsReturnOrderDTO
            {
                GoodsReturnOrderId = gro.GoodsReturnOrderId,
                UserId = gro.UserId,
                WarehouseId = warehouseId,
                SupplierId = gro.SupplierId,
                ReceiptPurchaseOrderId = gro.ReceiptPurchaseOrderId,
                WarehouseCode = gro.Warehouse.WarehouseCode,
                SupplierName = gro.Supplier.SupplierName
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<GoodsReturnOrderDTO>>.SuccessResponse(
            new PagedResult<GoodsReturnOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderByReceiptPurchaseOrderIdAsync(string userId, AddGoodsReturnOrderDTO dto)
    {
        var receiptPurchaseOrder = await _context.ReceiptPurchaseOrders
            .Include(rpo => rpo.GoodsReturnOrder)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (receiptPurchaseOrder == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Receipt Purchase Order is not found");

        if (receiptPurchaseOrder.GoodsReturnOrder != null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Receipt Purchase Order already has a Goods Return Order!");

        var goodsReturnOrder = new GoodsReturnOrder
        {
            UserId = userId,
            WarehouseId = receiptPurchaseOrder.WarehouseId,
            SupplierId = receiptPurchaseOrder.SupplierId,
            ReceiptPurchaseOrderId = dto.ReceiptPurchaseOrderId,
             Status = GeneralStatus.Draft,
              CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(goodsReturnOrder);
        await SaveChangesAsync();

        var model = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = res.GoodsReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> UpdateGoodsReturnOrderAsync(string userId, int goodsReturnOrderId, UpdateGoodsReturnOrderDTO dto)
    {
        var entity = await _context.GoodsReturnOrders.FirstOrDefaultAsync(e => e.GoodsReturnOrderId == dto.GoodsReturnOrderId);

        if (entity == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Goods Return Order not found");

        if (entity.GoodsReturnOrderId != goodsReturnOrderId)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("ID mismatch");

        // Update fields if needed
        entity.UserId = userId;

        entity.Comment = dto.Comment;
        entity.Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing;

        await _context.SaveChangesAsync();

        var result = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SupplierId = entity.SupplierId,
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId)
    {
        var res = await _context.GoodsReturnOrders.AsNoTracking()
            .Include(r=>r.ReceiptPurchaseOrder)
            .Include(e => e.Supplier)
            .Include(e => e.GoodsReturnOrderItems)
                .ThenInclude(i => i.Item)
                .ThenInclude(i=> i.ItemUomGroups)
            .Include(e => e.GoodsReturnOrderItems)
                .ThenInclude(i => i.GoodsReturnOrderBatches)
            .FirstOrDefaultAsync(gro => gro.ReceiptPurchaseOrderId == receiptPurchaseOrderId);

        if (res == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Not Found");

        var mapping = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = res.GoodsReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
            SupplierName = res.Supplier?.SupplierName,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            DueDate =res.ReceiptPurchaseOrder.DueDate,
            PostingDate =res.ReceiptPurchaseOrder.PostingDate,
            Status = res.Status.ToString(),


            Items = res.GoodsReturnOrderItems.Select(e => new GoodsReturnOrderItemDTO
            {
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                BarCode = e.BarCode,
                GoodsReturnOrderItemId = e.GoodsReturnOrderItemId,
                Quantity = e.Quantity,
                ErrorMessage = e.ErrorMessage,
                Comment = e.Comment,
                GoodsReturnOrderId = e.GoodsReturnOrderId,
                ReceiptPurchaseOrderItemId = e.ReceiptPurchaseOrderItemId,
                UnitPrice = e.UnitPrice,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,


                Batches = e.GoodsReturnOrderBatches.Select(b => new GoodsReturnOrderBatchDTO
                {
                    ExpiryDate = b.ExpiryDate,
                    Quantity = b.Quantity,
                    GoodsReturnOrderBatchId = b.GoodsReturnOrderBatchId,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                }).ToList(),
            }).ToList()
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<IEnumerable<GoodsReturnOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(gro => gro.UserId == userId).ToListAsync();
    }

    public async Task<GoodsReturnOrder?> GetWithItemsAsync(int goodsReturnOrderId)
    {
        return await QueryIncluding(false, gro => gro.GoodsReturnOrderItems)
            .FirstOrDefaultAsync(gro => gro.GoodsReturnOrderId == goodsReturnOrderId);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> GetWithItemsAndBatchesAsync(int goodsReturnOrderId)
    {
        var result = await _context.GoodsReturnOrders
            .AsNoTracking()
            .Where(g => g.GoodsReturnOrderId == goodsReturnOrderId)
            .Select(g => new GoodsReturnOrderDTO
            {
                GoodsReturnOrderId = g.GoodsReturnOrderId,
                UserId = g.UserId,
                WarehouseId = g.WarehouseId,
                SupplierId = g.SupplierId,
                ReceiptPurchaseOrderId = g.ReceiptPurchaseOrderId,
                WarehouseCode = g.Warehouse.WarehouseCode,
                SupplierName = g.Supplier.SupplierName,
                Items = g.GoodsReturnOrderItems.Select(i => new GoodsReturnOrderItemDTO
                {
                    GoodsReturnOrderItemId = i.GoodsReturnOrderItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    ErrorMessage = i.ErrorMessage,
                    Comment = i.Comment,
                    GoodsReturnOrderId = i.GoodsReturnOrderId,
                    ReceiptPurchaseOrderItemId = i.ReceiptPurchaseOrderItemId,
                    ItemId = i.ItemId,
                    Batches = i.GoodsReturnOrderBatches.Select(b => new GoodsReturnOrderBatchDTO
                    {
                        GoodsReturnOrderBatchId = b.GoodsReturnOrderBatchId,
                        GoodsReturnOrderItemId = b.GoodsReturnOrderItemId,
                        ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                        Quantity = b.Quantity,
                        Comment = b.Comment,
                        BatchNumber = b.BatchNumber,
                        ExpiryDate = b.ExpiryDate
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Not Found");

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<GoodsReturnOrder?> GetWithReceiptPurchaseOrderAsync(int goodsReturnOrderId)
    {
        return await QueryIncluding(false, gro => gro.ReceiptPurchaseOrder)
            .FirstOrDefaultAsync(gro => gro.GoodsReturnOrderId == goodsReturnOrderId);
    }

    public async Task<GoodsReturnOrder?> GetWithWarehouseAsync(int goodsReturnOrderId)
    {
        return await QueryIncluding(false, gro => gro.Warehouse)
            .FirstOrDefaultAsync(gro => gro.GoodsReturnOrderId == goodsReturnOrderId);
    }

}

