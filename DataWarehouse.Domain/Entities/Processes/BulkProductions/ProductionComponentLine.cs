using DataWarehouse.Domain.Entities.Actors;

namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public class ProductionComponentLine
    {
        public int ProductionComponentLineId { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal? IssuedQuantity { get; set; }
        public decimal? InWhsQuantity { get; set; }
        public string IssueType { get; set; } = "Backflush";
        public DateTime CreatedAt { get; set; }

        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public ICollection<ProductionComponentBatch> ProductionComponentBatches { get; set; } = new List<ProductionComponentBatch>();
    }
}
