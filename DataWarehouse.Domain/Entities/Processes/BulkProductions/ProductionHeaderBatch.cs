namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public class ProductionHeaderBatch
    {
        public int ProductionHeaderBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
    }
}
