using DataWarehouse.Domain.Entities.AllinAll;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.IsProgress
{
    public class ApprovalStep
    {
       
        public int ApprovalStepId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; }

        public int StepOrder { get; set; }

        [Required]
        public string RoleId { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFinalStep { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        // Navigation
        public ICollection<ProcessApproval> ProcessApprovals { get; set; } = new List<ProcessApproval>();


    }
}
