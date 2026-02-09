using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.Processes
{
    public class BulkProductionPlannedDto
    {
        public string DueDate { get; set; }
        public string PostingDate { get; set; }
        public decimal PlannedQuantity { get; set; }
        public string ItemNo { get; set; }
        public string Warehouse { get; set; }
        public int Series {  get; set; }

    }

    public class BulkProductionReleasedDto
    {
        public string ProductionOrderStatus { get; set; }
     

    }
}
