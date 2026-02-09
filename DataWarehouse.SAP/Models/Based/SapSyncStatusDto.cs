using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.Based
{
    public  class SapSyncStatusDto
    {
        public int SapSyncStatusId { get; set; }
        public string EntityName { get; set; }
        public DateTime LastSyncDate { get; set; }
    }
}
