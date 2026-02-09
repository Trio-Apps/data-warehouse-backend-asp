using DataWarehouse.Core.DTOs.Actors;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Services.Repository.Actors;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DataWarehouse.Api.Controllers.admin.Actors;

[Route("api/[controller]")]
[ApiController]
public class SapSyncStatusFrontController : ControllerBase
{
    private readonly ISapSyncStatusFrontRepository _sapSyncStatusFrontRepository;
    private readonly ILogger<SapSyncStatusFrontController> _logger;

    public SapSyncStatusFrontController(
        ISapSyncStatusFrontRepository sapSyncStatusFrontRepository, 
        ILogger<SapSyncStatusFrontController> logger)
    {
        _sapSyncStatusFrontRepository = sapSyncStatusFrontRepository;
        _logger = logger;
    }

    
    [HttpGet("GetIncrementalByuserIdAndEntityName/{entityName}/{userId}")]
    public async Task<ActionResult<SapSyncStatusFront>> GetIncrementalByuserIdAndEntityName(string entityName,string userId)
    {
        var item = await _sapSyncStatusFrontRepository.GetByEntityNameAsync(entityName,userId);
        if (item == null)
            return NotFound($"SapSyncStatusFront with EntityName '{entityName}' not found.");

        return Ok(item);
    }

    

    [HttpPatch("UpdateOrAddIncrementalSync/{entityName}/{userId}")]
    public async Task<IActionResult> UpdateOrAddIncrementalSync(string entityName, string userId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        try
        {
         

            var updated = await _sapSyncStatusFrontRepository.UpdateOrAddIncrementalSyncAsync(entityName, userId);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SapSyncStatusFront by EntityName {EntityName}", entityName);
            return StatusCode(500, "An error occurred while updating the entity.");
        }
    }
}

