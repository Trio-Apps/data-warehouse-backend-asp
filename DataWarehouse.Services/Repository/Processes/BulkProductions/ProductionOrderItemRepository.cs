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

    public async Task<IEnumerable<ProductionOrderItemDTO>> GetByProductionItemByProductionOrderIdAsync(int productionOrderId)
    {
        var res = await Query()
            .Where(poi => poi.ProductionOrderId == productionOrderId)
            .ToListAsync();

        return res.Select(e => new ProductionOrderItemDTO
        {
            ItemId = e.ItemId,
            AbsoluteEntry = e.AbsoluteEntry,
            Status = GetEnumString(e.Status),
            ErrorMessage = e.ErrorMessage,
            PlannedQuantity = e.PlannedQuantity,
            ProducedQuantity = e.ProducedQuantity ?? 0,
            ProductionOrderItemId = e.ProductionOrderItemId,
            ProductionOrderId = e.ProductionOrderId,
            ProcessedAt = e.ProcessedAt
        });
    }

    public async Task<GeneralResponse<PagedResult<ProductionOrderItemDTO>>> GetByProductionItemByProductionOrderIdWithPaginationAsync(
        int productionOrderId,
        string? status,
        int pageNumber,
        int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.ProductionOrderItems
            .AsNoTracking()
            .Where(b => b.ProductionOrderId == productionOrderId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GeneralItemStatus>(status, true, out var statusEnum))
        {
            query = query.Where(iw => iw.Status == statusEnum);
        }

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.ProductionOrderItemId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ProductionOrderItemDTO
            {
                ItemId = e.ItemId,
                AbsoluteEntry = e.AbsoluteEntry,
                Status = e.Status.ToString(),
                ErrorMessage = e.ErrorMessage,
                PlannedQuantity = e.PlannedQuantity,
                ProducedQuantity = e.ProducedQuantity ?? 0,
                ProductionOrderItemId = e.ProductionOrderItemId,
                ProductionOrderId = e.ProductionOrderId,
                ProcessedAt = e.ProcessedAt
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

    public async Task<GeneralResponse<ProductionOrderItemDTO>> AddProductionItemByProductionOrderIdAsync(
        int productionOrderid,
        AddProductionOrderItemDTO dto)
    {
        if (productionOrderid != dto.ProductionOrderId)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Route ProductionOrderId does not match request body.");

        var order = await _context.ProductionOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        if (order == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production order not found.");

        if (!await HasActiveBomInWarehouseAsync(order.WarehouseId, dto.ItemId))
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse(
                "Selected finished good does not have an active Production BOM in the selected warehouse.");
        }

        var existingForOrder = await _context.ProductionOrderItems
            .FirstOrDefaultAsync(x => x.ProductionOrderId == dto.ProductionOrderId);

        ProductionOrderItem mapping;
        if (existingForOrder != null)
        {
            existingForOrder.ItemId = dto.ItemId;
            existingForOrder.PlannedQuantity = dto.PlannedQuantity;
            existingForOrder.Status = GeneralItemStatus.Planned;
            existingForOrder.ErrorMessage = null;
            existingForOrder.ProducedQuantity = null;
            existingForOrder.AbsoluteEntry = null;
            existingForOrder.ProcessedAt = null;
            mapping = existingForOrder;
        }
        else
        {
            mapping = new ProductionOrderItem
            {
                Status = GeneralItemStatus.Planned,
                ProductionOrderId = dto.ProductionOrderId,
                ItemId = dto.ItemId,
                CreatedAt = DateTime.UtcNow,
                PlannedQuantity = dto.PlannedQuantity,
            };

            await AddAsync(mapping);
        }

        await SaveChangesAsync();

        var model = new ProductionOrderItemDTO
        {
            ProductionOrderId = mapping.ProductionOrderId,
            PlannedQuantity = mapping.PlannedQuantity,
            Status = GetEnumString(mapping.Status),
            ItemId = mapping.ItemId,
            ProductionOrderItemId = mapping.ProductionOrderItemId,
        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(model, "Finished good saved successfully.");
    }

    public async Task<GeneralResponse<ProductionOrderItemDTO>> UpdateProductionItemAsync(
        int productionItemId,
        bool? isRecevied,
        UpdateProductionOrderItemDTO dto)
    {
        var entity = await _context.ProductionOrderItems
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(e => e.ProductionOrderItemId == productionItemId);

        if (entity == null)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("not found");

        if (dto.ProductionOrderItemId != productionItemId)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("id not equal production order id!");

        if (entity.Status == GeneralItemStatus.Closed || entity.Status == GeneralItemStatus.Failed)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Closed or failed items cannot be edited.");

        if (!await HasActiveBomInWarehouseAsync(entity.ProductionOrder.WarehouseId, entity.ItemId))
        {
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse(
                "Selected finished good does not have an active Production BOM in the selected warehouse.");
        }

        if (isRecevied == true && entity.Status != GeneralItemStatus.Released)
            return GeneralResponse<ProductionOrderItemDTO>.FailResponse("Production item must be released before marking it as received.");

        entity.PlannedQuantity = dto.PlannedQuantity;
        entity.ProducedQuantity = dto.ProducedQuantity;

        if (isRecevied == true)
            entity.Status = GeneralItemStatus.Received;

        await _context.SaveChangesAsync();

        var result = new ProductionOrderItemDTO
        {
            ItemId = entity.ItemId,
            AbsoluteEntry = entity.AbsoluteEntry,
            Status = GetEnumString(entity.Status),
            ErrorMessage = entity.ErrorMessage,
            PlannedQuantity = entity.PlannedQuantity,
            ProducedQuantity = entity.ProducedQuantity ?? 0,
            ProductionOrderItemId = entity.ProductionOrderItemId,
            ProductionOrderId = entity.ProductionOrderId,
            ProcessedAt = entity.ProcessedAt,
        };

        return GeneralResponse<ProductionOrderItemDTO>.SuccessResponse(result, "Finished good updated successfully.");
    }

    private async Task<bool> HasActiveBomInWarehouseAsync(int warehouseId, int itemId)
    {
        return await _context.WarehouseItems
            .AsNoTracking()
            .AnyAsync(x =>
                x.WarehouseId == warehouseId &&
                x.ItemId == itemId &&
                x.IsActive &&
                x.HasActiveBOM);
    }

    private string GetEnumString(GeneralItemStatus status)
    {
        switch (status)
        {
            case GeneralItemStatus.Draft:
                return "Draft";
            case GeneralItemStatus.Planned:
                return "Planned";
            case GeneralItemStatus.Released:
                return "Released";
            case GeneralItemStatus.Received:
                return "Received";
            case GeneralItemStatus.Closed:
                return "Closed";
            case GeneralItemStatus.Failed:
                return "Failed";
            default:
                return "Unknown";
        }
    }
}
