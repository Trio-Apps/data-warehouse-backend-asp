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

public class ProductionComponentLineRepository : BaseRepository<ProductionComponentLine>, IProductionComponentLineRepository
{
    public ProductionComponentLineRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<GeneralResponse<PagedResult<ProductionComponentLineDTO>>> GetListAsync(string userId, int productionOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var order = await _context.ProductionOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductionOrderId == productionOrderId);

        if (order == null)
            return GeneralResponse<PagedResult<ProductionComponentLineDTO>>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<PagedResult<ProductionComponentLineDTO>>.FailResponse("You don't have access to this warehouse.");

        var query = _context.ProductionComponentLines
            .AsNoTracking()
            .Where(x => x.ProductionOrderId == productionOrderId);

        var totalRecords = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.ProductionComponentLineId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductionComponentLineDTO
            {
                ProductionComponentLineId = x.ProductionComponentLineId,
                ProductionOrderId = x.ProductionOrderId,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.ItemName,
                WarehouseId = x.WarehouseId,
                RequiredQuantity = x.RequiredQuantity,
                IssuedQuantity = x.IssuedQuantity,
                InWhsQuantity = x.InWhsQuantity,
                IssueType = x.IssueType
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<ProductionComponentLineDTO>>.SuccessResponse(new PagedResult<ProductionComponentLineDTO>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        });
    }

    public async Task<GeneralResponse<ProductionComponentLineDTO>> GetByIdDetailsAsync(string userId, int productionComponentLineId)
    {
        var entity = await _context.ProductionComponentLines
            .AsNoTracking()
            .Include(x => x.ProductionOrder)
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == productionComponentLineId);

        if (entity == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("You don't have access to this warehouse.");

        return GeneralResponse<ProductionComponentLineDTO>.SuccessResponse(ToDto(entity));
    }

    public async Task<GeneralResponse<ProductionComponentLineDTO>> CreateAsync(string userId, AddProductionComponentLineDTO dto)
    {
        if (dto.RequiredQuantity <= 0)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Required quantity must be greater than 0.");

        var issueType = NormalizeIssueType(dto.IssueType);
        if (issueType == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("IssueType must be either Manual or Backflush.");

        var order = await _context.ProductionOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        if (order == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Production order not found.");

        if (!await UserHasWarehouseAccessAsync(userId, order.WarehouseId))
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("You don't have access to this warehouse.");

        if (order.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Cannot add component lines after order enters processing.");

        if (dto.WarehouseId != order.WarehouseId)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Component line warehouse must match order warehouse.");

        var itemExists = await _context.Items.AsNoTracking().AnyAsync(x => x.ItemId == dto.ItemId);
        if (!itemExists)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Component item not found.");

        var warehouseItem = await _context.WarehouseItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WarehouseId == order.WarehouseId && x.ItemId == dto.ItemId);

        if (warehouseItem == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Component item is not available in the selected warehouse.");

        var entity = new ProductionComponentLine
        {
            ProductionOrderId = dto.ProductionOrderId,
            ItemId = dto.ItemId,
            WarehouseId = order.WarehouseId,
            RequiredQuantity = dto.RequiredQuantity,
            IssuedQuantity = null,
            InWhsQuantity = Convert.ToDecimal(warehouseItem.InStock ?? 0),
            IssueType = issueType,
            CreatedAt = DateTime.UtcNow
        };

        await AddAsync(entity);
        await SaveChangesAsync();

        return GeneralResponse<ProductionComponentLineDTO>.SuccessResponse(
            ToDto(entity),
            "Production component line added successfully.");
    }

    public async Task<GeneralResponse<ProductionComponentLineDTO>> UpdateAsync(string userId, int productionComponentLineId, UpdateProductionComponentLineDTO dto)
    {
        if (dto.RequiredQuantity <= 0)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Required quantity must be greater than 0.");

        var issueType = NormalizeIssueType(dto.IssueType);
        if (issueType == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("IssueType must be either Manual or Backflush.");

        var entity = await _context.ProductionComponentLines
            .Include(x => x.ProductionOrder)
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == productionComponentLineId);

        if (entity == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Cannot update component lines after order enters processing.");

        if (dto.WarehouseId != entity.ProductionOrder.WarehouseId)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Component line warehouse must match order warehouse.");

        if (string.Equals(entity.IssueType, "Backflush", StringComparison.OrdinalIgnoreCase)
            && dto.RequiredQuantity != entity.RequiredQuantity)
        {
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Required quantity can only be edited for components with IssueType = Manual.");
        }

        if (dto.IssuedQuantity.HasValue && dto.IssuedQuantity > dto.RequiredQuantity)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Issued quantity cannot exceed required quantity.");

        var warehouseItem = await _context.WarehouseItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WarehouseId == dto.WarehouseId && x.ItemId == entity.ItemId);

        if (warehouseItem == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Component item is not available in the selected warehouse.");

        entity.WarehouseId = dto.WarehouseId;
        entity.RequiredQuantity = dto.RequiredQuantity;
        entity.IssuedQuantity = dto.IssuedQuantity;
        entity.IssueType = issueType;
        entity.InWhsQuantity = Convert.ToDecimal(warehouseItem.InStock ?? 0);

        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionComponentLineDTO>.SuccessResponse(
            ToDto(entity),
            "Production component line updated successfully.");
    }

    public async Task<GeneralResponse<ProductionComponentLineDTO>> DeleteAsync(string userId, int productionComponentLineId)
    {
        var entity = await _context.ProductionComponentLines
            .Include(x => x.ProductionOrder)
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.ProductionComponentLineId == productionComponentLineId);

        if (entity == null)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Production component line not found.");

        if (!await UserHasWarehouseAccessAsync(userId, entity.ProductionOrder.WarehouseId))
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("You don't have access to this warehouse.");

        if (entity.ProductionOrder.Status == GeneralStatus.Processing)
            return GeneralResponse<ProductionComponentLineDTO>.FailResponse("Cannot delete component lines after order enters processing.");

        var result = ToDto(entity);
        _context.ProductionComponentLines.Remove(entity);
        await _context.SaveChangesAsync();

        return GeneralResponse<ProductionComponentLineDTO>.SuccessResponse(
            result,
            "Production component line deleted successfully.");
    }

    private static string? NormalizeIssueType(string? issueType)
    {
        if (string.IsNullOrWhiteSpace(issueType))
            return "Backflush";

        if (string.Equals(issueType, "Manual", StringComparison.OrdinalIgnoreCase))
            return "Manual";

        if (string.Equals(issueType, "Backflush", StringComparison.OrdinalIgnoreCase))
            return "Backflush";

        return null;
    }

    private static ProductionComponentLineDTO ToDto(ProductionComponentLine entity)
    {
        return new ProductionComponentLineDTO
        {
            ProductionComponentLineId = entity.ProductionComponentLineId,
            ProductionOrderId = entity.ProductionOrderId,
            ItemId = entity.ItemId,
            ItemCode = entity.Item?.ItemCode,
            ItemName = entity.Item?.ItemName,
            WarehouseId = entity.WarehouseId,
            RequiredQuantity = entity.RequiredQuantity,
            IssuedQuantity = entity.IssuedQuantity,
            InWhsQuantity = entity.InWhsQuantity,
            IssueType = entity.IssueType
        };
    }

    private Task<bool> UserHasWarehouseAccessAsync(string userId, int warehouseId)
    {
        return _context.UserWarehouses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.WarehouseId == warehouseId);
    }
}
