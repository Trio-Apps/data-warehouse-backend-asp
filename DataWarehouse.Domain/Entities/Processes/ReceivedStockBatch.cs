using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class ReceivedStockBatch : IOrderBatch
    {
        public int ReceivedStockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? TransferredStockBatchId { get; set; }
        public TransferredStockBatch? TransferredStockBatch { get; set; }

        public int ReceivedItemId { get; set; }
        public ReceivedItem ReceivedItem { get; set; }

        [NotMapped]
        public int OrderItemId
        {
            get => ReceivedItemId;
            set => ReceivedItemId = value;
        }
    }
}
