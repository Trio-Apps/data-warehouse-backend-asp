using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Approval
{
    public class ApprovalDto
    {
    }
    
    public class AddApprovalStepDto
    {
        [Required]
        [MaxLength(100)]
        public string StepName { get; set; }

        public int StepOrder { get; set; }

        [Required]
        public string RoleId { get; set; }

       // public bool IsFinalStep { get; set; }

    }

    public class UpdateApprovalStepDto
    {
        public int ApprovalStepId { get; set; }

        [MaxLength(100)]
        public string? StepName { get; set; }

        public int? StepOrder { get; set; }

        public string? RoleId { get; set; }

        // public bool IsFinalStep { get; set; }

    }

    public class ApprovalStepDto
    {
        public int ApprovalStepId { get; set; }
        [Required]
        [MaxLength(100)]
        public string StepName { get; set; }
        public int StepOrder { get; set; }
        [Required]
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFinalStep { get; set; }
        public int CompanyId { get; set; }
    }

 
    public class ProcessApprovalDto
    {
        public int ProcessApprovalId { get; set; }

        [Required]
        public string Status { get; set; }

        public string? Comment { get; set; }

        public DateTime? ActionDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        // Navigation
        public int ApprovalStepId { get; set; }
        public ApprovalStepDto ApprovalStep { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int ProcessItemIsProgressId { get; set; }
        public ProcessItemIsProgressDto ProcessItemIsProgress { get; set; }
    }
    public class ProcessItemIsProgressDto
    {
        public int ProcessItemIsProgressId { get; set; }   // SalesOrderId / POId / ...
        [Required]
        public string? ProcessType { get; set; } // SalesOrder / Transfer // Count Stock etc....
        public int ReferenceId { get; set; } // الـ ID الفعلي (SalesOrderId, POId, etc.)
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [Required]
        public string? Status { get; set; } // panding / Approval / reject
     
    }

    public class ApprovalAccessResult
    {
        public bool CanApprove { get; set; }
        public int? ProcessItemIsProgressId { get; set; }
        public int? ProcessApprovalId { get; set; } // Useful لو هتستخدمه في الموافقة المباشرة
        public string? Reason { get; set; } // سبب الرفض لو حبيت تستخدمه للـ Debug أو UI
    }


}
