using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.Processes;
using DataWarehouse.Services.Repository.SapRepo;
using DataWarehouse.Services.Services.Processes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderRepository : BaseRepository<ProductionOrder>, IProductionOrderRepository
{
    private readonly IBaseProcessesRepository<ProductionOrder> baseProcesses;
    private readonly ISapCache sapCache;
    private readonly IProcessesTypesDateRepository processes;
    private readonly IApprovalRepository approval;
    private readonly ReasonValidationService reasonValidationService;

    public ProductionOrderRepository(
        IBaseProcessesRepository<ProductionOrder> baseProcesses,
        ISapCache sapCache,
        IProcessesTypesDateRepository processes,
        IApprovalRepository approval,
        ReasonValidationService reasonValidationService,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.sapCache = sapCache;
        this.processes = processes;
        this.approval = approval;
        this.reasonValidationService = reasonValidationService;
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

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Production);

        // ?? Filtering


       

        var totalRecords = await query.CountAsync();
        
        var data = await query

            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new
            {
                Order = iw,
                HasProgress = processQuery.Any(p => p.ReferenceId == iw.ProductionOrderId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == iw.ProductionOrderId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new ProductionOrderDTO
            {
                DueDate = x.Order.DueDate,
                PostingDate = x.Order.PostingDate,
                ProductionOrderId = x.Order.ProductionOrderId,
                Remarks = x.Order.Remarks,
                Status = x.Order.Status.ToString(),
                ReasonId = x.Order.ReasonId,
                ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,
                UserId = x.Order.UserId,
                WarehouseId = warehouseId,
                NumberOfProductionItem = x.Order.ProductionOrderItems.Count(),
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
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
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Production);
       // var sapId = await sapCache.Get();

      //  var checkValidDate = await ValidateBusinessDatesAsync(dto.PostingDate,dto.DueDate);

        //if (!checkValidDate.IsValid)
        //    return GeneralResponse<ProductionOrderDTO>.FailResponse($"{checkValidDate.Message}");

        if (!await UserHasWarehouseAccessAsync(userId, dto.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        var mapping = new ProductionOrder
        {
           Status = GeneralStatus.Draft,
            PostingDate = dto.PostingDate,
            
             DueDate = dto.DueDate,
                 CreatedAt = DateTime.UtcNow,
               UserId = userId, 
                WarehouseId =dto.WarehouseId,
                 Remarks = dto.Remarks,
                 ReasonId = dto.ReasonId,
        };

        var res = await AddAsync(mapping);
        await SaveChangesAsync();

        var model = new ProductionOrderDTO
        {
             ProductionOrderId = res.ProductionOrderId,
            DueDate = res.DueDate,
             PostingDate = res.PostingDate, 
            Status = res.Status.ToString(), // <-- Â‰« »‰ÕÊ· «·‹ enum · string
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null
             
        };

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<IEnumerable<ProductionOrderDTO>>> AddProductionOrdersByWarehouseIdAsync(string userId,
           IEnumerable<AddProductionOrderDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return GeneralResponse<IEnumerable<ProductionOrderDTO>>.FailResponse("No production orders were provided.");

        // Validate access and required fields per order.
        foreach (var dto in dtos)
        {
            if (!await UserHasWarehouseAccessAsync(userId, dto.WarehouseId))
                return GeneralResponse<IEnumerable<ProductionOrderDTO>>.FailResponse("You don't have access to one or more warehouses.");
        }

        var createdOrders = new List<ProductionOrderDTO>();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var dto in dtos)
            {
                // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Production);
                var mapping = new ProductionOrder
                {
                    Status = GeneralStatus.Draft,
                    PostingDate = dto.PostingDate,
                    DueDate = dto.DueDate,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    WarehouseId = dto.WarehouseId,
                    Remarks = dto.Remarks,
                    ReasonId = dto.ReasonId,
                };

                var res = await AddAsync(mapping);

                createdOrders.Add(new ProductionOrderDTO
                {
                    ProductionOrderId = res.ProductionOrderId,
                    DueDate = res.DueDate,
                    PostingDate = res.PostingDate,
                    Status = res.Status.ToString(),
                    UserId = res.UserId,
                    WarehouseId = res.WarehouseId,
                    Remarks = res.Remarks,
                    ReasonId = res.ReasonId,
                    ReasonName = res.Reason != null ? res.Reason.Name : null
                });
            }

            await SaveChangesAsync();
            await transaction.CommitAsync();

            return GeneralResponse<IEnumerable<ProductionOrderDTO>>.SuccessResponse(createdOrders);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return GeneralResponse<IEnumerable<ProductionOrderDTO>>.FailResponse($"Failed to create bulk production orders: {ex.Message}");
        }
    }
    private async Task<(bool IsValid, string Message)> ValidateBusinessDatesAsync(
      DateTime postingDate,
      DateTime dueDate)
    {
        // 1?? ÃÌ» «·‹ valid business dates
        var validBusinessDates = await processes.GetByProcessesTypeForProductionAsync();

        // If business dates are not configured yet, do not block editing/saving.
        // This keeps production draft updates usable in fresh environments.
        if (!validBusinessDates.Any())
        {
            return (true, "No business dates configured. Validation skipped.");
        }

        // 2?? ÕÊ· ·‹ DateOnly
        var postingDateOnly = DateOnly.FromDateTime(postingDate);
        var dueDateOnly = DateOnly.FromDateTime(dueDate);

        // 3??  ‘Ìﬂ PostingDate
        var isPostingDateValid = validBusinessDates.Any(d => d.PostingDate == postingDateOnly);

        // 4??  ‘Ìﬂ DueDate
        var isDueDateValid = validBusinessDates.Any(d => d.DueDate == dueDateOnly);

        // 5?? —Ã⁄ «·‰ ÌÃ… „⁄ —”«·… Ê«÷Õ…
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
        // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.Production);
        // 1?? Get existing Company auth (record «·ÊÕÌœ)
        var entity = await _context.ProductionOrders.FirstOrDefaultAsync(e => e.ProductionOrderId == productionId);

        if (entity == null)
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse("not found");
        }

        if (dto.ProductionOrderId != productionId )
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse("id not equal production order id!");
        }

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        var checkValidDate = await ValidateBusinessDatesAsync(dto.PostingDate, dto.DueDate);

        if (!checkValidDate.IsValid)
            return GeneralResponse<ProductionOrderDTO>.FailResponse($"{checkValidDate.Message}");


        // 2?? Update fields


        entity.DueDate = dto.DueDate;
         entity.PostingDate = dto.PostingDate;
        entity.UserId  = userId;
        entity.ReasonId = dto.ReasonId;
        
       // Company.IsActive = dto.IsActive;

        // 3?? Save changes
        await _context.SaveChangesAsync();

        // 4?? Map to DTO
        var result = new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(), // <-- Â‰« »‰ÕÊ· «·‹ enum · string
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        };

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> SubmitProductionOrderAsync(string userId, int productionOrderId, string? note = null)
    {
        var order = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
            .Include(x => x.ProductionHeaderBatches)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (order == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        if (order.Status != GeneralStatus.Draft)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Only Draft orders can be submitted.");

        if (order.ProductionOrderItems == null || !order.ProductionOrderItems.Any())
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order must contain at least one finished good item.");

        var plannedQty = order.ProductionOrderItems.Sum(x => x.PlannedQuantity);
        if (plannedQty <= 0)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Planned quantity must be greater than 0.");

        var finishedGoodItemIds = order.ProductionOrderItems
            .Select(x => x.ItemId)
            .Distinct()
            .ToList();

        var batchManagedFinishedGoods = await _context.WarehouseItems
            .AsNoTracking()
            .Where(x => x.WarehouseId == order.WarehouseId
                        && x.IsBatchManaged
                        && finishedGoodItemIds.Contains(x.ItemId))
            .Select(x => x.ItemId)
            .ToListAsync();

        if (batchManagedFinishedGoods.Count > 0)
        {
            if (order.ProductionHeaderBatches == null || !order.ProductionHeaderBatches.Any())
                return GeneralResponse<ProductionOrderDTO>.FailResponse("Batch-managed finished good requires header batches before submit.");

            var totalHeaderQty = order.ProductionHeaderBatches.Sum(x => x.Quantity);
            if (!AreEqual(totalHeaderQty, plannedQty))
                return GeneralResponse<ProductionOrderDTO>.FailResponse("Header batch quantity total must equal finished good quantity.");
        }

        try
        {
            await approval.StartProcessAsync(
                processType: ProcessType.Production,
                referenceId: order.ProductionOrderId,
                warehouseId: order.WarehouseId,
                userId: userId
            );
        }
        catch (Exception ex)
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse(ex.Message);
        }

        order.Status = GeneralStatus.Processing;
        order.UserId = userId;

        if (!string.IsNullOrWhiteSpace(note))
        {
            var cleanNote = note.Trim();
            order.Remarks = string.IsNullOrWhiteSpace(order.Remarks)
                ? cleanNote
                : $"{order.Remarks}{Environment.NewLine}{cleanNote}";
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = order.ProductionOrderId,
            DueDate = order.DueDate,
            PostingDate = order.PostingDate,
            Status = order.Status.ToString(),
            Remarks = order.Remarks,
            UserId = order.UserId,
            WarehouseId = order.WarehouseId,
            NumberOfProductionItem = order.ProductionOrderItems.Count,
            ReasonId = order.ReasonId,
            ReasonName = order.Reason != null ? order.Reason.Name : null
        });
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> DuplicateProductionOrderAsync(
        string userId,
        int productionOrderId,
        CancellationToken cancellationToken = default)
    {
        var source = await _context.ProductionOrders
            .AsNoTracking()
            .Include(x => x.ProductionOrderItems)
            .Include(x => x.ProductionHeaderBatches)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId, cancellationToken);
        if (source == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("not found");
        if (!await UserHasWarehouseAccessAsync(userId, source.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");
        var clone = OrderDuplicationHelper.Clone(source, userId);
        await _context.ProductionOrders.AddAsync(clone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = clone.ProductionOrderId,
            PostingDate = clone.PostingDate,
            DueDate = clone.DueDate,
            Remarks = clone.Remarks,
            Status = clone.Status.ToString(),
            UserId = clone.UserId,
            WarehouseId = clone.WarehouseId,
            NumberOfProductionItem = clone.ProductionOrderItems.Count,
            ReasonId = clone.ReasonId,
            ReasonName = clone.Reason != null ? clone.Reason.Name : null
        });
    }    public async Task<IEnumerable<ProductionOrder>> GetByItemIdAsync(int itemId)
    {
        return await QueryIncluding(false, po => po.ProductionOrderItems)
            .Where(po => po.ProductionOrderItems.Any(poi => poi.ItemId == itemId))
            .ToListAsync();
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int productionOrderId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<ProductionOrder>(
            productionOrderId,
            ProcessType.Production,
            x => x.ProductionOrderId == productionOrderId,
            _context.ProductionOrders
        );
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

    public async Task<GeneralResponse<PagedResult<ProductionOrderDTO>>> SearchProductionOrdersAsync(string userId, string? query, int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        if (!await UserHasWarehouseAccessAsync(userId, warehouseId))
            return GeneralResponse<PagedResult<ProductionOrderDTO>>.FailResponse("You don't have access to this warehouse.");

        var queryable = _context.ProductionOrders
            .AsNoTracking()
            .Where(po => po.WarehouseId == warehouseId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(po =>
                po.Remarks.Contains(query) ||
                po.ProductionOrderId.ToString().Contains(query));
        }

        var totalRecords = await queryable.CountAsync();

        var data = await queryable
            .OrderByDescending(po => po.ProductionOrderId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(po => new ProductionOrderDTO
            {
                ProductionOrderId = po.ProductionOrderId,
                PostingDate = po.PostingDate,
                DueDate = po.DueDate,
                Remarks = po.Remarks,
                Status = po.Status.ToString(),
                UserId = po.UserId,
                WarehouseId = po.WarehouseId,
                NumberOfProductionItem = po.ProductionOrderItems.Count(),
                ReasonId = po.ReasonId,
                ReasonName = po.Reason != null ? po.Reason.Name : null
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionOrderDTO>>.SuccessResponse(new PagedResult<ProductionOrderDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<PagedResult<ProductionOrderReportItemDto>>> GetProductionOrdersReportAsync(
        string userId,
        ProductionOrderReportFilterDto filter)
    {
        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(x => x.WarehouseId == filter.WarehouseId);

        if (!warehouseExists)
            return GeneralResponse<PagedResult<ProductionOrderReportItemDto>>.FailResponse("Warehouse not found.");

        if (!await UserHasWarehouseAccessAsync(userId, filter.WarehouseId))
            return GeneralResponse<PagedResult<ProductionOrderReportItemDto>>.FailResponse("You don't have access to this warehouse.");

        GeneralStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (!Enum.TryParse<GeneralStatus>(filter.Status.Trim(), true, out var parsedStatus))
                return GeneralResponse<PagedResult<ProductionOrderReportItemDto>>.FailResponse("Invalid production order status.");

            statusFilter = parsedStatus;
        }

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.Production);

        var query = _context.ProductionOrders
            .AsNoTracking()
            .Where(po => po.WarehouseId == filter.WarehouseId);

        if (filter.FromDate.HasValue)
        {
            var fromDate = filter.FromDate.Value.Date;
            query = query.Where(po => po.PostingDate >= fromDate);
        }

        if (filter.ToDate.HasValue)
        {
            var toExclusive = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(po => po.PostingDate < toExclusive);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(po => po.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(po =>
                po.ProductionOrderId.ToString().Contains(term) ||
                (po.Remarks != null && po.Remarks.Contains(term)) ||
                po.ProductionOrderItems.Any(item =>
                    item.Item.ItemCode.Contains(term) ||
                    item.Item.ItemName.Contains(term)));
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(po => po.PostingDate)
            .ThenByDescending(po => po.ProductionOrderId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(po => new
            {
                Order = po,
                HasProgress = processQuery.Any(p => p.ReferenceId == po.ProductionOrderId),
                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == po.ProductionOrderId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new ProductionOrderReportItemDto
            {
                ProductionOrderId = x.Order.ProductionOrderId,
                PostingDate = x.Order.PostingDate,
                DueDate = x.Order.DueDate,
                Status = x.Order.Status.ToString(),
                Remarks = x.Order.Remarks,
                WarehouseId = x.Order.WarehouseId,
                WarehouseCode = x.Order.Warehouse.WarehouseCode,
                NumberOfProductionItems = x.Order.ProductionOrderItems.Count(),
                TotalPlannedQuantity = x.Order.ProductionOrderItems.Sum(item => item.PlannedQuantity),
                TotalProducedQuantity = x.Order.ProductionOrderItems.Sum(item => item.ProducedQuantity ?? 0),
                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionOrderReportItemDto>>.SuccessResponse(new PagedResult<ProductionOrderReportItemDto>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }

    private static bool AreEqual(decimal left, decimal right)
    {
        return Math.Abs(left - right) <= 0.000001m;
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

