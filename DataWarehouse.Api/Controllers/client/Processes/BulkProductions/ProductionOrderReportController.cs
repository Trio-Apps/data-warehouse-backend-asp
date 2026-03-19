using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.DTOs.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.client.Processes.BulkProductions;

[Route("api/client/production-orders")]
[ApiController]
[Authorize]
public class ProductionOrderReportController : ControllerBase
{
    private readonly IProductionOrderRepository _productionOrderRepository;

    public ProductionOrderReportController(IProductionOrderRepository productionOrderRepository)
    {
        _productionOrderRepository = productionOrderRepository;
    }

    [HttpGet("reports")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Reports_Get}")]
    [ProducesResponseType(typeof(GeneralResponse<PagedResult<ProductionOrderReportItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GeneralResponse<PagedResult<ProductionOrderReportItemDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductionOrdersReport([FromQuery] ProductionOrderReportFilterDto filter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var report = await _productionOrderRepository.GetProductionOrdersReportAsync(userId, filter);

        if (!report.Success)
            return BadRequest(report);

        return Ok(report);
    }
}
