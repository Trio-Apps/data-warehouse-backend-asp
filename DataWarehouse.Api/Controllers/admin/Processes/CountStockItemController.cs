using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CountStockItemController : ControllerBase
{
    private readonly ICountStockItemRepository _repository;
    private readonly ILogger<CountStockItemController> _logger;

    public CountStockItemController(
        ICountStockItemRepository repository,
        ILogger<CountStockItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockItem>>> GetAll()
    {
        var countStockItems = await _repository.GetAllAsync();
        return Ok(countStockItems);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<CountStockItem>> GetById(int id)
    {
        var countStockItem = await _repository.GetByIdAsync(id);
        if (countStockItem == null)
            return NotFound($"CountStockItem with ID {id} not found.");

        return Ok(countStockItem);
    }

    [HttpGet("count-stock/{CountStockId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockItemDTO>>> GetByCountStockId(int CountStockId)
    {
        var countStockItems = await _repository.GetByCountStockItemByCountStockIdAsync(CountStockId);
        return Ok(countStockItems);
    }

    [HttpGet("status/count-stock/{CountStockId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Get}")]
    public async Task<ActionResult<IEnumerable<CountStockItemDTO>>>
        GetByCountStockIdWithPagination(int CountStockId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetByCountStockItemByCountStockIdWithPaginationAsync(
            CountStockId, status, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("Count-stock/{CountStockId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Create}")]
    public async Task<ActionResult<CountStockItem>> Create(
        int CountStockId,
        bool isBarcode,
        AddCountStockItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Case 1: Barcode = true
        if (isBarcode)
        {
            if (dto.Barcode == null)
                return BadRequest(" barcode should not be sent with static type");
        }

        var created = await _repository.AddCountStockItemByCountStockIdAsync(
            CountStockId,
            isBarcode,
            dto.Barcode,
            dto.Item);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("Count-stock-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateCountStockItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.CountStockItemId = id;
        var res = await _repository.UpdateCountStockItemAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("Count-stock-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Counting_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var countStockItem = await _repository.GetByIdAsync(id);
        if (countStockItem == null)
            return NotFound($"CountStockItem with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Ok("delete");
    }
}
