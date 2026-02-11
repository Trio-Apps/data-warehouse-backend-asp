using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesReturnOrderItemRepository : BaseRepository<SalesReturnOrderItem>, ISalesReturnOrderItemRepository
{
    private readonly ISalesReturnOrderRepository salesReturn;

    public SalesReturnOrderItemRepository(ISalesReturnOrderRepository salesReturn, DataWarehouseDbContext context) : base(context)
    {
        this.salesReturn = salesReturn;
    }

    public async Task<GeneralResponse<IEnumerable<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdAsync(int salesReturnOrderId)
    {
        var res = await Query()
            .Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId)
            .Select(e => new SalesReturnOrderItemDTO
            {
                SalesReturnOrderItemId = e.SalesReturnOrderItemId,
                Quantity = e.Quantity,
                UoMEntry = e.UoMEntry,
                BarCode = e.BarCode,
                UnitPrice = e.UnitPrice,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),
                SalesReturnOrderId = e.SalesReturnOrderId,
                SalesOrderItemId = e.SalesOrderItemId,
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                UnitName = e.Item.ItemUomGroups.FirstOrDefault(i => i.UomEntry == e.UoMEntry).UomCode,

            })
            .ToListAsync();

        return GeneralResponse < IEnumerable < SalesReturnOrderItemDTO >>.SuccessResponse( res);
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderItemDTO>>> GetBySalesReturnOrderIdWithPaginationAsync(int salesReturnOrderId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrderItems
            .AsNoTracking()
            .Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId);

        var totalRecords = await query.CountAsync();

        var data = query.Select(e => new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = e.SalesReturnOrderItemId,
            Quantity = e.Quantity,
            UoMEntry = e.UoMEntry,
            BarCode = e.BarCode,
            UnitPrice = e.UnitPrice,
            ErrorMessage = e.ErrorMessage,
            Status = e.Status.ToString(),
            SalesReturnOrderId = e.SalesReturnOrderId,
            SalesOrderItemId = e.SalesOrderItemId,
            ItemId = e.ItemId
        }).ToList();

        return GeneralResponse<PagedResult<SalesReturnOrderItemDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderItemDTO>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });
    }

    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnOrderItemBySalesOrderItemIdAsync(string userId,
        int salesOrderId,
        AddSalesReturnOrderItemDTO dto)
    {
        // Validate SalesReturnOrder exists
        var salesReturnOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(sro => sro.SalesOrderId == salesOrderId);
        if (salesReturnOrder == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order not found");


        var checkApprovalStatus = await GetProcessItem(salesOrderId, ProcessType.Sales);

        if (checkApprovalStatus == null ||  ( checkApprovalStatus != null && checkApprovalStatus.Status != ProcessStatus.Approved))
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("You cannot add any item to return because its approval status is not 'Approved' and all approval steps haven't been completed.");



        if (salesReturnOrder.SalesReturnOrder == null)
        {
            var modelGood = new AddSalesReturnOrderDTO
            {
                 SalesOrderId = salesOrderId,
                Comment = dto.Comment,
            };
            var addGoodOrder = await salesReturn.AddSalesReturnOrderBySalesOrderIdAsync(userId, modelGood);
        }

          salesReturnOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(sro => sro.SalesOrderId == salesOrderId);

        // Get SalesOrderItem with its batches
        var salesOrderItem = await _context.SalesOrderItems
            .Include(soi => soi.SalesOrderBatches)
            .Include(soi => soi.Item)
            .FirstOrDefaultAsync(soi => soi.SalesOrderItemId == dto.SalesOrderItemId);

        if (salesOrderItem == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order Item not found");

        // Check if this sales item already has a return item
        var existingReturnItem = await _context.SalesReturnOrderItems
            .FirstOrDefaultAsync(sroi => sroi.SalesOrderItemId == dto.SalesOrderItemId);

        if (existingReturnItem != null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("This Sales Order Item already has a return item");

        // Validate quantity doesn't exceed sales item quantity
        if (dto.Quantity > salesOrderItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed sales quantity");

        // Create SalesReturnOrderItem
        var salesReturnOrderItem = new SalesReturnOrderItem
        {
            SalesReturnOrderId = salesReturnOrder.SalesReturnOrder.SalesReturnOrderId,
            SalesOrderItemId = dto.SalesOrderItemId,
            ItemId = salesOrderItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = salesOrderItem.UoMEntry,
            BarCode = salesOrderItem.BarCode,
            UnitPrice = salesOrderItem.UnitPrice,
            Status = salesOrderItem.Status
        };

        var res = await AddAsync(salesReturnOrderItem);
        await SaveChangesAsync();

        // Automatically add batches from SalesOrderBatch
        if (salesOrderItem.SalesOrderBatches != null && salesOrderItem.SalesOrderBatches.Any())
        {
            var batchesToAdd = new List<SalesReturnOrderBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var salesBatch in salesOrderItem.SalesOrderBatches.OrderBy(b => b.CreatedAt))
            {
                if (remainingQuantity <= 0)
                    break;

                decimal batchQuantity = remainingQuantity > salesBatch.Quantity ? salesBatch.Quantity : remainingQuantity;

                var returnBatch = new SalesReturnOrderBatch
                {
                    SalesReturnOrderItemId = res.SalesReturnOrderItemId,
                    SalesOrderBatchId = salesBatch.SalesOrderBatchId,
                    Quantity = batchQuantity,
                    BatchNumber = salesBatch.BatchNumber,
                    ExpiryDate = salesBatch.ExpiryDate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                batchesToAdd.Add(returnBatch);
                remainingQuantity -= batchQuantity;
            }

            if (batchesToAdd.Any())
            {
                await _context.SalesReturnOrderBatches.AddRangeAsync(batchesToAdd);
                await SaveChangesAsync();
            }
        }

        // Reload with batches
        var finalItem = await _context.SalesReturnOrderItems
            .Include(sroi => sroi.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == res.SalesReturnOrderItemId);

        var model = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = finalItem.SalesReturnOrderItemId,
            Quantity = finalItem.Quantity,
            UoMEntry = finalItem.UoMEntry,
            BarCode = finalItem.BarCode,
            UnitPrice = finalItem.UnitPrice,
            ErrorMessage = finalItem.ErrorMessage,
            Status = finalItem.Status.ToString(),
            SalesReturnOrderId = finalItem.SalesReturnOrderId,
            SalesOrderItemId = finalItem.SalesOrderItemId,
            ItemId = finalItem.ItemId,
            Batches = finalItem.SalesReturnOrderBatches?.Select(b => new SalesReturnOrderBatchDTO
            {
                SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                SalesOrderBatchId = b.SalesOrderBatchId,
                Quantity = b.Quantity,
                Comment = b.Comment,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate
            }).ToList()
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnOrderItemAsync(
        int salesReturnOrderItemId,
        UpdateSalesReturnOrderItemDTO dto)
    {
        var entity = await _context.SalesReturnOrderItems
            .Include(sroi => sroi.SalesOrderItem)
            .FirstOrDefaultAsync(e => e.SalesReturnOrderItemId == salesReturnOrderItemId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Return Order Item not found");


        var checkApprovalStatus = await GetProcessItem(entity.SalesReturnOrderId, ProcessType.SalesReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("You cannot edit any return item because its approval status is 'Approved' and all approval steps have been completed.");


        if (entity.SalesReturnOrderItemId != salesReturnOrderItemId)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed sales item quantity
        if (dto.Quantity.HasValue && dto.Quantity.Value > entity.SalesOrderItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed sales quantity");

        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
            entity.Quantity = dto.Quantity.Value;

        await _context.SaveChangesAsync();

        var result = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
            Status = entity.Status.ToString(),
            SalesReturnOrderId = entity.SalesReturnOrderId,
            SalesOrderItemId = entity.SalesOrderItemId,
            ItemId = entity.ItemId
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(result);
    }

    public async Task<IEnumerable<SalesReturnOrderItem>> GetBySalesReturnOrderIdEntitiesAsync(int salesReturnOrderId)
    {
        return await Query().Where(sroi => sroi.SalesReturnOrderId == salesReturnOrderId).ToListAsync();
    }

    public async Task<IEnumerable<SalesReturnOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await Query().Where(sroi => sroi.ItemId == itemId).ToListAsync();
    }

    public async Task<SalesReturnOrderItem?> GetWithSalesReturnOrderAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.SalesReturnOrder)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithSalesOrderItemAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.SalesOrderItem)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithItemAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.Item)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }

    public async Task<SalesReturnOrderItem?> GetWithBatchesAsync(int salesReturnOrderItemId)
    {
        return await QueryIncluding(false, sroi => sroi.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(sroi => sroi.SalesReturnOrderItemId == salesReturnOrderItemId);
    }
}

