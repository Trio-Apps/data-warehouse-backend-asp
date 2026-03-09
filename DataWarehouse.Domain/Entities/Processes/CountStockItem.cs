using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class CountStockItem : IOrderItem
    {
        public int CountStockItemId { get; set; }

        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }
        public int? LineNum { get; set; }

        [NotMapped]
        public int OrderId
        {
            get => CountStockId;
            set => CountStockId = value;
        }

        // Navigation
        public int CountStockId { get; set; }
        public CountStock CountStock { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public ICollection<CountStockBatch> CountStockBatches { get; set; } = new List<CountStockBatch>();
    }
}
