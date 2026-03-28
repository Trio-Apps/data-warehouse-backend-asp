using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class DeliveryNoteOrder : IOrder
    {
        public int DeliveryNoteOrderId { get; set; }

        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Comment { get; set; }

        public int? DocEntry { get; set; }
        public int? DocNum { set; get; }
        public string? DocType { get; set; }
        public int? ReasonId { get; set; }

        [NotMapped]
        public int Id
        {
            get => DeliveryNoteOrderId;
            set => DeliveryNoteOrderId = value;
        }
        // Navigation
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reason? Reason { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int? SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; }

        // customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } // or User

        public SalesReturnOrder SalesReturnOrder { get; set; }

        public ICollection<DeliveryNoteItem> DeliveryNoteItems { get; set; } = new List<DeliveryNoteItem>();


    }
}
