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
public class TransferredRequestBatchController : ControllerBase
{
    private readonly ITransferredRequestBatchRepository _repository;
    private readonly ILogger<TransferredRequestBatchController> _logger;

    public TransferredRequestBatchController(
        ITransferredRequestBatchRepository repository,
        ILogger<TransferredRequestBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredRequestBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }


    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<TransferredRequestBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"TransferredRequestBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("transferred-request-item/{transferredRequestItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByTransferredRequestItemId(int transferredRequestItemId)
    {
        var res = await _repository.GetByTransferredRequestItemIdAsync(transferredRequestItemId);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("transferred-request-item/{transferredRequestItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByTransferredRequestItemIdWithPagination(
        int transferredRequestItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByTransferredRequestItemIdWithPaginationAsync(transferredRequestItemId, skip, pageSize);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("transferred-request-item/{transferredRequestItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Create}")]
    public async Task<IActionResult> Create(int transferredRequestItemId, AddTransferredRequestBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var mappedDto = new GeneralBatchDto
        {
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = dto.BatchNumber,
            ExpiryDate = dto.ExpiryDate
        };

        var created = await _repository.AddByTransferredRequestItemIdAsync(transferredRequestItemId, mappedDto);
        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Edit}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransferredRequestBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.TransferredRequestBatchId = id;

        var mappedDto = new UpdateGeneralBatchDto
        {
            Quantity = dto.Quantity,
            Comment = dto.Comment,
            BatchNumber = dto.BatchNumber,
            ExpiryDate = dto.ExpiryDate
        };

        var res = await _repository.UpdateTransferredRequestBatchAsync(id, mappedDto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteTransferredRequestBatchAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
