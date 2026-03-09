using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class QuantityAdjustmentStockItem : IOrderItem
    {
        public int QuantityAdjustmentStockItemId { get; set; }

        public decimal Quantity { get; set; }
        public int UoMEntry { get; set; }
        public string? BarCode { get; set; }

        // Pending, Planned, Released, Received, Closed, Failed
        public GeneralItemStatus Status { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Comment { get; set; }
        public int? LineNum { get; set; }


        [NotMapped]
        public int OrderId
        {
            get => QuantityAdjustmentStockId;
            set => QuantityAdjustmentStockId = value;
        }


        // Navigation
        public int QuantityAdjustmentStockId { get; set; }
        public QuantityAdjustmentStock QuantityAdjustmentStock { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public ICollection<QuantityAdjustmentStockBatch> QuantityAdjustmentStockBatches { get; set; } = new List<QuantityAdjustmentStockBatch>();

    }
}
