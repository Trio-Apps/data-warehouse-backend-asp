using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Based
{
    public interface IBaseProcessesRepository<T>
    {

        Task<GeneralWithTwoGenericResponse<PagedResult<TDto>, TExtra>> GetOrderItemsByOrderIdWithPaginationAsync<TOrder, TOrderItem, TDto, TExtra, TStatusEnum>(
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
   where TStatusEnum : struct, Enum;

        Task<GeneralResponse<IEnumerable<TDto>>> GetOrderItemsByOrderIdAsync<TOrder, TOrderItem, TDto>(
        int orderId,
        Expression<Func<TOrder, bool>> orderIdSelector,
        DbSet<TOrder> orderSet,
        DbSet<TOrderItem> itemSet,
        Expression<Func<TOrderItem, bool>> itemFilter,
        Func<IQueryable<TOrderItem>, IQueryable<TOrderItem>>? include = null,
        Expression<Func<TOrderItem, TDto>> selector = null!
    )
        where TOrder : class, IOrder
        where TOrderItem : class, IOrderItem;

        Task<GeneralResponse<TOrderItem>> AddOrderItemAsync<TOrder, TOrderItem>(
        int orderId,
        ProcessType processType,
        bool isBarcode,
        DynamicBarcodesDto? barcodeDto,
        AddGeneralItemDto? dto,
         Expression<Func<TOrder, bool>> orderIdSelector,
        DbSet<TOrder> orderSet,
        DbSet<TOrderItem> itemSet)
        where TOrder : class, IOrder
        where TOrderItem : class, IOrderItem, new();

       Task<GeneralResponse<TOrderItem>> UpdateOrderItemAsync<TOrder, TOrderItem>(
    int itemIdFromRoute,
    ProcessType processType,
    UpdateGeneralItemDto dto,
       DbSet<TOrder> orderSet,
    Expression<Func<TOrderItem, bool>> itemSelector,
    DbSet<TOrderItem> itemSet)
    where TOrder : class, IOrder
    where TOrderItem : class, IOrderItem;


        Task<GeneralResponse<TOrderItem>> DeleteOrderItemAsync<TOrder, TOrderItem>(
       int itemIdFromRoute,
       ProcessType processType,
         DbSet<TOrder> orderSet,
       Expression<Func<TOrderItem, bool>> itemSelector,
       DbSet<TOrderItem> itemSet)
        where TOrder : class, IOrder
       where TOrderItem : class, IOrderItem;
   
        // batch

           Task<GeneralResponse<IEnumerable<TDto>>> GetOrderBatchesAsync<TOrderItem, TBatch, TDto>(
    int orderItemId,
    Expression<Func<TOrderItem, bool>> orderItemSelector,
    DbSet<TOrderItem> orderItemSet,
    Expression<Func<TBatch, bool>> batchItemSelector,
    DbSet<TBatch> batchSet,
    Func<TBatch, TDto> map)
    where TOrderItem : class, IOrderItem
    where TBatch : class, IOrderBatch;
        Task<GeneralResponse<TBatch>> AddOrderBatchAsync<TOrder, TOrderItem, TBatch>(
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
            where TBatch : class, IOrderBatch, new();


        Task<GeneralResponse<TBatch>> UpdateOrderBatchAsync<TOrder, TOrderItem, TBatch>(
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
     where TBatch : class, IOrderBatch;

        Task<GeneralResponse<TBatch>> DeleteOrderBatchAsync<TOrder, TOrderItem, TBatch>(
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
              where TBatch : class;

        Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync<TEntity>(
          int entityId,
          ProcessType processType,
          Expression<Func<TEntity, bool>> selector,
          DbSet<TEntity> dbSet)
          where TEntity : class, IOrder;
    }
}
