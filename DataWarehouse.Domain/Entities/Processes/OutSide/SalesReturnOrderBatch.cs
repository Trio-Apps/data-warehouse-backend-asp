using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesReturnOrderBatch
    {

        public int SalesReturnOrderBatchId { get; set; }    
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SalesOrderBatchId { get; set; }

        public SalesOrderBatch SalesOrderBatch { get; set; }

        public int SalesReturnOrderItemId { get; set; }
        public SalesReturnOrderItem SalesReturnOrderItem { get; set; }
    }
}
