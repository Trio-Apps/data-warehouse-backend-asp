using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class DeliveryNoteBatch : IOrderBatch
    {
        public int DeliveryNoteBatchId { get; set; }
        [NotMapped]
        public int OrderItemId
        {
            get => DeliveryNoteItemId;
            set => DeliveryNoteItemId = value;
        }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SalesOrderBatchId { get; set; }
        public SalesOrderBatch? SalesOrderBatch { get; set; }

        public SalesReturnOrderBatch? SalesReturnOrderBatch { get; set; } 

        public int DeliveryNoteItemId { get; set; }
        public DeliveryNoteItem DeliveryNoteItem { get; set; }



    }
}
