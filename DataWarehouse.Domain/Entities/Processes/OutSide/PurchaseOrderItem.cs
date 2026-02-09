using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public  class PurchaseOrderItem
    {
        public int PurchaseOrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation
        public int PurchaseOrderId { get; set; }
        public int ItemId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public Item Item { get; set; }

     //   public ICollection<PurchaseReceipt> PurchaseReceipts { get; set; } = new List<PurchaseReceipt>();

    }
}
