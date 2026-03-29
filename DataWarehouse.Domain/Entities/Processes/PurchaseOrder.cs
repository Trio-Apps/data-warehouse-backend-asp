using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class PurchaseOrder : IOrder
    {
        public int PurchaseOrderId { get; set; }  // Primary Key
        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public int Id
        {
            get => PurchaseOrderId;
            set => PurchaseOrderId = value;
        }

        public int? DocEntry {  get; set; }
        public int? DocNum {  set; get; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }

        // Header Totals
        public decimal? TotalBeforeVat { get; set; }
        public decimal? TotalVat { get; set; }
        public decimal? TotalAfterVat { get; set; }

        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }


        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public ICollection<ReceiptPurchaseOrder> ReceiptPurchaseOrders { get; set; }
            = new List<ReceiptPurchaseOrder>();

        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
            = new List<PurchaseOrderItem>();
    }
}
