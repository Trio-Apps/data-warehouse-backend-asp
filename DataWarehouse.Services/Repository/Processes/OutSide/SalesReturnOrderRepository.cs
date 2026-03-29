using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.Processes;
using DataWarehouse.Services.Services.Processes;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace DataWarehouse.Services.Repository.Processes.OutSide;


public class SalesReturnOrderRepository : BaseRepository<SalesReturnOrder>, ISalesReturnOrderRepository
{
    private readonly IBaseProcessesRepository<SalesReturnOrder> baseProcesses;
    private readonly IProcessItemIsProgressRepository progress;
    private readonly IApprovalRepository approval;
    private readonly IAppNotificationTrigger notificationTrigger;
    private readonly ReasonValidationService reasonValidationService;

    public SalesReturnOrderRepository(
        IBaseProcessesRepository<SalesReturnOrder> baseProcesses,
        IProcessItemIsProgressRepository progress,
        IApprovalRepository approval,
        IAppNotificationTrigger notificationTrigger,
        ReasonValidationService reasonValidationService,
        DataWarehouseDbContext context) : base(context)
    {
        this.baseProcesses = baseProcesses;
        this.progress = progress;
        this.approval = approval;
        this.notificationTrigger = notificationTrigger;
        this.reasonValidationService = reasonValidationService;
    }

    public async Task<IEnumerable<SalesReturnOrder>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await Query().Include(sro => sro.Reason).Where(sro => sro.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrders
            .AsNoTracking()
            .Include(sro => sro.Reason)
            .Where(sro => sro.WarehouseId == warehouseId);

        query = query.OrderByDescending(e => e.CreatedAt);

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
                DeliveryNoteOrderId = sro.DeliveryNoteOrderId,
                WarehouseCode = sro.Warehouse.WarehouseCode,
                CustomerName = sro.Customer.CustomerName,
                ErrorMessage = sro.ErrorMessage,
                ReasonId = sro.ReasonId,
                ReasonName = sro.Reason != null ? sro.Reason.Name : null,
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

    public async Task<GeneralResponse<PagedResult<SalesReturnOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync
      (int warehouseId, string userId, int? customerId, DateTime? postingDate, DateTime? DueDate, string? status,
       int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _context.SalesReturnOrders
            .AsNoTracking()
            .Include(e => e.SalesReturnOrderItems)
            .Include(e => e.Customer)
            .Include(e => e.Reason)
            // .Include(e => e.DeliveryNoteOrder) // �������: ���� �� ������ �� DTO �� ����ɡ ��� �� �������� �������� ��� �������
            .Where(sro => sro.WarehouseId == warehouseId);

        // ?? Customer Filter
        if (customerId.HasValue)
        {
            query = query.Where(e => e.CustomerId == customerId);
        }

        // ?? Status Filter
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<GeneralStatus>(status, out var statusEnum))
            {
                query = query.Where(e => e.Status == statusEnum);
            }
        }

        // ?? Posting Date Filter (��� GoodsReturnOrder)
        if (postingDate.HasValue)
        {
            var postDate = postingDate.Value.Date;
            query = query.Where(e => e.PostingDate.Date == postDate);
        }

        // ?? Due Date Filter (��� GoodsReturnOrder)
        if (DueDate.HasValue)
        {
            var dueDate = DueDate.Value.Date;
            query = query.Where(e => e.DueDate.Date == dueDate);
        }


        query = query.OrderByDescending(e => e.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);

        var processQuery = _context.ProcessItemIsProgresses
            .AsNoTracking()
            .Where(p => p.ProcessType == ProcessType.SalesReturn);

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(iw => new
            {
                Order = iw,

                HasProgress = processQuery.Any(p => p.ReferenceId == iw.SalesReturnOrderId),

                LatestStatus = processQuery
                    .Where(p => p.ReferenceId == iw.SalesReturnOrderId)
                    .OrderByDescending(p => p.ProcessItemIsProgressId)
                    .Select(p => (ProcessStatus?)p.Status)
                    .FirstOrDefault()
            })
            .Select(x => new SalesReturnOrderDTO
            {
                SalesReturnOrderId = x.Order.SalesReturnOrderId,

                // ? �� GoodsReturn: �� ��� ��� Return Order
                DueDate = x.Order.DueDate,
                PostingDate = x.Order.PostingDate,

                Status = x.Order.Status.ToString(),
                Comment = x.Order.Comment,
                UserId = x.Order.UserId,
                WarehouseId = x.Order.WarehouseId,

                CustomerId = x.Order.CustomerId,
                CustomerName = x.Order.Customer.CustomerName,
                ErrorMessage = x.Order.ErrorMessage,

                ItemCount = x.Order.SalesReturnOrderItems.Count(),
                ReasonId = x.Order.ReasonId,
                ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,

                Approval = x.HasProgress,
                ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);

        return GeneralResponse<PagedResult<SalesReturnOrderDTO>>.SuccessResponse(
            new PagedResult<SalesReturnOrderDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Data = data
            });
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetSalesReturnOrderByIdAsync(string userId, int salesReturnOrderId, CancellationToken cancellationToken = default)
    {
        var res = await _context.SalesReturnOrders
            .Include(s => s.Customer)
            .Include(s => s.DeliveryNoteOrder)
            .Include(s => s.Reason)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId, cancellationToken);

        if (res == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Not Found");

        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.SalesReturn, res.SalesReturnOrderId);

        var mapping = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            DeliveryNoteOrderId = res.DeliveryNoteOrderId,
            Status = res.Status.ToString(),
            Comment = res.Comment,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            CustomerName = res.Customer.CustomerName,
            DueDate = res.DueDate,
            PostingDate = res.PostingDate,
            CreatedAt = res.CreatedAt,
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Approval = approvalModel.hasProgress,
            ApprovalStatus = approvalModel.ApprovalStatus,
            Reason = approvalModel.Reason
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(mapping);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderWithoutRefAsync(
      string userId,
      AddSalesReturnOrderWithoutRefDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.SalesReturn);
        }
        catch (Exception ex)
        {
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(ex.Message);
        }

        // ? ��� ���� GoodsReturn: Validate Customer
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

        if (customer == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Customer is not found");

        var salesReturnOrder = new SalesReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = dto.WarehouseId,
            Comment = dto.Comment,
            CustomerId = dto.CustomerId,
            ReasonId = dto.ReasonId,
        };

        var res = await AddAsync(salesReturnOrder);
        await SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.SalesReturn, res.SalesReturnOrderId, userId, dto.IsDraft);

        // ? ��� ��� Approval Workflow �� �� Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.SalesReturn,
                referenceId: res.SalesReturnOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }

        var model = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,

            // �� DTO ���� ��� ������ �� ����� ������:
            PostingDate = res.PostingDate,
            DueDate = res.DueDate,

            Comment = res.Comment,
            Status = res.Status.ToString(),
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,

            // WithoutRef
            DeliveryNoteOrderId = res.DeliveryNoteOrderId
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> DuplicateSalesReturnOrderAsync(string userId, int salesReturnOrderId, CancellationToken cancellationToken = default)
    {
        var source = await _context.SalesReturnOrders
            .AsNoTracking()
            .Include(x => x.SalesReturnOrderItems)
                .ThenInclude(x => x.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(x => x.SalesReturnOrderId == salesReturnOrderId, cancellationToken);

        if (source == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales return order not found");

        var clone = OrderDuplicationHelper.Clone(source, userId);
        await _context.SalesReturnOrders.AddAsync(clone, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.SalesReturn, clone.SalesReturnOrderId, userId, true);

        return await GetSalesReturnOrderByIdAsync(userId, clone.SalesReturnOrderId, cancellationToken);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderAsync(
      string userId,
      AddSalesReturnOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.SalesReturn);
        }
        catch (Exception ex)
        {
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(ex.Message);
        }

        var DeliveryNoteOrder = await _context.DeliveryNoteOrders
            .Include(so => so.SalesReturnOrder) // ���� ����� �� ��� Return ��� ���
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == dto.DeliveryNoteOrderId);

        if (DeliveryNoteOrder == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order is not found");

        // ��� ���� GoodsReturn: ���� ���� Processing
        if (DeliveryNoteOrder.Status != GeneralStatus.Processing)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("You can add Sales Return, if sales order status is completed only");

        if (DeliveryNoteOrder.SalesReturnOrder != null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order has sales return order already!");

        var salesReturnOrder = new SalesReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = DeliveryNoteOrder.WarehouseId,
            Comment = dto.Comment,
            CustomerId = DeliveryNoteOrder.CustomerId,
            DeliveryNoteOrderId = dto.DeliveryNoteOrderId,
            ReasonId = dto.ReasonId
        };

        var res = await AddAsync(salesReturnOrder);
        await SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.SalesReturn, res.SalesReturnOrderId, userId, dto.IsDraft);

        // ? ��� ��� Approval Workflow �� �� Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.SalesReturn,
                referenceId: res.SalesReturnOrderId,
                warehouseId: res.WarehouseId,
                userId: userId
            );
        }

        var model = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            DeliveryNoteOrderId = res.DeliveryNoteOrderId,
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> AddSalesReturnOrderAndItemsByDeliveryNoteOrderIdAsync(
       string userId,
       AddSalesReturnOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.SalesReturn);
        }
        catch (Exception ex)
        {
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(ex.Message);
        }

        var DeliveryNoteOrder = await _context.DeliveryNoteOrders
            .Include(so => so.SalesReturnOrder)
            .Include(so => so.DeliveryNoteItems) // ? ��� items
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == dto.DeliveryNoteOrderId);

        if (DeliveryNoteOrder == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order is not found");

        if (DeliveryNoteOrder.Status != GeneralStatus.Processing)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("You can add Sales Return, if sales order status is completed only");

        if (DeliveryNoteOrder.SalesReturnOrder != null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order already has a Sales Return Order!");

        if (DeliveryNoteOrder.DeliveryNoteItems == null || !DeliveryNoteOrder.DeliveryNoteItems.Any())
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Order has no items to return");

        var returnOrder = new SalesReturnOrder
        {
            Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,

            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,

            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            WarehouseId = DeliveryNoteOrder.WarehouseId,
            Comment = dto.Comment,
            CustomerId = DeliveryNoteOrder.CustomerId,
            DeliveryNoteOrderId = dto.DeliveryNoteOrderId,
            ReasonId = dto.ReasonId,

            // ? Copy DeliveryNoteItems -> SalesReturnOrderItems
            SalesReturnOrderItems = DeliveryNoteOrder.DeliveryNoteItems.Select(soi => new SalesReturnOrderItem
            {
                DeliveryNoteItemId = soi.DeliveryNoteItemId,
                ItemId = soi.ItemId,
                Quantity = soi.Quantity,
                UoMEntry = soi.UoMEntry,
                BarCode = soi.BarCode,
                UnitPrice = soi.UnitPrice,

                VatPercent = soi.VatPercent,

                VatAmount = soi.VatAmount,

                LineTotalBeforeVat = soi.LineTotalBeforeVat,

                LineTotalAfterVat = soi.LineTotalAfterVat,

                // ���� �������: ��� ������� �� ���
                Status = GeneralItemStatus.Planned,

                // optional
                ErrorMessage = null,
            }).ToList()
        };

        await _context.SalesReturnOrders.AddAsync(returnOrder);
        await SaveChangesAsync();
        await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.SalesReturn, returnOrder.SalesReturnOrderId, userId, dto.IsDraft);

        // ? ��� ��� Approval Workflow �� �� Draft
        if (!dto.IsDraft)
        {
            await approval.StartProcessAsync(
                processType: ProcessType.SalesReturn,
                referenceId: returnOrder.SalesReturnOrderId,
                warehouseId: returnOrder.WarehouseId,
                userId: userId
            );
        }

        var model = new SalesReturnOrderDTO
        {
            DeliveryNoteOrderId = returnOrder.DeliveryNoteOrderId,
            DueDate = returnOrder.DueDate,
            PostingDate = returnOrder.PostingDate,
            Status = returnOrder.Status.ToString(),
            UserId = returnOrder.UserId,
            WarehouseId = returnOrder.WarehouseId,
            SalesReturnOrderId = returnOrder.SalesReturnOrderId,
            CustomerId = returnOrder.CustomerId,
            Comment = returnOrder.Comment,
            ReasonId = returnOrder.ReasonId,
            ReasonName = returnOrder.Reason != null ? returnOrder.Reason.Name : null
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(model);
    }

    public async Task<GeneralResponse<SalesReturnOrderDTO>> UpdateSalesReturnOrderAsync(
     string userId,
     int salesReturnOrderId,
     UpdateSalesReturnOrderDTO dto)
    {
        try
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.SalesReturn);
        }
        catch (Exception ex)
        {
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(ex.Message);
        }

        var entity = await _context.SalesReturnOrders
            .FirstOrDefaultAsync(e => e.SalesReturnOrderId == salesReturnOrderId);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Sales Return Order not found");

        if (entity.SalesReturnOrderId != salesReturnOrderId)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("ID mismatch");

        // ? ��� ����� Customer �� ��� SalesReturn ���� ��� DeliveryNoteOrder (�� ReceiptPurchaseOrderId �� GoodsReturn)
        if (entity.DeliveryNoteOrderId != null)
        {
            if (dto.CustomerId.HasValue && dto.CustomerId.Value != entity.CustomerId)
            {
                return GeneralResponse<SalesReturnOrderDTO>.FailResponse(
                    "You cannot edit on customer, because customer is based on Sales order.");
            }
        }


        var checkApprovalStatus = await GetProcessItem(entity.SalesReturnOrderId, ProcessType.SalesReturn);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(
                "You cannot edit any sales return order because its approval status is 'Approved' and all approval steps have been completed.");

        // Update fields
        entity.UserId = userId;

        if (dto.PostingDate.HasValue)
            entity.PostingDate = dto.PostingDate.Value;

        if (dto.DueDate.HasValue)
            entity.DueDate = dto.DueDate.Value;

        if (dto.CustomerId.HasValue && entity.DeliveryNoteOrderId == null)
            entity.CustomerId = dto.CustomerId.Value;
        entity.ReasonId = dto.ReasonId;

        entity.Comment = dto.Comment ?? entity.Comment;

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

        await _context.SaveChangesAsync();

        var result = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = entity.SalesReturnOrderId,
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            CustomerId = entity.CustomerId,
            DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
            Comment = entity.Comment,
            Status = entity.Status.ToString(),
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        };

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(result);
    }
    public async Task<GeneralResponse<SalesReturnOrderDTO>> DeleteSalesReturnOrderAsync(
      int salesReturnOrderId,
      CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesReturnOrders
            .FirstOrDefaultAsync(e => e.SalesReturnOrderId == salesReturnOrderId, cancellationToken);

        if (entity == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("not found");

        var checkApprovalStatus = await approval.GetProcessItem(
            entity.SalesReturnOrderId,
            ProcessType.SalesReturn,
            cancellationToken);

        if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse(
                "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");

        // Snapshot ��� ����� ����� ����� �� ��� response
        var result = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = entity.SalesReturnOrderId,
            DueDate = entity.DueDate,
            PostingDate = entity.PostingDate,
            Status = entity.Status.ToString(),
            UserId = entity.UserId,
            WarehouseId = entity.WarehouseId,
            CustomerId = entity.CustomerId,
            DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
            Comment = entity.Comment,
            ReasonId = entity.ReasonId,
            ReasonName = entity.Reason != null ? entity.Reason.Name : null
        };

        _context.SalesReturnOrders.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(result);
    }

    public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int salesReturnOrderId)
    {
        return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<SalesReturnOrder>(
            salesReturnOrderId,
            ProcessType.SalesReturn,
            x => x.SalesReturnOrderId == salesReturnOrderId,
            _context.SalesReturnOrders
        );
    }
  
    

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////
     // not used
    /// <returns></returns>
    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetWithCustomerAsync(int deliveryNoteOrderId, string userId, CancellationToken cancellationToken = default)
    {
        var res = await _context.SalesReturnOrders.Include(so => so.Customer)
            .Include(s => s.DeliveryNoteOrder)
            .Include(s => s.Reason)
            .FirstOrDefaultAsync(so => so.DeliveryNoteOrderId == deliveryNoteOrderId);

        if (res == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("this DeliveryNoteOrderId is not found");


        var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.SalesReturn, res.SalesReturnOrderId);

        var checkApprovalStatus = await approval.GetProcessItem(res.SalesReturnOrderId, ProcessType.SalesReturn, cancellationToken);


        bool hasProgress = checkApprovalStatus != null;

        string? approvalStatus = checkApprovalStatus?.Status.ToString();

        var mapping = new SalesReturnOrderDTO
        {
            DueDate = res.DeliveryNoteOrder.DueDate,
            PostingDate = res.DeliveryNoteOrder.PostingDate,
            DeliveryNoteOrderId = res.DeliveryNoteOrderId,
            Status = res.Status.ToString(),
            Comment = res.Comment,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            SalesReturnOrderId = res.SalesReturnOrderId,
            CustomerName = res.Customer.CustomerName,
            CustomerId = res.CustomerId,
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,
            CanApprove = approvalModel.CanApprove,
            ProcessApprovalId = approvalModel.ProcessApprovalId,
            ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
            Reason = approvalModel.Reason,
            Approval = hasProgress,
            ApprovalStatus = checkApprovalStatus != null ? approvalStatus : null
        };


        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(mapping);

    }

 
    public async Task<GeneralResponse<SalesReturnOrderDTO>> GetByDeliveryNoteOrderIdAsync(int deliveryNoteOrderId)
    {

        var res = await _context.SalesReturnOrders.AsNoTracking()
            .Include(e=>e.DeliveryNoteOrder)
            .Include(e => e.Customer)
            .Include(e => e.Reason)
            .Include(e => e.SalesReturnOrderItems)
                .ThenInclude(i => i.Item)
                .ThenInclude(i => i.ItemUomGroups)
            .Include(e => e.SalesReturnOrderItems)
                .ThenInclude(i => i.SalesReturnOrderBatches)
            .FirstOrDefaultAsync(gro => gro.DeliveryNoteOrderId == deliveryNoteOrderId);

        if (res == null)
            return GeneralResponse<SalesReturnOrderDTO>.FailResponse("Not Found");

        var mapping = new SalesReturnOrderDTO
        {
            SalesReturnOrderId = res.SalesReturnOrderId,
            UserId = res.UserId,
            WarehouseId = res.WarehouseId,
            CustomerId = res.CustomerId,
            CustomerName = res.Customer?.CustomerName,
            DeliveryNoteOrderId = res.DeliveryNoteOrderId,
            DueDate = res.DeliveryNoteOrder.DueDate,
            PostingDate = res.DeliveryNoteOrder.PostingDate,
            Status = res.Status.ToString(),
            ReasonId = res.ReasonId,
            ReasonName = res.Reason != null ? res.Reason.Name : null,



            Items = res.SalesReturnOrderItems.Select(e => new SalesReturnOrderItemDTO
            {
                ItemId = e.ItemId,
                ItemCode = e.Item.ItemCode,
                ItemName = e.Item.ItemName,
                BarCode = e.BarCode,
                SalesReturnOrderItemId = e.SalesReturnOrderItemId,
                Quantity = e.Quantity,
                ErrorMessage = e.ErrorMessage,
                Status = e.Status.ToString(),


                SalesReturnOrderId = e.SalesReturnOrderId,
                DeliveryNoteItemId = e.DeliveryNoteItemId,
                UnitPrice = e.UnitPrice,
                VatPercent = e.VatPercent,
                VatAmount = e.VatAmount,
                LineTotalBeforeVat = e.LineTotalBeforeVat,
                LineTotalAfterVat = e.LineTotalAfterVat,
                UoMEntry = e.UoMEntry,
                UnitName = e.Item.ItemUomGroups
                    .Where(i => i.UomEntry == e.UoMEntry)
                    .Select(i => i.UomCode)
                    .FirstOrDefault(),

                Batches = e.SalesReturnOrderBatches.Select(b => new SalesReturnOrderBatchDTO
                {
                    ExpiryDate = b.ExpiryDate,
                    Quantity = b.Quantity,
                    SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                    SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                    //DeliveryNoteOrderBatchId = b.DeliveryNoteOrderBatch.DeliveryNoteOrderBatchId,
                    Comment = b.Comment,
                    BatchNumber = b.BatchNumber,
                }).ToList(),
            }).ToList()


        };


        return GeneralResponse<SalesReturnOrderDTO>.SuccessResponse(mapping);
    }

 
    public async Task<IEnumerable<SalesReturnOrder>> GetByUserIdAsync(string userId)
    {
        return await Query().Include(sro => sro.Reason).Where(sro => sro.UserId == userId).ToListAsync();
    }

    public async Task<SalesReturnOrder?> GetWithItemsAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.SalesReturnOrderItems, sro => sro.Reason)
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
                DeliveryNoteOrderId = s.DeliveryNoteOrderId,
                WarehouseCode = s.Warehouse.WarehouseCode,
                CustomerName = s.Customer.CustomerName,
                ReasonId = s.ReasonId,
                ReasonName = s.Reason != null ? s.Reason.Name : null,
                Items = s.SalesReturnOrderItems.Select(i => new SalesReturnOrderItemDTO
                {
                    SalesReturnOrderItemId = i.SalesReturnOrderItemId,
                    Quantity = i.Quantity,
                    UoMEntry = i.UoMEntry,
                    BarCode = i.BarCode,
                    UnitPrice = i.UnitPrice,
                    VatPercent = i.VatPercent,
                    VatAmount = i.VatAmount,
                    LineTotalBeforeVat = i.LineTotalBeforeVat,
                    LineTotalAfterVat = i.LineTotalAfterVat,
                    ErrorMessage = i.ErrorMessage,
                    Status = i.Status.ToString(),
                    SalesReturnOrderId = i.SalesReturnOrderId,
                    DeliveryNoteItemId = i.DeliveryNoteItemId,
                    ItemId = i.ItemId,
                    ItemCode = i.Item.ItemCode,
                    ItemName = i.Item.ItemName,
                    UnitName = i.Item.ItemUomGroups
                        .Where(u => u.UomEntry == i.UoMEntry)
                        .Select(u => u.UomCode)
                        .FirstOrDefault(),
                    Batches = i.SalesReturnOrderBatches.Select(b => new SalesReturnOrderBatchDTO
                    {
                        SalesReturnOrderBatchId = b.SalesReturnOrderBatchId,
                        SalesReturnOrderItemId = b.SalesReturnOrderItemId,
                        DeliveryNoteBatchId = b.DeliveryNoteBatchId,
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

    public async Task<SalesReturnOrder?> GetWithDeliveryNoteOrderAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.DeliveryNoteOrder, sro => sro.Reason)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId);
    }

    public async Task<SalesReturnOrder?> GetWithWarehouseAsync(int salesReturnOrderId)
    {
        return await QueryIncluding(false, sro => sro.Warehouse, sro => sro.Reason)
            .FirstOrDefaultAsync(sro => sro.SalesReturnOrderId == salesReturnOrderId);
    }

 

 
  
}


