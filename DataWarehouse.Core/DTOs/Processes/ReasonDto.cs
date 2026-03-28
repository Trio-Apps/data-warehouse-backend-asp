using DataWarehouse.Domain.Enums.Approval;

namespace DataWarehouse.Core.DTOs.Processes;

public class ReasonDto
{
    public int ReasonId { get; set; }
    public string Name { get; set; } = null!;
    public ProcessType ProcessType { get; set; }
}

public class AddReasonDto
{
    public string Name { get; set; } = null!;
    public ProcessType ProcessType { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateReasonDto
{
    public string Name { get; set; } = null!;
    public ProcessType ProcessType { get; set; }
    public bool IsActive { get; set; } = true;
}
