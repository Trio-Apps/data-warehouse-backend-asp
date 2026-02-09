using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class GoodsReturnOrder
    {
        public int GoodsReturnOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public GeneralStatus Status { get; set; }
        public string? Comment { get; set; }

        // navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int ReceiptPurchaseOrderId { get; set; }
        public ReceiptPurchaseOrder ReceiptPurchaseOrder { get; set; }

        public ICollection<GoodsReturnOrderItem> GoodsReturnOrderItems { get; set; } = new List<GoodsReturnOrderItem>();

    }
}
