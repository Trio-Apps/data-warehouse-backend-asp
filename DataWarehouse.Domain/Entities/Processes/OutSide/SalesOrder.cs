using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes.OutSide
{
    public class SalesOrder : IOrder
    {
        public int SalesOrderId { get; set; }

        // Draft=1, Processing=2, Completed=3, PartiallyFailed=4
        public GeneralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }


        [NotMapped]
        public int Id
        {
            get => SalesOrderId;
            set => SalesOrderId = value;
        }

        public int? DocEntry { get; set; }
        public int? DocNum { set; get; }
        public string? DocType { get; set; }

        // Navigation
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        // customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } // or User

        public int SapId { get; set; }
        public Sap Sap { get; set; }
      //  public SalesReturnOrder SalesReturnOrder { get; set; }

        public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
        public ICollection<DeliveryNoteOrder> DeliveryNoteOrders { get; set; } = new List<DeliveryNoteOrder>();

        
    }
}
