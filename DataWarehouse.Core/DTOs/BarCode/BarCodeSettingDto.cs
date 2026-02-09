using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.BarCode
{
    public class BarCodeSettingDto
    {
        public int BarCodeSettingId { get; set; }
        public int TotalLength { get; set; }
        public string StartsWith { get; set; }
        public int SapStartPosition { get; set; }
        public int SapLength { get; set; }
        public int QuantityStartPosition { get; set; }
        public int QuantityLength { get; set; }
        public bool IgnoreLastDigit { get; set; }
        public string DefaultUom { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class AddBarCodeSettingDto
    {

        [Required(ErrorMessage = "Total barcode length is required")]
        [Range(1, 50, ErrorMessage = "Total length must be between 1 and 50 digits")]
        [Display(Name = "Total Barcode Length")]
        public int TotalLength { get; set; }

        [Required(ErrorMessage = "Barcode start characters are required")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Start characters must be between 1 and 10 characters")]
        [Display(Name = "Barcode Starts With")]
        public string StartsWith { get; set; } = string.Empty;

        [Required(ErrorMessage = "SAP start position is required")]
        [Range(0, 49, ErrorMessage = "SAP start position must be between 0 and 49")]
        [Display(Name = "SAP Start Position (0-based)")]
        public int SapStartPosition { get; set; }

        [Required(ErrorMessage = "SAP length is required")]
        [Range(1, 20, ErrorMessage = "SAP length must be between 1 and 20 digits")]
        [Display(Name = "SAP Length")]
        public int SapLength { get; set; }

        [Required(ErrorMessage = "Quantity start position is required")]
        [Range(0, 49, ErrorMessage = "Quantity start position must be between 0 and 49")]
        [Display(Name = "Quantity Start Position (0-based)")]
        public int QuantityStartPosition { get; set; }

        [Required(ErrorMessage = "Quantity length is required")]
        [Range(1, 20, ErrorMessage = "Quantity length must be between 1 and 20 digits")]
        [Display(Name = "Quantity Length")]
        public int QuantityLength { get; set; }

        [Display(Name = "Ignore Last Digit (Check Digit)")]
        public bool IgnoreLastDigit { get; set; }

        [Required(ErrorMessage = "Default unit of measure is required")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Unit of measure must be between 1 and 10 characters")]
        //[RegularExpression(@"^(KG|PCS|L|M|CM|MM|G|ML|BAG|BOX|PAIR)$",
        //    ErrorMessage = "Invalid unit of measure. Valid values: KG, PCS, L, M, CM, MM, G, ML, BAG, BOX, PAIR")]
        [Display(Name = "Default Unit of Measure")]
        public string DefaultUom { get; set; } = "KG";

    }

    public class UpdateBarCodeSettingDto
    {
        public int BarCodeSettingId { get; set; }

        public int? TotalLength { get; set; }
        public string? StartsWith { get; set; }
        public int? SapStartPosition { get; set; }
        public int? SapLength { get; set; }
        public int? QuantityStartPosition { get; set; }
        public int? QuantityLength { get; set; }
        public bool? IgnoreLastDigit { get; set; }
        public string? DefaultUom { get; set; }

    }
}
