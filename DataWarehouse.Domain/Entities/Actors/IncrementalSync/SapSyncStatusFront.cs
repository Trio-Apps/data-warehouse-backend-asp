using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors.IncrementalSync
{
    public class SapSyncStatusFront
    {
        public int SapSyncStatusFrontId { get; set; }
        public string EntityName { get; set; }
        public DateTime LastSyncDate { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
