using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class Customer
    {
        public int CustomerId { get; set; }



        [MaxLength(50)]
        public string CustomerCode { get; set; }

        [MaxLength(200)]
        public string? CustomerName { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? PriceListNum { get; set; }

        public int SapId { get; set; }
        public Sap Sap { get; set; }


        // Navigation: Customer ↔ SalesOrders (CreatedBy)
        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        public ICollection<DeliveryNoteOrder> DeliveryNoteOrders { get; set; } = new List<DeliveryNoteOrder>();
        public ICollection<SalesReturnOrder> SalesReturnOrders { get; set; } = new List<SalesReturnOrder>();
    }
}
