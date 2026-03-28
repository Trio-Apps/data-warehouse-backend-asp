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
    public class DeliveryNoteItem : IOrderItem
    {

        public int DeliveryNoteItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => DeliveryNoteOrderId;
            set => DeliveryNoteOrderId = value;
        }

        public GeneralItemStatus Status { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public decimal? UnitPrice { get; set; }
        public decimal? VatPercent { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? LineTotalBeforeVat { get; set; }
        public decimal? LineTotalAfterVat { get; set; }
        public string? ErrorMessage { get; set; }
        public int? LineNum { get; set; }


        // Navigation
        public int DeliveryNoteOrderId { get; set; } // FK to DeliveryNoteOrder
        public DeliveryNoteOrder DeliveryNoteOrder { get; set; }

        public int ItemId { get; set; } // FK to Item
        public Item Item { get; set; }

        public int? SalesOrderItemId { get; set; }
        public SalesOrderItem? SalesOrderItem { get; set; }
        public SalesReturnOrderItem? SalesReturnOrderItem { get; set; }


        public ICollection<DeliveryNoteBatch> DeliveryNoteBatches { get; set; } = new List<DeliveryNoteBatch>();
    }
}
