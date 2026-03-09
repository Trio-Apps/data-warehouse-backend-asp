using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Sync;
using DataWarehouse.Core.Interfaces.Sync;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Sync;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SapSyncResetController : ControllerBase
{
    private readonly ISapSyncResetRepository _sapSyncResetRepository;
    private readonly ILogger<SapSyncResetController> _logger;

    public SapSyncResetController(
        ISapSyncResetRepository sapSyncResetRepository,
        ILogger<SapSyncResetController> logger)
    {
        _sapSyncResetRepository = sapSyncResetRepository;
        _logger = logger;
    }



    [HttpPatch("item/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetItemSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetItemSyncAsync, sapId, "item");



    [HttpPatch("warehouse/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetWarehouseSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetWarehouseSyncAsync, sapId, "warehouse");


    [HttpPatch("purchase/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetPurchaseSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetPurchaseSyncAsync, sapId, "purchase");



    [HttpPatch("count/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetCountSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetCountSyncAsync, sapId, "count");



    [HttpPatch("business-partners/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetBusinessPartnersSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetBusinessPartnersSyncAsync, sapId, "businessPartners");


    [HttpPatch("item-uom-group/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetItemUomGroupSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetItemUomGroupSyncAsync, sapId, "itemUomGroup");



    [HttpPatch("sales/{sapId:int}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Edit}")]
    public Task<IActionResult> ResetSalesSync(int sapId) =>
        ExecuteResetAsync(_sapSyncResetRepository.ResetSalesSyncAsync, sapId, "sales");

     

    [HttpGet("all-sync")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Saps_Get}")]
    public Task<IActionResult> GetCompanySyncState(int companyId) =>
        ExecuteGetCompanySyncStateAsync();



    private async Task<IActionResult> ExecuteResetAsync(
        Func<int, Task<GeneralResponse<bool>>> resetAction,
        int sapId,
        string entityName)
    {
        if (sapId <= 0)
            return BadRequest(GeneralResponse<bool>.FailResponse("Invalid sapId."));

        try
        {
            var result = await resetAction(sapId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while resetting sync for entity {EntityName} and sapId {SapId}", entityName, sapId);
            return StatusCode(500, GeneralResponse<bool>.FailResponse("An error occurred while resetting sync."));
        }
    }


    private async Task<IActionResult> ExecuteGetCompanySyncStateAsync()
    {
       
        try
        {
            var result = await _sapSyncResetRepository.GetCompanySyncStateAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
         //   _logger.LogError(ex, "Error while getting sync state for companyId {Comp);
            return StatusCode(500, GeneralResponse<List<SapSyncStateDto>>.FailResponse("An error occurred while getting sync state."));
        }
    }
}
