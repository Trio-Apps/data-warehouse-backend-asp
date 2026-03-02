using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderRepository : BaseRepository<ProductionOrder>, IProductionOrderRepository
{
    private readonly IApprovalRepository _approval;

    public ProductionOrderRepository(
        IApprovalRepository approval,
        DataWarehouseDbContext context) : base(context)
    {
        _approval = approval;
    }

    public async Task<GeneralResponse<PagedResult<ProductionOrderDTO>>> GetListAsync(string userId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var userWarehouseIds = _context.UserWarehouses
            .Where(x => x.UserId == userId)
            .Select(x => x.WarehouseId);

        var query = _context.ProductionOrders
            .AsNoTracking()
            .Include(x => x.ProductionOrderItems)
            .Where(x => userWarehouseIds.Contains(x.WarehouseId));

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.ProductionOrderId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductionOrderDTO
            {
                ProductionOrderId = x.ProductionOrderId,
                PostingDate = x.PostingDate,
                DueDate = x.DueDate,
                Remarks = x.Remarks,
                Status = x.Status.ToString(),
                WarehouseId = x.WarehouseId,
                UserId = x.UserId,
                NumberOfProductionItem = x.ProductionOrderItems.Count
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

    public async Task<GeneralResponse<ProductionOrderDTO>> GetDetailsAsync(string userId, int productionOrderId)
    {
        var entity = await _context.ProductionOrders
            .AsNoTracking()
            .Include(x => x.ProductionOrderItems)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (entity == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        var progress = await _approval.GetProcessItem(entity.ProductionOrderId, ProcessType.Production);

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            Remarks = entity.Remarks,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            NumberOfProductionItem = entity.ProductionOrderItems.Count,
            Approval = progress != null,
            ApprovalStatus = progress?.Status.ToString(),
            CanSubmit = entity.Status == GeneralStatus.Draft
        });
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> CreateAsync(string userId, AddProductionOrderDTO dto)
    {
        var validation = await ValidateOrderDataAsync(userId, dto.WarehouseId, dto.PostingDate, dto.DueDate);
        if (!validation.Success)
            return GeneralResponse<ProductionOrderDTO>.FailResponse(validation.Message);

        var entity = new ProductionOrder
        {
            Status = GeneralStatus.Draft,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            Remarks = dto.Remarks?.Trim(),
            WarehouseId = dto.WarehouseId,
            UserId = userId
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            Remarks = entity.Remarks,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            NumberOfProductionItem = 0,
            CanSubmit = true
        }, "Production order added successfully.");
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> UpdateAsync(string userId, int productionOrderId, UpdateProductionOrderDTO dto)
    {
        var entity = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
            .Include(x => x.ProductionComponentLines)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (entity == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.Status == GeneralStatus.Processing || entity.ProductionOrderItems.Any(i => i.Status == GeneralItemStatus.Released))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Cannot update order after it reached processing/released stage.");

        var targetWarehouseId = dto.WarehouseId ?? entity.WarehouseId;
        var validation = await ValidateOrderDataAsync(userId, targetWarehouseId, dto.PostingDate, dto.DueDate);
        if (!validation.Success)
            return GeneralResponse<ProductionOrderDTO>.FailResponse(validation.Message);

        if (targetWarehouseId != entity.WarehouseId)
        {
            foreach (var orderItem in entity.ProductionOrderItems)
            {
                var itemExistsInWarehouse = await _context.WarehouseItems
                    .AsNoTracking()
                    .AnyAsync(x => x.WarehouseId == targetWarehouseId && x.ItemId == orderItem.ItemId);

                if (!itemExistsInWarehouse)
                    return GeneralResponse<ProductionOrderDTO>.FailResponse($"Item {orderItem.ItemId} is not available in the selected warehouse.");
            }

            foreach (var componentLine in entity.ProductionComponentLines)
            {
                var warehouseItem = await _context.WarehouseItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.WarehouseId == targetWarehouseId && x.ItemId == componentLine.ItemId);

                if (warehouseItem == null)
                    return GeneralResponse<ProductionOrderDTO>.FailResponse($"Component item {componentLine.ItemId} is not available in the selected warehouse.");

                componentLine.WarehouseId = targetWarehouseId;
                componentLine.InWhsQuantity = Convert.ToDecimal(warehouseItem.InStock ?? 0);
            }
        }

        entity.PostingDate = dto.PostingDate;
        entity.DueDate = dto.DueDate;
        entity.Remarks = dto.Remarks?.Trim();
        entity.WarehouseId = targetWarehouseId;
        entity.UserId = userId;

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            Remarks = entity.Remarks,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            NumberOfProductionItem = entity.ProductionOrderItems.Count,
            CanSubmit = entity.Status == GeneralStatus.Draft
        }, "Production order updated successfully.");
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> DeleteProductionOrderAsync(string userId, int productionOrderId)
    {
        var entity = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (entity == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        var progress = await _approval.GetProcessItem(entity.ProductionOrderId, ProcessType.Production);
        if (progress != null && progress.Status == ProcessStatus.Approved)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Cannot delete approved order.");

        var alreadySynced = entity.ProductionOrderItems.Any(i => i.AbsoluteEntry.HasValue);
        if (alreadySynced)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Cannot delete order that is already synced to SAP.");

        var snapshot = new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            Remarks = entity.Remarks,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            NumberOfProductionItem = entity.ProductionOrderItems.Count
        };

        _context.ProductionOrders.Remove(entity);
        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(
            snapshot,
            "Production order deleted successfully.");
    }

    public async Task<GeneralResponse<ProductionOrderDTO>> SubmitAsync(string userId, int productionOrderId, SubmitProductionOrderDTO? dto = null)
    {
        var entity = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
            .Include(x => x.ProductionHeaderBatches)
            .Include(x => x.ProductionComponentLines)
                .ThenInclude(x => x.ProductionComponentBatches)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (entity == null)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.WarehouseId))
            return GeneralResponse<ProductionOrderDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.Status != GeneralStatus.Draft)
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Only draft orders can be submitted.");

        if (!entity.ProductionOrderItems.Any())
            return GeneralResponse<ProductionOrderDTO>.FailResponse("Cannot submit production order without items.");

        foreach (var item in entity.ProductionOrderItems)
        {
            var warehouseItem = await _context.WarehouseItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WarehouseId == entity.WarehouseId && x.ItemId == item.ItemId);

            if (warehouseItem == null)
                return GeneralResponse<ProductionOrderDTO>.FailResponse($"Item {item.ItemId} is not available in warehouse.");

            if (!warehouseItem.IsActive || !warehouseItem.FinishedGood || !warehouseItem.HasActiveBOM)
                return GeneralResponse<ProductionOrderDTO>.FailResponse(BuildProductionEligibilityMessage(warehouseItem.ItemId, warehouseItem.IsActive, warehouseItem.FinishedGood, warehouseItem.HasActiveBOM));
        }

        var hasBatchManagedItem = await _context.WarehouseItems
            .AsNoTracking()
            .AnyAsync(x => x.WarehouseId == entity.WarehouseId
                        && entity.ProductionOrderItems.Select(i => i.ItemId).Contains(x.ItemId)
                        && x.IsBatchManaged);

        if (hasBatchManagedItem)
        {
            if (!entity.ProductionHeaderBatches.Any())
                return GeneralResponse<ProductionOrderDTO>.FailResponse("Batch-managed item requires header batches before submit.");

            var plannedQty = entity.ProductionOrderItems.Sum(x => x.PlannedQuantity);
            var batchQty = entity.ProductionHeaderBatches.Sum(x => x.Quantity);
            if (plannedQty != batchQty)
                return GeneralResponse<ProductionOrderDTO>.FailResponse("Header batches quantity must equal planned quantity.");
        }

        foreach (var componentLine in entity.ProductionComponentLines.Where(x =>
                     string.Equals(x.IssueType, "Manual", StringComparison.OrdinalIgnoreCase)))
        {
            var componentIsBatchManaged = await _context.WarehouseItems
                .AsNoTracking()
                .AnyAsync(x => x.WarehouseId == componentLine.WarehouseId
                            && x.ItemId == componentLine.ItemId
                            && x.IsBatchManaged);

            if (!componentIsBatchManaged)
                continue;

            var totalBatchesQty = componentLine.ProductionComponentBatches.Sum(x => x.Quantity);
            if (totalBatchesQty != componentLine.RequiredQuantity)
            {
                return GeneralResponse<ProductionOrderDTO>.FailResponse(
                    $"Component line {componentLine.ProductionComponentLineId} batch quantities must equal required quantity.");
            }
        }

        entity.Status = GeneralStatus.Processing;
        entity.Remarks = string.IsNullOrWhiteSpace(dto?.Note) ? entity.Remarks : dto!.Note!.Trim();
        entity.UserId = userId;

        try
        {
            await _approval.StartProcessAsync(
                processType: ProcessType.Production,
                referenceId: entity.ProductionOrderId,
                warehouseId: entity.WarehouseId,
                userId: userId);
        }
        catch (Exception ex)
        {
            return GeneralResponse<ProductionOrderDTO>.FailResponse(ex.Message);
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderDTO>.SuccessResponse(new ProductionOrderDTO
        {
            ProductionOrderId = entity.ProductionOrderId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            Remarks = entity.Remarks,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            NumberOfProductionItem = entity.ProductionOrderItems.Count,
            Approval = true,
            ApprovalStatus = ProcessStatus.InProgress.ToString(),
            CanSubmit = false
        });
    }

    private async Task<(bool Success, string Message)> ValidateOrderDataAsync(string userId, int warehouseId, DateTime postingDate, DateTime dueDate)
    {
        if (postingDate == default || dueDate == default)
            return (false, "PostingDate and DueDate are required.");

        if (dueDate.Date < postingDate.Date)
            return (false, "DueDate must be greater than or equal to PostingDate.");

        var warehouseExists = await _context.Warehouses.AsNoTracking().AnyAsync(x => x.WarehouseId == warehouseId);
        if (!warehouseExists)
            return (false, "Warehouse not found.");

        if (!await UserHasWarehouseAccessAsync(userId, warehouseId))
            return (false, "You don't have access to this warehouse.");

        return (true, string.Empty);
    }

    private static string BuildProductionEligibilityMessage(int itemId, bool isActive, bool finishedGood, bool hasActiveBOM)
    {
        var reasons = new List<string>();

        if (!isActive)
            reasons.Add("inactive");

        if (!finishedGood)
            reasons.Add("not marked as finished good");

        if (!hasActiveBOM)
            reasons.Add("no active BOM");

        return reasons.Count == 0
            ? $"Item {itemId} must be active finished good with active BOM."
            : $"Item {itemId} is not eligible for production: {string.Join(", ", reasons)}.";
    }

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }
}
