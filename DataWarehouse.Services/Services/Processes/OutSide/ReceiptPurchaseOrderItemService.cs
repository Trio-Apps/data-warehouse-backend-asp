using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.IServices.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Processes.OutSide;

public class ReceiptPurchaseOrderItemService : BaseService<ReceiptPurchaseOrderItem>, IReceiptPurchaseOrderItemService
{
    private readonly IReceiptPurchaseOrderItemRepository _receiptPurchaseOrderItemRepository;

    public ReceiptPurchaseOrderItemService(IReceiptPurchaseOrderItemRepository receiptPurchaseOrderItemRepository) : base(receiptPurchaseOrderItemRepository)
    {
        _receiptPurchaseOrderItemRepository = receiptPurchaseOrderItemRepository;
    }

    public async Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByReceiptPurchaseOrderIdAsync(int receiptPurchaseOrderId)
    {
        return await _receiptPurchaseOrderItemRepository.GetByReceiptPurchaseOrderIdAsync(receiptPurchaseOrderId);
    }

    public async Task<IEnumerable<ReceiptPurchaseOrderItem>> GetByItemIdAsync(int itemId)
    {
        return await _receiptPurchaseOrderItemRepository.GetByItemIdAsync(itemId);
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithReceiptPurchaseOrderAsync(int receiptPurchaseOrderItemId)
    {
        return await _receiptPurchaseOrderItemRepository.GetWithReceiptPurchaseOrderAsync(receiptPurchaseOrderItemId);
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithItemAsync(int receiptPurchaseOrderItemId)
    {
        return await _receiptPurchaseOrderItemRepository.GetWithItemAsync(receiptPurchaseOrderItemId);
    }

    public async Task<ReceiptPurchaseOrderItem?> GetWithCommentAsync(int receiptPurchaseOrderItemId)
    {
        return await _receiptPurchaseOrderItemRepository.GetWithCommentAsync(receiptPurchaseOrderItemId);
    }
}
