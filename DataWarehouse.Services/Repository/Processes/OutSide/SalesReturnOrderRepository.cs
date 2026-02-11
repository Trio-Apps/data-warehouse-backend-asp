using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace DataWarehouse.Services.Repository.Processes.OutSide;

public class SalesReturnOrderRepository : BaseRepository<SalesReturnOrder>, ISalesReturnOrderRepository
{
    private readonly IApprovalRepository approval;

    public SalesReturnOrderRepository(IApprovalRepository approval, DataWarehouseDbContext context) : base(context)
    {
        this.approval = approval;
    }

    public async Task<IEnumerable<SalesReturnOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Where(sro => sro.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrders
            .AsNoTracking()
            .Where(sro => sro.WarehouseId == warehouseId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(sro => new SalesReturnOrderDTO
            {
                SalesReturnOrderId = sro.SalesReturnOrderId,
                UserId = sro.UserId,
                WarehouseId = warehouseId,
                CustomerId = sro.CustomerId,
                SalesOrderId = sro.SalesOrderId,
                WarehouseCode = sro.Warehouse.WarehouseCode,
                CustomerName = sro.Customer.CustomerName
            })
            .ToListAsync();

        return GeneralResponse<PagedResult<SalesReturnOrderDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderBySalesOrderIdAsync(string userId, AddSalesReturnOrderDTO dto)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.SalesReturnOrder)
            .FirstOrDefaultAsync(so => so.SalesOrderId == dto.SalesOrderId);

        if (salesOrder == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order is not found");


        if (salesOrder.SalesReturnOrder != null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order already has a Sales Return Order!");


        var salesReturnOrder = new SalesReturnOrder
        {
            UserId = userId,
            WarehouseId = salesOrder.WarehouseId,
            CustomerId = salesOrder.CustomerId,
            SalesOrderId = dto.SalesOrderId,
            Status = GeneralStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var res = await AddAsync(salesReturnOrder);
        await SaveChangesAsync();

        var model = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            SalesOrderId = res.SalesOrderId
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> UpdateSalesReturnOrderAsync(string userId, int salesReturnOrderId, UpdateSalesReturnOrderDTO dto)
    {
        var entity = await _context.SalesReturnOrders.FirstOrDefaultAsync(e => e.SalesReturnOrderId == dto.SalesReturnOrderId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Return Order not found");


        if (entity.SalesReturnOrderId != salesReturnOrderId)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("ID mismatch");

        #region like
        var checkApprovalStatus = await GetProcessItem(entity.SalesReturnOrderId, ProcessType.SalesReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("You cannot edit any sales return order because its approval status is 'Approved' and all approval steps have been completed.");


        // Update fields if needed
        entity.UserId = userId;
        entity.Comment = dto.Comment;



        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.SalesReturn,
                referenceId: entity.SalesReturnOrderId,
                warehouseId: entity.WarehouseId,
                userId: userId
            );

            entity.Status = GeneralStatus.Processing;
        }
        else
        {
            entity.Status = entity.Status == GeneralStatus.Processing ? GeneralStatus.Processing : GeneralStatus.Draft;
        }

        #endregion


     

        await _context.SaveChangesAsync();
        var result = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = entity.SalesReturnOrderId,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            CustomerId = entity.CustomerId,
            SalesOrderId = entity.SalesOrderId
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetWithCustomerAsync(int salesOrderId, string userId, CancellationToken cancellationToken = default)
    {
        var res = await _context.SalesReturnOrders.Include(so => so.Customer)
            .Include(s => s.SalesOrder)
            .FirstOrDefaultAsync(so => so.SalesOrderId == salesOrderId);

        if (res == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("this SalesOrderId is not found");


        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.SalesReturn, res.SalesReturnOrderId);

        var checkApprovalStatus = await approval.GetProcessItem(res.SalesReturnOrderId, ProcessType.SalesReturn, cancellationToken);


        bool hasProgress = checkApprovalStatus != null;

        string? approvalStatus = checkApprovalStatus?.Status.ToString();

        var mapping = new SalesReturnOrderDTO
        {
            DueDate = res.SalesOrder.DueDate,
            PostingDate = res.SalesOrder.PostingDate,
            SalesOrderId = res.SalesOrderId,
            Status = res.Status.ToString(),
            Comment = res.Comment,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SalesReturnOrderId = res.SalesReturnOrderId,
            CustomerName = res.Customer.CustomerName,
            CustomerId = res.CustomerId,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Reason = approvalModel.Reason,
            Approval = hasProgress,
            ApprovalStatus = checkApprovalStatus != null ? approvalStatus : null
        };


        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(mapping);

    }


    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetBySalesOrderIdAsync(int salesOrderId)
    {

        var res = await _context.SalesReturnOrders.AsNoTracking()
            .Include(e=>e.SalesOrder)
            .Include(e => e.Customer)
            .Include(e => e.SalesReturnOrderItems)
                .ThenInclude(i => i.Item)
            .Include(e => e.SalesReturnOrderItems)
                .ThenInclude(i => i.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(gro => gro.SalesOrderId == salesOrderId);

        if (res == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Not Found");

        var mapping = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            CustomerName = res.Customer?.CustomerName,
            SalesOrderId = res.SalesOrderId,
            DueDate = res.SalesOrder.DueDate,
            PostingDate = res.SalesOrder.PostingDate,
            Status = res.Status.ToString(),



            Items = res.SalesReturnOrderItems.Select(e => new SalesReturnOrderItemDTO
            {
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                BarCode = e.BarCode,
                SalesReturnOrderItemId = e.SalesReturnOrderItemId,
                Quantity = e.Quantity,
                ErrorMessage = e.ErrorMessage,


                SalesReturnOrderId = e.SalesReturnOrderId,
                SalesOrderItemId = e.SalesOrderItemId,
                UnitPrice = e.UnitPrice,
                UoMEntry = e.UoMEntry,

                Batches = e.SalesReturnOrderBatches.Select(b => new SalesReturnOrderBatchDTO
                {
                    ExpiryDate = b.ExpiryDate,
                    Quantity = b.Quantity,
                    SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                    SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                    //SalesOrderBatchId = b.SalesOrderBatch.SalesOrderBatchId,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                }).ToList(),
            }).ToList()


        };


        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<IEnumerable<SalesReturnOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Where(sro => sro.UserId == userId).ToListAsync();
    }

    public async Task<SalesReturnOrder?> GetWithItemsAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.SalesReturnOrderItems)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetWithItemsAndBatchesAsync(int salesReturnOrderId)
    {
        var result = await _context.SalesReturnOrders
            .AsNoTracking()
            .Where(s => s.SalesReturnOrderId == salesReturnOrderId)
            .Select(s => new SalesReturnOrderDTO
            {
                SalesReturnOrderId = s.SalesReturnOrderId,
                UserId = s.UserId,
                WarehouseId = s.WarehouseId,
                CustomerId = s.CustomerId,
                SalesOrderId = s.SalesOrderId,
                WarehouseCode = s.Warehouse.WarehouseCode,
                CustomerName = s.Customer.CustomerName,
                Items = s.SalesReturnOrderItems.Select(i => new SalesReturnOrderItemDTO
                {
                    SalesReturnOrderItemId = i.SalesReturnOrderItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    ErrorMessage = i.ErrorMessage,
                    Status = i.Status.ToString(),
                    SalesReturnOrderId = i.SalesReturnOrderId,
                    SalesOrderItemId = i.SalesOrderItemId,
                    ItemId = i.ItemId,
                    Batches = i.SalesReturnOrderBatches.Select(b => new SalesReturnOrderBatchDTO
                    {
                        SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                        SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                        SalesOrderBatchId = b.SalesOrderBatchId,
                        Quantity = b.Quantity,
                        Comment = b.Comment,
                        BatchNumber = b.BatchNumber,
                        ExpiryDate = b.ExpiryDate
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Not Found");

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<SalesReturnOrder?> GetWithSalesOrderAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.SalesOrder)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId);
    }

    public async Task<SalesReturnOrder?> GetWithWarehouseAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.Warehouse)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId);
    }
}

