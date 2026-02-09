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
    public class TransferredStock
    {
        public int TransferredStockId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public GeneralStatus Status { get; set; }

        public string? Comment { get; set; }


        // Navigation
        public ReceivedStock ReceivedStock { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }

        public int DistinationWarehouseId { get; set; }

        public Warehouse DistinationWarehouse { get; set; }

        public ICollection<TransferredItem> TransferredItems { get; set; }= new List<TransferredItem>();
    }
}
