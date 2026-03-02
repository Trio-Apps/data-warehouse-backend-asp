using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesOrderItem : IOrderItem
    {
       
        public int SalesOrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => SalesOrderId;
            set => SalesOrderId = value;
        }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation
        public int SalesOrderId { get; set; } // FK to SalesOrder
        public SalesOrder SalesOrder { get; set; }

        public int ItemId { get; set; } // FK to Item
        public Item Item { get; set; }


     // public SalesReturnOrderItem SalesReturnOrderItem { get; set; }
        public ICollection<SalesOrderBatch> SalesOrderBatches { get; set; } = new List<SalesOrderBatch>();
       
        public ICollection<DeliveryNoteItem> DeliveryNoteItems { get; set; } = new List<DeliveryNoteItem>();
    }
}
