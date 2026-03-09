using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class Warehouse
    {

        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }
        public DateTime UpdateDate { get; set; }

        public int SapId { get; set; }
        public Sap Sap { get; set; }



        ////b
        //public string UserId { get; set; }
        //public ApplicationUser User { get; set; }

        //WarehouseItem
        public ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();


        //d
        public ICollection<UserWarehouses> UserWarehouses { get; set; } = new List<UserWarehouses>();


        /// <summary>
        ///  Processes
        /// </summary>
        /// 

        // Finished item
   //     public ICollection<FinishedGoodItem> FinishedGoodItems { get; set; } = new List<FinishedGoodItem>();

        //PurchaseOrder
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();


        // 
        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        public ICollection<DeliveryNoteOrder> DeliveryNoteOrders { get; set; } = new List<DeliveryNoteOrder>();

        // TransferredStock
        public ICollection<TransferredStock> TransferredStocks { get; set; } = new List<TransferredStock>();
        // TransferredStock // this Working as a Customer to Warehouse Customer Others
        public ICollection<TransferredStock> DistinationTransferredStocks { get; set; } = new List<TransferredStock>();
        public ICollection<TransferredRequest> TransferredRequests { get; set; } = new List<TransferredRequest>();
        public ICollection<TransferredRequest> DistinationTransferredRequests { get; set; } = new List<TransferredRequest>();

        // ReceivedStock
        public ICollection<ReceivedStock> ReceivedStocks { get; set; } = new List<ReceivedStock>();

        // SourceReceivedStocks /// this Working as a Supplier to Warehouse others
        public ICollection<ReceivedStock> SourceReceivedStocks { get; set; } = new List<ReceivedStock>();


        // CountStock
        public ICollection<CountStock> CountStocks { get; set; } = new List<CountStock>();
        public ICollection<QuantityAdjustmentStock> QuantityAdjustmentStocks { get; set; } = new List<QuantityAdjustmentStock>();

      
        // ProductionOrder
        public ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
        public ICollection<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; } = new List<ReceiptPurchaseOrder>();
        public ICollection<GoodsReturnOrder> GoodsReturnOrders { get; set; } = new List<GoodsReturnOrder>();

        // approval 
        public ICollection<ProcessApproval> ProcessApprovals { get; set; } = new List<ProcessApproval>();


    }
}
