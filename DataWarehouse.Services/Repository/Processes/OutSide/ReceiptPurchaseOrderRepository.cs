using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class ReceiptPurchaseOrderRepository : BaseRepository<ReceiptPurchaseOrder>, IReceiptPurchaseOrderRepository
{
    private readonly IApprovalRepository approval;

    public ReceiptPurchaseOrderRepository(IApprovalRepository approval, DataWarehouseDbContext context) : base(context)
    {
        this.approval = approval;
    }


    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(rpo => rpo.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;


        var query = _context.ReceiptPurchaseOrders
            .AsNoTracking()
            .Where(rpo => rpo.WarehouseId == warehouseId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ReceiptPurchaseOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                ReceiptPurchaseOrderId = iw.ReceiptPurchaseOrderId,
                Status = iw.Status.ToString(),
                UserId = iw.UserId,
                WarehouseId = warehouseId,
                PurchaseOrderId = iw.PurchaseOrderId,
                SupplierId = iw.SupplierId,
                ErrorMessage= iw.ErrorMessage,
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>.SuccessResponse(
            new PagedResult<ReceiptPurchaseOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
       (int warehouseId, string userId, int? supplierId, DateTime? postingDate, DateTime? DueDate, string? liveStatus, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceiptPurchaseOrders
            .AsNoTracking().Include(e => e.ReceiptPurchaseOrderItems)
            .Where(po => po.WarehouseId == warehouseId);

        // 🔹 Filtering


        if (supplierId.HasValue)
        {
            query = query.Where(e => e.SupplierId == supplierId);
        }
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

        if (!string.IsNullOrEmpty(liveStatus))
        {
            // true = receipt 
            // false = return
            
           query = query.Where(e => e.GoodsReturnOrder != null);

        }

        query = query.OrderByDescending(e => e.CreatedAt); // تأكد هنا

        // Approved references subquery (for Sales process only)
        var processQuery = _context.ProcessItemIsProgresses
       .AsNoTracking()
       .Where(p => p.ProcessType == ProcessType.Receipt);

        var totalRecords = await query.CountAsync();

        var data = await query
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
         .Select(iw => new
         {
             Order = iw,

             // هل فيه progress أصلاً؟
             HasProgress = processQuery.Any(p => p.ReferenceId == iw.PurchaseOrderId),

             // آخر Status (لو موجود)
             LatestStatus = processQuery
                 .Where(p => p.ReferenceId == iw.ReceiptPurchaseOrderId)
                 .OrderByDescending(p => p.ProcessItemIsProgressId) // أو CreatedAt لو عندك
                 .Select(p => (ProcessStatus?)p.Status)
                 .FirstOrDefault()
         })
         .Select(x => new ReceiptPurchaseOrderDTO
         {
             DueDate = x.Order.DueDate,
             PostingDate = x.Order.PostingDate,
             PurchaseOrderId = x.Order.PurchaseOrderId,
             Status = x.Order.Status.ToString(),
             Comment = x.Order.Comment,
             UserId = x.Order.UserId,
             WarehouseId = x.Order.WarehouseId,

             SupplierName = x.Order.Supplier.SupplierName,
             ItemCount = x.Order.ReceiptPurchaseOrderItems.Count(),

             // ✅ وجود progx.Orders
             Approval = x.HasProgress,

             // ✅ اسم الحالة الحالية (آخر Status)
             ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,


             ReceiptPurchaseOrderId = x.Order.ReceiptPurchaseOrderId,
           
             CreatedAt = x.Order.CreatedAt,
             SupplierId = x.Order.SupplierId,
             ErrorMessage = x.Order.ErrorMessage,
           
             IsReturn = x.Order.GoodsReturnOrder != null,
             ReturnOrderId = x.Order.GoodsReturnOrder != null ? x.Order.GoodsReturnOrder.GoodsReturnOrderId : null,
         })
         .ToListAsync(cancellationToken);


        return GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>.SuccessResponse(
            new PagedResult<ReceiptPurchaseOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetReceiptOrderByIdAsync(string userId, int receiptOrderId)
    {
        var res = await _context.ReceiptPurchaseOrders.Include(r => r.GoodsReturnOrder)
            .Include(r=>r.Supplier)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptOrderId);

        if (res == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Receipt, res.ReceiptPurchaseOrderId);


        var mapping = new ReceiptPurchaseOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            PurchaseOrderId = res.PurchaseOrderId,
            CreatedAt = res.CreatedAt,
            SupplierId = res.SupplierId,
            SupplierName =res.Supplier.SupplierName,
           SupplierCode = res.Supplier.SupplierCode,
            ErrorMessage = res.ErrorMessage,

            Comment = res.Comment,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,
            IsReturn = res.GoodsReturnOrder != null,
            ReturnOrderId = res.GoodsReturnOrder != null ? res.GoodsReturnOrder.GoodsReturnOrderId : null,
        };
        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(mapping);
    }


   // without reference
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderAsync(string userId, AddReceiptPurchaseOrderWithoutRefDTO dto)
    {

        var suppler = await _context.Suppliers.FirstOrDefaultAsync(p => p.SupplierId == dto.SupplierId);

        if (suppler == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("suppler is not found");



        var goodsReturnOrder = new ReceiptPurchaseOrder
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
                processType: ProcessType.Receipt,
                referenceId: res.ReceiptPurchaseOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }
        var model = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierId = res.SupplierId,
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderByPurchaseOrderIdAsync(string userId, AddReceiptPurchaseOrderDTO dto)
    {

        var purchaseOrder = await _context.PurchaseOrders.Include(p=>p.ReceiptPurchaseOrder).FirstOrDefaultAsync(p=>p.PurchaseOrderId == dto.PurchaseOrderId);

        if (purchaseOrder == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder is not found");

        if (purchaseOrder.Status != GeneralStatus.Completed)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You can add Receipt, if putchase order status is completed only");


        if (purchaseOrder.ReceiptPurchaseOrder != null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder has receipt purchaseOrder already!");


        var mapping = new ReceiptPurchaseOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = purchaseOrder.WarehouseId,
            Comment = dto.Comment,
            SupplierId = purchaseOrder.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Receipt,
                referenceId: res.ReceiptPurchaseOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }

        var model = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            PurchaseOrderId = res.PurchaseOrderId,
            SupplierId = purchaseOrder.SupplierId,
            Comment = res.Comment
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderAndItemsByPurchaseOrderIdAsync(
    string userId,
    AddReceiptPurchaseOrderDTO dto)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(p => p.ReceiptPurchaseOrder)
            .Include(p => p.PurchaseOrderItems) // ✅ هات items
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == dto.PurchaseOrderId);

        if (purchaseOrder == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder is not found");

        if (purchaseOrder.Status != GeneralStatus.Completed)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You can add Receipt, if purchase order status is completed only");

        if (purchaseOrder.ReceiptPurchaseOrder != null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder has receipt purchaseOrder already!");

        if (purchaseOrder.PurchaseOrderItems == null || !purchaseOrder.PurchaseOrderItems.Any())
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder has no items to receipt");

        var receipt = new ReceiptPurchaseOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = purchaseOrder.WarehouseId,
            Comment = dto.Comment,
            SupplierId = purchaseOrder.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId,

            // ✅ Copy PO items -> Receipt PO items
            ReceiptPurchaseOrderItems = purchaseOrder.PurchaseOrderItems.Select(poi => new ReceiptPurchaseOrderItem
            {
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

        await _context.ReceiptPurchaseOrders.AddAsync(receipt);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Receipt,
                referenceId: receipt.ReceiptPurchaseOrderId,
                warehouseId: receipt.WarehouseId,
                userId: userId
            );
        }

        var model = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = receipt.ReceiptPurchaseOrderId,
            DueDate = receipt.DueDate,
            PostingDate = receipt.PostingDate,
            Status = receipt.Status.ToString(),
            UserId = receipt.UserId,
            WarehouseId = receipt.WarehouseId,
            PurchaseOrderId = receipt.PurchaseOrderId,
            SupplierId = receipt.SupplierId,
            Comment = receipt.Comment
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }
    
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> UpdateReceiptPurchaseOrderAsync(string userId, int receiptPurchaseOrderId, UpdateReceiptPurchaseOrderDTO dto)
    {
        var entity = await _context.ReceiptPurchaseOrders.FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

        if (entity.ReceiptPurchaseOrderId != receiptPurchaseOrderId)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("id not equal receipt purchase order id!");
        }
        if (entity == null)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("not found");
        }

        var checkApprovalStatus = await approval.GetProcessItem(entity.ReceiptPurchaseOrderId, ProcessType.Receipt);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");

        if (entity.PurchaseOrderId != null)
        {
            if (dto.SupplierId != null)
            {
                return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You cannot edit on supplier, because supplier is based on purchase order.");

            }
        }
        if (dto.PostingDate.HasValue)
          entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.SupplierId.HasValue)
            entity.SupplierId = dto.SupplierId.Value;

        entity.UserId = userId;
        if (dto.Comment != null)
            entity.Comment = dto.Comment;
        entity.Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing;

        // entity.SupplierId = dto.SupplierId;


        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Receipt,
                referenceId: entity.ReceiptPurchaseOrderId,
                warehouseId: entity.WarehouseId,
                userId: userId
            );

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        await _context.SaveChangesAsync();

        var result = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            PurchaseOrderId = entity.PurchaseOrderId,
            SupplierId = entity.SupplierId,
            Comment = entity.Comment
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }

  
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> DeleteReceiptOrderAsync(
    int receiptOrderId,
    CancellationToken cancellationToken = default)
    {
        var entity = await _context.ReceiptPurchaseOrders
            .FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderId == receiptOrderId, cancellationToken);

        if (entity == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(
            entity.ReceiptPurchaseOrderId,
            ProcessType.Receipt,
            cancellationToken);


        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");


        // Snapshot قبل الحذف علشان نرجعه في الـ response
        var result = new ReceiptPurchaseOrderDTO
        {
            ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SupplierId = entity.SupplierId,
            Comment = entity.Comment
        };

        // لو عندك تفاصيل ومفيش Cascade Delete هتحتاج تمسحها الأول هنا
        _context.ReceiptPurchaseOrders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetByPurchaseOrderIdAsync(string userId, int purchaseOrderId)
    {
        var res = await _context.ReceiptPurchaseOrders.Include(r=>r.GoodsReturnOrder)
            .FirstOrDefaultAsync(rpo => rpo.PurchaseOrderId == purchaseOrderId);

        if (res == null)
           return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Receipt, res.ReceiptPurchaseOrderId);


        var mapping = new ReceiptPurchaseOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            ReceiptPurchaseOrderId = res.ReceiptPurchaseOrderId,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            PurchaseOrderId = res.PurchaseOrderId,
            SupplierId = res.SupplierId,
            Comment= res.Comment,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,
            IsReturn = res.GoodsReturnOrder != null,
            ReturnOrderId = res.GoodsReturnOrder != null ? res.GoodsReturnOrder.GoodsReturnOrderId : null,
        };
        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(po => po.Status == statusEnum)
                .Select(p => new ReceiptPurchaseOrderDTO
                {
                    DueDate = p.DueDate,
                    PostingDate = p.PostingDate,
                    PurchaseOrderId = p.PurchaseOrderId,
                    ReceiptPurchaseOrderId = p.ReceiptPurchaseOrderId,
                    Comment = p.Comment,
                    SupplierId = p.SupplierId,    
                    Status = p.Status.ToString(),
                    // string = enum
                    UserId = p.UserId,

                    WarehouseId = p.WarehouseId,

                }).ToListAsync();

            return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.SuccessResponse(query);

        }
        return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.FailResponse("Not found any purchase to this status now!");
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(rpo => rpo.UserId == userId).ToListAsync();
    }

    public async Task<ReceiptPurchaseOrder?> GetWithItemsAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.ReceiptPurchaseOrderItems)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetWithItemsAndBatchesAsync(int receiptPurchaseOrderId)
    {
        var result = await _context.ReceiptPurchaseOrders
     .AsNoTracking()
     .Where(r => r.ReceiptPurchaseOrderId == receiptPurchaseOrderId)
     .Select(r => new ReceiptPurchaseOrderDTO
     {
         ReceiptPurchaseOrderId = r.ReceiptPurchaseOrderId,
         Status = r.Status.ToString(),
         PostingDate = r.PostingDate,
         DueDate = r.DueDate,
         UserId = r.UserId,
         Comment = r.Comment,
         PurchaseOrderId = r.PurchaseOrderId,
         WarehouseId = r.WarehouseId,
         SupplierId = r.SupplierId,
         SupplierName = r.Supplier.SupplierName,
         WarehouseCode = r.Warehouse.WarehouseCode,

         Items = r.ReceiptPurchaseOrderItems.Select(i => new ReceiptPurchaseOrderItemDTO
         {
             ReceiptPurchaseOrderItemId = i.ReceiptPurchaseOrderItemId,
             Quantity = i.Quantity,
             UoMEntry = i.UoMEntry,
             BarCode = i.BarCode,
             UnitPrice = i.UnitPrice,
             ErrorMessage = i.ErrorMessage,
             Comment = i.Comment,
             ReceiptPurchaseOrderId = i.ReceiptPurchaseOrderId,
             ItemId = i.ItemId,

             Batches = i.ReceiptPurchaseOrderBatches.Select(b => new ReceiptPurchaseOrderBatchDTO
             {
                 ReceiptPurchaseOrderBatchId = b.ReceiptPurchaseOrderBatchId,
                 ReceiptPurchaseOrderItemId = b.ReceiptPurchaseOrderItemId,
                 Quantity = b.Quantity,
                 Comment = b.Comment,
                 BatchNumber = b.BatchNumber,
                 ExpiryDate = b.ExpiryDate
             }).ToList()
         }).ToList()
     })
     .FirstOrDefaultAsync();




        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }


    public async Task<ReceiptPurchaseOrder?> GetWithPurchaseOrderAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.PurchaseOrder)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<ReceiptPurchaseOrder?> GetWithWarehouseAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.Warehouse)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(rpo => rpo.CreatedAt >= startDate && rpo.CreatedAt <= endDate).ToListAsync();
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetPendingReceiptsAsync()
    {
        return await Query().Where(rpo => rpo.Status == GeneralStatus.Processing).ToListAsync();
    }
    //public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> UpdateReceiptPurchaseOrderWithoutRefAsync(string userId, int receiptPurchaseOrderId, UpdateReceiptPurchaseOrderWithoutRefDTO dto)
    //{
    //    var entity = await _context.ReceiptPurchaseOrders.FirstOrDefaultAsync(e => e.ReceiptPurchaseOrderId == dto.ReceiptPurchaseOrderId);

    //    if (entity.ReceiptPurchaseOrderId != receiptPurchaseOrderId)
    //    {
    //        return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("id not equal receipt purchase order id!");
    //    }
    //    if (entity == null)
    //    {
    //        return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("not found");
    //    }
    //    if (entity.PurchaseOrder != null)
    //        return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("this endpoint not valid, this for receipt without reference.");


    //    var checkApprovalStatus = await approval.GetProcessItem(entity.ReceiptPurchaseOrderId, ProcessType.Receipt);

    //    if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
    //        return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");


    //    if (dto.PostingDate.HasValue)
    //        entity.PostingDate = dto.PostingDate.Value;

    //    if (dto.DueDate.HasValue)
    //        entity.DueDate = dto.DueDate.Value;

    //    if (dto.SupplierId.HasValue)
    //        entity.SupplierId = dto.SupplierId.Value;

    //    entity.UserId = userId;

    //    if (dto.Comment != null)
    //        entity.Comment = dto.Comment;

    //    entity.Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing;

    //    // entity.SupplierId = dto.SupplierId;


    //    if (!dto.IsDraft)
    //    {
    //        await approval.StartProcessAsync(
    //            processType: ProcessType.Receipt,
    //            referenceId: entity.ReceiptPurchaseOrderId,
    //            warehouseId: entity.WarehouseId,
    //            userId: userId
    //        );

    //        entity.Status = GeneralStatus.Processing;
    //    }
    //    else
    //    {
    //        entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
    //    }

    //    await _context.SaveChangesAsync();

    //    var result = new ReceiptPurchaseOrderDTO
    //    {
    //        ReceiptPurchaseOrderId = entity.ReceiptPurchaseOrderId,
    //        DueDate = entity.DueDate,
    //        PostingDate = entity.PostingDate,
    //        Status = entity.Status.ToString(),
    //        UserId = entity.UserId,
    //        WarehouseId = entity.WarehouseId,
    //        PurchaseOrderId = entity.PurchaseOrderId,
    //        SupplierId = entity.SupplierId,
    //        Comment = entity.Comment
    //    };

    //    return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    //}

}
