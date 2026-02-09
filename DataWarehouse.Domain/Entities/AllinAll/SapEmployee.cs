using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.AllinAll
{
    public class SapEmployee
    {
        public int SapEmployeeId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int SapId { get; set; }
        public Sap Sap { get; set; }
    }
}
