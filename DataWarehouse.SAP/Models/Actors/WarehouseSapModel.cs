using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;

namespace DataWarehouse.SAP.Models.Actors
{
    public  class WarehouseSapModel
    {
        public class WarehouseSapResponse
        {
            public List<WarehouseDto> Value { get; set; }
        }
        public class WarehouseDto
        {
            public string WarehouseCode { get; set; }
            public string WarehouseName { get; set; }
        }

    }
}
