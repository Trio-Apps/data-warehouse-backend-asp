using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.BulkProductions
{
    public  class ProductionOrder
    {
        public int ProductionOrderId { get; set; }

        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Remarks { get; set; }


        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<ProductionOrderItem> ProductionOrderItems { get; set; }
            = new List<ProductionOrderItem>();


    }
}
