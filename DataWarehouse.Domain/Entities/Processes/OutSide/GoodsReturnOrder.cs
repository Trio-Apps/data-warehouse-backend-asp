using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class GoodsReturnOrder : IOrder
    {
        public int GoodsReturnOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public GeneralStatus Status { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }

        [NotMapped]
        public int Id
        {
            get => GoodsReturnOrderId;
            set => GoodsReturnOrderId = value;
        }
        // navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int? ReceiptPurchaseOrderId { get; set; }
        public ReceiptPurchaseOrder? ReceiptPurchaseOrder { get; set; }

        public ICollection<GoodsReturnOrderItem> GoodsReturnOrderItems { get; set; } = new List<GoodsReturnOrderItem>();

    }
}
