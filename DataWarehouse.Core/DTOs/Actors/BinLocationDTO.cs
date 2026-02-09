using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class BinLocationDTO
{
    public int? BinLocationId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Location must be between 1 and 100 characters")]
    public string Location { get; set; }

    [Required(ErrorMessage = "Item ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
    public int ItemId { get; set; }
}

public class BinLocationResponseDTO
{
    public int BinLocationId { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
}
