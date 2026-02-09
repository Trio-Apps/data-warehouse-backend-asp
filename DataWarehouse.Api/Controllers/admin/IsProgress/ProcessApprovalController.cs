using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.IsProgress;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProcessApprovalController : ControllerBase
{
    private readonly IProcessApprovalRepository processApprovalService;
    private readonly ILogger<ProcessApprovalController> _logger;

    public ProcessApprovalController(
        IProcessApprovalRepository processApprovalService,
        ILogger<ProcessApprovalController> logger)
    {
        this.processApprovalService = processApprovalService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetAll()
    {
        var processApprovals = await processApprovalService.GetAllAsync();
        return Ok(processApprovals);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<ProcessApproval>> GetById(int id)
    {
        var processApproval = await processApprovalService.GetByIdAsync(id);
        if (processApproval == null)
            return NotFound($"ProcessApproval with ID {id} not found.");

        return Ok(processApproval);
    }

    [HttpGet("process-item/{processItemIsProgressId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetByProcessItemIsProgressId(int processItemIsProgressId)
    {
        var processApprovals = await processApprovalService.GetByProcessItemIsProgressIdAsync(processItemIsProgressId);
        return Ok(processApprovals);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetByUserId(string userId)
    {
        var processApprovals = await processApprovalService.GetByUserIdAsync(userId);
        return Ok(processApprovals);
    }

    [HttpGet("approval-step/{approvalStepId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetByApprovalStepId(int approvalStepId)
    {
        var processApprovals = await processApprovalService.GetByApprovalStepIdAsync(approvalStepId);
        return Ok(processApprovals);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetByStatus(ApprovalStatus status)
    {
        var processApprovals = await processApprovalService.GetByStatusAsync(status);
        return Ok(processApprovals);
    }

    [HttpGet("pending")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<IEnumerable<ProcessApproval>>> GetPendingApprovals()
    {
        var processApprovals = await processApprovalService.GetPendingApprovalsAsync();
        return Ok(processApprovals);
    }

    [HttpGet("{id}/with-user")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<ProcessApproval>> GetWithUser(int id)
    {
        var processApproval = await processApprovalService.GetWithUserAsync(id);
        if (processApproval == null)
            return NotFound($"ProcessApproval with ID {id} not found.");

        return Ok(processApproval);
    }

    [HttpGet("{id}/with-approval-step")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Get}")]
    public async Task<ActionResult<ProcessApproval>> GetWithApprovalStep(int id)
    {
        var processApproval = await processApprovalService.GetWithApprovalStepAsync(id);
        if (processApproval == null)
            return NotFound($"ProcessApproval with ID {id} not found.");

        return Ok(processApproval);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Create}")]
    public async Task<ActionResult<ProcessApproval>> Create([FromBody] ProcessApproval processApproval)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await processApprovalService.AddAsync(processApproval);
        return CreatedAtAction(nameof(GetById), new { id = created.ProcessApprovalId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProcessApproval processApproval)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != processApproval.ProcessApprovalId)
            return BadRequest("ID mismatch.");

        var existing = await processApprovalService.GetByIdAsync(id);
        if (existing == null)
            return NotFound($"ProcessApproval with ID {id} not found.");

        await processApprovalService.UpdateAsync(processApproval);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ProcessApprovals_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var processApproval = await processApprovalService.GetByIdAsync(id);
        if (processApproval == null)
            return NotFound($"ProcessApproval with ID {id} not found.");

        await processApprovalService.DeleteAsync(id);
        return NoContent();
    }
}
