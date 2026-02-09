using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SupplierItemController : ControllerBase
{
    private readonly ISupplierItemService _supplierItemService;
    private readonly ILogger<SupplierItemController> _logger;

    public SupplierItemController(ISupplierItemService supplierItemService, ILogger<SupplierItemController> logger)
    {
        _supplierItemService = supplierItemService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierItem>>> GetAll()
    {
        var supplierItems = await _supplierItemService.GetAllAsync();
        return Ok(supplierItems);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierItem>> GetById(int id)
    {
        var supplierItem = await _supplierItemService.GetByIdAsync(id);
        if (supplierItem == null)
            return NotFound($"SupplierItem with ID {id} not found.");

        return Ok(supplierItem);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<ActionResult<IEnumerable<SupplierItem>>> GetBySupplierId(int supplierId)
    {
        var supplierItems = await _supplierItemService.GetBySupplierIdAsync(supplierId);
        return Ok(supplierItems);
    }

    [HttpGet("item/{itemId}")]
    public async Task<ActionResult<IEnumerable<SupplierItem>>> GetByItemId(int itemId)
    {
        var supplierItems = await _supplierItemService.GetByItemIdAsync(itemId);
        return Ok(supplierItems);
    }

    [HttpGet("supplier/{supplierId}/item/{itemId}")]
    public async Task<ActionResult<SupplierItem>> GetBySupplierAndItem(int supplierId, int itemId)
    {
        var supplierItem = await _supplierItemService.GetBySupplierAndItemAsync(supplierId, itemId);
        if (supplierItem == null)
            return NotFound($"SupplierItem with Supplier ID {supplierId} and Item ID {itemId} not found.");

        return Ok(supplierItem);
    }

    [HttpGet("item/{itemId}/preferred")]
    public async Task<ActionResult<IEnumerable<SupplierItem>>> GetPreferredSuppliersByItemId(int itemId)
    {
        var supplierItems = await _supplierItemService.GetPreferredSuppliersByItemIdAsync(itemId);
        return Ok(supplierItems);
    }

    [HttpGet("{id}/with-supplier")]
    public async Task<ActionResult<SupplierItem>> GetWithSupplier(int id)
    {
        var supplierItem = await _supplierItemService.GetWithSupplierAsync(id);
        if (supplierItem == null)
            return NotFound($"SupplierItem with ID {id} not found.");

        return Ok(supplierItem);
    }

    [HttpGet("{id}/with-item")]
    public async Task<ActionResult<SupplierItem>> GetWithItem(int id)
    {
        var supplierItem = await _supplierItemService.GetWithItemAsync(id);
        if (supplierItem == null)
            return NotFound($"SupplierItem with ID {id} not found.");

        return Ok(supplierItem);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierItem>> Create([FromBody] SupplierItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _supplierItemService.ExistsBySupplierAndItemAsync(dto.SupplierId, dto.ItemId))
            return Conflict($"SupplierItem with Supplier ID {dto.SupplierId} and Item ID {dto.ItemId} already exists.");

        var supplierItem = new SupplierItem
        {
            SupplierId = dto.SupplierId,
            ItemId = dto.ItemId,
            PurchasePrice = dto.PurchasePrice,
            LeadTimeDays = dto.LeadTimeDays,
            IsPreferred = dto.IsPreferred
        };

        var created = await _supplierItemService.AddAsync(supplierItem);
        return CreatedAtAction(nameof(GetById), new { id = created.SupplierItemId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var supplierItem = await _supplierItemService.GetByIdAsync(id);
        if (supplierItem == null)
            return NotFound($"SupplierItem with ID {id} not found.");

        supplierItem.SupplierId = dto.SupplierId;
        supplierItem.ItemId = dto.ItemId;
        supplierItem.PurchasePrice = dto.PurchasePrice;
        supplierItem.LeadTimeDays = dto.LeadTimeDays;
        supplierItem.IsPreferred = dto.IsPreferred;

        await _supplierItemService.UpdateAsync(supplierItem);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var supplierItem = await _supplierItemService.GetByIdAsync(id);
        if (supplierItem == null)
            return NotFound($"SupplierItem with ID {id} not found.");

        await _supplierItemService.DeleteAsync(id);
        return NoContent();
    }
}

