using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.BarCode
{
    public class ItemBarCode
    {
        
        public int ItemBarCodeId { get; set; }

        [Required]
        public string BarCode { get; set; }

        [Required]
        public int UoMEntry { get; set; }

        [Required]
        public int AbsEntry { get; set; }

        public string FreeText { get;set; }    

        public bool SapFlag {  get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
     
        // Navigation
        [Required]
        public int ItemId { get; set; }   // FK → Item
        public Item Item { get; set; }
        public int SapId { get; set; }
        public Sap Sap { get; set; }
        public ICollection<DynamicBarCode> DynamicBarCodes { get; set; } = new HashSet<DynamicBarCode>();

     
    }
}
