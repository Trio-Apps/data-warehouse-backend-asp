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
        public string? BarCode { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => SalesOrderId;
            set => SalesOrderId = value;
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

        [NotMapped]
        public decimal ExecuteQuantity { get; set; }

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
