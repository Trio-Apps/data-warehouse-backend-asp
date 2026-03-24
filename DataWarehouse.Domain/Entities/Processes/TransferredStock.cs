using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataWarehouse.Domain.Entities.Processes
{
    public class TransferredStock : IOrder
    {
        public int TransferredStockId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PostingDate { get; set; }
        public GeneralStatus Status { get; set; }
        public  ReceivingStatus ReceivingStatus { get; set; }

        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Reference { get; set; }


        public int? DocEntry { get; set; }
        public int? DocNum { set; get; }
        public string? DocType { get; set; }


        [NotMapped]
        public int Id
        {
            get => TransferredStockId;
            set => TransferredStockId = value;
        }

        // Navigation
        public int? TransferredRequestId { get; set; }
        public TransferredRequest? TransferredRequest { get; set; }

        public ReceivedStock? ReceivedStock { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }

        public int DistinationWarehouseId { get; set; }

        public Warehouse DistinationWarehouse { get; set; }

        public ICollection<TransferredItem> TransferredItems { get; set; }= new List<TransferredItem>();
    }
}
