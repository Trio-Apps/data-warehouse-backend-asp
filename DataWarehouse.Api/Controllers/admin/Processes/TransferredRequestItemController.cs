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
public class TransferredRequestItemController : ControllerBase
{
    private readonly ITransferredRequestItemRepository _repository;
    private readonly ILogger<TransferredRequestItemController> _logger;

    public TransferredRequestItemController(
        ITransferredRequestItemRepository repository,
        ILogger<TransferredRequestItemController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<IEnumerable<TransferredRequestItem>>> GetAll()
    {
        var items = await _repository.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<ActionResult<TransferredRequestItem>> GetById(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
            return NotFound($"TransferredRequestItem with ID {id} not found.");

        return Ok(item);
    }

    [HttpGet("transferred-request/{transferredRequestId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByTransferredRequestId(int transferredRequestId)
    {
        var res = await _repository.GetByTransferredRequestItemByTransferredRequestIdAsync(transferredRequestId);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("status/transferred-request/{transferredRequestId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Get}")]
    public async Task<IActionResult> GetByTransferredRequestIdWithPagination(
        int transferredRequestId, string? status, int skip, int pageSize)
    {
        var res = await _repository.GetByTransferredRequestItemByTransferredRequestIdWithPaginationAsync(
            transferredRequestId, status, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("transferred-request/{transferredRequestId}/add-barcode-or-no/{isBarcode:bool}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Create}")]
    public async Task<IActionResult> Create(
        int transferredRequestId,
        bool isBarcode,
        AddTransferredRequestItemCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (isBarcode && dto.Barcode == null)
            return BadRequest("barcode should be sent with dynamic/static barcode type");

        AddGeneralItemDto? itemDto = null;
        if (!isBarcode && dto.Item != null)
        {
            itemDto = new AddGeneralItemDto
            {
                ItemId = dto.Item.ItemId,
                Quantity = dto.Item.Quantity,
                UoMEntry = dto.Item.UoMEntry,
                UnitPrice = dto.Item.UnitPrice
            };
        }

        var created = await _repository.AddTransferredRequestItemByTransferredRequestIdAsync(
            transferredRequestId,
            isBarcode,
            dto.Barcode,
            itemDto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("transferred-request-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Edit}")]
    public async Task<IActionResult> Update(int id, UpdateTransferredRequestItemDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.TransferredRequestItemId = id;

        var mappedDto = new UpdateGeneralItemDto
        {
            Quantity = dto.Quantity,
            UoMEntry = dto.UoMEntry,
            UnitPrice = dto.UnitPrice,
            Comment = null
        };

        var res = await _repository.UpdateTransferredRequestItemAsync(id, mappedDto);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("transferred-request-item/{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.TransferredRequest_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _repository.DeleteTransferredRequestItemAsync(id);
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
