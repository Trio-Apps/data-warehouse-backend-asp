using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.IsProgress;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApprovalStepController : ControllerBase
{
    private readonly DataWarehouseDbContext context;
    private readonly IApprovalStepRepository approvalStepService;
    private readonly ILogger<ApprovalStepController> _logger;

    public ApprovalStepController(
        DataWarehouseDbContext context,
        IApprovalStepRepository approvalStepService,
        ILogger<ApprovalStepController> logger)
    {
        this.context = context;
        this.approvalStepService = approvalStepService;
        _logger = logger;
    }

    [HttpGet("{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<IEnumerable<ApprovalStep>>> GetAll(int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var approvalSteps = await approvalStepService.GetApprovalStepsAsync(userId, skip, pageSize);
        return Ok(approvalSteps);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<ApprovalStep>> GetById(int id)
    {
        var approvalStep = await approvalStepService.GetByIdAsync(id);
        if (approvalStep == null)
            return NotFound($"ApprovalStep with ID {id} not found.");

        return Ok(approvalStep);
    }

    [HttpGet("role/{roleId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<IEnumerable<ApprovalStep>>> GetByRoleId(string roleId)
    {
        var approvalSteps = await approvalStepService.GetByRoleIdAsync(roleId);
        return Ok(approvalSteps);
    }

    [HttpGet("ordered")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<IEnumerable<ApprovalStep>>> GetOrderedSteps()
    {
        var approvalSteps = await approvalStepService.GetOrderedStepsAsync();
        return Ok(approvalSteps);
    }

    [HttpGet("step-order/{stepOrder}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<ApprovalStep>> GetByStepOrder(int stepOrder)
    {
        var approvalStep = await approvalStepService.GetByStepOrderAsync(stepOrder);
        if (approvalStep == null)
            return NotFound($"ApprovalStep with step order {stepOrder} not found.");

        return Ok(approvalStep);
    }

    [HttpGet("{id}/with-approvals")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
    public async Task<ActionResult<ApprovalStep>> GetWithApprovals(int id)
    {
        var approvalStep = await approvalStepService.GetWithApprovalsAsync(id);
        if (approvalStep == null)
            return NotFound($"ApprovalStep with ID {id} not found.");

        return Ok(approvalStep);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Create}")]
    public async Task<ActionResult<ApprovalStep>> Create([FromBody] AddApprovalStepDto approvalStep)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var res = await approvalStepService.AddApprovalStepAsync(userId, approvalStep);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalStep approvalStep)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != approvalStep.ApprovalStepId)
            return BadRequest("ID mismatch.");

        var existing = await approvalStepService.GetByIdAsync(id);
        if (existing == null)
            return NotFound($"ApprovalStep with ID {id} not found.");

        await approvalStepService.UpdateAsync(approvalStep);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var approvalStep = await approvalStepService.GetByIdAsync(id);
        if (approvalStep == null)
            return NotFound($"ApprovalStep with ID {id} not found.");

        await approvalStepService.DeleteAsync(id);
        return NoContent();
    }
}
