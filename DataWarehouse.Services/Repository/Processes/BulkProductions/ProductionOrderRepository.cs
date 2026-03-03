using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderRepository : BaseRepository<ProductionOrder>, IProductionOrderRepository
{
    private readonly ISapCache sapCache;
    private readonly IProcessesTypesDateRepository processes;

    public ProductionOrderRepository(ISapCache sapCache,IProcessesTypesDateRepository processes,  DataWarehouseDbContext context) : base(context)
    {
        this.sapCache = sapCache;
        this.processes = processes;
    }

    public async Task<IEnumerable<ProductionOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(po => po.WarehouseId == warehouseId).ToListAsync();
    }
    public async Task<GeneralResponse<PagedResult<ProductionOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId,int pageNumber, int pageSize)

    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.Warehouses.Where(e => e.WarehouseId == warehouseId)
            .AsNoTracking()
            .SelectMany(e => e.ProductionOrders)
            .Include(e=>e.ProductionOrderItems);

        // 🔹 Filtering


       

        var totalRecords = await query.CountAsync();
        
        var data = await query

            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new ProductionOrderDTO
            {
                DueDate = iw.DueDate,
                PostingDate = iw.PostingDate,
                ProductionOrderId = iw.ProductionOrderId,
                Remarks = iw.Remarks,
                Status = iw.Status.ToString(),
                // string = enum
                UserId = iw.UserId,
                WarehouseId = warehouseId,
                NumberOfProductionItem = iw.ProductionOrderItems.Count(),

            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionOrderDTO>>.SuccessResponse(
            new PagedResult<ProductionOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<List<NameStatus>>> GetProductionOrderStatus()
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
    public async Task<GeneralResponse<ProductionOrderDTO>> AddProductionOrderByWarehouseIdAsync(string userId,
           AddProductionOrderDTO dto)
    {
       // var sapId = await sapCache.Get();

      //  var checkValidDate = await ValidateBusinessDatesAsync(dto.PostingDate,dto.DueDate);

        //if (!checkValidDate.IsValid)
        //    return GeneralResponse<ProductionOrderDTO>.FailResponse($"{checkValidDate.Message}");

       
        var mapping = new ProductionOrder
        {
           Status = GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            
             DueDate = dto.DueDate,
              CreatedAt = DateTime.UtcNow,
               UserId = userId, 
                WarehouseId =dto.WarehouseId,
                 Remarks = dto.Remarks,     
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ProductionOrderDTO
        {
             ProductionOrderId = res.ProductionOrderId,
            DueDate = res.DueDate,
             PostingDate = res.PostingDate, 
            Status = res.Status.ToString() // <-- هنا بنحول الـ enum ل string
            
        };

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(model);
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
    public async Task<GeneralResponse<ProductionOrderDTO>> UpdateProductionOrderAsync(string userId,int productionId, UpdateProductionOrderDTO dto)
    {

        // 1️⃣ Get existing Company auth (record الوحيد)
        var entity = await _context.ProductionOrders.FirstOrDefaultAsync(e => e.ProductionOrderId == dto.ProductionOrderId);

        if (entity.ProductionOrderId != productionId )
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse("id not equal production order id!");
        }
        if (entity == null)
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse("not found");
        }

        var checkValidDate = await ValidateBusinessDatesAsync(dto.PostingDate, dto.DueDate);

        if (!checkValidDate.IsValid)
            return GeneralResponse<ProductionOrderDTO>.FailResponse($"{checkValidDate.Message}");


        // 2️⃣ Update fields


        entity.DueDate = dto.DueDate;
         entity.PostingDate = dto.PostingDate;
        entity.UserId  = userId;
        
       // Company.IsActive = dto.IsActive;

        // 3️⃣ Save changes
        await _context.SaveChangesAsync();

        // 4️⃣ Map to DTO
        var result = new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString() // <-- هنا بنحول الـ enum ل string
        };

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<ProductionOrder>> GetByItemIdAsync(int itemId)
    {
        return await QueryIncluding(false, po => po.ProductionOrderItems)
            .Where(po => po.ProductionOrderItems.Any(poi => poi.ItemId == itemId))
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByStatusAsync(string status)
    {
        if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
        {
            return await Query().Where(po => po.Status == statusEnum).ToListAsync();
        }
        return new List<ProductionOrder>();
    }

    public async Task<IEnumerable<ProductionOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(po => po.UserId == userId).ToListAsync();
    }

    public async Task<ProductionOrder?> GetWithItemsAsync(int productionOrderId)
    {
        return await QueryIncluding(false, po => po.ProductionOrderItems)
            .FirstOrDefaultAsync(po => po.ProductionOrderId == productionOrderId);
    }

    public async Task<ProductionOrder?> GetWithWarehouseAsync(int productionOrderId)
    {
        return await QueryIncluding(false, po => po.Warehouse)
            .FirstOrDefaultAsync(po => po.ProductionOrderId == productionOrderId);
    }

    public async Task<IEnumerable<ProductionOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query().Where(po => po.CreatedAt >= startDate && po.CreatedAt <= endDate).ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetPendingOrdersAsync()
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
}

