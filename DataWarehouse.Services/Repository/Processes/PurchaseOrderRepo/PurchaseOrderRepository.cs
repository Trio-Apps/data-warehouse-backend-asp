using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes;

using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Security.Claims;


namespace DataWarehouse.Services.Repository.Processes.PurchaseOrderRepo;

public class PurchaseOrderRepository : BaseRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    private readonly ISapCache sapCache;
    private readonly ISapSettingsRepository sap;
    private readonly IProcessesTypesDateRepository processes;

    public PurchaseOrderRepository(ISapCache sapCache,ISapSettingsRepository sap, IProcessesTypesDateRepository processes, DataWarehouseDbContext context) : base(context)
    {
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
                CreatedDate = iw.CreatedAt,
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

    public async Task<GeneralResponse<PagedResult<PurchaseOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationAsync
        (int? warehouseId,string userId,DateTime? postingDate,DateTime? DueDate, string? status, int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

       
        var yourWarehouse = (await sap.GetYourWarehousesToEmployees(userId)).Data.FirstOrDefault();
        if (yourWarehouse == null)
            return GeneralResponse<PagedResult<PurchaseOrderDTO>>.FailResponse("user not valid");



        var query = _context.Warehouses.Where(e => e.WarehouseId == (warehouseId==null?yourWarehouse.WarehouseId:warehouseId))
            .AsNoTracking()
            .SelectMany(e => e.PurchaseOrders);

        // 🔹 Filtering

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<PurchaseStatus>(status, out var statusEnum))
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
                // string = enum
                UserId = iw.UserId,
                WarehouseId = iw.WarehouseId,
                SupplierName = iw.Supplier.SupplierName,
                Supplier = iw.Supplier,
                IsReceipt = iw.ReceiptPurchaseOrder == null ? false : true,
                ReceiptOrderId = iw.ReceiptPurchaseOrder == null ? null : iw.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
                IsReturn = iw.ReceiptPurchaseOrder == null ? false : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? false : true),
                ReturnOrderId = iw.ReceiptPurchaseOrder == null ? null : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? null : iw.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId)


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
       (int warehouseId, string userId, DateTime? postingDate, DateTime? DueDate,string? liveStatus, string? status, int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.PurchaseOrders
            .AsNoTracking().Include(e => e.PurchaseOrderItems)
            .Where(po => po.WarehouseId == warehouseId);

        // 🔹 Filtering

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<PurchaseStatus>(status, out var statusEnum))
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
                // string = enum
                UserId = iw.UserId,
                WarehouseId = iw.WarehouseId,
                SupplierName = iw.Supplier.SupplierName,
                Supplier = iw.Supplier,
                ItemCount = iw.PurchaseOrderItems.Count(),
               //س  Approval = 
                IsReceipt = iw.ReceiptPurchaseOrder == null ? false : true,
                ReceiptOrderId = iw.ReceiptPurchaseOrder == null ? null : iw.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
                IsReturn = iw.ReceiptPurchaseOrder == null ? false : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? false : true),
                ReturnOrderId = iw.ReceiptPurchaseOrder == null ? null : (iw.ReceiptPurchaseOrder.GoodsReturnOrder == null ? null : iw.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId)
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

    public async Task<GeneralResponse<List<NameStatus>>> GetPurchaseOrderStatus()
    {
        var statuses = Enum.GetValues(typeof(PurchaseStatus))
            .Cast<PurchaseStatus>()
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

        // var sapId = await sapCache.Get();

        //  var checkValidDate = await ValidateBusinessDatesAsync(dto.PostingDate,dto.DueDate);

        //if (!checkValidDate.IsValid)
        //    return GeneralResponse<PurchaseOrderDTO>.FailResponse($"{checkValidDate.Message}");

        var mapping = new PurchaseOrder
        {
            Status = dto.IsDraft?PurchaseStatus.Draft:PurchaseStatus.Processing,
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

        // 2) Extra validation (because DateTime/int are non-nullable and can be default/0)
        if (dto.PostingDate == default)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("Posting Date is required");

        if (dto.DueDate == default)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("Due Date is required");

        if (dto.DueDate < dto.PostingDate)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("Due Date must be >= Posting Date");

        if (dto.SupplierId <= 0)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("SupplierId is invalid");

        var supplierExists = await _context.Suppliers
            .AnyAsync(s => s.SupplierId == dto.SupplierId);

        if (!supplierExists)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("supplier not found");

        // 3) Update fields (Full Update)
        if (dto.PostingDate.HasValue)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.SupplierId.HasValue)
            entity.SupplierId = dto.SupplierId.Value;


        entity.Status = dto.IsDraft
            ? PurchaseStatus.Draft
            : PurchaseStatus.Processing;

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

    public async Task<IEnumerable<PurchaseOrder>> GetByItemIdAsync(int itemId)
    {
        return await QueryIncluding(false, po => po.PurchaseOrderItems)
            .Where(po => po.PurchaseOrderItems.Any(poi => poi.ItemId == itemId))
            .ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<PurchaseOrderDTO>>> GetByStatusAsync(string status)
    {
       

        if (Enum.TryParse<PurchaseStatus>(status, out var statusEnum))
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
    public async Task<GeneralResponse<PurchaseOrderDTO>> GetWithSupplierAsync(int PurchaseOrderId)
    {

        var res = await QueryIncluding(false, po => po.Supplier)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == PurchaseOrderId);

        if (res == null)
            return GeneralResponse<PurchaseOrderDTO>.FailResponse("this PurchaseOrderId is not found");

        var mapping = new PurchaseOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            PurchaseOrderId = res.PurchaseOrderId,
            Status = res.Status.ToString(),
             Comment = res.Comment,
            // string = enum
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SupplierName = res.Supplier.SupplierName,
            SupplierId = res.SupplierId
             //   Supplier = res.Supplier,
           // IsReceipt = res.ReceiptPurchaseOrder == null ? false : true,
           // ReceiptOrderId = res.ReceiptPurchaseOrder == null ? null : res.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
           // IsReturn = res.ReceiptPurchaseOrder == null ? false : (res.ReceiptPurchaseOrder.GoodsReturnOrder == null ? false : true),
          //  ReturnOrderId = res.ReceiptPurchaseOrder == null ? null : (res.ReceiptPurchaseOrder.GoodsReturnOrder == null ? null : res.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId)

        };


        return GeneralResponse<PurchaseOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(po => po.CreatedAt >= startDate && po.CreatedAt <= endDate).ToListAsync();
    }
    public async Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync()
    {
        return await Query().Where(po => po.Status == PurchaseStatus.Processing).ToListAsync();
    }




    private string GetEnumString(PurchaseStatus status)
    {
        switch (status)
        {
            case PurchaseStatus.Draft:
                return "Draft";
            case PurchaseStatus.Processing:
                return "Processing";
            case PurchaseStatus.Completed:
                return "Completed";
            case PurchaseStatus.PartiallyFailed:
                return "Partially Failed";
            default:
                return "Unknown";
        }
    }
}
