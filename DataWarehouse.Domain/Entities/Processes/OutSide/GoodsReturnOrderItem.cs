using DataWarehouse.Domain.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class GoodsReturnOrderItem
    {

        public int GoodsReturnOrderItemId { get; set; }


        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        //  public PurchaseItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }

        // Navigation

        public int GoodsReturnOrderId { get; set; }
        public GoodsReturnOrder GoodsReturnOrder { get; set; }

        public int ReceiptPurchaseOrderItemId { get; set; }
        public ReceiptPurchaseOrderItem ReceiptPurchaseOrderItem { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public ICollection<GoodsReturnOrderBatch> GoodsReturnOrderBatches { get; set; } = new List<GoodsReturnOrderBatch>();

    }
}
