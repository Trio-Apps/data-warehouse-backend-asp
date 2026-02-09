using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class ItemDTO
{
    public int? ItemId { get; set; }

    [Required(ErrorMessage = "Item Code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Item Code must be between 1 and 50 characters")]
    public string ItemCode { get; set; }

    [Required(ErrorMessage = "Item Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Item Name must be between 2 and 200 characters")]
    public string ItemName { get; set; }

    [StringLength(100, ErrorMessage = "Item Group cannot exceed 100 characters")]
    public string? ItemGroup { get; set; }

    [Required(ErrorMessage = "Unit of Measure is required")]
    [StringLength(20, ErrorMessage = "UoM cannot exceed 20 characters")]
    public string UoM { get; set; }

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Sales Price must be greater than or equal to 0")]
    public decimal SalesPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Purchase Price must be greater than or equal to 0")]
    public decimal PurchasePrice { get; set; }
}

public class ItemProcessDTO
{
    public int? ItemId { get; set; }

    [Required(ErrorMessage = "Item Code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Item Code must be between 1 and 50 characters")]
    public string ItemCode { get; set; }

    [Required(ErrorMessage = "Item Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Item Name must be between 2 and 200 characters")]
    public string ItemName { get; set; }

  

    [Required(ErrorMessage = "Warehouse ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Warehouse ID must be greater than 0")]
    public int WarehouseId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Sales Price must be greater than or equal to 0")]
    public decimal SalesPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Purchase Price must be greater than or equal to 0")]
    public decimal PurchasePrice { get; set; }


}

public class ItemResponseDTO
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public string? ItemGroup { get; set; }
    public string UoM { get; set; }
    public double? InStock {  get; set; }
    public string WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime? UpdateDate { get; set; }
}
