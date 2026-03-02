using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionOrderItemRepository : BaseRepository<ProductionOrderItem>, IProductionOrderItemRepository
{
    public ProductionOrderItemRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var order = await _context.ProductionOrders.AsNoTracking().FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);
        if (order == null)
            return GeneralResponse<PagedResult<ProductionOrderItemDTO>>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<PagedResult<ProductionOrderItemDTO>>.FailResponse("You don't have access to this warehouse.");

        var query = _context.ProductionOrderItems
            .AsNoTracking()
            .Where(x => x.ProductionOrderId == productionOrderId);

        var totalRecords = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ProductionOrderItemId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductionOrderItemDTO
            {
                ProductionOrderItemId = x.ProductionOrderItemId,
                ProductionOrderId = x.ProductionOrderId,
                ItemId = x.ItemId,
                PlannedQuantity = x.PlannedQuantity,
                ProducedQuantity = x.ProducedQuantity,
                AbsoluteEntry = x.AbsoluteEntry,
                Status = x.Status.ToString(),
                ErrorMessage = x.ErrorMessage,
                ProcessedAt = x.ProcessedAt
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionOrderItemDTO>>.SuccessResponse(new PagedResult<ProductionOrderItemDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> GetByIdDetailsAsync(string userId, int productionOrderItemId)
    {
        var entity = await _context.ProductionOrderItems
            .AsNoTracking()
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionOrderItemId == productionOrderItemId);

        if (entity == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order item not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("You don't have access to this warehouse.");

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(ToDto(entity));
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> CreateAsync(string userId, AddProductionOrderItemDTO dto)
    {
        if (dto.PlannedQuantity <= 0)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Planned quantity must be greater than 0.");

        var order = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        if (order == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("You don't have access to this warehouse.");

        if (order.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Cannot add items when order is processing.");

        if (order.ProductionOrderItems.Any())
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Only one finished-good item is allowed per production order.");

        var warehouseItem = await _context.WarehouseItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WarehouseId == order.WarehouseId && x.ItemId == dto.ItemId);

        if (warehouseItem == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Item does not exist in selected warehouse.");

        if (!warehouseItem.IsActive || !warehouseItem.FinishedGood || !warehouseItem.HasActiveBOM)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse(BuildProductionEligibilityMessage(warehouseItem.ItemId, warehouseItem.IsActive, warehouseItem.FinishedGood, warehouseItem.HasActiveBOM));

        if (order.ProductionOrderItems.Any(x => x.ItemId == dto.ItemId))
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Duplicate item in the same production order is not allowed.");

        var entity = new ProductionOrderItem
        {
            ProductionOrderId = dto.ProductionOrderId,
            ItemId = dto.ItemId,
            PlannedQuantity = dto.PlannedQuantity,
            ProducedQuantity = null,
            Status = GeneralItemStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        var autoLoadedCount = await TryAutoLoadComponentLinesAsync(order.WarehouseId, entity);

        var message = autoLoadedCount > 0
            ? $"Production order item added successfully. {autoLoadedCount} component line(s) loaded automatically."
            : "Production order item added successfully.";

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(
            ToDto(entity),
            message);
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync(string userId, int productionItemId, UpdateProductionOrderItemDTO dto)
    {
        if (dto.PlannedQuantity <= 0)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Planned quantity must be greater than 0.");

        if (dto.ProducedQuantity.HasValue && dto.ProducedQuantity <= 0)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Produced quantity must be greater than 0.");

        var entity = await _context.ProductionOrderItems
            .Include(x => x.ProductionOrder)
            .ThenInclude(x => x.ProductionOrderItems)
            .Include(x => x.ProductionOrder)
            .ThenInclude(x => x.ProductionComponentLines)
            .FirstOrDefaultAsync(x => x.ProductionOrderItemId == productionItemId);

        if (entity == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order item not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing ||
            entity.Status == GeneralItemStatus.Released ||
            entity.Status == GeneralItemStatus.Received ||
            entity.Status == GeneralItemStatus.Closed)
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Cannot edit item after processing/released stage.");
        }

        var oldPlannedQuantity = entity.PlannedQuantity;
        entity.PlannedQuantity = dto.PlannedQuantity;
        entity.ProducedQuantity = dto.ProducedQuantity;

        if (entity.ProductionOrder.ProductionOrderItems.Count == 1)
        {
            RecalculateComponentRequiredQuantities(
                entity.ProductionOrder.ProductionComponentLines,
                oldPlannedQuantity,
                dto.PlannedQuantity);
        }

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(ToDto(entity));
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> DeleteProductionItemAsync(string userId, int productionItemId)
    {
        var entity = await _context.ProductionOrderItems
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionOrderItemId == productionItemId);

        if (entity == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order item not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.AbsoluteEntry.HasValue)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Cannot delete item that is already synced.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing ||
            entity.Status == GeneralItemStatus.Released ||
            entity.Status == GeneralItemStatus.Received ||
            entity.Status == GeneralItemStatus.Closed)
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Cannot delete item after processing/released stage.");
        }

        var result = ToDto(entity);
        _context.ProductionOrderItems.Remove(entity);
        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(
            result,
            "Production order item deleted successfully.");
    }

    private static ProductionOrderItemDTO ToDto(ProductionOrderItem entity)
    {
        return new ProductionOrderItemDTO
        {
            ProductionOrderItemId = entity.ProductionOrderItemId,
            ProductionOrderId = entity.ProductionOrderId,
            ItemId = entity.ItemId,
            PlannedQuantity = entity.PlannedQuantity,
            ProducedQuantity = entity.ProducedQuantity,
            AbsoluteEntry = entity.AbsoluteEntry,
            Status = entity.Status.ToString(),
            ErrorMessage = entity.ErrorMessage,
            ProcessedAt = entity.ProcessedAt
        };
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
            ? "Only active finished-good items with active BOM are allowed."
            : $"Item {itemId} is not eligible for production: {string.Join(", ", reasons)}.";
    }

    private static void RecalculateComponentRequiredQuantities(
        IEnumerable<ProductionComponentLine> componentLines,
        decimal oldPlannedQuantity,
        decimal newPlannedQuantity)
    {
        if (oldPlannedQuantity <= 0 || oldPlannedQuantity == newPlannedQuantity)
            return;

        var factor = newPlannedQuantity / oldPlannedQuantity;
        foreach (var line in componentLines)
        {
            var recalculatedRequired = decimal.Round(line.RequiredQuantity * factor, 6, MidpointRounding.AwayFromZero);
            line.RequiredQuantity = recalculatedRequired;

            if (line.IssuedQuantity.HasValue && line.IssuedQuantity > recalculatedRequired)
                line.IssuedQuantity = recalculatedRequired;
        }
    }

    private async Task<int> TryAutoLoadComponentLinesAsync(int warehouseId, ProductionOrderItem currentOrderItem)
    {
        var existingLinesCount = await _context.ProductionComponentLines
            .AsNoTracking()
            .CountAsync(x => x.ProductionOrderId == currentOrderItem.ProductionOrderId);

        if (existingLinesCount > 0)
            return 0;

        var templateSource = await (
                from poi in _context.ProductionOrderItems.AsNoTracking()
                join po in _context.ProductionOrders.AsNoTracking()
                    on poi.ProductionOrderId equals po.ProductionOrderId
                where poi.ItemId == currentOrderItem.ItemId
                      && po.WarehouseId == warehouseId
                      && poi.ProductionOrderId != currentOrderItem.ProductionOrderId
                orderby poi.ProductionOrderItemId descending
                select new
                {
                    poi.ProductionOrderId,
                    poi.PlannedQuantity
                })
            .FirstOrDefaultAsync();

        if (templateSource == null || templateSource.PlannedQuantity <= 0)
            return 0;

        var templateLines = await _context.ProductionComponentLines
            .AsNoTracking()
            .Where(x => x.ProductionOrderId == templateSource.ProductionOrderId)
            .ToListAsync();

        if (!templateLines.Any())
            return 0;

        var scaleFactor = currentOrderItem.PlannedQuantity / templateSource.PlannedQuantity;

        var componentItemIds = templateLines
            .Select(x => x.ItemId)
            .Distinct()
            .ToList();

        var inStockByItemId = await _context.WarehouseItems
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId && componentItemIds.Contains(x.ItemId))
            .ToDictionaryAsync(x => x.ItemId, x => Convert.ToDecimal(x.InStock ?? 0));

        var newLines = templateLines
            .Select(x =>
            {
                inStockByItemId.TryGetValue(x.ItemId, out var inStock);

                return new ProductionComponentLine
                {
                    ProductionOrderId = currentOrderItem.ProductionOrderId,
                    ItemId = x.ItemId,
                    WarehouseId = warehouseId,
                    RequiredQuantity = decimal.Round(x.RequiredQuantity * scaleFactor, 6, MidpointRounding.AwayFromZero),
                    IssuedQuantity = null,
                    InWhsQuantity = inStock,
                    IssueType = string.IsNullOrWhiteSpace(x.IssueType) ? "Backflush" : x.IssueType,
                    CreatedAt = DateTime.UtcNow
                };
            })
            .ToList();

        if (!newLines.Any())
            return 0;

        await _context.ProductionComponentLines.AddRangeAsync(newLines);
        await _context.SaveChangesAsync();

        return newLines.Count;
    }

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }
}
