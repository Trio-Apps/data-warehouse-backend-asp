using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{

    // transfer draft 
    public class ReceivedStock
    {
        public int ReceivedStockId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }
        public GeneralStatus Status { get; set; }


        // Navigation
        public int TransferredStockId { get; set; }
        public TransferredStock TransferredStock { get; set; } 

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }
        public int SourceWarehouseId { get; set; }
        public Warehouse SourceWarehouse { get; set; }

        public ICollection<ReceivedItem> ReceivedItems { get; set; } = new List<ReceivedItem>();

    }
}
