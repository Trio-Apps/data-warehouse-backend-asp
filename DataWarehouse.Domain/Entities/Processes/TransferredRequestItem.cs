using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;

namespace DataWarehouse.Domain.Entities.Processes
{
public class TransferredRequestItem : IOrderItem
    {
        public int TransferredRequestItemId { get; set; }
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
            get => TransferredRequestId;
            set => TransferredRequestId = value;
        }

        // Navigation
        public int TransferredRequestId { get; set; }
        public TransferredRequest TransferredRequest { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public ICollection<TransferredRequestBatch> TransferredRequestBatches { get; set; } = new List<TransferredRequestBatch>();
        public ICollection<TransferredItem> TransferredItems { get; set; } = new List<TransferredItem>();
    }
}
