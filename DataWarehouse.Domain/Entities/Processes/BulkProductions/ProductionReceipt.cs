using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public class ProductionReceipt
    {
        public int ProductionReceiptId { get; set; }
        public int ProductionOrderItemId { get; set; }
        public ProductionOrderItem  ProductionOrderItem { get; set; }
        public decimal ProducedQuantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
