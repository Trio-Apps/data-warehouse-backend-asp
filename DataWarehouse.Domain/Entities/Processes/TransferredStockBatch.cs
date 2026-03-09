using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class TransferredStockBatch : IOrderBatch
    {
        public int TransferredStockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public int OrderItemId
        {
            get => TransferredItemId;
            set => TransferredItemId = value;
        }

        // Reference to request batch (like SalesOrderBatch on DeliveryNoteBatch)
        public int? TransferredRequestBatchId { get; set; }
        public TransferredRequestBatch? TransferredRequestBatch { get; set; }

        public ReceivedStockBatch? ReceivedStockBatch { get; set; }
        public int TransferredItemId { get; set; }
        public TransferredItem TransferredItem { get; set; }
    }
}
