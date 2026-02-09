using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.BarCode
{
    public class ItemUomGroup
    {
         public int ItemUomGroupId { get; set; }
         public float BaseQty { get; set; }

        public string UomCode {  get; set; }
        public int UomEntry {  get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }


        public int SapId { get; set; }
        public Sap Sap { get; set; }
    }
}
