using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class CountStockItem
    {
        public int CountStockItemId { get; set; }


        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }
        public int? LineNum { get; set; }


        // Navigation
        public int CountStockId { get; set; }
        public CountStock CountStock { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public ICollection<CountStockBatch> CountStockBatches { get; set; } = new List<CountStockBatch>();
       
    }
}
