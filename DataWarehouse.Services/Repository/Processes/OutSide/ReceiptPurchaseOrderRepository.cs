using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.Processes;
using DataWarehouse.Services.Services.Processes;
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
    private readonly IBaseProcessesRepository<ReceiptPurchaseOrder> baseProcesses;
    private readonly IApprovalRepository approval;
    private readonly ReasonValidationService reasonValidationService;

    public ReceiptPurchaseOrderRepository(
        IBaseProcessesRepository<ReceiptPurchaseOrder> baseProcesses,
        IApprovalRepository approval,
        ReasonValidationService reasonValidationService,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.approval = approval;
        this.reasonValidationService = reasonValidationService;
    }


    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Include(rpo => rpo.Reason).Where(rpo => rpo.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;


        var query = _context.ReceiptPurchaseOrders
            .AsNoTracking()
            .Include(rpo => rpo.Reason)
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
                ReasonId = iw.ReasonId,
                ReasonName = iw.Reason != null ? iw.Reason.Name : null,
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
            .Include(e => e.Reason)
            .Where(po => po.WarehouseId == warehouseId);

        // ?? Filtering


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

        // ?? Posting Date Filter
        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(e => e.PostingDate.Date == postDate);
        }

        // ?? Due Date Filter
        if (DueDate.HasValue)
        {
            var dueDate = DueDate.Value.Date;
            query = query.Where(e => e.DueDate.Date == dueDate);
        }

        if (!string.IsNullOrEmpty(liveStatus))
        {
            if (liveStatus == "return")
                query = query.Where(e => e.GoodsReturnOrder != null);
            else if (liveStatus == "receipt")
                query = query.Where(e => e.GoodsReturnOrder == null);
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
             HasProgress = processQuery.Any(p => p.ReferenceId == iw.ReceiptPurchaseOrderId),

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

             // ? وجود progx.Orders
             Approval = x.HasProgress,

             // ? اسم الحالة الحالية (آخر Status)
             ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,


             ReceiptPurchaseOrderId = x.Order.ReceiptPurchaseOrderId,
           
             CreatedAt = x.Order.CreatedAt,
             SupplierId = x.Order.SupplierId,
             ErrorMessage = x.Order.ErrorMessage,
             ReasonId = x.Order.ReasonId,
             ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,
           
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


    public async Task<GeneralResponse<PagedResult<ReceiptPurchaseOrderDTO>>> GetByPurchaseOrderIdAndStatusAndDateWithPaginationForDashboardAsync
       (int purchaseOrderId, string userId, int? supplierId, DateTime? postingDate, DateTime? DueDate, string? liveStatus, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ReceiptPurchaseOrders
            .AsNoTracking().Include(e => e.ReceiptPurchaseOrderItems)
            .Include(e => e.Reason)
            .Where(po => po.PurchaseOrderId == purchaseOrderId);

        // ?? Filtering


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

        // ?? Posting Date Filter
        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(e => e.PostingDate.Date == postDate);
        }

        // ?? Due Date Filter
        if (DueDate.HasValue)
        {
            var dueDate = DueDate.Value.Date;
            query = query.Where(e => e.DueDate.Date == dueDate);
        }

        if (!string.IsNullOrEmpty(liveStatus))
        {
            if (liveStatus == "return")
                query = query.Where(e => e.GoodsReturnOrder != null);
            else if (liveStatus == "receipt")
                query = query.Where(e => e.GoodsReturnOrder == null);
        }

        query = query.OrderByDescending(e => e.CreatedAt); // طھط£ظƒط¯ ظ‡ظ†ط§

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

             // ظ‡ظ„ ظپظٹظ‡ progress ط£طµظ„ط§ظ‹طں
             HasProgress = processQuery.Any(p => p.ReferenceId == iw.ReceiptPurchaseOrderId),

             // ط¢ط®ط± Status (ظ„ظˆ ظ…ظˆط¬ظˆط¯)
             LatestStatus = processQuery
                 .Where(p => p.ReferenceId == iw.ReceiptPurchaseOrderId)
                 .OrderByDescending(p => p.ProcessItemIsProgressId) // ط£ظˆ CreatedAt ظ„ظˆ ط¹ظ†ط¯ظƒ
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

             // ? ظˆط¬ظˆط¯ progx.Orders
             Approval = x.HasProgress,

             // ? ط§ط³ظ… ط§ظ„ط­ط§ظ„ط© ط§ظ„ط­ط§ظ„ظٹط© (ط¢ط®ط± Status)
             ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,


             ReceiptPurchaseOrderId = x.Order.ReceiptPurchaseOrderId,
           
             CreatedAt = x.Order.CreatedAt,
             SupplierId = x.Order.SupplierId,
             ErrorMessage = x.Order.ErrorMessage,
             ReasonId = x.Order.ReasonId,
             ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,
           
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
            .Include(r => r.Reason)
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
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,

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
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Receipt);
        }
        catch (Exception ex)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse(ex.Message);
        }

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
            ReasonId = dto.ReasonId,
        };


        var res = await AddAsync(goodsReturnOrder);
        await SaveChangesAsync();

        // ? شغل الـ Approval Workflow لو مش Draft
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
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderByPurchaseOrderIdAsync(string userId, AddReceiptPurchaseOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Receipt);
        }
        catch (Exception ex)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse(ex.Message);
        }

        var purchaseOrder = await _context.PurchaseOrders.FirstOrDefaultAsync(p=>p.PurchaseOrderId == dto.PurchaseOrderId);

        if (purchaseOrder == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder is not found");

        if (purchaseOrder.Status != GeneralStatus.Completed)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You can add Receipt, if putchase order status is completed only");

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
            PurchaseOrderId = dto.PurchaseOrderId,
            ReasonId = dto.ReasonId
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        // ? شغل الـ Approval Workflow لو مش Draft
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
            Comment = res.Comment,
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> AddReceiptPurchaseOrderAndItemsByPurchaseOrderIdAsync(
    string userId,
    AddReceiptPurchaseOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Receipt);
        }
        catch (Exception ex)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse(ex.Message);
        }

        var purchaseOrder = await _context.PurchaseOrders
                        .Include(p => p.PurchaseOrderItems) // ? هات items
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == dto.PurchaseOrderId);

        if (purchaseOrder == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder is not found");

        if (purchaseOrder.Status != GeneralStatus.Completed)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("You can add Receipt, if purchase order status is completed only");
        if (purchaseOrder.PurchaseOrderItems == null || !purchaseOrder.PurchaseOrderItems.Any())
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("purchaseOrder has no items to receipt");

        var purchaseOrderItemIds = purchaseOrder.PurchaseOrderItems
            .Select(x => x.PurchaseOrderItemId)
            .ToList();

        var executedByPurchaseOrderItem = await _context.ReceiptPurchaseOrderItems
            .AsNoTracking()
            .Where(x => x.PurchaseOrderItemId.HasValue && purchaseOrderItemIds.Contains(x.PurchaseOrderItemId.Value))
            .GroupBy(x => x.PurchaseOrderItemId!.Value)
            .Select(g => new
            {
                PurchaseOrderItemId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            })
            .ToDictionaryAsync(x => x.PurchaseOrderItemId, x => x.Quantity);

        var remainingItems = purchaseOrder.PurchaseOrderItems
            .Select(poi =>
            {
                var executed = executedByPurchaseOrderItem.TryGetValue(poi.PurchaseOrderItemId, out var qty) ? qty : 0m;
                var remaining = poi.Quantity - executed;

                return new { PurchaseOrderItem = poi, Remaining = remaining };
            })
            .Where(x => x.Remaining > 0m)
            .ToList();

        var overReceivedItem = purchaseOrder.PurchaseOrderItems
            .FirstOrDefault(poi =>
            {
                var executed = executedByPurchaseOrderItem.TryGetValue(poi.PurchaseOrderItemId, out var qty) ? qty : 0m;
                return executed > poi.Quantity;
            });

        if (overReceivedItem != null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("One or more purchase order items already exceed the allowed received quantity.");

        if (!remainingItems.Any())
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("All purchase order items are fully received.");

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
            ReasonId = dto.ReasonId,

            // Copy PO items -> Receipt PO items using remaining quantity only
            ReceiptPurchaseOrderItems = remainingItems.Select(x => new ReceiptPurchaseOrderItem
            {
                ItemId = x.PurchaseOrderItem.ItemId,
                Quantity = x.Remaining,
                UoMEntry = x.PurchaseOrderItem.UoMEntry,
                BarCode = x.PurchaseOrderItem.BarCode,
                UnitPrice = x.PurchaseOrderItem.UnitPrice,
                PurchaseOrderItemId = x.PurchaseOrderItem.PurchaseOrderItemId,

                VatPercent = x.PurchaseOrderItem.VatPercent,

                VatAmount = x.PurchaseOrderItem.VatAmount,

                LineTotalBeforeVat = x.PurchaseOrderItem.LineTotalBeforeVat,

                LineTotalAfterVat = x.PurchaseOrderItem.LineTotalAfterVat,

                // منطق الستاتس: لسه الاستلام ما تمش
                Status = GeneralItemStatus.Planned,

                // optional
                ErrorMessage = null,
                Comment = null
            }).ToList()
        };

        await _context.ReceiptPurchaseOrders.AddAsync(receipt);
        await SaveChangesAsync();

        // ? شغل الـ Approval Workflow لو مش Draft
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
            Comment = receipt.Comment,
            ReasonId = receipt.ReasonId,
            ReasonName = receipt.Reason != null ? receipt.Reason.Name : null
        };

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(model);
    }
    
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> UpdateReceiptPurchaseOrderAsync(string userId, int receiptPurchaseOrderId, UpdateReceiptPurchaseOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Receipt);
        }
        catch (Exception ex)
        {
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse(ex.Message);
        }

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
        entity.ReasonId = dto.ReasonId;

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
            Comment = entity.Comment,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
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
            Comment = entity.Comment,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        };

        // لو عندك تفاصيل ومفيش Cascade Delete هتحتاج تمسحها الأول هنا
        _context.ReceiptPurchaseOrders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<ReceiptPurchaseOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int receiptPurchaseOrderId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<ReceiptPurchaseOrder>(
            receiptPurchaseOrderId,
            ProcessType.Receipt,
            x => x.ReceiptPurchaseOrderId == receiptPurchaseOrderId,
            _context.ReceiptPurchaseOrders
        );
    }

    public async Task<GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>> GetReceiptPurchaseOrdersByPurchaseIdAsync(string userId, int purchaseId)
    {
        var res = await _context.ReceiptPurchaseOrders
            .AsNoTracking()
            .Include(r => r.GoodsReturnOrder)
            .Include(r => r.Reason)
            .Where(rpo => rpo.PurchaseOrderId == purchaseId)
            .OrderByDescending(rpo => rpo.ReceiptPurchaseOrderId)
            .ToListAsync();

        if (res == null || !res.Any())
            return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.FailResponse("Not Found");

        var result = new List<ReceiptPurchaseOrderDTO>();

        foreach (var receipt in res)
        {
            var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Receipt, receipt.ReceiptPurchaseOrderId);

            result.Add(new ReceiptPurchaseOrderDTO
            {
                DueDate = receipt.DueDate,
                PostingDate = receipt.PostingDate,
                ReceiptPurchaseOrderId = receipt.ReceiptPurchaseOrderId,
                Status = receipt.Status.ToString(),
                UserId = receipt.UserId,
                WarehouseId = receipt.WarehouseId,
                PurchaseOrderId = receipt.PurchaseOrderId,
                SupplierId = receipt.SupplierId,
                Comment = receipt.Comment,
                ReasonId = receipt.ReasonId,
                ReasonName = receipt.Reason != null ? receipt.Reason.Name : null,
                CanApprove = approvalModel.CanApprove,
                ProcessApprovalId = approvalModel.ProcessApprovalId,
                ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
                Approval = approvalModel.hasProgress,
                ApprovalStatus = approvalModel.ApprovalStatus,
                IsReturn = receipt.GoodsReturnOrder != null,
                ReturnOrderId = receipt.GoodsReturnOrder != null ? receipt.GoodsReturnOrder.GoodsReturnOrderId : null,
            });
        }

        return GeneralResponse<IEnumerable<ReceiptPurchaseOrderDTO>>.SuccessResponse(result);
    }
    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> GetByPurchaseOrderIdAsync(string userId, int purchaseOrderId)
    {
        var res = await _context.ReceiptPurchaseOrders.Include(r=>r.GoodsReturnOrder)
            .Include(r => r.Reason)
            .Where(rpo => rpo.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(rpo => rpo.ReceiptPurchaseOrderId)
            .FirstOrDefaultAsync();

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
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,
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

    public async Task<GeneralResponse<ReceiptPurchaseOrderDTO>> DuplicateReceiptPurchaseOrderAsync(string userId, int receiptPurchaseOrderId, CancellationToken cancellationToken = default)
    {
        var source = await _context.ReceiptPurchaseOrders
            .AsNoTracking()
            .Include(x => x.ReceiptPurchaseOrderItems)
                .ThenInclude(x => x.ReceiptPurchaseOrderBatches)
            .FirstOrDefaultAsync(x => x.ReceiptPurchaseOrderId == receiptPurchaseOrderId, cancellationToken);

        if (source == null)
            return GeneralResponse<ReceiptPurchaseOrderDTO>.FailResponse("Receipt purchase order not found");

        var clone = OrderDuplicationHelper.Clone(source, userId);
        await _context.ReceiptPurchaseOrders.AddAsync(clone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetReceiptOrderByIdAsync(userId, clone.ReceiptPurchaseOrderId);
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
                    ReasonId = p.ReasonId,
                    ReasonName = p.Reason != null ? p.Reason.Name : null,
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
        return await Query().Include(rpo => rpo.Reason).Where(rpo => rpo.UserId == userId).ToListAsync();
    }

    public async Task<ReceiptPurchaseOrder?> GetWithItemsAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.ReceiptPurchaseOrderItems, rpo => rpo.Reason)
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
         ReasonId = r.ReasonId,
         ReasonName = r.Reason != null ? r.Reason.Name : null,

         Items = r.ReceiptPurchaseOrderItems.Select(i => new ReceiptPurchaseOrderItemDTO
         {
             ReceiptPurchaseOrderItemId = i.ReceiptPurchaseOrderItemId,
             Quantity = i.Quantity,
             UoMEntry = i.UoMEntry,
             BarCode = i.BarCode,
             UnitPrice = i.UnitPrice,
             VatPercent = i.VatPercent,
             VatAmount = i.VatAmount,
             LineTotalBeforeVat = i.LineTotalBeforeVat,
             LineTotalAfterVat = i.LineTotalAfterVat,
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
        return await QueryIncluding(false, rpo => rpo.PurchaseOrder, rpo => rpo.Reason)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<ReceiptPurchaseOrder?> GetWithWarehouseAsync(int receiptPurchaseOrderId)
    {
        return await QueryIncluding(false, rpo => rpo.Warehouse, rpo => rpo.Reason)
            .FirstOrDefaultAsync(rpo => rpo.ReceiptPurchaseOrderId == receiptPurchaseOrderId);
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Include(rpo => rpo.Reason).Where(rpo => rpo.CreatedAt >= startDate && rpo.CreatedAt <= endDate).ToListAsync();
    }

    public async Task<IEnumerable<ReceiptPurchaseOrder>> GetPendingReceiptsAsync()
    {
        return await Query().Include(rpo => rpo.Reason).Where(rpo => rpo.Status == GeneralStatus.Processing).ToListAsync();
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
