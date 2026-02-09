using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors.IncrementalSync
{
    public class SapSyncStatus
    {
        public int SapSyncStatusId { get; set; }
       public string  EntityName { get; set; }
       public DateTime LastSyncDate { get; set; }

        public int SapId { get; set; }
        public Sap Sap { get; set; }
    }
}
