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
    public class SalesReturnOrder
    {
        public int SalesReturnOrderId { get; set; }

        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        //public SalesOrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public GeneralStatus Status { get; set; }
        public string? Comment { get; set; }

        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        // customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } // or User

        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        public ICollection<SalesReturnOrderItem> SalesReturnOrderItems { get; set; } = new List<SalesReturnOrderItem>();
    }
}
