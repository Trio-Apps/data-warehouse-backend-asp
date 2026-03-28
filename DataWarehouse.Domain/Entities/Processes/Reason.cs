using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Enums.Approval;

namespace DataWarehouse.Domain.Entities.Processes;

public class Reason
{
    public int ReasonId { get; set; }
    public string Name { get; set; } = null!;
    public ProcessType ProcessType { get; set; }
    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<QuantityAdjustmentStock> QuantityAdjustmentStocks { get; set; } = new List<QuantityAdjustmentStock>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<CountStock> CountStocks { get; set; } = new List<CountStock>();
    public ICollection<TransferredRequest> TransferredRequests { get; set; } = new List<TransferredRequest>();
    public ICollection<TransferredStock> TransferredStocks { get; set; } = new List<TransferredStock>();
    public ICollection<ReceivedStock> ReceivedStocks { get; set; } = new List<ReceivedStock>();
    public ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<DeliveryNoteOrder> DeliveryNoteOrders { get; set; } = new List<DeliveryNoteOrder>();
    public ICollection<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; } = new List<ReceiptPurchaseOrder>();
    public ICollection<SalesReturnOrder> SalesReturnOrders { get; set; } = new List<SalesReturnOrder>();
    public ICollection<GoodsReturnOrder> GoodsReturnOrders { get; set; } = new List<GoodsReturnOrder>();
}
