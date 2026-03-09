using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;

namespace DataWarehouse.Domain.Entities.Processes
{
public class TransferredRequestBatch : IOrderBatch
    {
        public int TransferredRequestBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public int OrderItemId
        {
            get => TransferredRequestItemId;
            set => TransferredRequestItemId = value;
        }

        // Navigation
        public int TransferredRequestItemId { get; set; }
        public TransferredRequestItem TransferredRequestItem { get; set; }

        public ICollection<TransferredStockBatch> TransferredStockBatches { get; set; } = new List<TransferredStockBatch>();
    }
}
