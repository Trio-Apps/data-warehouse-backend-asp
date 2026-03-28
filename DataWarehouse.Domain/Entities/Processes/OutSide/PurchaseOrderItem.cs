using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
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
    public  class PurchaseOrderItem : IOrderItem
    {
        public int PurchaseOrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }

        public int? LineNum { get; set; }

        // VAT
        public decimal? VatPercent { get; set; }
        public decimal? VatAmount { get; set; }

        // Totals
        public decimal? LineTotalBeforeVat { get; set; }
        public decimal? LineTotalAfterVat { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => PurchaseOrderId;
            set => PurchaseOrderId = value;
        }
        // Navigation
        public int PurchaseOrderId { get; set; }
        public int ItemId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public Item Item { get; set; }

        public ReceiptPurchaseOrderItem? ReceiptPurchaseOrderItem { get; set; }

     //   public ICollection<PurchaseReceipt> PurchaseReceipts { get; set; } = new List<PurchaseReceipt>();

    }
}
