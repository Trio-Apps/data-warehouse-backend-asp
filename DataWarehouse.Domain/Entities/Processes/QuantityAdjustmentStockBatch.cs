using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class QuantityAdjustmentStockBatch : IOrderBatch
    {
        public int QuantityAdjustmentStockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public int OrderItemId
        {
            get => QuantityAdjustmentStockItemId;
            set => QuantityAdjustmentStockItemId = value;
        }


        // Navigation
        public int QuantityAdjustmentStockItemId { get; set; }
        public QuantityAdjustmentStockItem QuantityAdjustmentStockItem { get; set; }
    }
}
