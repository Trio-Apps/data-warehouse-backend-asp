using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class ProcessesType
    {
        public int ProcessesTypeId { get; set; }
        public string ProcessesName { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public ICollection<ProcessesTypesDate> ProcessesTypesDates { get; set; } = new HashSet<ProcessesTypesDate>();
    }
}
