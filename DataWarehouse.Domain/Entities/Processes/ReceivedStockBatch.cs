using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class ReceivedStockBatch
    {
        public int ReceivedStockBatchId { get; set; }   
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        // SAP Goods Receipt Document (DocEntry)
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TransferredStockBatchId { get; set; }
        public TransferredStockBatch TransferredStockBatch { get; set; }
     
        public int ReceivedItemId { get; set; }
        public ReceivedItem ReceivedItem { get; set; }
    
    }
}
