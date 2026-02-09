using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class AddCompanyDto
    {
        public string Name { get; set; }
    }
    public class UpdateCompanyDto
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }




}
