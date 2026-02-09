using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GoodsReturnOrderBatchController : ControllerBase
{
    private readonly IGoodsReturnOrderBatchRepository _repository;
    private readonly ILogger<GoodsReturnOrderBatchController> _logger;

    public GoodsReturnOrderBatchController(
        IGoodsReturnOrderBatchRepository repository,
        ILogger<GoodsReturnOrderBatchController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderBatch>>> GetAll()
    {
        var batches = await _repository.GetAllAsync();
        return Ok(batches);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<GoodsReturnOrderBatch>> GetById(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"GoodsReturnOrderBatch with ID {id} not found.");

        return Ok(batch);
    }

    [HttpGet("goods-return-order-item/{goodsReturnOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderBatch>>> GetByGoodsReturnOrderItemId(int goodsReturnOrderItemId)
    {
        var res = await _repository.GetByGoodsReturnOrderItemIdAsync(goodsReturnOrderItemId);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpGet("goods-return-order-item/{goodsReturnOrderItemId}/{skip}/{pageSize}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Get}")]
    public async Task<ActionResult<IEnumerable<GoodsReturnOrderBatch>>> GetByGoodsReturnOrderItemIdWithPagination(
        int goodsReturnOrderItemId, int skip, int pageSize)
    {
        var res = await _repository.GetByGoodsReturnOrderItemIdWithPaginationAsync(
            goodsReturnOrderItemId, skip, pageSize);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpPost("goods-return-order-item/{goodsReturnOrderItemId}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Create}")]
    public async Task<ActionResult<GoodsReturnOrderBatch>> Create(
        int goodsReturnOrderItemId, AddGoodsReturnOrderBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.AddByGoodsReturnOrderItemIdAsync(goodsReturnOrderItemId, dto);

        if (!created.Success)
            return BadRequest(created);

        return Ok(created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Edit}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateGoodsReturnOrderBatchDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var res = await _repository.UpdateGoodsReturnOrderBatchAsync(id, dto);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.GoodsReturn_Delete}")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _repository.GetByIdAsync(id);
        if (batch == null)
            return NotFound($"GoodsReturnOrderBatch with ID {id} not found.");

        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
}

