using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class ProcessesTypesDate
    {
        public int ProcessesTypesDateId { get; set; }
        public DateOnly PostingDate {  get; set; }
        public DateOnly DueDate { get; set; }

        public int ProcessesTypeId { get; set; }
        public ProcessesType ProcessesType { get; set; }
    }
}
