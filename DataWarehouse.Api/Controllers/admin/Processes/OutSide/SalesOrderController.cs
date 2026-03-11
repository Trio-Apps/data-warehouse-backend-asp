using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SalesOrderController : ControllerBase
{
    private readonly ISapJobQueuer jobQueuer;
    private readonly ISalesOrderRepository _repository;
    private readonly ILogger<SalesOrderController> _logger;

    public SalesOrderController(
        ISapJobQueuer jobQueuer,
        ISalesOrderRepository repository,
        ILogger<SalesOrderController> logger)
    {
        this.jobQueuer = jobQueuer;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("search-pagination/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<IActionResult> GetByWarehouseId(
int warehouseId,
[FromQuery] string? itemCodeOrItemName,
[FromQuery] int pageNumber = 1,
[FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetByWarehouseIdAsync(
            warehouseId,
            itemCodeOrItemName,
            pageNumber,
            pageSize);

        return Ok(result);
    }

  
    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetAll()
    {
        var salesOrders = await _repository.GetAllAsync();
        return Ok(salesOrders);
    }

    [HttpGet("item-for-sales/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<IActionResult> GetItemForSalesByWarehouseId(int warehouseId)
    {
        var finishedGoodItems = await _repository.GetItemForSalesByWarehouseIdAsync(warehouseId);
        return Ok(finishedGoodItems);
    }
    [HttpGet("warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetByWarehouseId(int warehouseId)
    {
        var salesOrders = await _repository.GetByWarehouseIdAsync(warehouseId);
        return Ok(salesOrders);
    }


    [HttpGet("items-in-warehouse/warehouse/{warehouseId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetItemsByWarehouseId(int warehouseId)
    {
        var salesOrders = await _repository.GetItemsByWarehouseIdAsync(warehouseId);
        return Ok(salesOrders);
    }

    [HttpGet("warehouse/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int skip, int pageSize)
    {
        var res = await _repository.GetByWarehouseIdWithPaginationAsync(warehouseId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
    
    [HttpGet("with-customer/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<IActionResult> GetByIdWithCustomer(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var res = await _repository.GetWithCustomerAsync(id,userId);
        if (!res.Success)
            return NotFound(res);

        return Ok(res);
    }



    //[HttpGet("warehouse/status/posting-date/due-date/{skip}/{pageSize}")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    //public async Task<IActionResult> GetByWarehouseIdWithPagination(int? warehouseId, string? status, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationAsync(warehouseId, userId, postingDate, dueDate, status, skip, pageSize);
    //    if (!res.Success)
    //        return BadRequest(res);

    //    return Ok(res);
    //}

    [HttpGet("dashboard/warehouse/status/posting-date/due-date/{warehouseId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<IActionResult> GetByWarehouseIdWithPagination(int warehouseId, int? customerId, string? status, string? liveStatus, DateTime? postingDate, DateTime? dueDate, int skip, int pageSize)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var res = await _repository.GetByWarehouseIdAndStatusAndDateWithPaginationForDashboardAsync(warehouseId, userId, customerId, postingDate, dueDate, liveStatus, status, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }



    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]
    public async Task<ActionResult<SalesOrder>> GetById(int id)
    {
        var salesOrder = await _repository.GetByIdAsync(id);
        if (salesOrder == null)
            return NotFound($"SalesOrder with ID {id} not found.");

        return Ok(salesOrder);
    }


    [HttpGet("status")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<IActionResult> Status()
    {
        var salesOrder = await _repository.GetSalesOrderStatus();

        if (!salesOrder.Success)
            return NotFound(salesOrder);

        return Ok(salesOrder);
    }

    [HttpGet("status/{status}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    public async Task<IActionResult> Status(string status)
    {
        var salesOrder = await _repository.GetByStatusAsync(status);

        if (!salesOrder.Success)
            return NotFound(salesOrder);

        return Ok(salesOrder);
    }

    [HttpPost]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Create}")]

    public async Task<ActionResult<SalesOrder>> Create(AddSalesOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        var created = await _repository.AddSalesOrderByWarehouseIdAsync(userId, dto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Edit}")]

    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesOrderDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User ID not found in token.");

        dto.SalesOrderId = id;

        var res = await _repository.UpdateSalesOrderAsync(userId, id, dto);
        if (!res.Success) return BadRequest(res);

        return Ok(res);
    }

    [HttpPatch("{id}/revert-partially-failed")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Edit}")]
    public async Task<IActionResult> RevertPartiallyFailedStatus(int id)
    {
        var res = await _repository.RevertPartiallyFailedStatusToProcessingAsync(id);

        await jobQueuer.DistributionOrders(res.Data);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Delete}")]

    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteSalesOrderAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    //[HttpGet("processing")]
    //[Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Sales_Get}")]

    //public async Task<ActionResult<IEnumerable<SalesOrder>>> GetPendingOrders()
    //{
    //    var salesOrders = await _repository.GetPendingOrdersAsync();
    //    return Ok(salesOrders);
    //}


}

