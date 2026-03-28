using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
    // transfer draft
    public class ReceivedStock : IOrder
    {
        public int ReceivedStockId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }
        public GeneralStatus Status { get; set; }

        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }

        // Navigation
        public int? TransferredStockId { get; set; }
        public TransferredStock? TransferredStock { get; set; }
        public Reason? Reason { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int SourceWarehouseId { get; set; }
        public Warehouse SourceWarehouse { get; set; }

        public ICollection<ReceivedItem> ReceivedItems { get; set; } = new List<ReceivedItem>();

        [NotMapped]
        public int Id
        {
            get => ReceivedStockId;
            set => ReceivedStockId = value;
        }
    }
}
