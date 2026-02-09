using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.BarCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.BarCode
{
    public class DynamicBarCode
    {
        public int DynamicBarCodeId { get; set; }
        public string BarCode { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public int AbsEntry { get; set; }

        public bool SapFlag { get; set; }
        public int ItemBarCodeId { get; set; }
        public ItemBarCode  ItemBarCode { get; set; }
        public int SapId { get; set; }
        public Sap Sap { get; set; }

    }
}
