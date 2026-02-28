using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading;


namespace DataWarehouse.Services.Repository.Processes.PurchaseOrderRepo;

public class PurchaseOrderRepository : BaseRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    private readonly IApprovalRepository approval;
    private readonly ISapCache sapCache;
    private readonly ISapSettingsRepository sap;
    private readonly IProcessesTypesDateRepository processes;

    public PurchaseOrderRepository(IApprovalRepository approval, ISapCache sapCache,ISapSettingsRepository sap, IProcessesTypesDateRepository processes, DataWarehouseDbContext context) : base(context)
    {
        this.approval = approval;
        this.sapCache = sapCache;
        this.sap = sap;
        this.processes = processes;
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(po => po.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Warehouses.Where(e => e.WarehouseId == warehouseId)
            .AsNoTracking()
            .SelectMany(e => e.PurchaseOrders);

        // 🔹 Filtering




        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new PurchaseOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                PurchaseOrderId = iw.PurchaseOrderId,
                Status = iw.Status.ToString(),
                 Comment = iw.Comment,
                CreatedAt = iw.CreatedAt,
                // string = enum
                UserId = iw.UserId,
                WarehouseId = warehouseId,
                SupplierName = iw.Supplier.SupplierName,
                Supplier = iw.Supplier,
                IsReceipt = iw.ReceiptPurchaseOrder == null ? false : true
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<PurchaseOrderDTO>>.SuccessResponse(
            new PagedResult<PurchaseOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
       (int warehouseId, string userId,int? supplierId, DateTime? postingDate, DateTime? DueDate,string? liveStatus, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken=default)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.PurchaseOrders
            .AsNoTracking().Include(e => e.PurchaseOrderItems)
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
            query = query.Where(e => e.ReceiptPurchaseOrder != null);

           if (liveStatus == "return")
              query = query.Where(e => e.ReceiptPurchaseOrder.GoodsReturnOrder != null);
           
        }

        query = query.OrderByDescending(e => e.CreatedAt); // تأكد هنا

        // Approved references subquery (for Sales process only)
        var processQuery = _context.ProcessItemIsProgresses
       .AsNoTracking()
       .Where(p => p.ProcessType == ProcessType.Purchase);

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
                 .Where(p => p.ReferenceId == iw.PurchaseOrderId)
                 .OrderByDescending(p => p.ProcessItemIsProgressId) // أو CreatedAt لو عندك
                 .Select(p => (ProcessStatus?)p.Status)
                 .FirstOrDefault()
         })
         .Select(x => new PurchaseOrderDTO
         {
             DueDate = x.Order.DueDate,
             PostingDate = x.Order.PostingDate,
             PurchaseOrderId = x.Order.PurchaseOrderId,
             Status = x.Order.Status.ToString(),
             Comment = x.Order.Comment,
             UserId = x.Order.UserId,
             WarehouseId = x.Order.WarehouseId,
             SupplierName = x.Order.Supplier.SupplierName,
             Supplier = x.Order.Supplier,
             ItemCount = x.Order.PurchaseOrderItems.Count(),
             // ✅ وجود progress
             Approval = x.HasProgress,

             // ✅ اسم الحالة الحالية (آخر Status)
             ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,

             // ✅ Special fields (Receipt/Return)
             IsReceipt = x.Order.ReceiptPurchaseOrder != null,
             ReceiptOrderId = x.Order.ReceiptPurchaseOrder != null
                 ? x.Order.ReceiptPurchaseOrder.ReceiptPurchaseOrderId
                 : null,

             IsReturn = x.Order.ReceiptPurchaseOrder != null
                 && x.Order.ReceiptPurchaseOrder.GoodsReturnOrder != null,

             ReturnOrderId = x.Order.ReceiptPurchaseOrder != null
                 && x.Order.ReceiptPurchaseOrder.GoodsReturnOrder != null
                     ? x.Order.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId
                     : null
         })
         .ToListAsync(cancellationToken);


        return GeneralResponse<PagedResult<PurchaseOrderDTO>>.SuccessResponse(
            new PagedResult<PurchaseOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<PurchaseOrderDTO>> GetWithSupplierAsync(string userId, int PurchaseOrderId,CancellationToken cancellationToken = default)
    {

        var res = await _context.PurchaseOrders
            .Include(po => po.ReceiptPurchaseOrder)
            .Include( po => po.Supplier)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == PurchaseOrderId);

        if (res == null)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("this PurchaseOrderId is not found");
     
        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Purchase, res.PurchaseOrderId);


        var mapping = new PurchaseOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            PurchaseOrderId = res.PurchaseOrderId,
            Status = res.Status.ToString(),
            Comment = res.Comment,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CreatedAt = res.CreatedAt,


            SupplierName = res.Supplier.SupplierName,
            
            SupplierId = res.SupplierId,
            SupplierCode = res.Supplier.SupplierCode,


            // ✅ Approval fields (same shape as sales)
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Reason = approvalModel.Reason,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,


            // ✅ Special fields (Receipt / Return)
            IsReceipt = res.ReceiptPurchaseOrder != null,
            ReceiptOrderId = res.ReceiptPurchaseOrder != null
          ? res.ReceiptPurchaseOrder.ReceiptPurchaseOrderId
          : null,

            IsReturn = res.ReceiptPurchaseOrder != null
          && res.ReceiptPurchaseOrder.GoodsReturnOrder != null,

            ReturnOrderId = res.ReceiptPurchaseOrder != null
          && res.ReceiptPurchaseOrder.GoodsReturnOrder != null
              ? res.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId
              : null
        };



        return GeneralResponse<PurchaseOrderDTO>.SuccessResponse(mapping);
    }
   
    public async Task<GeneralResponse<List<NameStatus>>> GetPurchaseOrderStatus()
    {
        var statuses = Enum.GetValues(typeof(GeneralStatus))
            .Cast<GeneralStatus>()
            .Select(s => new NameStatus
            {
                Id = (int)s,
                Name = s.ToString()
            })
            .ToList();

        return await Task.FromResult(new GeneralResponse<List<NameStatus>>
        {
            Success = true,
            Message = "Purchase statuses retrieved successfully",
            Data = statuses
        });
    }

    // create
    public async Task<GeneralResponse<PurchaseOrderDTO>> AddPurchaseOrderByWarehouseIdAsync(string userId,
           AddPurchaseOrderDTO dto)
    {

        var suppler = await _context.Suppliers.FirstOrDefaultAsync(p => p.SupplierId == dto.SupplierId);

        if (suppler == null)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("suppler is not found");

        var mapping = new PurchaseOrder
        {
            Status = dto.IsDraft?GeneralStatus.Draft:GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            SupplierId = dto.SupplierId,
        };

        var res = await AddAsync(mapping);
         await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Purchase,
                referenceId: res.PurchaseOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }


        var model = new PurchaseOrderDTO
        {
            PurchaseOrderId = res.PurchaseOrderId,
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            Status = res.Status.ToString() // <-- هنا بنحول الـ enum ل string
        };

        return GeneralResponse<PurchaseOrderDTO>.SuccessResponse(model);
    }

    private async Task<(bool IsValid, string Message)> ValidateBusinessDatesAsync(
      DateTime postingDate,
      DateTime dueDate)
    {
        // 1️⃣ جيب الـ valid business dates
        var validBusinessDates = await processes.GetByProcessesTypeForProductionAsync();

        if (!validBusinessDates.Any())
        {
            return (false, "No valid business dates found");
        }

        // 2️⃣ حول لـ DateOnly
        var postingDateOnly = DateOnly.FromDateTime(postingDate);
        var dueDateOnly = DateOnly.FromDateTime(dueDate);

        // 3️⃣ تشيك PostingDate
        var isPostingDateValid = validBusinessDates.Any(d => d.PostingDate == postingDateOnly);

        // 4️⃣ تشيك DueDate
        var isDueDateValid = validBusinessDates.Any(d => d.DueDate == dueDateOnly);

        // 5️⃣ رجع النتيجة مع رسالة واضحة
        if (!isPostingDateValid && !isDueDateValid)
        {
            return (false, "Both PostingDate and DueDate are not valid business dates");
        }

        if (!isPostingDateValid)
        {
            return (false, $"PostingDate {postingDate:yyyy-MM-dd} is not a valid business date");
        }

        if (!isDueDateValid)
        {
            return (false, $"DueDate {dueDate:yyyy-MM-dd} is not a valid business date");
        }

        return (true, "Both dates are valid");
    }
    // update
    public async Task<GeneralResponse<PurchaseOrderDTO>> UpdatePurchaseOrderAsync(
       string userId,
       int productionId,
       UpdatePurchaseOrderDTO dto)
    {
        // 1) Get existing entity
        var entity = await _context.PurchaseOrders
            .FirstOrDefaultAsync(e => e.PurchaseOrderId == dto.PurchaseOrderId);

        if (entity == null)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("not found");



        if (entity.PurchaseOrderId != productionId)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("id not equal production order id!");

        var checkApprovalStatus = await approval.GetProcessItem(entity.PurchaseOrderId, ProcessType.Purchase);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("You cannot edit this order because its approval status is 'Approved' and all approval steps have been completed.");


       
        // 3) Update fields (Full Update)
        if (dto.PostingDate.HasValue)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers
           .AnyAsync(s => s.SupplierId == dto.SupplierId);

            if (!supplierExists)
                return GeneralResponse<PurchaseOrderDTO>.FailResponse("supplier not found");


            entity.SupplierId = dto.SupplierId.Value;
        }

        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Purchase,
                referenceId: entity.PurchaseOrderId,
                warehouseId: entity.WarehouseId,
                userId: userId
            );

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }


        entity.UserId = userId;

        // 4) Save
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // SQL Server message (FK / NOT NULL / UNIQUE / CHECK...)
            var sqlMsg = ex.InnerException?.Message ?? ex.Message;
            return GeneralResponse<PurchaseOrderDTO>.FailResponse(sqlMsg);
        }

        // 5) Map to DTO
        var result = new PurchaseOrderDTO
        {
            PurchaseOrderId = entity.PurchaseOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            SupplierId = entity.SupplierId,
            Status = entity.Status.ToString()
        };

        return GeneralResponse<PurchaseOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<PurchaseOrderDTO>> DeletePurchaseOrderAsync(
  int PurchaseOrderId,
  CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .FirstOrDefaultAsync(e => e.PurchaseOrderId == PurchaseOrderId, cancellationToken);

        if (entity == null)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(
            entity.PurchaseOrderId,
            ProcessType.Purchase,
            cancellationToken);


        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");


        // Snapshot قبل الحذف علشان نرجعه في الـ response
        var result = new PurchaseOrderDTO
        {
            PurchaseOrderId = entity.PurchaseOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            SupplierId = entity.SupplierId,
            Comment = entity.Comment
        };

        // لو عندك تفاصيل ومفيش Cascade Delete هتحتاج تمسحها الأول هنا
        _context.PurchaseOrders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<PurchaseOrderDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByItemIdAsync(int itemId)
    {
        return await QueryIncluding(false, po => po.PurchaseOrderItems)
            .Where(po => po.PurchaseOrderItems.Any(poi => poi.ItemId == itemId))
            .ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<PurchaseOrderDTO>>> GetByStatusAsync(string status)
    {
       

        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            var query =  await Query().Where(po => po.Status == statusEnum)
                .Select(p => new PurchaseOrderDTO
            {
                DueDate = p.DueDate,
                PostingDate = p.PostingDate,
                PurchaseOrderId = p.PurchaseOrderId,

                Status = p.Status.ToString(),
                // string = enum
                UserId = p.UserId,

                WarehouseId = p.WarehouseId,

            }).ToListAsync();

            return GeneralResponse<IEnumerable<PurchaseOrderDTO>>.SuccessResponse(query);
        
        }
        return GeneralResponse<IEnumerable<PurchaseOrderDTO>>.FailResponse("Not found any purchase to this status now!");
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(po => po.UserId == userId).ToListAsync();
    }

    public async Task<PurchaseOrder?> GetWithItemsAsync(int PurchaseOrderId)
    {
        return await QueryIncluding(false, po => po.PurchaseOrderItems)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == PurchaseOrderId);
    }

    public async Task<PurchaseOrder?> GetWithWarehouseAsync(int PurchaseOrderId)
    {
        return await QueryIncluding(false, po => po.Warehouse)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == PurchaseOrderId);
    }
  

    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(po => po.CreatedAt >= startDate && po.CreatedAt <= endDate).ToListAsync();
    }
    public async Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync()
    {
        return await Query().Where(po => po.Status == GeneralStatus.Processing).ToListAsync();
    }




    private string GetEnumString(GeneralStatus status)
    {
        switch (status)
        {
            case GeneralStatus.Draft:
                return "Draft";
            case GeneralStatus.Processing:
                return "Processing";
            case GeneralStatus.Completed:
                return "Completed";
            case GeneralStatus.PartiallyFailed:
                return "Partially Failed";
            default:
                return "Unknown";
        }
    }
    //public async Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationAsync
    //  (int? warehouseId, string userId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize)

    //{
    //    pageNumber = pageNumber <= 0 ? 1 : pageNumber;
    //    pageSize = pageSize <= 0 ? 10 : pageSize;


    //    var yourWarehouse = (await sap.GetYourWarehousesToEmployees(userId)).Data.FirstOrDefault();
    //    if (yourWarehouse == null)
    //        return GeneralResponse<PagedResult<PurchaseOrderDTO>>.FailResponse("user not valid");



    //    var query = _context.Warehouses.Where(e => e.WarehouseId == (warehouseId == null ? yourWarehouse.WarehouseId : warehouseId))
    //        .AsNoTracking()
    //        .SelectMany(e => e.PurchaseOrders);

    //    // 🔹 Filtering

    //    if (!string.IsNullOrEmpty(status))
    //    {
    //        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
    //        {
    //            query = query.Where(e => e.Status == statusEnum);
    //        }
    //    }

    //    // 🔹 Posting Date Filter
    //    if (postingDate.HasValue)
    //    {
    //        var postDate = postingDate.Value.Date;
    //        query = query.Where(e => e.PostingDate.Date == postDate);
    //    }

    //    // 🔹 Due Date Filter
    //    if (DueDate.HasValue)
    //    {
    //        var dueDate = DueDate.Value.Date;
    //        query = query.Where(e => e.DueDate.Date == dueDate);
    //    }



    //    var totalRecords = await query.CountAsync();

    //    var data = await query

    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .Select(iw => new PurchaseOrderDTO
    //        {
    //            DueDate = iw.DueDate,
    //            PostingDate = iw.PostingDate,
    //            PurchaseOrderId = iw.PurchaseOrderId,
    //            Status = iw.Status.ToString(),
    //            Comment = iw.Comment,
    //            // string = enum
    //            UserId = iw.UserId,
    //            WarehouseId = iw.WarehouseId,
    //            SupplierName = iw.Supplier.SupplierName,
    //            Supplier = iw.Supplier,
    //            IsReceipt = iw.ReceiptPurchaseOrder == null ? false : true,
    //            ReceiptOrderId = iw.ReceiptPurchaseOrder == null ? null : iw.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
    //            IsReturn = iw.ReceiptPurchaseOrder == null ? false : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? false : true),
    //            ReturnOrderId = iw.ReceiptPurchaseOrder == null ? null : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? null : iw.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId)


    //        })
    //        .ToListAsync();

    //    return GeneralResponse<PagedResult<PurchaseOrderDTO>>.SuccessResponse(
    //        new PagedResult<PurchaseOrderDTO>
    //        {
    //            PageNumber = pageNumber,
    //            PageSize = pageSize,
    //            TotalRecords = totalRecords,
    //            Data = data
    //        });
    //}

}
