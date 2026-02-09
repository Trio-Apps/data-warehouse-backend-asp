using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BinLocationController : ControllerBase
{
    private readonly IBinLocationService _binLocationService;
    private readonly ILogger<BinLocationController> _logger;

    public BinLocationController(IBinLocationService binLocationService, ILogger<BinLocationController> logger)
    {
        _binLocationService = binLocationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BinLocation>>> GetAll()
    {
        var binLocations = await _binLocationService.GetAllAsync();
        return Ok(binLocations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BinLocation>> GetById(int id)
    {
        var binLocation = await _binLocationService.GetByIdAsync(id);
        if (binLocation == null)
            return NotFound($"BinLocation with ID {id} not found.");

        return Ok(binLocation);
    }

    [HttpGet("item/{itemId}")]
    public async Task<ActionResult<IEnumerable<BinLocation>>> GetByItemId(int itemId)
    {
        var binLocations = await _binLocationService.GetByItemIdAsync(itemId);
        return Ok(binLocations);
    }

    [HttpGet("location/{location}")]
    public async Task<ActionResult<BinLocation>> GetByLocation(string location)
    {
        var binLocation = await _binLocationService.GetByLocationAsync(location);
        if (binLocation == null)
            return NotFound($"BinLocation with location '{location}' not found.");

        return Ok(binLocation);
    }

    [HttpGet("{id}/with-item")]
    public async Task<ActionResult<BinLocation>> GetWithItem(int id)
    {
        var binLocation = await _binLocationService.GetWithItemAsync(id);
        if (binLocation == null)
            return NotFound($"BinLocation with ID {id} not found.");

        return Ok(binLocation);
    }

    [HttpPost]
    public async Task<ActionResult<BinLocation>> Create([FromBody] BinLocationDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _binLocationService.ExistsByLocationAsync(dto.Location))
            return Conflict($"BinLocation with location '{dto.Location}' already exists.");

        var binLocation = new BinLocation
        {
            Description = dto.Description,
            Location = dto.Location,
            ItemId = dto.ItemId
        };

        var created = await _binLocationService.AddAsync(binLocation);
        return CreatedAtAction(nameof(GetById), new { id = created.BinLocationId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BinLocationDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var binLocation = await _binLocationService.GetByIdAsync(id);
        if (binLocation == null)
            return NotFound($"BinLocation with ID {id} not found.");

        binLocation.Description = dto.Description;
        binLocation.Location = dto.Location;
        binLocation.ItemId = dto.ItemId;

        await _binLocationService.UpdateAsync(binLocation);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var binLocation = await _binLocationService.GetByIdAsync(id);
        if (binLocation == null)
            return NotFound($"BinLocation with ID {id} not found.");

        await _binLocationService.DeleteAsync(id);
        return NoContent();
    }
}

