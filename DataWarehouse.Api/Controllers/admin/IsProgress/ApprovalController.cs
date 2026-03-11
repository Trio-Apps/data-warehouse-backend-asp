using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.IsProgress
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprovalController : ControllerBase
    {
        private readonly ISapJobQueuer jobQueuer;
        private readonly IApprovalRepository repository;

        public ApprovalController(ISapJobQueuer jobQueuer, IApprovalRepository repository)
        {
            this.jobQueuer = jobQueuer;
            this.repository = repository;
        }

        // get process to user
        [HttpGet("my-approval-process/{skip}/{pageSize}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_GetMy}")]
        public async Task<IActionResult> GetMyAppovalProcess(int skip, int pageSize)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var res = await repository.GetPendingApprovalsForUserAsync(userId, skip, pageSize);

            return Ok(res);
        }

        [HttpPatch("make-order-is-approval/{approval}/order-process-approval-id/{processApproval}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Approvals_Action}")]
        public async Task<IActionResult> MakeTheOrderIsApproval(bool approval, int processApproval, string? comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            GeneralResponse<ProcessApprovalDto> res;

            if (approval)
            {
                res = await repository.ApproveStepAsync(processApproval, userId, comment);

                await jobQueuer.DistributionOrders(res.Data.ProcessItemIsProgress);
            }
            else
                res = await repository.RejectStepAsync(processApproval, userId, comment);


            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }
    }

}
