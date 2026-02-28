using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
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
    private readonly IBaseProcessesRepository<SalesReturnOrderItem> baseProcesses;
    private readonly ISalesReturnOrderRepository salesReturn;

   
    public SalesReturnOrderItemRepository(IBaseProcessesRepository<SalesReturnOrderItem> baseProcesses, ISalesReturnOrderRepository salesReturn, DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
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

    //
    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnItemBySalesReturnOrderIdWithoutRefAsync(
    int salesReturnOrderId,
    bool isBarcode,
    DynamicBarcodesDto? barcodeDto,
    AddGeneralItemDto? dto)
    {
        var res = await baseProcesses.AddOrderItemAsync<SalesReturnOrder, SalesReturnOrderItem>(
            salesReturnOrderId,
            ProcessType.SalesReturn,
            isBarcode,
            barcodeDto,
            dto,
            x => x.SalesReturnOrderId == salesReturnOrderId,
            _context.SalesReturnOrders,
            _context.SalesReturnOrderItems
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse(res.Message);

        var modelfin = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderId = res.Data.SalesReturnOrderId,
            Quantity = res.Data.Quantity,
            ItemId = res.Data.ItemId,
            SalesReturnOrderItemId = res.Data.SalesReturnOrderItemId,
            UoMEntry = res.Data.UoMEntry,
            BarCode = res.Data.BarCode,
            UnitPrice = res.Data.UnitPrice,
            ErrorMessage = res.Data.ErrorMessage
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(modelfin);
    }

    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> UpdateSalesReturnItemWithoutRefAsync(
    int salesReturnOrderItemId,
    UpdateGeneralItemDto dto)
    {
        var res = await baseProcesses.UpdateOrderItemAsync<SalesReturnOrderItem>(
            itemIdFromRoute: salesReturnOrderItemId,
            processType: ProcessType.SalesReturn,
            dto: dto,
            itemSelector: x => x.SalesReturnOrderItemId == salesReturnOrderItemId,
            itemSet: _context.SalesReturnOrderItems
        );

        if (!res.Success)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse(res.Message);

        var entity = res.Data;

        var result = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderId = entity.SalesReturnOrderId,
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            Quantity = entity.Quantity,
            ItemId = entity.ItemId,
            SalesOrderItemId = entity.SalesOrderItemId,
            BarCode = entity.BarCode,
            UoMEntry = entity.UoMEntry,
            ErrorMessage = entity.ErrorMessage,
            UnitPrice = entity.UnitPrice
        };

        return GeneralResponse<SalesReturnOrderItemDTO>.SuccessResponse(result);
    }


    public async Task<GeneralResponse<SalesReturnOrderItemDTO>> AddSalesReturnOrderItemBySalesOrderItemIdAsync(
    string userId,
    int salesOrderId,
    AddSalesReturnOrderItemDTO dto)
    {
        // Validate SalesOrder exists
        var salesOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);

        if (salesOrder == null)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order not found");

        // لو مفيش SalesReturnOrder، اعمله
        if (salesOrder.SalesReturnOrder == null)
        {
            var modelReturnOrder = new AddSalesReturnOrderDTO
            {
                SalesOrderId = salesOrderId,
                Comment = dto.Comment
            };

            await salesReturn.AddSalesReturnOrderAsync(userId, modelReturnOrder);
        }

        // reload
        salesOrder = await _context.SalesOrders
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);

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
            SalesReturnOrderId = salesOrder.SalesReturnOrder.SalesReturnOrderId,
            SalesOrderItemId = dto.SalesOrderItemId,
            ItemId = salesOrderItem.ItemId,
            Quantity = dto.Quantity,
            UoMEntry = salesOrderItem.UoMEntry,
            BarCode = salesOrderItem.BarCode,
            UnitPrice = salesOrderItem.UnitPrice,
        };

        var res = await AddAsync(salesReturnOrderItem);
        await SaveChangesAsync();

        // Automatically add batches from SalesOrderBatch (نفس لوجيك GoodsReturn)
        if (salesOrderItem.SalesOrderBatches != null && salesOrderItem.SalesOrderBatches.Any())
        {
            var batchesToAdd = new List<SalesReturnOrderBatch>();
            decimal remainingQuantity = dto.Quantity;

            foreach (var salesBatch in salesOrderItem.SalesOrderBatches.OrderBy(b => b.CreatedAt))
            {
                decimal batchQuantity = remainingQuantity > salesBatch.Quantity ? salesBatch.Quantity : remainingQuantity;

                if (remainingQuantity <= 0)
                    batchQuantity = 0;

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

        if (entity.SalesReturnOrderItemId != salesReturnOrderItemId)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("ID mismatch");

        // Validate quantity doesn't exceed sales item quantity
        if (dto.Quantity.HasValue && dto.Quantity.Value > entity.SalesOrderItem.Quantity)
            return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Return quantity cannot exceed sales quantity");

        // بعد تحديث الكمية:
        if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
        {
            entity.Quantity = dto.Quantity.Value;

            // 🧹 احذف الباتشات القديمة
            var existingBatches = await _context.SalesReturnOrderBatches
                .Where(b => b.SalesReturnOrderItemId == entity.SalesReturnOrderItemId)
                .ToListAsync();

            if (existingBatches.Any())
            {
                _context.SalesReturnOrderBatches.RemoveRange(existingBatches);
                await _context.SaveChangesAsync();
            }

            // 🔄 رجّع SalesOrderItem وباتشاته
            var salesOrderItem = await _context.SalesOrderItems
                .Include(soi => soi.SalesOrderBatches)
                .FirstOrDefaultAsync(soi => soi.SalesOrderItemId == entity.SalesOrderItemId);

            if (salesOrderItem == null)
                return GeneralResponse<SalesReturnOrderItemDTO>.FailResponse("Sales Order Item not found");

            // ✅ إعادة بناء الباتشات
            var newBatches = new List<SalesReturnOrderBatch>();
            decimal remainingQty = dto.Quantity.Value;

            foreach (var salesBatch in salesOrderItem.SalesOrderBatches.OrderBy(b => b.CreatedAt))
            {
                decimal batchQty = Math.Min(salesBatch.Quantity, remainingQty);

                if (batchQty <= 0)
                    batchQty = 0;

                newBatches.Add(new SalesReturnOrderBatch
                {
                    SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
                    SalesOrderBatchId = salesBatch.SalesOrderBatchId,
                    Quantity = batchQty,
                    BatchNumber = salesBatch.BatchNumber,
                    ExpiryDate = salesBatch.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    Comment = dto.Comment
                });

                remainingQty -= batchQty;
            }

            if (newBatches.Any())
            {
                await _context.SalesReturnOrderBatches.AddRangeAsync(newBatches);
                await _context.SaveChangesAsync();
            }
        }

       

        await _context.SaveChangesAsync();

        var result = new SalesReturnOrderItemDTO
        {
            SalesReturnOrderItemId = entity.SalesReturnOrderItemId,
            Quantity = entity.Quantity,
            UoMEntry = entity.UoMEntry,
            BarCode = entity.BarCode,
            UnitPrice = entity.UnitPrice,
            ErrorMessage = entity.ErrorMessage,
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

