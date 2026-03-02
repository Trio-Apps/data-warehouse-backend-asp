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

public class ProductionComponentBatchRepository : BaseRepository<ProductionComponentBatch>, IProductionComponentBatchRepository
{
    public ProductionComponentBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<PagedResult<ProductionComponentBatchDTO>>> GetListAsync(string userId, int productionComponentLineId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var componentLine = await _context.ProductionComponentLines
            .AsNoTracking()
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == productionComponentLineId);

        if (componentLine == null)
            return GeneralResponse<PagedResult<ProductionComponentBatchDTO>>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, componentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<PagedResult<ProductionComponentBatchDTO>>.FailResponse("You don't have access to this warehouse.");

        var query = _context.ProductionComponentBatches
            .AsNoTracking()
            .Where(x => x.ProductionComponentLineId == productionComponentLineId);

        var totalRecords = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ProductionComponentBatchId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductionComponentBatchDTO
            {
                ProductionComponentBatchId = x.ProductionComponentBatchId,
                ProductionComponentLineId = x.ProductionComponentLineId,
                Quantity = x.Quantity,
                BatchNumber = x.BatchNumber,
                ExpiryDate = x.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionComponentBatchDTO>>.SuccessResponse(new PagedResult<ProductionComponentBatchDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<IEnumerable<AvailableComponentBatchDTO>>> GetAvailableBatchesAsync(string userId, int productionComponentLineId)
    {
        var componentLine = await _context.ProductionComponentLines
            .AsNoTracking()
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == productionComponentLineId);

        if (componentLine == null)
            return GeneralResponse<IEnumerable<AvailableComponentBatchDTO>>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, componentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<IEnumerable<AvailableComponentBatchDTO>>.FailResponse("You don't have access to this warehouse.");

        var receivedBatches = await _context.ReceiptPurchaseOrderBatches
            .AsNoTracking()
            .Where(x => x.BatchNumber != null
                        && x.ReceiptPurchaseOrderItem.ItemId == componentLine.ItemId
                        && x.ReceiptPurchaseOrderItem.ReceiptPurchaseOrder.WarehouseId == componentLine.WarehouseId)
            .GroupBy(x => new { x.BatchNumber, x.ExpiryDate })
            .Select(g => new
            {
                BatchNumber = g.Key.BatchNumber!,
                g.Key.ExpiryDate,
                QuantityReceived = g.Sum(x => x.Quantity)
            })
            .ToListAsync();

        var allocatedInOrder = await _context.ProductionComponentBatches
            .AsNoTracking()
            .Where(x => x.ProductionComponentLine.ProductionOrderId == componentLine.ProductionOrderId
                        && x.ProductionComponentLine.ItemId == componentLine.ItemId
                        && x.ProductionComponentLine.WarehouseId == componentLine.WarehouseId
                        && x.BatchNumber != null)
            .GroupBy(x => x.BatchNumber)
            .Select(g => new
            {
                BatchNumber = g.Key!,
                QuantityAllocated = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(x => x.BatchNumber, x => x.QuantityAllocated);

        var result = receivedBatches
            .Select(x =>
            {
                allocatedInOrder.TryGetValue(x.BatchNumber, out var allocated);
                var available = x.QuantityReceived - allocated;
                return new AvailableComponentBatchDTO
                {
                    BatchNumber = x.BatchNumber,
                    ExpiryDate = x.ExpiryDate,
                    QuantityReceived = x.QuantityReceived,
                    QuantityAllocatedInOrder = allocated,
                    QuantityAvailable = available > 0 ? available : 0
                };
            })
            .OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue)
            .ThenBy(x => x.BatchNumber)
            .ToList();

        return GeneralResponse<IEnumerable<AvailableComponentBatchDTO>>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProductionComponentBatchDTO>> GetByIdDetailsAsync(string userId, int productionComponentBatchId)
    {
        var entity = await _context.ProductionComponentBatches
            .AsNoTracking()
            .Include(x => x.ProductionComponentLine)
                .ThenInclude(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionComponentBatchId == productionComponentBatchId);

        if (entity == null)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Production component batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionComponentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("You don't have access to this warehouse.");

        return GeneralResponse<ProductionComponentBatchDTO>.SuccessResponse(ToDto(entity));
    }

    public async Task<GeneralResponse<ProductionComponentBatchDTO>> CreateAsync(string userId, AddProductionComponentBatchDTO dto)
    {
        if (dto.Quantity <= 0)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Quantity must be greater than 0.");

        var componentLine = await _context.ProductionComponentLines
            .Include(x => x.ProductionOrder)
            .Include(x => x.ProductionComponentBatches)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == dto.ProductionComponentLineId);

        if (componentLine == null)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, componentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (componentLine.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Cannot add component batches after order enters processing.");

        if (!string.Equals(componentLine.IssueType, "Manual", StringComparison.OrdinalIgnoreCase))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Component batches are only allowed for components with IssueType = Manual.");

        var isBatchManaged = await _context.WarehouseItems
            .AsNoTracking()
            .AnyAsync(x => x.WarehouseId == componentLine.WarehouseId && x.ItemId == componentLine.ItemId && x.IsBatchManaged);

        if (!isBatchManaged)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Component item is not batch-managed in the selected warehouse.");

        var currentBatchQty = componentLine.ProductionComponentBatches.Sum(x => x.Quantity);
        if (currentBatchQty + dto.Quantity > componentLine.RequiredQuantity)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Component batch quantities cannot exceed required quantity.");

        var entity = new ProductionComponentBatch
        {
            ProductionComponentLineId = dto.ProductionComponentLineId,
            Quantity = dto.Quantity,
            BatchNumber = dto.BatchNumber.Trim(),
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        return GeneralResponse<ProductionComponentBatchDTO>.SuccessResponse(
            ToDto(entity),
            "Production component batch added successfully.");
    }

    public async Task<GeneralResponse<ProductionComponentBatchDTO>> UpdateAsync(string userId, int productionComponentBatchId, UpdateProductionComponentBatchDTO dto)
    {
        if (dto.Quantity <= 0)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Quantity must be greater than 0.");

        var entity = await _context.ProductionComponentBatches
            .Include(x => x.ProductionComponentLine)
                .ThenInclude(x => x.ProductionOrder)
            .Include(x => x.ProductionComponentLine)
                .ThenInclude(x => x.ProductionComponentBatches)
            .FirstOrDefaultAsync(x => x.ProductionComponentBatchId == productionComponentBatchId);

        if (entity == null)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Production component batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionComponentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionComponentLine.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Cannot update component batches after order enters processing.");

        if (!string.Equals(entity.ProductionComponentLine.IssueType, "Manual", StringComparison.OrdinalIgnoreCase))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Component batches are only allowed for components with IssueType = Manual.");

        var otherQty = entity.ProductionComponentLine.ProductionComponentBatches
            .Where(x => x.ProductionComponentBatchId != entity.ProductionComponentBatchId)
            .Sum(x => x.Quantity);

        if (otherQty + dto.Quantity > entity.ProductionComponentLine.RequiredQuantity)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Component batch quantities cannot exceed required quantity.");

        entity.Quantity = dto.Quantity;
        entity.BatchNumber = dto.BatchNumber.Trim();
        entity.ExpiryDate = dto.ExpiryDate;

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionComponentBatchDTO>.SuccessResponse(
            ToDto(entity),
            "Production component batch updated successfully.");
    }

    public async Task<GeneralResponse<ProductionComponentBatchDTO>> DeleteAsync(string userId, int productionComponentBatchId)
    {
        var entity = await _context.ProductionComponentBatches
            .Include(x => x.ProductionComponentLine)
                .ThenInclude(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionComponentBatchId == productionComponentBatchId);

        if (entity == null)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Production component batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionComponentLine.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionComponentLine.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentBatchDTO>.FailResponse("Cannot delete component batches after order enters processing.");

        var result = ToDto(entity);
        _context.ProductionComponentBatches.Remove(entity);
        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionComponentBatchDTO>.SuccessResponse(
            result,
            "Production component batch deleted successfully.");
    }

    private static ProductionComponentBatchDTO ToDto(ProductionComponentBatch entity)
    {
        return new ProductionComponentBatchDTO
        {
            ProductionComponentBatchId = entity.ProductionComponentBatchId,
            ProductionComponentLineId = entity.ProductionComponentLineId,
            Quantity = entity.Quantity,
            BatchNumber = entity.BatchNumber,
            ExpiryDate = entity.ExpiryDate
        };
    }

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }
}
