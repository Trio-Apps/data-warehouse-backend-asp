using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class SupplierItemDTO
{
    public int? SupplierItemId { get; set; }

    [Required(ErrorMessage = "Supplier ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Supplier ID must be greater than 0")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Purchase Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Purchase Price must be greater than or equal to 0")]
    public decimal PurchasePrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Lead Time Days must be greater than or equal to 0")]
    public int LeadTimeDays { get; set; }

    public bool IsPreferred { get; set; } = false;
}

public class SupplierItemResponseDTO
{
    public int SupplierItemId { get; set; }
    public int SupplierId { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal PurchasePrice { get; set; }
    public int LeadTimeDays { get; set; }
    public bool IsPreferred { get; set; }
}
