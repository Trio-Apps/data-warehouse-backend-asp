using DataWarehouse.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.AllinAll
{
    public class CompanyUser
    {
        public int CompanyUserId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
