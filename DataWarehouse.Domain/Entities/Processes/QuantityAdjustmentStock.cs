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
    public class QuantityAdjustmentStock : IOrder
    {
        public int QuantityAdjustmentStockId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public GeneralStatus Status { get; set; }
        public string? Comment { get; set; }
        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public string? DocType { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ReasonId { get; set; }

        [NotMapped]
        public int Id
        {
            get => QuantityAdjustmentStockId;
            set => QuantityAdjustmentStockId = value;
        }


        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<QuantityAdjustmentStockItem> QuantityAdjustmentStockItems { get; set; } = new List<QuantityAdjustmentStockItem>();
    }
    
}
