using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Core.DTOs.Processes;

public class ProcessesTypesDateDTO
{
    public int? ProcessesTypesDateId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateOnly PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateOnly DueDate { get; set; }

    [Required(ErrorMessage = "Processes Type ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Processes Type ID must be greater than 0")]
    public int ProcessesTypeId { get; set; }
}

public class UpdateProcessesTypesDateDTO
{
    public int ProcessesTypesDateId { get; set; }

    [Required(ErrorMessage = "Posting Date is required")]
    public DateOnly PostingDate { get; set; }

    [Required(ErrorMessage = "Due Date is required")]
    public DateOnly DueDate { get; set; }

   
}


public class ProcessesTypesDateResponseDTO
{
    public int ProcessesTypesDateId { get; set; }
    public DateOnly PostingDate { get; set; }
    public DateOnly DueDate { get; set; }
    public int ProcessesTypeId { get; set; }
    public string? ProcessesTypeName { get; set; }
}

