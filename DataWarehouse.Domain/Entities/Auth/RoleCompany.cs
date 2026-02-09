using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Auth
{
    public class RoleCompany
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public string RoleId { get; set; }
        public ApplicationRole Role { get; set; }
    }
}
