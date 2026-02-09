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
    public class CountStock
    {
        public int CountStockId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public GeneralStatus Status { get; set; }

        public string? Comment { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Navigation
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public ICollection<CountStockItem> CountStockItem { get; set; }
            = new List<CountStockItem>();
    }
}
