using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes;
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
    public interface IDeliveryNoteItemRepository : IBaseRepository<DeliveryNoteItem>
    {
        // Get by DeliveryNoteOrderId
        Task<GeneralResponse<IEnumerable<DeliveryNoteItemDTO>>> GetByDeliveryNoteOrderIdAsync(int deliveryNoteOrderId);

        Task<GeneralResponse<PagedResult<DeliveryNoteItemDTO>>> GetByDeliveryNoteOrderIdWithPaginationAsync(
            int deliveryNoteOrderId, int pageNumber, int pageSize);

        // Add item to DeliveryNoteOrder WITHOUT reference (manual add)
        // نفس فكرة AddSalesReturnItemBySalesReturnOrderIdWithoutRefAsync
        Task<GeneralResponse<DeliveryNoteItemDTO>> AddDeliveryNoteItemByDeliveryNoteOrderIdWithoutRefAsync(
            int deliveryNoteOrderId,
            bool isBarcode,
            DynamicBarcodesDto? barcodeDto,
            AddGeneralItemDto? dto);

        // Update item WITHOUT reference
        Task<GeneralResponse<DeliveryNoteItemDTO>> UpdateDeliveryNoteItemWithoutRefAsync(
            int deliveryNoteItemId,
            UpdateGeneralItemDto dto);

        // Add item to DeliveryNoteOrder BY SalesOrderItemId (reference)
        // نفس فكرة AddSalesReturnOrderItemByDeliveryNoteItemIdAsync
        Task<GeneralResponse<DeliveryNoteItemDTO>> AddDeliveryNoteItemBySalesOrderItemIdAsync(
            string userId,
            int deliveryNoteOrderId,
            AddDeliveryNoteOrderItemDTO dto);

        // Update DeliveryNoteItem (reference/manual)
        Task<GeneralResponse<DeliveryNoteItemDTO>> UpdateDeliveryNoteItemAsync(
            int deliveryNoteItemId,
            UpdateDeliveryNoteOrderItemDTO dto);

        // Entities queries
        Task<IEnumerable<DeliveryNoteItem>> GetByDeliveryNoteOrderIdEntitiesAsync(int deliveryNoteOrderId);

        Task<IEnumerable<DeliveryNoteItem>> GetByItemIdAsync(int itemId);

        // Includes
        Task<DeliveryNoteItem?> GetWithDeliveryNoteOrderAsync(int deliveryNoteItemId);

        Task<DeliveryNoteItem?> GetWithSalesOrderItemAsync(int deliveryNoteItemId);

        Task<DeliveryNoteItem?> GetWithItemAsync(int deliveryNoteItemId);

        Task<DeliveryNoteItem?> GetWithBatchesAsync(int deliveryNoteItemId);
    }
}
