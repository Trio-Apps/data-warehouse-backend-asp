using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Entities.Actors;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Notifications;



namespace DataWarehouse.Domain.Entities.Auth;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }

    // public SapUser? UserSap { get; set; }

    // admin
    public CompanyUser? CompanyUser { get; set; }
    // manager
    public ICollection<SapUser>? UserSaps { get; set; } = new List<SapUser>();
    // employee
    //public SapEmployee? SapEmployee { get; set; }
    public ICollection<UserWarehouses>? UserWarehouses { get; set; } = new List<UserWarehouses>();

    //b
    public ICollection<SapSyncStatusFront> SapSyncStatusFronts { get; set; } = new List<SapSyncStatusFront>();

    // 

    public ICollection<ProcessApproval> ProcessApprovals { get; set; } = new List<ProcessApproval>();

    // 
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<DeliveryNoteOrder> DeliveryNoteOrders { get; set; } = new List<DeliveryNoteOrder>();

    public ICollection<SalesReturnOrder> SalesReturnOrders { get; set; } = new List<SalesReturnOrder>();

    // TransferredStock
    public ICollection<TransferredStock> TransferredStocks { get; set; } = new List<TransferredStock>();
    public ICollection<TransferredRequest> TransferredRequests { get; set; } = new List<TransferredRequest>();
  
    // ReceivedStock
    public ICollection<ReceivedStock> ReceivedStocks { get; set; } = new List<ReceivedStock>();

 
    // CountStock
    public ICollection<CountStock> CountStocks { get; set; } = new List<CountStock>();
    public ICollection<QuantityAdjustmentStock> QuantityAdjustmentStocks { get; set; } = new List<QuantityAdjustmentStock>();

    // PurchaseOrder
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    // ProductionOrder
    public ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();

    ////
    ///

    // Receipt Good
    public ICollection<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; } = new List<ReceiptPurchaseOrder>();
    public ICollection<GoodsReturnOrder> GoodsReturnOrders { get; set; } = new List<GoodsReturnOrder>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

}
