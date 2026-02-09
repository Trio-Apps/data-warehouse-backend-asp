using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.DTOs.Processes.PurchaseOrders;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesOrderRepository : BaseRepository<SalesOrder>, ISalesOrderRepository
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IApprovalRepository approval;
    private readonly ISapSettingsRepository sap;

    public SalesOrderRepository(UserManager<ApplicationUser> userManager, IApprovalRepository approval, ISapSettingsRepository sap, DataWarehouseDbContext context) : base(context)
    {
        this.userManager = userManager;
        this.approval = approval;
        this.sap = sap;
    }

    public async Task<IEnumerable<SalesOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(so => so.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Warehouses.Where(e => e.WarehouseId == warehouseId)
            .AsNoTracking()
            .SelectMany(e => e.SalesOrders);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(so => new SalesOrderDTO
            {
                DueDate = so.DueDate,
                PostingDate = so.PostingDate,
                SalesOrderId = so.SalesOrderId,
                Status = so.Status.ToString(),
                UserId = so.UserId,
                WarehouseId = warehouseId,
                CustomerId = so.CustomerId,
                Comment = so.Comment
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesOrderDTO>>.SuccessResponse(
            new PagedResult<SalesOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }
   
    
    public async Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationAsync
     (int? warehouseId, string userId, DateTime? postingDate, DateTime? DueDate, string? status, int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;


        var yourWarehouse = (await sap.GetYourWarehousesToEmployees(userId)).Data.FirstOrDefault();
        if (yourWarehouse == null)
            return GeneralResponse<PagedResult<SalesOrderDTO>>.FailResponse("user not valid");



        var query = _context.Warehouses.Where(e => e.WarehouseId == (warehouseId == null ? yourWarehouse.WarehouseId : warehouseId))
            .AsNoTracking()
            .SelectMany(e => e.SalesOrders);

        // 🔹 Filtering

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<SalesOrderStatus>(status, out var statusEnum))
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
            .Select(iw => new SalesOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                SalesOrderId = iw.SalesOrderId,
                Status = iw.Status.ToString(),
                Comment = iw.Comment,
                // string = enum
                UserId = iw.UserId,
                WarehouseId = iw.WarehouseId,
               CustomerName  = iw.Customer.CustomerName,
                Customer = iw.Customer,
                IsReturn = iw.SalesReturnOrder == null ? false : (iw.SalesReturnOrder == null ? false : true),
                ReturnOrderId = iw.SalesReturnOrder == null ? null : (iw.SalesReturnOrder == null ? null : iw.SalesReturnOrder.SalesReturnOrderId)


            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesOrderDTO>>.SuccessResponse(
            new PagedResult<SalesOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<GeneralResponse<PagedResult<SalesOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
       (int warehouseId, string userId, DateTime? postingDate, DateTime? DueDate, string? liveStatus, string? status, int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;


        var query = _context.SalesOrders
            .AsNoTracking()
            .Where(e => e.WarehouseId == warehouseId);
        // 🔹 Filtering
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<SalesOrderStatus>(status, out var statusEnum))
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
            query = query.Where(e => e.SalesReturnOrder != null);

        }

        var totalRecords = await query.CountAsync();

        var data = await query

            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new SalesOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                SalesOrderId = iw.SalesOrderId,
                Status = iw.Status.ToString(),
                Comment = iw.Comment,
                // string = enum
                UserId = iw.UserId,
                WarehouseId = iw.WarehouseId,
                CustomerName = iw.Customer.CustomerName,
                ItemCount = iw.SalesOrderItems.Count(),
                Customer = iw.Customer,
                IsReturn = iw.SalesReturnOrder == null ? false : (iw.SalesReturnOrder == null ? false : true),
                ReturnOrderId = iw.SalesReturnOrder == null ? null : (iw.SalesReturnOrder == null ? null : iw.SalesReturnOrder.SalesReturnOrderId),

                // ✅ الحالة المطلوبة
             Approval = _context.ProcessItemIsProgresses
            .Any(p =>
                p.ProcessType == ProcessType.Sales &&
                p.ReferenceId == iw.SalesOrderId &&
                p.Status == ProcessStatus.Approved)
            }).ToListAsync();

        return GeneralResponse<PagedResult<SalesOrderDTO>>.SuccessResponse(
            new PagedResult<SalesOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }


    public async Task<GeneralResponse<List<NameStatus>>> GetSalesOrderStatus()
    {
        var statuses = Enum.GetValues(typeof(SalesOrderStatus))
            .Cast<SalesOrderStatus>()
            .Select(s => new NameStatus
            {
                Id = (int)s,
                Name = s.ToString()
            })
            .ToList();

        return await Task.FromResult(new GeneralResponse<List<NameStatus>>
        {
            Success = true,
            Message = "Sales order statuses retrieved successfully",
            Data = statuses
        });
    }

    public async Task<GeneralResponse<SalesOrderDTO>> AddSalesOrderByWarehouseIdAsync(string userId, AddSalesOrderDTO dto)
    {

        var suppler = await _context.Customers.FirstOrDefaultAsync(p => p.CustomerId == dto.CustomerId);

        if (suppler == null)
            return GeneralResponse<SalesOrderDTO>.FailResponse("Customer is not found");


        var mapping = new SalesOrder
        {
            Status = dto.IsDraft ? SalesOrderStatus.Draft : SalesOrderStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            CustomerId = dto.CustomerId,
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        // ✅ شغل الـ Approval Workflow لو مش Draft
        if (!dto.IsDraft)
        {

            await approval.StartProcessAsync(
                processType: ProcessType.Sales,
                referenceId: res.SalesOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }

        var model = new SalesOrderDTO
        {
            SalesOrderId = res.SalesOrderId,
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            Status = res.Status.ToString(),
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            Comment = res.Comment
        };

        return GeneralResponse<SalesOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesOrderDTO>> UpdateSalesOrderAsync(string userId, int salesOrderId, UpdateSalesOrderDTO dto)
    {
        var entity = await _context.SalesOrders.FirstOrDefaultAsync(e => e.SalesOrderId == dto.SalesOrderId);

        if (entity.SalesOrderId != salesOrderId)
        {
            return GeneralResponse<SalesOrderDTO>.FailResponse("id not equal sales order id!");
        }
        if (entity == null)
        {
            return GeneralResponse<SalesOrderDTO>.FailResponse("not found");
        }

        // 👇 التعديل الذكي، خاصية خاصية
        if (dto.PostingDate.HasValue && entity.PostingDate != dto.PostingDate.Value)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue && entity.DueDate != dto.DueDate.Value)
            entity.DueDate = dto.DueDate.Value;

        if (dto.CustomerId.HasValue && entity.CustomerId != dto.CustomerId.Value)
            entity.CustomerId = dto.CustomerId.Value;

      

        if (!string.IsNullOrWhiteSpace(dto.Comment) && entity.Comment != dto.Comment)
            entity.Comment = dto.Comment;

        if (entity.UserId != userId)
            entity.UserId = userId;



        if (!dto.IsDraft)
        {
                await approval.StartProcessAsync(
                    processType: ProcessType.Sales,
                    referenceId: entity.SalesOrderId,
                    warehouseId: entity.WarehouseId,
                    userId: userId
                );

            entity.Status = SalesOrderStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == SalesOrderStatus.Processing ? SalesOrderStatus.Processing : SalesOrderStatus.Draft;
        }
        await _context.SaveChangesAsync();


        var result = new SalesOrderDTO
        {
            SalesOrderId = entity.SalesOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            CustomerId = entity.CustomerId,
            Comment = entity.Comment
        };

       
        
        return GeneralResponse<SalesOrderDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<SalesOrder>> GetByCustomerIdAsync(int customerId)
    {
        return await Query().Where(so => so.CustomerId == customerId).ToListAsync();
    }

    public async Task<GeneralResponse<IEnumerable<SalesOrderDTO>>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<SalesOrderStatus>(status, out var statusEnum))
        {
            var query = await Query().Where(so => so.Status == statusEnum)
                .Select(so => new SalesOrderDTO
                {
                    DueDate = so.DueDate,
                    PostingDate = so.PostingDate,
                    SalesOrderId = so.SalesOrderId,
                    Status = so.Status.ToString(),
                    UserId = so.UserId,
                    WarehouseId = so.WarehouseId,
                    CustomerId = so.CustomerId,
                    Comment = so.Comment
                }).ToListAsync();

            return GeneralResponse<IEnumerable<SalesOrderDTO>>.SuccessResponse(query);
        }
        return GeneralResponse<IEnumerable<SalesOrderDTO>>.FailResponse("Not found any sales order to this status now!");
    }

    public async Task<IEnumerable<SalesOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(so => so.UserId == userId).ToListAsync();
    }

    public async Task<SalesOrder?> GetWithItemsAsync(int salesOrderId)
    {
        return await QueryIncluding(false, so => so.SalesOrderItems)
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);
    }

    public async Task<GeneralResponse<SalesOrderDTO>> GetWithCustomerAsync(int salesOrderId,string userId)
    {
           var res = await QueryIncluding(false, so => so.Customer)
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);

        if (res == null)
            return GeneralResponse<SalesOrderDTO>.FailResponse("this SalesOrderId is not found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.Sales, res.SalesOrderId);     

        var mapping = new SalesOrderDTO
        {
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
             SalesOrderId = res.SalesOrderId,
            Status = res.Status.ToString(),
            Comment = res.Comment,
            // string = enum
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerName = res.Customer.CustomerName,
            CustomerId = res.CustomerId,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Reason = approvalModel.Reason,

            //   Supplier = res.Supplier,
            // IsReceipt = res.ReceiptPurchaseOrder == null ? false : true,
            // ReceiptOrderId = res.ReceiptPurchaseOrder == null ? null : res.ReceiptPurchaseOrder.ReceiptPurchaseOrderId,
            // IsReturn = res.ReceiptPurchaseOrder == null ? false : (res.ReceiptPurchaseOrder.GoodsReturnOrder == null ? false : true),
            //  ReturnOrderId = res.ReceiptPurchaseOrder == null ? null : (res.ReceiptPurchaseOrder.GoodsReturnOrder == null ? null : res.ReceiptPurchaseOrder.GoodsReturnOrder.GoodsReturnOrderId)

        };


        return GeneralResponse<SalesOrderDTO>.SuccessResponse(mapping);

    }

    public async Task<SalesOrder?> GetWithWarehouseAsync(int salesOrderId)
    {
        return await QueryIncluding(false, so => so.Warehouse)
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);
    }

    public async Task<IEnumerable<SalesOrder>> GetPendingOrdersAsync()
    {
        return await Query().Where(so => so.Status == SalesOrderStatus.Processing).ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetDraftOrdersAsync()
    {
        return await Query().Where(so => so.Status == SalesOrderStatus.Draft).ToListAsync();
    }

    public async Task<IEnumerable<SalesOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(so => so.CreatedAt >= startDate && so.CreatedAt <= endDate).ToListAsync();
    }
}
