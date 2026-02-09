using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesReturnOrderItem
    {
        public int SalesReturnOrderItemId { get; set; } 
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation
        public int SalesReturnOrderId { get; set; }
        public SalesReturnOrder SalesReturnOrder { get; set; }
        public int ItemId { get; set; } // FK to Item
        public Item Item { get; set; }


        public int SalesOrderItemId { get; set; }
        public SalesOrderItem SalesOrderItem { get; set; }
        public ICollection<SalesReturnOrderBatch> SalesReturnOrderBatches { get; set; } = new List<SalesReturnOrderBatch>();
    }
}
