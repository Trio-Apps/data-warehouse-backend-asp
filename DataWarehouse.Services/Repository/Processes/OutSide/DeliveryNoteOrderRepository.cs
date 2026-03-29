using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Based;
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

using Microsoft.EntityFrameworkCore;


namespace DataWarehouse.Services.Repository.Processes.OutSide
{
   
    public class DeliveryNoteOrderRepository : BaseRepository<DeliveryNoteOrder>, IDeliveryNoteOrderRepository
    {
        private readonly IBaseProcessesRepository<DeliveryNoteOrder> baseProcesses;
        private readonly IProcessItemIsProgressRepository progress;
        private readonly IApprovalRepository approval;
        private readonly IAppNotificationTrigger notificationTrigger;
        private readonly DataWarehouseDbContext _context;
        private readonly ReasonValidationService reasonValidationService;

        public DeliveryNoteOrderRepository(
            IBaseProcessesRepository<DeliveryNoteOrder> baseProcesses,
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
            _context = context;
        }

        public async Task<IEnumerable<DeliveryNoteOrder>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await Query()
                .Where(dno => dno.WarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>> GetByWarehouseIdWithPaginationAsync(
            int warehouseId, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DeliveryNoteOrders
                .AsNoTracking()
                .Where(dno => dno.WarehouseId == warehouseId)
                .OrderByDescending(e => e.CreatedAt);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(dno => new DeliveryNoteOrderDTO
                {
                    DeliveryNoteOrderId = dno.DeliveryNoteOrderId,
                    UserId = dno.UserId,
                    WarehouseId = dno.WarehouseId,
                    CustomerId = dno.CustomerId,
                    SalesOrderId = dno.SalesOrderId,
                    ErrorMessage = dno.ErrorMessage,
                    WarehouseCode = dno.Warehouse.WarehouseCode,
                    CustomerName = dno.Customer.CustomerName,
                    PostingDate = dno.PostingDate,
                    DueDate = dno.DueDate,
                    Status = dno.Status.ToString(),
                    Comment = dno.Comment,
                    CreatedAt = dno.CreatedAt,
                    ReasonId = dno.ReasonId,
                    ReasonName = dno.Reason != null ? dno.Reason.Name : null
                })
                .ToListAsync();

            return GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>.SuccessResponse(
                new PagedResult<DeliveryNoteOrderDTO>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    Data = data
                });
        }

        public async Task<GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            int warehouseId,
            string userId,
            int? customerId,
            DateTime? postingDate,
            DateTime? DueDate,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DeliveryNoteOrders
                .AsNoTracking()
                .Include(e => e.DeliveryNoteItems)
                .Include(e => e.Customer)
                .Where(dno => dno.WarehouseId == warehouseId);

            // Customer filter
            if (customerId.HasValue)
                query = query.Where(e => e.CustomerId == customerId.Value);

            // Status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<GeneralStatus>(status, out var statusEnum))
                query = query.Where(e => e.Status == statusEnum);

            // Posting date filter
            if (postingDate.HasValue)
            {
                var post = postingDate.Value.Date;
                query = query.Where(e => e.PostingDate.Date == post);
            }

            // Due date filter
            if (DueDate.HasValue)
            {
                var due = DueDate.Value.Date;
                query = query.Where(e => e.DueDate.Date == due);
            }

            query = query.OrderByDescending(e => e.CreatedAt);

            var totalRecords = await query.CountAsync(cancellationToken);

            var processQuery = _context.ProcessItemIsProgresses
                .AsNoTracking()
                .Where(p => p.ProcessType == ProcessType.DeliveryNote);

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(dno => new
                {
                    Order = dno,
                    HasProgress = processQuery.Any(p => p.ReferenceId == dno.DeliveryNoteOrderId),
                    LatestStatus = processQuery
                        .Where(p => p.ReferenceId == dno.DeliveryNoteOrderId)
                        .OrderByDescending(p => p.ProcessItemIsProgressId)
                        .Select(p => (ProcessStatus?)p.Status)
                        .FirstOrDefault()
                })
                .Select(x => new DeliveryNoteOrderDTO
                {
                    DeliveryNoteOrderId = x.Order.DeliveryNoteOrderId,
                    SalesOrderId = x.Order.SalesOrderId,
                    PostingDate = x.Order.PostingDate,
                    DueDate = x.Order.DueDate,
                    Status = x.Order.Status.ToString(),
                    Comment = x.Order.Comment,
                    UserId = x.Order.UserId,
                    ErrorMessage = x.Order.ErrorMessage,
                    WarehouseId = x.Order.WarehouseId,
                    CustomerId = x.Order.CustomerId,
                    CustomerName = x.Order.Customer.CustomerName,
                    ItemCount = x.Order.DeliveryNoteItems.Count(),
                    Approval = x.HasProgress,
                    ApprovalStatus = x.LatestStatus.HasValue ? x.LatestStatus.Value.ToString() : null,
               IsReturn = x.Order.SalesReturnOrder != null,
                    ReturnOrderId = x.Order.SalesReturnOrder != null ? x.Order.SalesReturnOrder.SalesReturnOrderId : null,
                    ReasonId = x.Order.ReasonId,
                    ReasonName = x.Order.Reason != null ? x.Order.Reason.Name : null,

                })
                .ToListAsync(cancellationToken);

            return GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>.SuccessResponse(
                new PagedResult<DeliveryNoteOrderDTO>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    Data = data
                });
        }

        /// <summary>
        /// not used (���� ���� ���� ����)
        /// </summary>
        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> GetWithCustomerAsync(
            int deliveryNoteOrderId, string userId, CancellationToken cancellationToken = default)
        {
            var res = await _context.DeliveryNoteOrders
                .Include(d => d.Customer)
                .Include(d => d.SalesOrder)
                .FirstOrDefaultAsync(d => d.DeliveryNoteOrderId == deliveryNoteOrderId, cancellationToken);

            if (res == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("this DeliveryNoteOrderId is not found");

            var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.DeliveryNote, res.DeliveryNoteOrderId);
            var checkApprovalStatus = await approval.GetProcessItem(res.DeliveryNoteOrderId, ProcessType.DeliveryNote, cancellationToken);

            bool hasProgress = checkApprovalStatus != null;
            string? approvalStatus = checkApprovalStatus?.Status.ToString();

            var mapping = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = res.DeliveryNoteOrderId,
                SalesOrderId = res.SalesOrderId,
                DueDate = res.DueDate,
                PostingDate = res.PostingDate,
                Status = res.Status.ToString(),
                Comment = res.Comment,
                UserId = res.UserId,
                WarehouseId = res.WarehouseId,
                CustomerName = res.Customer.CustomerName,
                CustomerId = res.CustomerId,
                ReasonId = res.ReasonId,
                ReasonName = res.Reason != null ? res.Reason.Name : null,

                CanApprove = approvalModel.CanApprove,
                ProcessApprovalId = approvalModel.ProcessApprovalId,
                ProcessItemIsProgressId = approvalModel.ProcessItemIsProgressId,
                Reason = approvalModel.Reason,
                Approval = hasProgress,
                ApprovalStatus = checkApprovalStatus != null ? approvalStatus : null,
                IsReturn = res.SalesReturnOrder != null,
                ReturnOrderId = res.SalesReturnOrder != null ? res.SalesReturnOrder.SalesReturnOrderId : null,

            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(mapping);
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> GetDeliveryNoteOrderByIdAsync(
            string userId, int deliveryNoteOrderId, CancellationToken cancellationToken = default)
        {
            var res = await _context.DeliveryNoteOrders
                .Include(d => d.Customer)
                .Include(d => d.SalesOrder)
                .FirstOrDefaultAsync(d => d.DeliveryNoteOrderId == deliveryNoteOrderId, cancellationToken);

            if (res == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Not Found");

            var approvalModel = await approval.CheckUserCanApproveAsync(userId, ProcessType.DeliveryNote, res.DeliveryNoteOrderId);

            var mapping = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = res.DeliveryNoteOrderId,
                SalesOrderId = res.SalesOrderId,
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

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(mapping);
        }

        /// <summary>
        /// ����� Delivery Note ����� ��� Sales Order (Parent) + ��� items
        /// </summary>
        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderAndItemsBySalesOrderIdAsync(
            string userId, AddDeliveryNoteOrderDTO dto)
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.DeliveryNote);
            var salesOrder = await _context.SalesOrders
//                .Include(so => so.DeliveryNoteOrder)  // ������ �� ����� DN ��� ���
                .Include(so => so.SalesOrderItems)    // ���� items
                .FirstOrDefaultAsync(so => so.SalesOrderId == dto.SalesOrderId);

            if (salesOrder == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order is not found");

            //if (salesOrder.DeliveryNoteOrder != null)
            //    return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order already has a Delivery Note Order!");

            if (salesOrder.SalesOrderItems == null || !salesOrder.SalesOrderItems.Any())
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order has no items");

            var deliveryNote = new DeliveryNoteOrder
            {
                Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
                PostingDate = dto.PostingDate,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,

                WarehouseId = salesOrder.WarehouseId,
                CustomerId = salesOrder.CustomerId,

                Comment = dto.Comment,
                ReasonId = dto.ReasonId,

                // Reference
                SalesOrderId = dto.SalesOrderId,

                DeliveryNoteItems = salesOrder.SalesOrderItems.Select(soi => new DeliveryNoteItem
                {
                    SalesOrderItemId = soi.SalesOrderItemId,
                    ItemId = soi.ItemId,
                    Quantity = soi.Quantity,
                    UoMEntry = soi.UoMEntry,
                    BarCode = soi.BarCode,
                    UnitPrice = soi.UnitPrice,

                    VatPercent = soi.VatPercent,

                    VatAmount = soi.VatAmount,

                    LineTotalBeforeVat = soi.LineTotalBeforeVat,

                    LineTotalAfterVat = soi.LineTotalAfterVat,

                    Status = GeneralItemStatus.Planned,
                    ErrorMessage = null
                }).ToList()
            };

            await _context.DeliveryNoteOrders.AddAsync(deliveryNote);
            await _context.SaveChangesAsync();
            await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.DeliveryNote, deliveryNote.DeliveryNoteOrderId, userId, dto.IsDraft);

            if (!dto.IsDraft)
            {
                await approval.StartProcessAsync(
                    processType: ProcessType.DeliveryNote,
                    referenceId: deliveryNote.DeliveryNoteOrderId,
                    warehouseId: deliveryNote.WarehouseId,
                    userId: userId
                );
            }

            var model = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = deliveryNote.DeliveryNoteOrderId,
                UserId = deliveryNote.UserId,
                WarehouseId = deliveryNote.WarehouseId,
                CustomerId = deliveryNote.CustomerId,
                SalesOrderId = deliveryNote.SalesOrderId,
                PostingDate = deliveryNote.PostingDate,
                DueDate = deliveryNote.DueDate,
                Comment = deliveryNote.Comment,
                Status = deliveryNote.Status.ToString(),
                ReasonId = deliveryNote.ReasonId,
                ReasonName = deliveryNote.Reason != null ? deliveryNote.Reason.Name : null
            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(model);
        }

        /// <summary>
        /// ����� Delivery Note ���� SalesOrder reference
        /// </summary>
        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderWithoutRefAsync(
            string userId, AddDeliveryNoteOrderWithoutRefDTO dto)
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.DeliveryNote);
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (customer == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Customer is not found");

            var deliveryNote = new DeliveryNoteOrder
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

                // ���� ����
                SalesOrderId = null
            };

            var res = await AddAsync(deliveryNote);
            await SaveChangesAsync();
            await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.DeliveryNote, res.DeliveryNoteOrderId, userId, dto.IsDraft);

            if (!dto.IsDraft)
            {
                await approval.StartProcessAsync(
                    processType: ProcessType.DeliveryNote,
                    referenceId: res.DeliveryNoteOrderId,
                    warehouseId: res.WarehouseId,
                    userId: userId
                );
            }

            var model = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = res.DeliveryNoteOrderId,
                UserId = res.UserId,
                WarehouseId = res.WarehouseId,
                CustomerId = res.CustomerId,
                SalesOrderId = res.SalesOrderId,
                PostingDate = res.PostingDate,
                DueDate = res.DueDate,
                Comment = res.Comment,
                Status = res.Status.ToString(),
                ReasonId = res.ReasonId,
                ReasonName = res.Reason != null ? res.Reason.Name : null
            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> DuplicateDeliveryNoteOrderAsync(string userId, int deliveryNoteOrderId, CancellationToken cancellationToken = default)
        {
            var source = await _context.DeliveryNoteOrders
                .AsNoTracking()
                .Include(x => x.DeliveryNoteItems)
                    .ThenInclude(x => x.DeliveryNoteBatches)
                .FirstOrDefaultAsync(x => x.DeliveryNoteOrderId == deliveryNoteOrderId, cancellationToken);

            if (source == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Delivery note order not found");

            var clone = OrderDuplicationHelper.Clone(source, userId);
            await _context.DeliveryNoteOrders.AddAsync(clone, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.DeliveryNote, clone.DeliveryNoteOrderId, userId, true);

            return await GetDeliveryNoteOrderByIdAsync(userId, clone.DeliveryNoteOrderId, cancellationToken);
        }

        /// <summary>
        /// ����� Delivery Note ����� ��� SalesOrderId (Reference ���) ���� ��� items
        /// </summary>
        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderAsync(
            string userId, AddDeliveryNoteOrderDTO dto)
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.DeliveryNote);
            var salesOrder = await _context.SalesOrders
               // .Include(so => so.DeliveryNoteOrder)
                .FirstOrDefaultAsync(so => so.SalesOrderId == dto.SalesOrderId);

            if (salesOrder == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order is not found");

            //if (salesOrder.DeliveryNoteOrder != null)
            //    return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Sales Order already has a Delivery Note Order!");

            var deliveryNote = new DeliveryNoteOrder
            {
                Status = dto.IsDraft ? GeneralStatus.Draft : GeneralStatus.Processing,
                PostingDate = dto.PostingDate,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,

                WarehouseId = salesOrder.WarehouseId,
                CustomerId = salesOrder.CustomerId,

                Comment = dto.Comment,
                ReasonId = dto.ReasonId,
                SalesOrderId = dto.SalesOrderId
            };

            var res = await AddAsync(deliveryNote);
            await SaveChangesAsync();
            await notificationTrigger.TriggerOrderCreatedNotificationAsync(ProcessType.DeliveryNote, res.DeliveryNoteOrderId, userId, dto.IsDraft);

            if (!dto.IsDraft)
            {
                await approval.StartProcessAsync(
                    processType: ProcessType.DeliveryNote,
                    referenceId: res.DeliveryNoteOrderId,
                    warehouseId: res.WarehouseId,
                    userId: userId
                );
            }

            var model = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = res.DeliveryNoteOrderId,
                UserId = res.UserId,
                WarehouseId = res.WarehouseId,
                CustomerId = res.CustomerId,
                SalesOrderId = res.SalesOrderId,
                PostingDate = res.PostingDate,
                DueDate = res.DueDate,
                Comment = res.Comment,
                Status = res.Status.ToString(),
                ReasonId = res.ReasonId,
                ReasonName = res.Reason != null ? res.Reason.Name : null
            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> UpdateDeliveryNoteOrderAsync(
            string userId, int deliveryNoteOrderId, UpdateDeliveryNoteOrderDTO dto)
        {
            // await reasonValidationService.ValidateAsync(dto.ReasonId, ProcessType.DeliveryNote);
            var entity = await _context.DeliveryNoteOrders
                .FirstOrDefaultAsync(e => e.DeliveryNoteOrderId == deliveryNoteOrderId);

            if (entity == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Delivery Note Order not found");

            if (entity.DeliveryNoteOrderId != deliveryNoteOrderId)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("ID mismatch");

            // ? ��� ����� Customer �� ��� DeliveryNote ���� ��� SalesOrder
            if (entity.SalesOrderId != null)
            {
                if (dto.CustomerId.HasValue && dto.CustomerId.Value != entity.CustomerId)
                {
                    return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse(
                        "You cannot edit on customer, because customer is based on Sales order.");
                }
            }

            var checkApprovalStatus = await approval.GetProcessItem(entity.DeliveryNoteOrderId, ProcessType.DeliveryNote);

            if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            {
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse(
                    "You cannot edit any delivery note order because its approval status is 'Approved' and all approval steps have been completed.");
            }

            entity.UserId = userId;

            if (dto.PostingDate.HasValue)
                entity.PostingDate = dto.PostingDate.Value;

            if (dto.DueDate.HasValue)
                entity.DueDate = dto.DueDate.Value;

            if (dto.CustomerId.HasValue && entity.SalesOrderId == null)
                entity.CustomerId = dto.CustomerId.Value;

            entity.Comment = dto.Comment ?? entity.Comment;
            entity.ReasonId = dto.ReasonId;

            if (!dto.IsDraft)
            {
                await approval.StartProcessAsync(
                    processType: ProcessType.DeliveryNote,
                    referenceId: entity.DeliveryNoteOrderId,
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

            var result = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
                UserId = entity.UserId,
                WarehouseId = entity.WarehouseId,
                CustomerId = entity.CustomerId,
                SalesOrderId = entity.SalesOrderId,
                Comment = entity.Comment,
                Status = entity.Status.ToString(),
                PostingDate = entity.PostingDate,
                DueDate = entity.DueDate,
                ReasonId = entity.ReasonId,
                ReasonName = entity.Reason != null ? entity.Reason.Name : null
            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> DeleteDeliveryNoteOrderAsync(
            int deliveryNoteOrderId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DeliveryNoteOrders
                .FirstOrDefaultAsync(e => e.DeliveryNoteOrderId == deliveryNoteOrderId, cancellationToken);

            if (entity == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("not found");

            var checkApprovalStatus = await approval.GetProcessItem(
                entity.DeliveryNoteOrderId,
                ProcessType.DeliveryNote,
                cancellationToken);

            if (checkApprovalStatus != null && checkApprovalStatus.Status == ProcessStatus.Approved)
            {
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse(
                    "You cannot delete this order because its approval status is 'Approved' and all approval steps have been completed.");
            }

            var result = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = entity.DeliveryNoteOrderId,
                DueDate = entity.DueDate,
                PostingDate = entity.PostingDate,
                Status = entity.Status.ToString(),
                UserId = entity.UserId,
                WarehouseId = entity.WarehouseId,
                CustomerId = entity.CustomerId,
                SalesOrderId = entity.SalesOrderId,
                Comment = entity.Comment,
                ReasonId = entity.ReasonId,
                ReasonName = entity.Reason != null ? entity.Reason.Name : null
            };

            _context.DeliveryNoteOrders.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(result);
        }

        public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int deliveryNoteOrderId)
        {
            return await baseProcesses.RevertPartiallyFailedStatusToProcessingAsync<DeliveryNoteOrder>(
                deliveryNoteOrderId,
                ProcessType.DeliveryNote,
                x => x.DeliveryNoteOrderId == deliveryNoteOrderId,
                _context.DeliveryNoteOrders
            );
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            var res = await _context.DeliveryNoteOrders
                .AsNoTracking()
                .Include(e => e.SalesOrder)
                .Include(e => e.Customer)
                .Include(e => e.DeliveryNoteItems)
                    .ThenInclude(i => i.Item)
                    .ThenInclude(i => i.ItemUomGroups)
                .Include(e => e.DeliveryNoteItems)
                    .ThenInclude(i => i.DeliveryNoteBatches)
                .FirstOrDefaultAsync(dno => dno.SalesOrderId == salesOrderId);

            if (res == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Not Found");

            var mapping = new DeliveryNoteOrderDTO
            {
                DeliveryNoteOrderId = res.DeliveryNoteOrderId,
                UserId = res.UserId,
                WarehouseId = res.WarehouseId,
                CustomerId = res.CustomerId,
                CustomerName = res.Customer?.CustomerName,
                SalesOrderId = res.SalesOrderId,
                DueDate = res.DueDate,
                PostingDate = res.PostingDate,
                Status = res.Status.ToString(),
                ReasonId = res.ReasonId,
                ReasonName = res.Reason != null ? res.Reason.Name : null,

                Items = res.DeliveryNoteItems.Select(e => new DeliveryNoteItemDTO
                {
                    ItemId = e.ItemId,
                    ItemCode = e.Item.ItemCode,
                    ItemName = e.Item.ItemName,
                    BarCode = e.BarCode,
                    DeliveryNoteItemId = e.DeliveryNoteItemId,
                    Quantity = e.Quantity,
                    ErrorMessage = e.ErrorMessage,
                    Status = e.Status.ToString(),
                    DeliveryNoteOrderId = e.DeliveryNoteOrderId,
                    SalesOrderItemId = e.SalesOrderItemId,
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

                    Batches = e.DeliveryNoteBatches.Select(b => new DeliveryNoteBatchDTO
                    {
                        ExpiryDate = b.ExpiryDate,
                        Quantity = b.Quantity,
                        DeliveryNoteItemId = b.DeliveryNoteItemId,
                        DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                        Comment = b.Comment,
                        BatchNumber = b.BatchNumber,
                    }).ToList(),
                }).ToList()
            };

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(mapping);
        }

        public async Task<IEnumerable<DeliveryNoteOrder>> GetByUserIdAsync(string userId)
        {
            return await Query()
                .Where(dno => dno.UserId == userId)
                .ToListAsync();
        }

        public async Task<DeliveryNoteOrder?> GetWithItemsAsync(int deliveryNoteOrderId)
        {
            return await QueryIncluding(false, dno => dno.DeliveryNoteItems)
                .FirstOrDefaultAsync(dno => dno.DeliveryNoteOrderId == deliveryNoteOrderId);
        }

        public async Task<DeliveryNoteOrder?> GetWithSalesOrderAsync(int deliveryNoteOrderId)
        {
            return await QueryIncluding(false, dno => dno.SalesOrder)
                .FirstOrDefaultAsync(dno => dno.DeliveryNoteOrderId == deliveryNoteOrderId);
        }

        public async Task<DeliveryNoteOrder?> GetWithWarehouseAsync(int deliveryNoteOrderId)
        {
            return await QueryIncluding(false, dno => dno.Warehouse)
                .FirstOrDefaultAsync(dno => dno.DeliveryNoteOrderId == deliveryNoteOrderId);
        }

        public async Task<GeneralResponse<DeliveryNoteOrderDTO>> GetWithItemsAndBatchesAsync(int deliveryNoteOrderId)
        {
            var result = await _context.DeliveryNoteOrders
                .AsNoTracking()
                .Where(d => d.DeliveryNoteOrderId == deliveryNoteOrderId)
                .Select(d => new DeliveryNoteOrderDTO
                {
                    DeliveryNoteOrderId = d.DeliveryNoteOrderId,
                    UserId = d.UserId,
                    WarehouseId = d.WarehouseId,
                    CustomerId = d.CustomerId,
                    SalesOrderId = d.SalesOrderId,
                    WarehouseCode = d.Warehouse.WarehouseCode,
                    CustomerName = d.Customer.CustomerName,
                    PostingDate = d.PostingDate,
                    DueDate = d.DueDate,
                    Status = d.Status.ToString(),
                    Comment = d.Comment,
                    ReasonId = d.ReasonId,
                    ReasonName = d.Reason != null ? d.Reason.Name : null,

                    Items = d.DeliveryNoteItems.Select(i => new DeliveryNoteItemDTO
                    {
                        DeliveryNoteItemId = i.DeliveryNoteItemId,
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
                        DeliveryNoteOrderId = i.DeliveryNoteOrderId,
                        SalesOrderItemId = i.SalesOrderItemId,
                        ItemId = i.ItemId,
                        ItemCode = i.Item.ItemCode,
                        ItemName = i.Item.ItemName,
                        UnitName = i.Item.ItemUomGroups
                            .Where(u => u.UomEntry == i.UoMEntry)
                            .Select(u => u.UomCode)
                            .FirstOrDefault(),
                        Batches = i.DeliveryNoteBatches.Select(b => new DeliveryNoteBatchDTO
                        {
                            DeliveryNoteBatchId = b.DeliveryNoteBatchId,
                            DeliveryNoteItemId = b.DeliveryNoteItemId,
                            Quantity = b.Quantity,
                            Comment = b.Comment,
                            BatchNumber = b.BatchNumber,
                            ExpiryDate = b.ExpiryDate
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return GeneralResponse<DeliveryNoteOrderDTO>.FailResponse("Not Found");

            return GeneralResponse<DeliveryNoteOrderDTO>.SuccessResponse(result);
        }
    }
}

