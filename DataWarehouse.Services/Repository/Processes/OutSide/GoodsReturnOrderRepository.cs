using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class GoodsReturnOrderRepository : BaseRepository<GoodsReturnOrder>, IGoodsReturnOrderRepository
{
    private readonly IProcessItemIsProgressRepository progress;
    private readonly IApprovalRepository approval;
    public GoodsReturnOrderRepository(IProcessItemIsProgressRepository progress, IApprovalRepository approval, DataWarehouseDbContext context) : base(context)
    {
        this.progress = progress;
        this.approval = approval;
    }

    public async Task<IEnumerable<GoodsReturnOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(gro => gro.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<GoodsReturnOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
    (int warehouseId, string userId, int? supplierId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.GoodsReturnOrders
            .AsNoTracking()
            .Where(gro => gro.WarehouseId == warehouseId);


        if (supplierId.HasValue)
        {
            query = query.Where(e => e.SupplierId == supplierId);
        }
        // 🔹 Filtering
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            {
                query = query.Where(e => e.Status == statusEnum);
            }
        }
        // 🔹 Posting Date Filter
        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(e => e.PostingDate.Date == postDate);
        }
        // 🔹 Due Date Filter
        if (DueDate.HasValue)
        {
            var dueDate = DueDate.Value.Date;
            query = query.Where(e => e.DueDate.Date == dueDate);
        }



        query = query.OrderByDescending(e => e.CreatedAt); // تأكد هنا

        var totalRecords = await query.CountAsync();

        var processQuery = _context.ProcessItemIsProgresses
        .AsNoTracking()
        .Where(p => p.ProcessType == ProcessType.GoodsReturn);

        var data = await query
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(iw => new
         {
             Order = iw,
             // هل فيه progress أصلاً؟
             HasProgress = processQuery.Any(p => p.ReferenceId == iw.GoodsReturnOrderId),
             // آخر Status (لو موجود)
             LatestStatus = processQuery
                 .Where(p => p.ReferenceId == iw.GoodsReturnOrderId)
                 .OrderByDescending(p => p.ProcessItemIsProgressId) // أو CreatedAt لو عندك
                 .Select(p => (ProcessStatus?)p.Status)
                 .FirstOrDefault()
         })
            .Select(x => new GoodsReturnOrderDTO
            {
                GoodsReturnOrderId = x.Order.GoodsReturnOrderId,
                DueDate = x.Order.DueDate,
                PostingDate = x.Order.PostingDate,
                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,
                ErrorMessage = x.Order.ErrorMessage,

                SupplierName = x.Order.Supplier.SupplierName,
                SupplierCode = x.Order.Supplier.SupplierCode,
                ItemCount = x.Order.GoodsReturnOrderItems.Count(),

                // ✅ وجود progress
                Approval = x.HasProgress,

                // ✅ اسم الحالة الحالية (آخر Status)
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,

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
    
    public async Task<GeneralResponse<GoodsReturnOrderDTO>> GetGoodsReturnOrderByIdAsync(string userId, int goodsReturnOrderId)
    {
        var res = await _context.GoodsReturnOrders
            .Include(g => g.Supplier)
            .FirstOrDefaultAsync(rpo => rpo.GoodsReturnOrderId == goodsReturnOrderId);

        if (res == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.GoodsReturn, res.GoodsReturnOrderId);


        var mapping = new GoodsReturnOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            GoodsReturnOrderId = res.GoodsReturnOrderId,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
            Comment = res.Comment,
            SupplierName = res.Supplier.SupplierName,
            SupplierCode = res.Supplier.SupplierCode,
            ErrorMessage = res.ErrorMessage,
            CreatedAt = res.CreatedAt,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,



        };
        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(mapping);
    }

    // without reference
    public async Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderWithoutRefAsync(string userId, AddGoodsReturnOrderWithoutRefDTO dto)
    {
       
        var suppler = await _context.Suppliers.FirstOrDefaultAsync(p => p.SupplierId == dto.SupplierId);

        if (suppler == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("suppler is not found");



        var goodsReturnOrder = new GoodsReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            SupplierId = dto.SupplierId,
        };


        var res = await AddAsync(goodsReturnOrder);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.GoodsReturn,
                referenceId: res.GoodsReturnOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }
        var model = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = res.GoodsReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(model);
    }

    // ref
    public async Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderAsync(string userId, AddGoodsReturnOrderDTO dto)
    {

        var receiptOrder = await _context.ReceiptPurchaseOrders.FirstOrDefaultAsync(p => p.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (receiptOrder == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("purchaseOrder is not found");

        if (receiptOrder.Status != GeneralStatus.Processing)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("You can add Receipt, if purchase order status is completed only");

        if (receiptOrder.GoodsReturnOrder != null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("purchaseOrder has receipt purchaseOrder already!");


        var goodsReturnOrder = new GoodsReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = receiptOrder.WarehouseId,
            Comment = dto.Comment,
            SupplierId = receiptOrder.SupplierId,
            ReceiptPurchaseOrderId = dto.ReceiptPurchaseOrderId
        };


        var res = await AddAsync(goodsReturnOrder);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.GoodsReturn,
                referenceId: res.GoodsReturnOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }
        var model = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = res.GoodsReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderAndItemsByReceiptOrderAsync(
    string userId,
    AddGoodsReturnOrderDTO dto)
    {
        var receiptOrder = await _context.ReceiptPurchaseOrders
            .Include(p => p.GoodsReturnOrder)
            .Include(p => p.ReceiptPurchaseOrderItems) // ✅ هات items
            .FirstOrDefaultAsync(p => p.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (receiptOrder == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("purchaseOrder is not found");

        if (receiptOrder.Status != GeneralStatus.Processing)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("You can add Goods Return, if receipt order status is completed only");

        if (receiptOrder.GoodsReturnOrder != null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("purchaseOrder has receipt purchaseOrder already!");

        if (receiptOrder.ReceiptPurchaseOrderItems == null || !receiptOrder.ReceiptPurchaseOrderItems.Any())
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("purchaseOrder has no items to receipt");

        var returnOrder = new GoodsReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,

            PostingDate = dto.PostingDate,

            DueDate = dto.DueDate,


            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = receiptOrder.WarehouseId,
            Comment = dto.Comment,
            SupplierId = receiptOrder.SupplierId,
            ReceiptPurchaseOrderId = dto.ReceiptPurchaseOrderId,

            // ✅ Copy PO items -> Receipt PO items
            GoodsReturnOrderItems = receiptOrder.ReceiptPurchaseOrderItems.Select(poi => new GoodsReturnOrderItem
            {
                ReceiptPurchaseOrderItemId = poi.ReceiptPurchaseOrderItemId,
                ItemId = poi.ItemId,
                Quantity = poi.Quantity,
                UoMEntry = poi.UoMEntry,
                BarCode = poi.BarCode,
                UnitPrice = poi.UnitPrice,

                // منطق الستاتس: لسه الاستلام ما تمش
                Status = GeneralItemStatus.Planned,

                // optional
                ErrorMessage = null,
                Comment = null
            }).ToList()
        };

        await _context.GoodsReturnOrders.AddAsync(returnOrder);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.GoodsReturn,
                referenceId: returnOrder.GoodsReturnOrderId,
                warehouseId: returnOrder.WarehouseId,
                userId: userId
            );
        }

        var model = new GoodsReturnOrderDTO
        {
            ReceiptPurchaseOrderId = returnOrder.ReceiptPurchaseOrderId,
            DueDate = returnOrder.DueDate,
            PostingDate = returnOrder.PostingDate,
            Status = returnOrder.Status.ToString(),
            UserId = returnOrder.UserId,
            WarehouseId = returnOrder.WarehouseId,
            GoodsReturnOrderId = returnOrder.GoodsReturnOrderId,
            SupplierId = returnOrder.SupplierId,
            Comment = returnOrder.Comment
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> UpdateGoodsReturnOrderAsync(string userId, int goodsReturnOrderId, UpdateGoodsReturnOrderDTO dto)
    {
        var entity = await _context.GoodsReturnOrders.FirstOrDefaultAsync(e => e.GoodsReturnOrderId == goodsReturnOrderId);

        if (entity == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Goods Return Order not found");

        if (entity.GoodsReturnOrderId != goodsReturnOrderId)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("ID mismatch");


        var checkApprovalStatus = await GetProcessItem(entity.GoodsReturnOrderId, ProcessType.GoodsReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("You cannot edit any sales return order because its approval status is 'Approved' and all approval steps have been completed.");


        if (entity.ReceiptPurchaseOrderId != null)
        {
            if (dto.SupplierId != null)
            {
                return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("You cannot edit on supplier, because supplier is based on Receipt Purchase order.");

            }
        }

        // Update fields if needed
        entity.UserId = userId;

        if (dto.PostingDate.HasValue)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.SupplierId.HasValue)
            entity.SupplierId = dto.SupplierId.Value;

        entity.Comment = dto.Comment ?? entity.Comment;

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.GoodsReturn,
                referenceId: entity.GoodsReturnOrderId,
                warehouseId: entity.WarehouseId,
                userId: userId
            );

            entity.Status = GeneralStatus.Processing;
        }
        else
        {

            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await SaveChangesAsync();

        var result = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SupplierId = entity.SupplierId,
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            Comment = entity.Comment
        };

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<GoodsReturnOrderDTO>> DeleteGoodsReturnOrderAsync(
      int goodsReturnOrderId,
      CancellationToken cancellationToken = default)
    {
        var entity = await _context.GoodsReturnOrders
            .FirstOrDefaultAsync(e => e.GoodsReturnOrderId == goodsReturnOrderId, cancellationToken);

        if (entity == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(
            entity.GoodsReturnOrderId,
            ProcessType.GoodsReturn,
            cancellationToken);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");

        // Snapshot قبل الحذف علشان نرجعه في الـ response
        var result = new GoodsReturnOrderDTO
        {
            GoodsReturnOrderId = entity.GoodsReturnOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SupplierId = entity.SupplierId,
            Comment = entity.Comment
        };

        _context.GoodsReturnOrders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<GoodsReturnOrderDTO>.SuccessResponse(result);
    }
    
    public async Task<GeneralResponse<GoodsReturnOrderDTO>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId)
    {
        var res = await _context.GoodsReturnOrders.AsNoTracking()
            .Include(r => r.ReceiptPurchaseOrder)
            .Include(e => e.Supplier)
            .Include(e => e.GoodsReturnOrderItems)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.ItemUomGroups)
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
            DueDate = res.ReceiptPurchaseOrder.DueDate,
            PostingDate = res.ReceiptPurchaseOrder.PostingDate,
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

    // inside
    public async Task<GeneralResponse<GoodsReturnOrderDTO>> AddGoodsReturnOrderByReceiptPurchaseOrderIdAsync(string userId, AddGoodsReturnOrderModel dto)
    {
        var receiptPurchaseOrder = await _context.ReceiptPurchaseOrders
            .Include(rpo => rpo.GoodsReturnOrder)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (receiptPurchaseOrder == null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Receipt Purchase Order is not found");

        if (receiptPurchaseOrder.Status != GeneralStatus.Completed)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("You can add Receipt, if purchase order status is completed only");

        if (receiptPurchaseOrder.GoodsReturnOrder != null)
            return GeneralResponse<GoodsReturnOrderDTO>.FailResponse("Receipt Purchase Order already has a Goods Return Order!");

        var goodsReturnOrder = new GoodsReturnOrder
        {
            UserId = userId,
            WarehouseId = receiptPurchaseOrder.WarehouseId,
            SupplierId = receiptPurchaseOrder.SupplierId,
            DueDate = receiptPurchaseOrder.DueDate,
            PostingDate = receiptPurchaseOrder.PostingDate,
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


}