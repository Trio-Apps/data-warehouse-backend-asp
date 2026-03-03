using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Processes
{
    public class GeneralOrderDto : ApprovalAccessResult
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Warehouse ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
        public int WarehouseId { get; set; }

        public string? Comment { get; set; }
        public string? WarehouseCode { get; set; }

        public string? ErrorMessage { get; set; }


        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } // PurchaseStatus enum
        public int? ItemCount { get; set; }

        [Required(ErrorMessage = "Posting Date is required")]
        public DateTime PostingDate { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool? Approval { get; set; }
        public string ApprovalStatus { get; set; }
        public bool? IsReturn { get; set; }
        public int? ReturnOrderId { get; set; }
    }

    public class AddGeneralOrderDto
    {
        [Required(ErrorMessage = "Posting Date is required")]
        public DateTime PostingDate { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        public DateTime DueDate { get; set; }

        public string? Comment { get; set; }
        public bool IsDraft { get; set; }
    }
    public class UpdateGeneralOrderDto
    {
        public DateTime? PostingDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Comment { get; set; }
        public bool IsDraft { get; set; }


    }
}
