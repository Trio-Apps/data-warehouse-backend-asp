using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReasonsController : ControllerBase
{
    private readonly IReasonRepository _reasonRepository;

    public ReasonsController(IReasonRepository reasonRepository)
    {
        _reasonRepository = reasonRepository;
    }


    [HttpGet("process-types")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Reason_Get}")]

    public IActionResult GetAllProcessTypes()
    {
        var processTypes = Enum
            .GetValues<ProcessType>()
            .Select(x => new
            {
                Id = (int)x,
                Name = x.ToString()
            });

        return Ok(processTypes);
    }

    [HttpGet("by-process-type/{processType}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Reason_Get}")]

    public async Task<IActionResult> GetByProcessType(string processType)
    {
        if (!Enum.TryParse<ProcessType>(processType, true, out var processTypeEnum))
            return BadRequest($"Invalid process type: {processType}");


        var result = await _reasonRepository.GetActiveByProcessTypeAsync(processTypeEnum);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Receipt_Create}")]

    public async Task<IActionResult> Add([FromBody] AddReasonDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await _reasonRepository.AddReasonAsync(dto);
       
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{reasonId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Reason_Edit}")]
    public async Task<IActionResult> Update(int reasonId, [FromBody] UpdateReasonDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reasonRepository.UpdateReasonAsync(reasonId, dto);
        if (!result.Success)
        {
            if (result.Message == "Reason not found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{reasonId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Reason_Delete}")]
    public async Task<IActionResult> Delete(int reasonId)
    {
        var result = await _reasonRepository.DeleteReasonAsync(reasonId);
        if (!result.Success)
        {
            if (result.Message == "Reason not found.")
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
