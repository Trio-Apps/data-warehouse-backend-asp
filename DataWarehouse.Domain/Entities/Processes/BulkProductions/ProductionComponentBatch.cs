namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public class ProductionComponentBatch
    {
        public int ProductionComponentBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProductionComponentLineId { get; set; }
        public ProductionComponentLine ProductionComponentLine { get; set; }
    }
}
