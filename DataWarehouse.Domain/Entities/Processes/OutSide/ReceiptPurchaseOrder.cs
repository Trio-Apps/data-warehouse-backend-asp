using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public  class ReceiptPurchaseOrder : IOrder
    {
        public int ReceiptPurchaseOrderId { get; set; }
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }


        public int? DocEntry { get; set; }
        public int? DocNum { set; get; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }


        [NotMapped]
        public int Id
        {
            get => ReceiptPurchaseOrderId;
            set => ReceiptPurchaseOrderId = value;
        }
        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }


        // Navigation
        public GoodsReturnOrder? GoodsReturnOrder { get; set; }
        public int? PurchaseOrderId { get; set; }  // FK ? PurchaseOrder
        public PurchaseOrder? PurchaseOrder { get; set; }   // Assuming PurchaseStock entity
        public ICollection<ReceiptPurchaseOrderItem> ReceiptPurchaseOrderItems { get; set; }
            = new List<ReceiptPurchaseOrderItem>();

    }
}



