using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataWarehouse.Domain.Entities.Processes
{
public class TransferredRequest : IOrder
    {
        public int TransferredRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PostingDate { get; set; }

        public DateTime DueDate { get; set; }
        public GeneralStatus Status { get; set; }
        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }


        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }

        [NotMapped]
        public int Id
        {
            get => TransferredRequestId;
            set => TransferredRequestId = value;
        }


        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int DistinationWarehouseId { get; set; }
        public Warehouse DistinationWarehouse { get; set; }

        public ICollection<TransferredRequestItem> TransferredRequestItems { get; set; } = new List<TransferredRequestItem>();
        public TransferredStock? TransferredStock { get; set; }
    }
}
