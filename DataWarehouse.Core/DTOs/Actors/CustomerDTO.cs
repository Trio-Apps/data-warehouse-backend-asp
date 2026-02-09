using DataWarehouse.Core.DTOs.Processes.OutSide;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Actors;

public class CustomerDTO
{
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Customer Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Customer Name must be between 2 and 200 characters")]
    public string CustomerName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SalesOrderDTO>? SalesOrders { get; set; }
}

public class CustomerResponseDTO
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SalesOrdersCount { get; set; }
}
