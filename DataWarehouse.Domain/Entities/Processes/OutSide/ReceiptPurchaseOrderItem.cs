using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public  class ReceiptPurchaseOrderItem
    {
        public int ReceiptPurchaseOrderItemId { get; set; }


        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }

        // Navigation

        public GoodsReturnOrderItem GoodsReturnOrderItem { get; set; }
        public int ReceiptPurchaseOrderId { get; set; }
        public int ItemId { get; set; }
        public ReceiptPurchaseOrder ReceiptPurchaseOrder { get; set; }
        public Item Item { get; set; }

        public ICollection<ReceiptPurchaseOrderBatch> ReceiptPurchaseOrderBatches { get; set; } = new List<ReceiptPurchaseOrderBatch>();

    }
}
