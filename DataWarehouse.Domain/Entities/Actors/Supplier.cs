using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class Supplier
    {

        public int SupplierId { get; set; }

        [MaxLength(50)]
        public string SupplierCode { get; set; }

        [MaxLength(200)]
        public string SupplierName { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }


        // Navigation property: one supplier has many supplier items
        public ICollection<SupplierItem> SupplierItems { get; set; } = new List<SupplierItem>();

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

        public ICollection<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; } = new List<ReceiptPurchaseOrder>();

        public ICollection<GoodsReturnOrder> GoodsReturnOrders { get; set; } = new List<GoodsReturnOrder>();
    }
}
