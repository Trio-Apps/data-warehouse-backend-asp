using DataWarehouse.Domain.Entities.Processes.BulkProductions;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class ReceiptPurchaseOrderBatch : IOrderBatch
    {
        public int ReceiptPurchaseOrderBatchId { get; set; }
     
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotMapped]
        public int OrderItemId
        {
            get => ReceiptPurchaseOrderItemId;
            set => ReceiptPurchaseOrderItemId = value;
        }


        public GoodsReturnOrderBatch GoodsReturnOrderBatch { get; set; } 

        public int ReceiptPurchaseOrderItemId { get; set; }
        public ReceiptPurchaseOrderItem ReceiptPurchaseOrderItem { get; set; }
    }
}
