using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }  // Primary Key
        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }


        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }


        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public ReceiptPurchaseOrder ReceiptPurchaseOrder { get; set; }

        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
            = new List<PurchaseOrderItem>();
    }
}
