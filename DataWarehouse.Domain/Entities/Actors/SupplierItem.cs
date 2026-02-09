using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class SupplierItem
    {
     
        public int SupplierItemId { get; set; }

        public decimal PurchasePrice { get; set; }
        public int LeadTimeDays { get; set; }
        public bool IsPreferred { get; set; }

        // Navigation properties

        //
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        //
        public int ItemId { get; set; } 
        public Item Item { get; set; }
    }
}
