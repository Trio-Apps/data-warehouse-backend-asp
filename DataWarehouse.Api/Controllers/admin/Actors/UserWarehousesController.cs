using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserWarehousesController : ControllerBase
{
    private readonly IUserWarehousesService _userWarehousesService;
    private readonly ILogger<UserWarehousesController> _logger;

    public UserWarehousesController(IUserWarehousesService userWarehousesService, ILogger<UserWarehousesController> logger)
    {
        _userWarehousesService = userWarehousesService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWarehouses>>> GetAll()
    {
        var userWarehouses = await _userWarehousesService.GetAllAsync();
        return Ok(userWarehouses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserWarehouses>> GetById(int id)
    {
        var userWarehouse = await _userWarehousesService.GetByIdAsync(id);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with ID {id} not found.");

        return Ok(userWarehouse);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserWarehouses>>> GetByUserId(string userId)
    {
        var userWarehouses = await _userWarehousesService.GetByUserIdAsync(userId);
        return Ok(userWarehouses);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<UserWarehouses>>> GetByWarehouseId(int warehouseId)
    {
        var userWarehouses = await _userWarehousesService.GetByWarehouseIdAsync(warehouseId);
        return Ok(userWarehouses);
    }

    [HttpGet("user/{userId}/warehouse/{warehouseId}")]
    public async Task<ActionResult<UserWarehouses>> GetByUserAndWarehouse(string userId, int warehouseId)
    {
        var userWarehouse = await _userWarehousesService.GetByUserAndWarehouseAsync(userId, warehouseId);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with User ID {userId} and Warehouse ID {warehouseId} not found.");

        return Ok(userWarehouse);
    }

    [HttpGet("{id}/with-user")]
    public async Task<ActionResult<UserWarehouses>> GetWithUser(int id)
    {
        var userWarehouse = await _userWarehousesService.GetWithUserAsync(id);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with ID {id} not found.");

        return Ok(userWarehouse);
    }

    [HttpGet("{id}/with-warehouse")]
    public async Task<ActionResult<UserWarehouses>> GetWithWarehouse(int id)
    {
        var userWarehouse = await _userWarehousesService.GetWithWarehouseAsync(id);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with ID {id} not found.");

        return Ok(userWarehouse);
    }

    [HttpPost]
    public async Task<ActionResult<UserWarehouses>> Create([FromBody] UserWarehousesDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _userWarehousesService.ExistsByUserAndWarehouseAsync(dto.UserId, dto.WarehouseId))
            return Conflict($"UserWarehouse with User ID {dto.UserId} and Warehouse ID {dto.WarehouseId} already exists.");

        var userWarehouse = new UserWarehouses
        {
            UserId = dto.UserId,
            WarehouseId = dto.WarehouseId
        };

        var created = await _userWarehousesService.AddAsync(userWarehouse);
        return CreatedAtAction(nameof(GetById), new { id = created.UserWarehousesId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserWarehousesDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userWarehouse = await _userWarehousesService.GetByIdAsync(id);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with ID {id} not found.");

        userWarehouse.UserId = dto.UserId;
        userWarehouse.WarehouseId = dto.WarehouseId;

        await _userWarehousesService.UpdateAsync(userWarehouse);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userWarehouse = await _userWarehousesService.GetByIdAsync(id);
        if (userWarehouse == null)
            return NotFound($"UserWarehouse with ID {id} not found.");

        await _userWarehousesService.DeleteAsync(id);
        return NoContent();
    }
}

