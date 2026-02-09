using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Domain.Entities.Auth;
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
    public class ProcessApproval
    {
      
        public int ProcessApprovalId { get; set; }

        
        [Required]
        public ApprovalStatus Status { get; set; }

        public string? Comment { get; set; }

        public DateTime? ActionDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        // Navigation
        public int ApprovalStepId { get; set; }
        public ApprovalStep ApprovalStep { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int ProcessItemIsProgressId { get; set; }
        public ProcessItemIsProgress ProcessItemIsProgress { get; set; }

    }
}
