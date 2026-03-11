using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.IsProgress
{
    public class ProcessSettingApproval
    {
        public int ProcessSettingApprovalId { get; set; }

        public ProcessType ProcessType { get; set; } // SalesOrder / Transfer // Count Stock etc....

        public bool IgnoreSteps { get; set; } = true;

        public int CompanyId { get; set; }

        public Company Company { get; set; }


        public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>(); 
    }
}
