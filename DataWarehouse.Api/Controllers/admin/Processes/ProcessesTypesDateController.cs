using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProcessesTypesDateController : ControllerBase
{
    private readonly IProcessesTypesDateRepository _repository;
    private readonly ILogger<ProcessesTypesDateController> _logger;

    public ProcessesTypesDateController(
        IProcessesTypesDateRepository repository,
        ILogger<ProcessesTypesDateController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProcessesTypesDateResponseDTO>>> GetAll()
    {
        var entities = await _repository.QueryIncluding(false, ptd => ptd.ProcessesType)
            .ToListAsync();
        var dtos = entities.Select(e => new ProcessesTypesDateResponseDTO
        {
            ProcessesTypesDateId = e.ProcessesTypesDateId,
            PostingDate = e.PostingDate,
            DueDate = e.DueDate,
            ProcessesTypeId = e.ProcessesTypeId,
            ProcessesTypeName = e.ProcessesType?.ProcessesName
        });
        return Ok(dtos);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ProcessesTypesDateResponseDTO>> GetById(int id)
    {
        var entity = await _repository.QueryIncluding(false, ptd => ptd.ProcessesType)
            .FirstOrDefaultAsync(ptd => ptd.ProcessesTypesDateId == id);
        
        if (entity == null)
            return NotFound($"ProcessesTypesDate with ID {id} not found.");

        var dto = new ProcessesTypesDateResponseDTO
        {
            ProcessesTypesDateId = entity.ProcessesTypesDateId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            ProcessesTypeId = entity.ProcessesTypeId,
            ProcessesTypeName = entity.ProcessesType?.ProcessesName
        };

        return Ok(dto);
    }

    [HttpGet("by-processes-type/{processesTypeId}")]
    public async Task<ActionResult<IEnumerable<ProcessesTypesDateResponseDTO>>> GetByProcessesTypeId(int processesTypeId)
    {
        var entities = await _repository.GetByProcessesTypeIdAsync(processesTypeId);
        var dtos = entities.Select(e => new ProcessesTypesDateResponseDTO
        {
            ProcessesTypesDateId = e.ProcessesTypesDateId,
            PostingDate = e.PostingDate,
            DueDate = e.DueDate,
            ProcessesTypeId = e.ProcessesTypeId,
            ProcessesTypeName = e.ProcessesType?.ProcessesName
        });
        return Ok(dtos);
    }

    [HttpGet("type-dates-for-production")]
    public async Task<ActionResult<IEnumerable<ProcessesTypesDateResponseDTO>>> GetTypeDatesForProduction()
    {
        var entities = await _repository.GetByProcessesTypeForProductionAsync();
        var dtos = entities.Select(e => new ProcessesTypesDateResponseDTO
        {
            ProcessesTypesDateId = e.ProcessesTypesDateId,
            PostingDate = e.PostingDate,
            DueDate = e.DueDate,
            ProcessesTypeId = e.ProcessesTypeId,
            ProcessesTypeName = e.ProcessesType?.ProcessesName
        });
        return Ok(dtos);
    }

    [HttpPost("by-processe-id")]
    public async Task<ActionResult<ProcessesTypesDateResponseDTO>> Create([FromBody] ProcessesTypesDateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new ProcessesTypesDate
        {
            PostingDate = dto.PostingDate,
            DueDate = dto.DueDate,
            ProcessesTypeId = dto.ProcessesTypeId
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        var responseDto = new ProcessesTypesDateResponseDTO
        {
            ProcessesTypesDateId = entity.ProcessesTypesDateId,
            PostingDate = entity.PostingDate,
            DueDate = entity.DueDate,
            ProcessesTypeId = entity.ProcessesTypeId
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.ProcessesTypesDateId }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProcessesTypesDateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return NotFound($"ProcessesTypesDate with ID {id} not found.");

        entity.PostingDate = dto.PostingDate;
        entity.DueDate = dto.DueDate;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.DeleteAsync(id);
        if (entity == null)
            return NotFound($"ProcessesTypesDate with ID {id} not found.");

        await _repository.SaveChangesAsync();
        return NoContent();
    }
}

