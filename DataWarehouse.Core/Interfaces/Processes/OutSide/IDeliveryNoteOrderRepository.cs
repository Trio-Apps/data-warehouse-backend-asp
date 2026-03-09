using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes.OutSide
{
 

    public interface IDeliveryNoteOrderRepository : IBaseRepository<DeliveryNoteOrder>
    {
        Task<IEnumerable<DeliveryNoteOrder>> GetByWarehouseIdAsync(int warehouseId);

        Task<GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>> GetByWarehouseIdWithPaginationAsync(int warehouseId, int pageNumber, int pageSize);

        Task<GeneralResponse<PagedResult<DeliveryNoteOrderDTO>>> GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            int warehouseId,
            string userId,
            int? customerId,
            DateTime? postingDate,
            DateTime? DueDate,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> GetWithCustomerAsync(int deliveryNoteOrderId, string userId, CancellationToken cancellationToken = default);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> GetDeliveryNoteOrderByIdAsync(string userId, int deliveryNoteOrderId, CancellationToken cancellationToken = default);

        // إنشاء Delivery Note بناءً على Sales Order (الـ Parent)
        Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderAndItemsBySalesOrderIdAsync(
               string userId,
               AddDeliveryNoteOrderDTO dto);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderWithoutRefAsync(string userId, AddDeliveryNoteOrderWithoutRefDTO dto);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> AddDeliveryNoteOrderAsync(string userId, AddDeliveryNoteOrderDTO dto);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> UpdateDeliveryNoteOrderAsync(string userId, int deliveryNoteOrderId, UpdateDeliveryNoteOrderDTO dto);
        Task<GeneralResponse<ProcessItemIsProgressDto>> RevertPartiallyFailedStatusToProcessingAsync(int deliveryNoteOrderId);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> DeleteDeliveryNoteOrderAsync(int deliveryNoteOrderId, CancellationToken cancellationToken = default);

        // الحصول على الـ Delivery Note المرتبطة بـ Sales Order معين
        Task<GeneralResponse<DeliveryNoteOrderDTO>> GetBySalesOrderIdAsync(int salesOrderId);

        Task<IEnumerable<DeliveryNoteOrder>> GetByUserIdAsync(string userId);

        Task<DeliveryNoteOrder?> GetWithItemsAsync(int deliveryNoteOrderId);

        Task<DeliveryNoteOrder?> GetWithSalesOrderAsync(int deliveryNoteOrderId);

        Task<DeliveryNoteOrder?> GetWithWarehouseAsync(int deliveryNoteOrderId);

        Task<GeneralResponse<DeliveryNoteOrderDTO>> GetWithItemsAndBatchesAsync(int deliveryNoteOrderId);
    }
}
