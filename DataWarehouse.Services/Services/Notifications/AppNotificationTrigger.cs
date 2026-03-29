using DataWarehouse.Core.DTOs.Notifications;
using DataWarehouse.Core.Interfaces.Notifications;
using DataWarehouse.Core.IServices.Notifications;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Enums.Approval;
using Microsoft.EntityFrameworkCore;

namespace DataWarehouse.Services.Services.Notifications;

public class AppNotificationTrigger : IAppNotificationTrigger
{
    private readonly DataWarehouseDbContext _context;
    private readonly IAppNotificationService _notificationService;

    public AppNotificationTrigger(DataWarehouseDbContext context, IAppNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task TriggerProcessStatusNotificationAsync(ProcessType processType, int referenceId, string actionType)
    {
        var userId = await GetOrderOwnerUserIdAsync(processType, referenceId);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var normalizedActionType = actionType switch
        {
            "Approved" => "تمت الموافقة",
            "Rejected" => "تم الرفض",
            _ => actionType
        };

        var processName = FormatProcessType(processType);
        var title = actionType switch
        {
            "Approved" => $"{processName} تمت الموافقة",
            "Rejected" => $"{processName} تم الرفض",
            _ => $"{processName} {actionType}"
        };

        var message = actionType switch
        {
            "Approved" => $"{processName} رقم {referenceId} تمت الموافقة عليه.",
            "Rejected" => $"{processName} رقم {referenceId} تم رفضه.",
            _ => $"{processName} رقم {referenceId} تم تحديثه."
        };

        await _notificationService.AddNotificationAsync(new CreateAppNotificationDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            ActionType = normalizedActionType,
            DocumentType = processName,
            ProcessType = processType.ToString(),
            ReferenceId = referenceId
        });
    }

    public async Task TriggerOrderCreatedNotificationAsync(ProcessType processType, int referenceId, string userId, bool isDraft)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var processName = FormatProcessType(processType);
        var title = isDraft
            ? $"{processName} تم إنشاؤه كمسودة"
            : $"{processName} تم إنشاؤه";
        var message = isDraft
            ? $"{processName} رقم {referenceId} تم حفظه كمسودة."
            : $"{processName} رقم {referenceId} تم إنشاؤه بنجاح.";

        await _notificationService.AddNotificationAsync(new CreateAppNotificationDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            ActionType = "إنشاء",
            DocumentType = processName,
            ProcessType = processType.ToString(),
            ReferenceId = referenceId
        });
    }

    private async Task<string?> GetOrderOwnerUserIdAsync(ProcessType processType, int referenceId)
    {
        return processType switch
        {
            ProcessType.Purchase => await _context.PurchaseOrders.Where(x => x.PurchaseOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Sales => await _context.SalesOrders.Where(x => x.SalesOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.SalesReturn => await _context.SalesReturnOrders.Where(x => x.SalesReturnOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Receipt => await _context.ReceiptPurchaseOrders.Where(x => x.ReceiptPurchaseOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.GoodsReturn => await _context.GoodsReturnOrders.Where(x => x.GoodsReturnOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Production => await _context.ProductionOrders.Where(x => x.ProductionOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Transferred => await _context.TransferredStocks.Where(x => x.TransferredStockId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Received => await _context.ReceivedStocks.Where(x => x.ReceivedStockId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.Counting => await _context.CountStocks.Where(x => x.CountStockId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.DeliveryNote => await _context.DeliveryNoteOrders.Where(x => x.DeliveryNoteOrderId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.TransferredRequest => await _context.TransferredRequests.Where(x => x.TransferredRequestId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            ProcessType.QuantityAdjustment => await _context.QuantityAdjustmentStocks.Where(x => x.QuantityAdjustmentStockId == referenceId).Select(x => x.UserId).FirstOrDefaultAsync(),
            _ => null
        };
    }

    private static string FormatProcessType(ProcessType processType)
    {
        return processType switch
        {
            ProcessType.GoodsReturn => "أمر مرتجع المشتريات",
            ProcessType.SalesReturn => "أمر المرتجع",
            ProcessType.DeliveryNote => "أمر التسليم",
            ProcessType.TransferredRequest => "طلب تحويل",
            ProcessType.QuantityAdjustment => "أمر تسوية الكمية",
            ProcessType.Received => "أمر الاستلام المخزني",
            ProcessType.Transferred => "أمر التحويل",
            ProcessType.Counting => "أمر الجرد",
            ProcessType.Receipt => "أمر الاستلام",
            ProcessType.Purchase => "أمر الشراء",
            ProcessType.Sales => "أمر البيع",
            ProcessType.Production => "أمر الإنتاج",
            _ => processType.ToString()
        };
    }
}
