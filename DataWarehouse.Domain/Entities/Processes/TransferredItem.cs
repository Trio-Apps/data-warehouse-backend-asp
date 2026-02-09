using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Enums;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class TransferredItem
    {
        public int TransferredItemId { get; set; }

        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }
        // Pending, Planned, Released, Received, Closed, Failed
          public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }

        // Navigation
        public ReceivedItem ReceivedItem { get; set; }
        public int TransferredStockId { get; set; }
        public TransferredStock TransferredStock { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public ICollection<TransferredStockBatch> TransferredStockBatches { get; set; } = new List<TransferredStockBatch>();

    }
}
