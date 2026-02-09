using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.IsProgress;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProcessItemIsProgressController : ControllerBase
{
    private readonly IProcessItemIsProgressRepository processItemIsProgressService;
    private readonly ILogger<ProcessItemIsProgressController> _logger;

    public ProcessItemIsProgressController(
        IProcessItemIsProgressRepository processItemIsProgressService,
        ILogger<ProcessItemIsProgressController> logger)
    {
        this.processItemIsProgressService = processItemIsProgressService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessItemIsProgress>>> GetAll()
    {
        var processItemIsProgresses = await processItemIsProgressService.GetAllAsync();
        return Ok(processItemIsProgresses);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<ProcessItemIsProgress>> GetById(int id)
    {
        var processItemIsProgress = await processItemIsProgressService.GetByIdAsync(id);
        if (processItemIsProgress == null)
            return NotFound($"ProcessItemIsProgress with ID {id} not found.");

        return Ok(processItemIsProgress);
    }

    [HttpGet("process-type/{processType}/process-id/{processId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<ProcessItemIsProgress>> GetByProcessTypeAndId(ProcessType processType, int processId)
    {
        var processItemIsProgress = await processItemIsProgressService.GetByProcessTypeAndIdAsync(processType, processId);
        if (processItemIsProgress == null)
            return NotFound($"ProcessItemIsProgress with ProcessType {processType} and ProcessId {processId} not found.");

        return Ok(processItemIsProgress);
    }

    [HttpGet("process-type/{processType}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessItemIsProgress>>> GetByProcessType(ProcessType processType)
    {
        var processItemIsProgresses = await processItemIsProgressService.GetByProcessTypeAsync(processType);
        return Ok(processItemIsProgresses);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessItemIsProgress>>> GetByStatus(ProcessStatus status)
    {
        var processItemIsProgresses = await processItemIsProgressService.GetByStatusAsync(status);
        return Ok(processItemIsProgresses);
    }

    [HttpGet("pending")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessItemIsProgress>>> GetPendingProcesses()
    {
        var processItemIsProgresses = await processItemIsProgressService.GetPendingProcessesAsync();
        return Ok(processItemIsProgresses);
    }

    [HttpGet("{id}/with-approvals")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Get}")]
    public async Task<ActionResult<ProcessItemIsProgress>> GetWithApprovals(int id)
    {
        var processItemIsProgress = await processItemIsProgressService.GetWithApprovalsAsync(id);
        if (processItemIsProgress == null)
            return NotFound($"ProcessItemIsProgress with ID {id} not found.");

        return Ok(processItemIsProgress);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Create}")]
    public async Task<ActionResult<ProcessItemIsProgress>> Create([FromBody] ProcessItemIsProgress processItemIsProgress)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await processItemIsProgressService.AddAsync(processItemIsProgress);
        return CreatedAtAction(nameof(GetById), new { id = created.ProcessItemIsProgressId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProcessItemIsProgress processItemIsProgress)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != processItemIsProgress.ProcessItemIsProgressId)
            return BadRequest("ID mismatch.");

        var existing = await processItemIsProgressService.GetByIdAsync(id);
        if (existing == null)
            return NotFound($"ProcessItemIsProgress with ID {id} not found.");

        await processItemIsProgressService.UpdateAsync(processItemIsProgress);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessProgresses_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var processItemIsProgress = await processItemIsProgressService.GetByIdAsync(id);
        if (processItemIsProgress == null)
            return NotFound($"ProcessItemIsProgress with ID {id} not found.");

        await processItemIsProgressService.DeleteAsync(id);
        return NoContent();
    }
}
