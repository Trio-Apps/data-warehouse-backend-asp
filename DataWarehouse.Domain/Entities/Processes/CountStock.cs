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

namespace DataWarehouse.Domain.Entities.Processes
{

    public class CountStock : IOrder
    {
        public int CountStockId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public GeneralStatus Status { get; set; }

        public int? DocEntry { get; set; }
        public int? DocNum { set; get; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }
        public string? ErrorMessage { get; set; }


        [NotMapped]
        public int Id
        {
            get => CountStockId;
            set => CountStockId = value;
        }


        public string? Comment { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }

        // Navigation
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public ICollection<CountStockItem> CountStockItem { get; set; }
            = new List<CountStockItem>();
    }

}
