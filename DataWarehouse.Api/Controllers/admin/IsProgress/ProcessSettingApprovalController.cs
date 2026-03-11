using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Services.Repository.Permissions;
using global::DataWarehouse.Core.Interfaces.IsProgress;
using global::DataWarehouse.Domain.Entities.IsProgress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;


namespace DataWarehouse.Api.Controllers.admin.IsProgress
{


    [Route("api/[controller]")]
    [ApiController]
    public class ProcessSettingApprovalController : ControllerBase
    {
        private readonly IProcessSettingApprovalRepository _repository;

        public ProcessSettingApprovalController(IProcessSettingApprovalRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get all process settings for current company
        /// </summary>
        [HttpGet]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
        public async Task<ActionResult<IReadOnlyList<ProcessSettingApproval>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _repository.GetProcessSettingsAsync(cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get process setting by id
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
        public async Task<ActionResult<ProcessSettingApproval>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(id, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        [HttpPatch("{id:int}/toggle-ignore")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.ApprovalSteps_Get}")]
        public async Task<IActionResult> ToggleIgnoreSteps(int id, CancellationToken cancellationToken)
        {
            var updated = await _repository.ToggleIgnoreStepsAsync(id, cancellationToken);

            if (!updated)
                return NotFound("Process setting not found.");

            return Ok();
        }
    }

    }
