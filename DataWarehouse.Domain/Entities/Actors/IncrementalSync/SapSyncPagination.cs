using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors.IncrementalSync
{
    public class SapSyncPagination
    {
        public int SapSyncPaginationId { get; set; }
        public string EntityName { get; set; }
        public int Skip { get; set; }

      public int SapId { get; set; }
        public Sap Sap { get; set; }

    }
}
