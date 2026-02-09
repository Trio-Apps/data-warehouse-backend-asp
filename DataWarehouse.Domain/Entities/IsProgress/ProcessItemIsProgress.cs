using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.IsProgress
{
    public class ProcessItemIsProgress
    {
        public int ProcessItemIsProgressId { get; set; }   // SalesOrderId / POId / ...
        [Required]
        public ProcessType ProcessType { get; set; } // SalesOrder / Transfer // Count Stock etc....
        public int ReferenceId { get; set; } // الـ ID الفعلي (SalesOrderId, POId, etc.)
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [Required]
        public ProcessStatus Status { get; set; } // panding / Approval / reject
        public int CurrentStepOrder { get; set; } = 1; // الخطوة الحالية

        public ICollection<ProcessApproval> ProcessApprovals { get; set; } = new List<ProcessApproval>();
    }
}
