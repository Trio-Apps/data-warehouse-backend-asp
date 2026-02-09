using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public  class ReceiptPurchaseOrder
    {
        public int ReceiptPurchaseOrderId { get; set; }
        public PurchaseStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }


        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }


        // Navigation
        public GoodsReturnOrder GoodsReturnOrder { get; set; }
        public int PurchaseOrderId { get; set; }  // FK → PurchaseOrder
        public PurchaseOrder PurchaseOrder { get; set; }   // Assuming PurchaseStock entity
        public ICollection<ReceiptPurchaseOrderItem> ReceiptPurchaseOrderItems { get; set; }
            = new List<ReceiptPurchaseOrderItem>();
    }
}
