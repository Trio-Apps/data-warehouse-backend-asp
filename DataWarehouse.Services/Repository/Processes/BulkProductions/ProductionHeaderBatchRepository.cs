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
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.BulkProductions;

public class ProductionHeaderBatchRepository : BaseRepository<ProductionHeaderBatch>, IProductionHeaderBatchRepository
{
    public ProductionHeaderBatchRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<PagedResult<ProductionHeaderBatchDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var order = await _context.ProductionOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (order == null)
            return GeneralResponse<PagedResult<ProductionHeaderBatchDTO>>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<PagedResult<ProductionHeaderBatchDTO>>.FailResponse("You don't have access to this warehouse.");


        var query = _context.ProductionHeaderBatches
            .AsNoTracking()
            .Where(x => x.ProductionOrderId == productionOrderId);


        var totalRecords = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ProductionHeaderBatchId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductionHeaderBatchDTO
            {
                ProductionHeaderBatchId = x.ProductionHeaderBatchId,
                ProductionOrderId = x.ProductionOrderId,
                Quantity = x.Quantity,
                BatchNumber = x.BatchNumber,
                ExpiryDate = x.ExpiryDate
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionHeaderBatchDTO>>.SuccessResponse(new PagedResult<ProductionHeaderBatchDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<ProductionHeaderBatchDTO>> GetByIdDetailsAsync(string userId, int productionHeaderBatchId)
    {
        var entity = await _context.ProductionHeaderBatches
            .AsNoTracking()
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionHeaderBatchId == productionHeaderBatchId);

        if (entity == null)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Production header batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("You don't have access to this warehouse.");

        return GeneralResponse<ProductionHeaderBatchDTO>.SuccessResponse(
            ToDto(entity),
            "Production header batch added successfully.");
    }

    public async Task<GeneralResponse<ProductionHeaderBatchDTO>> CreateAsync(string userId, AddProductionHeaderBatchDTO dto)
    {
        if (dto.Quantity <= 0)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Quantity must be greater than 0.");

        var order = await _context.ProductionOrders
            .Include(x => x.ProductionOrderItems)
          //  .Include(x => x.ProductionHeaderBatches)
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        if (order == null)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (order.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Cannot add batches after order enters processing.");

        var hasBatchManagedItems = await HasBatchManagedItemsAsync(order.ProductionOrderId, order.WarehouseId);
        if (!hasBatchManagedItems)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("No batch-managed items found for this production order.");

        var plannedQty = order.ProductionOrderItems.Sum(x => x.PlannedQuantity);
      //  var currentBatchQty = order.ProductionHeaderBatches.Sum(x => x.Quantity);
        //if (currentBatchQty + dto.Quantity > plannedQty)
        //    return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Header batches quantity cannot exceed planned quantity.");

        var entity = new ProductionHeaderBatch
        {
            ProductionOrderId = dto.ProductionOrderId,
            Quantity = dto.Quantity,
            BatchNumber = dto.BatchNumber.Trim(),
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        return GeneralResponse<ProductionHeaderBatchDTO>.SuccessResponse(
            ToDto(entity),
            "Production header batch updated successfully.");
    }

    public async Task<GeneralResponse<ProductionHeaderBatchDTO>> UpdateAsync(string userId, int productionHeaderBatchId, UpdateProductionHeaderBatchDTO dto)
    {
        if (dto.Quantity <= 0)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Quantity must be greater than 0.");

        var entity = await _context.ProductionHeaderBatches
            .Include(x => x.ProductionOrder)
                .ThenInclude(x => x.ProductionOrderItems)
            .Include(x => x.ProductionOrder)
               // .ThenInclude(x => x.ProductionHeaderBatches)
            .FirstOrDefaultAsync(x => x.ProductionHeaderBatchId == productionHeaderBatchId);

        if (entity == null)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Production header batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Cannot update batches after order enters processing.");

        var plannedQty = entity.ProductionOrder.ProductionOrderItems.Sum(x => x.PlannedQuantity);
        //var otherBatchQty = entity.ProductionOrder.ProductionHeaderBatches
        //    .Where(x => x.ProductionHeaderBatchId != entity.ProductionHeaderBatchId)
        //    .Sum(x => x.Quantity);

        //if (otherBatchQty + dto.Quantity > plannedQty)
        //    return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Header batches quantity cannot exceed planned quantity.");

        entity.Quantity = dto.Quantity;
        entity.BatchNumber = dto.BatchNumber.Trim();
        entity.ExpiryDate = dto.ExpiryDate;

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionHeaderBatchDTO>.SuccessResponse(ToDto(entity));
    }

    public async Task<GeneralResponse<ProductionHeaderBatchDTO>> DeleteAsync(string userId, int productionHeaderBatchId)
    {
        var entity = await _context.ProductionHeaderBatches
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.ProductionHeaderBatchId == productionHeaderBatchId);

        if (entity == null)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Production header batch not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionHeaderBatchDTO>.FailResponse("Cannot delete batches after order enters processing.");

        var result = ToDto(entity);
        _context.ProductionHeaderBatches.Remove(entity);
        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionHeaderBatchDTO>.SuccessResponse(
            result,
            "Production header batch deleted successfully.");
    }

    private async Task<bool> HasBatchManagedItemsAsync(int productionOrderId, int warehouseId)
    {
        var orderItemIds = _context.ProductionOrderItems
            .Where(x => x.ProductionOrderId == productionOrderId)
            .Select(x => x.ItemId);

        return await _context.WarehouseItems
            .AsNoTracking()
            .AnyAsync(x => x.WarehouseId == warehouseId && x.IsBatchManaged && orderItemIds.Contains(x.ItemId));
    }

    private static ProductionHeaderBatchDTO ToDto(ProductionHeaderBatch entity)
    {
        return new ProductionHeaderBatchDTO
        {
            ProductionHeaderBatchId = entity.ProductionHeaderBatchId,
            ProductionOrderId = entity.ProductionOrderId,
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
