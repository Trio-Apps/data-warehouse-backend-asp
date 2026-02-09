using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Processes
{
    public class GeneralItemDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Unit of Measure is required")]
        public int UoMEntry { get; set; }

        public string? BarCode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be zero or greater")]
        public decimal? UnitPrice { get; set; }

        public string? ErrorMessage { get; set; }

        public string Status { get; set; }

        [Required(ErrorMessage = "ItemId is required")]
        public int ItemId { get; set; }

        public string? Comment { get; set; }


        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public bool? IsBatches { get; set; }

        public string? UnitName { get; set; }
    }

    public class AddGeneralItemDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    public class UpdateGeneralItemDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal? Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }


    public class GeneralBatchDto
    {

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        [StringLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters")]
        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
    public class AddGeneralBatchDto
    {

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
    public class UpdateGeneralBatchDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

}
