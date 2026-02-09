using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public class ProductionOrderItem
    {
        public int ProductionOrderItemId { get; set; }

    
        public decimal PlannedQuantity { get; set; }
        public decimal? ProducedQuantity { get; set; }

        // SAP Production Order AbsoluteEntry
        public int? AbsoluteEntry { get; set; }

        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }

        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
       
        // finished 
        public DateTime? ProcessedAt { get; set; }

        // Navigation
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public ICollection<ProductionReceipt> ProductionReceipts { get; set; } = new List<ProductionReceipt>();
    }
}
