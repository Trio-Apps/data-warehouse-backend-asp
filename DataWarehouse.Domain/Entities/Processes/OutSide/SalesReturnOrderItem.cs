using DataWarehouse.Domain.Entities.Actors;
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
    public class SalesReturnOrderItem : IOrderItem
    {
        public int SalesReturnOrderItemId { get; set; } 
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => SalesReturnOrderId;
            set => SalesReturnOrderId = value;
        }

        // Navigation
        public int SalesReturnOrderId { get; set; }
        public SalesReturnOrder SalesReturnOrder { get; set; }
        public int ItemId { get; set; } // FK to Item
        public Item Item { get; set; }


        public int? DeliveryNoteItemId { get; set; }
        public DeliveryNoteItem? DeliveryNoteItem { get; set; }
        public ICollection<SalesReturnOrderBatch> SalesReturnOrderBatches { get; set; } = new List<SalesReturnOrderBatch>();
    }
}
