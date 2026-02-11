using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
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

        Task<GeneralResponse<TOrderItem>> UpdateOrderItemAsync<TOrderItem>(
    int itemIdFromRoute,
    ProcessType processType,
    UpdateGeneralItemDto dto,
    Expression<Func<TOrderItem, bool>> itemSelector,
    DbSet<TOrderItem> itemSet)
    where TOrderItem : class, IOrderItem;


        Task<GeneralResponse<TOrderItem>> DeleteOrderItemAsync<TOrderItem>(
        int itemIdFromRoute,
        ProcessType processType,
        Expression<Func<TOrderItem, bool>> itemSelector,
        DbSet<TOrderItem> itemSet)
        where TOrderItem : class, IOrderItem;

    

    Task<GeneralResponse<TBatch>> AddOrderBatchAsync<TOrderItem, TBatch>(
    int orderItemId,
    ProcessType processType,
    GeneralBatchDto dto,
    Expression<Func<TOrderItem, bool>> orderItemSelector,
    DbSet<TOrderItem> orderItemSet,
    Expression<Func<TBatch, bool>> batchItemSelector,
    DbSet<TBatch> batchSet)
    where TOrderItem : class, IOrderItem
    where TBatch : class, IOrderBatch, new();

        Task<GeneralResponse<TBatch>> UpdateOrderBatchAsync<TOrderItem, TBatch>(
                 int batchId,
                 ProcessType processType,
                 UpdateGeneralBatchDto dto,

                 DbSet<TBatch> batchSet,
                 DbSet<TOrderItem> orderItemSet,

                 Expression<Func<TBatch, int>> batchIdSelector,
                 Expression<Func<TBatch, int>> orderItemIdSelector,
                 Expression<Func<TOrderItem, int>> orderItemIdForItemSelector)
                 where TOrderItem : class, IOrderItem
                 where TBatch : class, IOrderBatch;

        Task<GeneralResponse<TBatch>> DeleteOrderBatchAsync<TOrderItem, TBatch>(
         int batchIdFromRoute,
         ProcessType processType,

         DbSet<TBatch> batchSet,
         DbSet<TOrderItem> orderItemSet,

         // selectors (على الأعمدة الحقيقية)
         Expression<Func<TBatch, int>> batchIdSelector,
         Expression<Func<TBatch, int>> batchOrderItemIdSelector,
         Expression<Func<TOrderItem, int>> orderItemPkSelector,
         Expression<Func<TOrderItem, int>> orderIdSelector)
         where TOrderItem : class
         where TBatch : class;

    }

}
