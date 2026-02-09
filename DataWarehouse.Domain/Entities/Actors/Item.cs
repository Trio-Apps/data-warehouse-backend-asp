using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class Item
    {
 
        public int ItemId { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string UoM { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool BatchNumbers { get; set; }
        public int SapId { get; set; }
        public Sap Sap { get; set; }
        public string ProcurementType { get; set; }
        // last updated 

        // Navigation property


        //WarehouseItem
        public ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();

        //
        public ICollection<SupplierItem> SupplierItems { get; set; } = new List<SupplierItem>();

        //
        public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
        public ICollection<SalesReturnOrderItem> SalesReturnOrderItems { get; set; } = new List<SalesReturnOrderItem>();

        // BinLocations
        public ICollection<BinLocation> BinLocations { get; set; } = new List<BinLocation>();

        // StaticBarCodeItem
        // DynamicBarCodeItem
        public ICollection<ItemBarCode> ItemBarCodes { get; set; } = new List<ItemBarCode>();
        public ICollection<ItemUomGroup> ItemUomGroups { get; set; } = new List<ItemUomGroup>();



        /// <summary>
        ///  Processes
        /// </summary>
        /// 

        // Finished item
    //    public ICollection<FinishedGoodItem> FinishedGoodItems { get; set; } = new List<FinishedGoodItem>();

        //ReceivedItem
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();


        //TransferredItem
        public ICollection<TransferredItem> TransferredItems { get; set; } = new List<TransferredItem>();

        //ReceivedItem
        public ICollection<ReceivedItem> ReceivedItems { get; set; } = new List<ReceivedItem>();

        //CountStockItem
        public ICollection<CountStockItem> CountStockItems { get; set; } = new List<CountStockItem>();


      

        //ReceiptPurchaseOrderItem
        public ICollection<ReceiptPurchaseOrderItem> ReceiptPurchaseOrderItems { get; set; } = new List<ReceiptPurchaseOrderItem>();
        //ProductionOrderItem
        public ICollection<ProductionOrderItem> ProductionOrderItems { get; set; } = new List<ProductionOrderItem>();

        public ICollection<GoodsReturnOrderItem> GoodsReturnOrderItems { get; set; } = new List<GoodsReturnOrderItem>();


    }
}
