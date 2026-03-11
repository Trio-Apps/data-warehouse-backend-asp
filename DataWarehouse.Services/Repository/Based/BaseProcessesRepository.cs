using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Based
{
    public class BaseProcessesRepository<T> : BaseRepository<T>, IBaseProcessesRepository<T> where T : class
    {
        private readonly IProcessItemIsProgressRepository progressRepository;
        private readonly IBarCodeOrdersRepository barcodeOrder;
        private readonly ISapCache sapCache;

        public BaseProcessesRepository(IProcessItemIsProgressRepository progressRepository, IBarCodeOrdersRepository barcodeOrder, ISapCache sapCache, DataWarehouseDbContext context) : base(context)
        {
            this.progressRepository = progressRepository;
            this.barcodeOrder = barcodeOrder;
            this.sapCache = sapCache;
        }

        #region processes

        public async Task<ProcessItemIsProgress> GetProcessItem(int OrderId, ProcessType type, CancellationToken cancellationToken = default)
        {
            return await _context.ProcessItemIsProgresses
         .AsNoTracking()
         .Where(p =>
         p.ProcessType == type &&
         p.ReferenceId == OrderId)
         .OrderByDescending(p => p.ProcessItemIsProgressId) // آخر حالة
         .FirstOrDefaultAsync(cancellationToken);
        }

        #region order item
        public async Task<GeneralWithTwoGenericResponse<PagedResult<TDto>, TExtra>> GetOrderItemsByOrderIdWithPaginationAsync<TOrder, TOrderItem, TDto, TExtra, TStatusEnum>(
  int orderId,
  int pageNumber,
  int pageSize,
  string? status,
  Func<TOrder, TExtra> extraSelector,
  Expression<Func<TOrder, bool>> orderIdSelector,
  DbSet<TOrder> orderSet,
  DbSet<TOrderItem> itemSet,
  Expression<Func<TOrderItem, bool>> itemFilter,
  Func<IQueryable<TOrderItem>, IQueryable<TOrderItem>>? include,
  Expression<Func<TOrderItem, TDto>> selector,
  Expression<Func<TOrderItem, int>> orderByDescSelector,
  Expression<Func<TOrderItem, TStatusEnum>> itemStatusSelector
)
  where TOrder : class, IOrder
  where TOrderItem : class, IOrderItem
  where TStatusEnum : struct, Enum
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var order = await orderSet.AsNoTracking().FirstOrDefaultAsync(orderIdSelector);
            if (order == null)
                return GeneralWithTwoGenericResponse<PagedResult<TDto>, TExtra>.FailResponse("Order not found");



            IQueryable<TOrderItem> query = itemSet.AsNoTracking();

            if (include != null)
                query = include(query);

            query = query.Where(itemFilter);

            // Status filter (string -> enum)
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<TStatusEnum>(status, true, out var statusEnum))
                    return GeneralWithTwoGenericResponse<PagedResult<TDto>, TExtra>.FailResponse("Invalid status");

                query = query.Where(x => EF.Property<TStatusEnum>(x, ((MemberExpression)itemStatusSelector.Body).Member.Name).Equals(statusEnum));
                // NOTE: السطر ده بيعتمد ان itemStatusSelector عبارة عن x => x.Status (member access)
                // لو ممكن تبقى expression أعقد، قولي وهنعملها safer.
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(orderByDescSelector)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            var extra = extraSelector(order);

            return GeneralWithTwoGenericResponse<PagedResult<TDto>, TExtra>.SuccessResponse(
                new PagedResult<TDto>
                {
                    Data = data,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                },
                extra
            );
        }


        public async Task<GeneralResponse<IEnumerable<TDto>>> GetOrderItemsByOrderIdAsync<TOrder, TOrderItem, TDto>(
        int orderId,
        Expression<Func<TOrder, bool>> orderIdSelector,
        DbSet<TOrder> orderSet,
        DbSet<TOrderItem> itemSet,
        Expression<Func<TOrderItem, bool>> itemFilter,
        Func<IQueryable<TOrderItem>, IQueryable<TOrderItem>>? include = null,
        Expression<Func<TOrderItem, TDto>> selector = null!
    )
        where TOrder : class, IOrder
        where TOrderItem : class, IOrderItem
        {
            // 1) Validate Order exists
            var order = await orderSet.AsNoTracking().FirstOrDefaultAsync(orderIdSelector);
            if (order == null)
                return GeneralResponse<IEnumerable<TDto>>.FailResponse("Order not found");



            // 3) Build query
            IQueryable<TOrderItem> query = itemSet.AsNoTracking();

            if (include != null)
                query = include(query);

            var data = await query
                .Where(itemFilter)
                .Select(selector)
                .ToListAsync();

            return GeneralResponse<IEnumerable<TDto>>.SuccessResponse(data);
        }


        public async Task<GeneralResponse<TOrderItem>> AddOrderItemAsync<TOrder, TOrderItem>(
            int orderId,
            ProcessType processType,
            bool isBarcode,
            DynamicBarcodesDto? barcodeDto,
            AddGeneralItemDto? dto,
            Expression<Func<TOrder, bool>> orderIdSelector,
            DbSet<TOrder> orderSet,
            DbSet<TOrderItem> itemSet)
            where TOrder : class, IOrder
            where TOrderItem : class, IOrderItem, new()
        {

            var order = await orderSet.FirstOrDefaultAsync(orderIdSelector);

            if (order == null)
                return GeneralResponse<TOrderItem>.FailResponse("Order not found");

            var approval = await GetProcessItem(orderId, processType);

            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TOrderItem>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");
           
            
            if (order.Status == GeneralStatus.Completed)
                return GeneralResponse<TOrderItem>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");


            TOrderItem model = new TOrderItem();

            if (isBarcode)
            {
                if (barcodeDto == null)
                    return GeneralResponse<TOrderItem>.FailResponse("Barcode required");

                var isDynamic = await CheckDynamicCodeValidationLocal(barcodeDto.BarCode);

                var res = isDynamic
                    ? await barcodeOrder.GetItemByDynamicBarCodeAsync(order.WarehouseId, barcodeDto)
                    : await barcodeOrder.GetItemByStaticBarCodeAsync(order.WarehouseId, barcodeDto);

                if (!res.Success || res.Data == null)
                    return GeneralResponse<TOrderItem>.FailResponse(res.Message);

                model.OrderId = orderId;
                model.ItemId = res.Data.Id;
                model.Quantity = res.Data.Quantity;
                model.BarCode = res.Data.Barcode;
                model.UnitPrice = res.Data.Price;
                model.UoMEntry = res.Data.UoMEntry;
                model.Status = GeneralItemStatus.Planned;
            }
            else
            {
                if (dto == null)
                    return GeneralResponse<TOrderItem>.FailResponse("DTO required");

                var item = await _context.Items.FirstOrDefaultAsync(x => x.ItemId == dto.ItemId);

                if (item == null)
                    return GeneralResponse<TOrderItem>.FailResponse("Item not found");

                model.OrderId = orderId;
                model.ItemId = dto.ItemId;
                model.Quantity = dto.Quantity;
                model.BarCode = "";
                model.UnitPrice = dto.UnitPrice ?? item.SalesPrice;
                model.UoMEntry = dto.UoMEntry;
                model.Status = GeneralItemStatus.Planned;
            }

            await itemSet.AddAsync(model);
            await _context.SaveChangesAsync();

            return GeneralResponse<TOrderItem>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<TOrderItem>> UpdateOrderItemAsync<TOrder,TOrderItem>(
    int itemIdFromRoute,
    ProcessType processType,
    UpdateGeneralItemDto dto,
       DbSet<TOrder> orderSet,
    Expression<Func<TOrderItem, bool>> itemSelector,
    DbSet<TOrderItem> itemSet)
    where TOrder : class, IOrder
    where TOrderItem : class, IOrderItem
        {
            var entity = await itemSet.FirstOrDefaultAsync(itemSelector);


            if (entity == null)
                return GeneralResponse<TOrderItem>.FailResponse("id is not found");


            var order = await orderSet.FindAsync(entity.OrderId);


            if (order == null)
                return GeneralResponse<TOrderItem>.FailResponse("order id is not found");


            // Approval check
            //  var approval = await GetProcessItem(entity.OrderId, processType);

            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TOrderItem>.FailResponse(
            //        "You cannot edit any item because its approval status is 'Approved' and all approval steps have been completed."
            //    );

            if (order.Status == GeneralStatus.Completed)
                return GeneralResponse<TOrderItem>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");


            // Update rules (زي اللي عندك)
            if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
                entity.Quantity = dto.Quantity.Value;

            if (dto.UoMEntry > 0)
            {
                entity.UoMEntry = dto.UoMEntry;
            }

            if (dto.UnitPrice.HasValue)
            {
                entity.UnitPrice = dto.UnitPrice;
            }

            await _context.SaveChangesAsync();

            return GeneralResponse<TOrderItem>.SuccessResponse(entity);
        }

        public async Task<GeneralResponse<TOrderItem>> DeleteOrderItemAsync<TOrder, TOrderItem>(
    int itemIdFromRoute,
    ProcessType processType,
      DbSet<TOrder> orderSet,
    Expression<Func<TOrderItem, bool>> itemSelector,
    DbSet<TOrderItem> itemSet)
     where TOrder : class, IOrder
    where TOrderItem : class, IOrderItem
        {
            var entity = await itemSet.FirstOrDefaultAsync(itemSelector);

            if (entity == null)
                return GeneralResponse<TOrderItem>.FailResponse("id is not found");

            var order = await orderSet.FindAsync(entity.OrderId);

            if (order == null)
                return GeneralResponse<TOrderItem>.FailResponse("order id is not found");


            // Approval check
            // var approval = await GetProcessItem(entity.OrderId, processType);

            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TOrderItem>.FailResponse(
            //        "You cannot delete any item because its approval status is 'Approved' and all approval steps have been completed."
            //    );

            if (order.Status == GeneralStatus.Completed)
                return GeneralResponse<TOrderItem>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");

            // Snapshot قبل الحذف علشان نرجعه في response
            var snapshot = entity;

            itemSet.Remove(entity);
            await _context.SaveChangesAsync();

            return GeneralResponse<TOrderItem>.SuccessResponse(snapshot);
        }


        #endregion


        #region batches
        public async Task<GeneralResponse<IEnumerable<TDto>>> GetOrderBatchesAsync<TOrderItem, TBatch, TDto>(
int orderItemId,

Expression<Func<TOrderItem, bool>> orderItemSelector,
DbSet<TOrderItem> orderItemSet,
Expression<Func<TBatch, bool>> batchItemSelector,
DbSet<TBatch> batchSet,
Func<TBatch, TDto> map)
where TOrderItem : class, IOrderItem
where TBatch : class, IOrderBatch
        {
            // 1) Load OrderItem
            var orderItem = await orderItemSet
                .AsNoTracking()
                .FirstOrDefaultAsync(orderItemSelector);

            if (orderItem == null)
                return GeneralResponse<IEnumerable<TDto>>.FailResponse("Order Item not found");


            // 3) Get batches
            var batches = await batchSet
                .AsNoTracking()
                .Where(batchItemSelector)
                .ToListAsync();

            // 4) Map
            var dtos = batches.Select(map);

            return GeneralResponse<IEnumerable<TDto>>.SuccessResponse(dtos);
        }
        public async Task<GeneralResponse<TBatch>> AddOrderBatchAsync<TOrder, TOrderItem, TBatch>(
    int orderItemId,
    ProcessType processType,
    GeneralBatchDto dto,
           DbSet<TOrder> orderSet,
    Expression<Func<TOrderItem, bool>> orderItemSelector,
    DbSet<TOrderItem> orderItemSet,
    Expression<Func<TBatch, bool>> batchItemSelector,
    DbSet<TBatch> batchSet)
    where TOrder : class, IOrder
    where TOrderItem : class, IOrderItem
    where TBatch : class, IOrderBatch, new()
        {
            // 1) Load OrderItem
            var orderItem = await orderItemSet.FirstOrDefaultAsync(orderItemSelector);
            if (orderItem == null)
                return GeneralResponse<TBatch>.FailResponse("Order Item not found");

            var order = await orderSet.FindAsync(orderItem.OrderId);

            if (order == null)
                return GeneralResponse<TBatch>.FailResponse("order id is not found");


            var canAddBatch = await CanItemAcceptBatchesAsync(orderItem.OrderId, orderItem.ItemId, processType);
            if (!canAddBatch)
                return GeneralResponse<TBatch>.FailResponse("You cannot add any batch to this Item.");

            // 2) Approval check (هنا نستخدم OrderId من الـ entity بعد تحميله)
            //var approval = await GetProcessItem(orderItem.OrderId, processType);
            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TBatch>.FailResponse(
            //        "You cannot add any batch because its approval status is 'Approved' and all approval steps have been completed."
            //    );

            if (order.Status == GeneralStatus.Completed)
                return GeneralResponse<TBatch>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");


            // 3) Sum existing batches
            var existingTotal = await batchSet
                .Where(batchItemSelector)
                .SumAsync(b => (decimal?)b.Quantity) ?? 0m;

            // 4) Validate total qty <= item qty
            var totalAfterAdd = existingTotal + dto.Quantity;
            if (totalAfterAdd > orderItem.Quantity)
                return GeneralResponse<TBatch>.FailResponse("Total batch quantities exceed order item quantity");

            // 5) Add batch
            var batch = new TBatch
            {
                OrderItemId = orderItemId,     // ✅ wrapper يكتب في FK الحقيقي
                Quantity = dto.Quantity,
                Comment = dto.Comment,
                ExpiryDate = dto.ExpiryDate,
                CreatedAt = DateTime.UtcNow,
                BatchNumber = dto.BatchNumber,
                // BatchNumber = dto.BatchNumber // لو موجودة عندك في dto
            };

            await batchSet.AddAsync(batch);
            await _context.SaveChangesAsync();

            return GeneralResponse<TBatch>.SuccessResponse(batch);
        }



        private async Task<bool> CanItemAcceptBatchesAsync(int orderId, int itemId, ProcessType processType)
        {
            // Match production-like behavior for Stock Counting: allow batches regardless of batch-managed flag.
            if (processType == ProcessType.Counting)
                return true;

            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
                return false;

            var warehouseId = await ResolveWarehouseIdByOrderAsync(orderId, processType);
            if (warehouseId.HasValue)
            {
                var warehouseManaged = await _context.WarehouseItems
                    .AsNoTracking()
                    .Where(w => w.WarehouseId == warehouseId.Value && w.ItemId == itemId)
                    .Select(w => (bool?)w.IsBatchManaged)
                    .FirstOrDefaultAsync();

                if (warehouseManaged.HasValue)
                    return warehouseManaged.Value;
            }

            return item.BatchNumbers;
        }

        private async Task<int?> ResolveWarehouseIdByOrderAsync(int orderId, ProcessType processType)
        {
            return processType switch
            {
                ProcessType.Sales => await _context.SalesOrders
                    .AsNoTracking()
                    .Where(x => x.SalesOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.SalesReturn => await _context.SalesReturnOrders
                    .AsNoTracking()
                    .Where(x => x.SalesReturnOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Purchase => await _context.PurchaseOrders
                    .AsNoTracking()
                    .Where(x => x.PurchaseOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Receipt => await _context.ReceiptPurchaseOrders
                    .AsNoTracking()
                    .Where(x => x.ReceiptPurchaseOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.GoodsReturn => await _context.GoodsReturnOrders
                    .AsNoTracking()
                    .Where(x => x.GoodsReturnOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Production => await _context.ProductionOrders
                    .AsNoTracking()
                    .Where(x => x.ProductionOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Transferred => await _context.TransferredStocks
                    .AsNoTracking()
                    .Where(x => x.TransferredStockId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Received => await _context.ReceivedStocks
                    .AsNoTracking()
                    .Where(x => x.ReceivedStockId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.Counting => await _context.CountStocks
                    .AsNoTracking()
                    .Where(x => x.CountStockId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.DeliveryNote => await _context.DeliveryNoteOrders
                    .AsNoTracking()
                    .Where(x => x.DeliveryNoteOrderId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.TransferredRequest => await _context.TransferredRequests
                    .AsNoTracking()
                    .Where(x => x.TransferredRequestId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                ProcessType.QuantityAdjustment => await _context.QuantityAdjustmentStocks
                    .AsNoTracking()
                    .Where(x => x.QuantityAdjustmentStockId == orderId)
                    .Select(x => (int?)x.WarehouseId)
                    .FirstOrDefaultAsync(),

                _ => null
            };
        }

        public async Task<GeneralResponse<TBatch>> UpdateOrderBatchAsync<TOrder, TOrderItem, TBatch>(
            int batchId,
            ProcessType processType,
            UpdateGeneralBatchDto dto,
            DbSet<TOrder> orderSet,
            DbSet<TBatch> batchSet,
            DbSet<TOrderItem> orderItemSet,

            Expression<Func<TBatch, int>> batchIdSelector,
            Expression<Func<TBatch, int>> orderItemIdSelector,
            Expression<Func<TOrderItem, int>> orderItemIdForItemSelector)
                where TOrder : class, IOrder
            where TOrderItem : class, IOrderItem
            where TBatch : class, IOrderBatch
        {
            // 1️⃣ Load batch
            var batch = await batchSet
                .FirstOrDefaultAsync(BuildEquals(batchIdSelector, batchId));

            if (batch == null)
                return GeneralResponse<TBatch>.FailResponse("Order Batch not found");

            // 2️⃣ Load OrderItem
            var orderItemId = batch.OrderItemId;

            var orderItem = await orderItemSet
                .FirstOrDefaultAsync(BuildEquals(orderItemIdForItemSelector, orderItemId));

        
            if (orderItem == null)
                return GeneralResponse<TBatch>.FailResponse("Order Item not found");

            var order = await orderSet.FindAsync(orderItem.OrderId);

            if (order == null)
                return GeneralResponse<TBatch>.FailResponse("order id is not found");



            // 3️⃣ Approval
            //var approval = await GetProcessItem(orderItem.OrderId, processType);
            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TBatch>.FailResponse(
            //        "You cannot edit any batch because its approval status is 'Approved'.");

            if (order.Status == GeneralStatus.Completed)
                return GeneralResponse<TBatch>.FailResponse("You cannot add any item because its approval status is 'Approved' and all approval steps have been completed.");

            // 4️⃣ Sum other batches
            var otherTotal = await batchSet
                .Where(BuildEquals(orderItemIdSelector, orderItemId))
                .SumAsync(b => (decimal?)b.Quantity) ?? 0m;

            var newQty = dto.Quantity ?? batch.Quantity;

            if ((otherTotal - batch.Quantity) + newQty > orderItem.Quantity)
                return GeneralResponse<TBatch>.FailResponse(
                    "Total batch quantities exceed order item quantity");

            // 5️⃣ Update
            if (dto.Quantity.HasValue)
                batch.Quantity = dto.Quantity.Value;

            if (dto.Comment != null)
                batch.Comment = dto.Comment;

            if (dto.BatchNumber != null)
                batch.BatchNumber = dto.BatchNumber;

            if (dto.ExpiryDate.HasValue)
                batch.ExpiryDate = dto.ExpiryDate;

            await _context.SaveChangesAsync();

            return GeneralResponse<TBatch>.SuccessResponse(batch);
        }

        private static Expression<Func<T, bool>> BuildEquals<T>(
    Expression<Func<T, int>> selector,
    int value)
        {
            var param = selector.Parameters[0];
            var body = Expression.Equal(selector.Body, Expression.Constant(value));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }



        public async Task<GeneralResponse<TBatch>> DeleteOrderBatchAsync<TOrder,TOrderItem, TBatch>(
            int batchIdFromRoute,
            ProcessType processType,
                        DbSet<TOrder> orderSet,

            DbSet<TBatch> batchSet,
            DbSet<TOrderItem> orderItemSet,

            // selectors (على الأعمدة الحقيقية)
            Expression<Func<TBatch, int>> batchIdSelector,
            Expression<Func<TBatch, int>> batchOrderItemIdSelector,
            Expression<Func<TOrderItem, int>> orderItemPkSelector,
            Expression<Func<TOrderItem, int>> orderIdSelector)
              where TOrder : class
            where TOrderItem : class
            where TBatch : class
        {
            // 1) Load batch by PK
            var batch = await batchSet.FirstOrDefaultAsync(BuildEquals(batchIdSelector, batchIdFromRoute));
            if (batch == null)
                return GeneralResponse<TBatch>.FailResponse("Batch not found");

            // 2) Get OrderItemId from batch (ترجمة SQL مش لازمة هنا لأن entity اتحملت بالفعل)
            var orderItemId = GetValue(batchOrderItemIdSelector, batch);

            // 3) Load order item
            var orderItem = await orderItemSet.FirstOrDefaultAsync(BuildEquals(orderItemPkSelector, orderItemId));
        
            if (orderItem == null)
                return GeneralResponse<TBatch>.FailResponse("Order Item not found");

          
        


            // 4) Approval check using OrderId from orderItem
            var orderId = GetValue(orderIdSelector, orderItem);

            var order = await orderSet.FindAsync(orderId);

            if (order == null)
                return GeneralResponse<TBatch>.FailResponse("order id is not found");


            //var approval = await GetProcessItem(orderId, processType);
            //if (approval != null && approval.Status == ProcessStatus.Approved)
            //    return GeneralResponse<TBatch>.FailResponse(
            //        "You cannot delete any batch because its approval status is 'Approved' and all approval steps have been completed."
            //    );

            // 5) Delete
            var snapshot = batch;

            batchSet.Remove(batch);
            await _context.SaveChangesAsync();

            return GeneralResponse<TBatch>.SuccessResponse(snapshot);
        }

        #endregion

        #region Revert Partially Status
        public async Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync<TEntity>(
       int entityId,
       ProcessType processType,
       Expression<Func<TEntity, bool>> selector,
       DbSet<TEntity> dbSet)
       where TEntity : class, IOrder
        {
            var entity = await dbSet.FirstOrDefaultAsync(selector);

            if (entity == null)
                return GeneralResponse<ProcessItemIsProgressDto>
                    .FailResponse("Entity not found.");

            if (entity.Status != GeneralStatus.PartiallyFailed)
                return GeneralResponse<ProcessItemIsProgressDto>
                    .FailResponse("Status is not PartiallyFailed.");

            entity.Status = GeneralStatus.Processing;

            await _context.SaveChangesAsync();

            var res = await progressRepository.GetProcessItemIsProgressAsync(
                processType,
                entityId);

            return GeneralResponse<ProcessItemIsProgressDto>
                .SuccessResponse(res, "Status changed to Processing.");
        }
        #endregion

        // ===== helpers =====

        private static int GetValue<TEntity>(
            Expression<Func<TEntity, int>> selector,
            TEntity entity)
        {
            // Compile هنا safe لأننا بنقرأ من entity in-memory (مش Query)
            return selector.Compile()(entity);
        }
        private async Task<bool> CheckDynamicCodeValidationLocal(string barCode)
        {
            var sapId = await sapCache.Get();

            var settings = await _context.BarCodeSettings
                .Where(bs => bs.Company.Saps.Any(s => s.SapId == sapId))
                .ToListAsync();

            foreach (var setting in settings)
            {
                if (barCode.Length != setting.TotalLength)
                    continue;

                return true;
            }

            return false;
        }

        private string GetEnumString(GeneralItemStatus status)
        {
            switch (status)
            {

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
        #endregion
    }
}
