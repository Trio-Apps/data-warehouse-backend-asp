using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class GoodsReturnOrderBatch : IOrderBatch
    {
        public int GoodsReturnOrderBatchId { get; set; }

        [NotMapped]
        public int OrderItemId
        {
            get => GoodsReturnOrderItemId;
            set => GoodsReturnOrderItemId = value;
        }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ReceiptPurchaseOrderBatchId { get; set; }
        public ReceiptPurchaseOrderBatch? ReceiptPurchaseOrderBatch { get; set; }
        public int GoodsReturnOrderItemId { get; set; }
        public GoodsReturnOrderItem GoodsReturnOrderItem { get; set; }

    }
}
