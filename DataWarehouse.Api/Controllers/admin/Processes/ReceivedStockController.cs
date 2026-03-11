using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReceivedStockController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly IReceivedStockRepository repository;
    private readonly ILogger<ReceivedStockController> logger;

    public ReceivedStockController(
        ISapJobQueuer jobQueuer,
        IReceivedStockRepository repository,
        ILogger<ReceivedStockController> logger)
    {
        this.jobQueuer = jobQueuer;
        this.repository = repository;
        this.logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetAll()
    {
        var receivedStocks = await repository.GetAllAsync();
        return Ok(receivedStocks);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetByWarehouseId(int warehouseId)
    {
        var receivedStocks = await repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(receivedStocks);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByWarehouseIdForDashboard(
        int warehouseId,
        int? sourceWarehouseId,
        string? status,
        DateTime? postingDate,
        DateTime? dueDate,
        int skip,
        int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(
            warehouseId,
            userId!,
            sourceWarehouseId,
            postingDate,
            dueDate,
            status,
            skip,
            pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetReceivedStockById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.GetReceivedStockByIdAsync(userId!, id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("transferred-stock/{transferredStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetByTransferredStockId(int transferredStockId)
    {
        var receivedStock = await repository.GetByTransferredStockIdAsync(transferredStockId);
        if (!receivedStock.Success)
            return NotFound($"ReceivedStock for TransferredStock ID {transferredStockId} not found.");

        return Ok(receivedStock);
    }

    // not used - same style as DeliveryNote
    [HttpGet("by-received-stock/{receivedStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetWithSourceWarehouse(int receivedStockId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.GetReceivedStockByIdAsync(userId!, receivedStockId);
        if (!res.Success)
            return NotFound($"ReceivedStock ID {receivedStockId} not found.");

        return Ok(res);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<IEnumerable<ReceivedStock>>> GetByUserId(string userId)
    {
        var receivedStocks = await repository.GetByUserIdAsync(userId);
        return Ok(receivedStocks);
    }

    [HttpGet("{id}/with-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetWithItems(int id)
    {
        var receivedStock = await repository.GetWithItemsAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpGet("{id}/with-items-batches")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<IActionResult> GetWithItemsAndBatches(int id)
    {
        var res = await repository.GetWithItemsAndBatchesAsync(id);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }

    [HttpGet("{id}/with-transferred-stock")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetWithTransferredStock(int id)
    {
        var receivedStock = await repository.GetWithTransferredStockAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpGet("{id}/with-warehouse")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Get}")]
    public async Task<ActionResult<ReceivedStock>> GetWithWarehouse(int id)
    {
        var receivedStock = await repository.GetWithWarehouseAsync(id);
        if (receivedStock == null)
            return NotFound($"ReceivedStock with ID {id} not found.");

        return Ok(receivedStock);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedStock>> Create(AddReceivedStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await repository.AddReceivedStockByTransferredStockIdAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("with-default-items")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedStock>> CreateWithDefaultItems(AddReceivedStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await repository.AddReceivedStockAndItemsByTransferredStockIdAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPost("without-reference")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Create}")]
    public async Task<ActionResult<ReceivedStock>> CreateWithoutReference(AddReceivedStockWithoutRefDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var created = await repository.AddReceivedStockWitoutRefAsync(userId!, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReceivedStockDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var res = await repository.UpdateReceivedStockAsync(userId!, id, dto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPatch("{id}/revert-partially-failed")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Edit}")]
    public async Task<IActionResult> RevertPartiallyFailedStatus(int id)
    {
        var res = await repository.RevertPartiallyFailedStatusToProcessingAsync(id);

        await jobQueuer.DistributionOrders(res.Data);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Received_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await repository.DeleteReceivedStockAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
