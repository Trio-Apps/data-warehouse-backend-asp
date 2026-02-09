using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesOrderBatch
    {
        public int SalesOrderBatchId {  get; set; }

        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public SalesReturnOrderBatch SalesReturnOrderBatch { get; set; }
        public int SalesOrderItemId { get; set; }
        public SalesOrderItem SalesOrderItem { get; set; }

    }
}
