using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class CountStockBatch
    {
        public int CountStockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CountStockItemId { get; set; }
        public CountStockItem CountStockItem { get; set; }
    }
}
