using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class CountStockBatch : IOrderBatch
    {
        public int CountStockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; } = null;
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }


        [NotMapped]
        public int OrderItemId
        {
            get => CountStockItemId;
            set => CountStockItemId = value;
        }


        public int CountStockItemId { get; set; }
        public CountStockItem CountStockItem { get; set; }
    }
}
