using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryNoteBatchController : ControllerBase
    {
        private readonly IDeliveryNoteBatchRepository _repository;
        private readonly ILogger<DeliveryNoteBatchController> _logger;

        public DeliveryNoteBatchController(
            IDeliveryNoteBatchRepository repository,
            ILogger<DeliveryNoteBatchController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<IEnumerable<DeliveryNoteBatch>>> GetAll()
        {
            var batches = await _repository.GetAllAsync();
            return Ok(batches);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<DeliveryNoteBatch>> GetById(int id)
        {
            var batch = await _repository.GetByIdAsync(id);
            if (batch == null)
                return NotFound($"DeliveryNoteBatch with ID {id} not found.");

            return Ok(batch);
        }


        [HttpGet("delivery-note-item/{deliveryNoteItemId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<IActionResult> GetByDeliveryNoteItemId(int deliveryNoteItemId)
        {
            var res = await _repository.GetByDeliveryNoteItemIdAsync(deliveryNoteItemId);

            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }


        [HttpGet("delivery-note-item/{deliveryNoteItemId}/{skip}/{pageSize}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<IActionResult> GetByDeliveryNoteItemIdWithPagination(
            int deliveryNoteItemId, int skip, int pageSize)
        {
            var res = await _repository.GetByDeliveryNoteItemIdWithPaginationAsync(deliveryNoteItemId, skip, pageSize);

            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }

        [HttpPost("delivery-note-item/{deliveryNoteItemId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
        public async Task<IActionResult> Create(int deliveryNoteItemId, GeneralBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.AddByDeliveryNoteItemIdAsync(deliveryNoteItemId, dto);

            if (!created.Success)
                return BadRequest(created);

            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Edit}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGeneralBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _repository.UpdateDeliveryNoteBatchAsync(id, dto);

            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Delete}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _repository.DeleteDeliveryNoteBatchAsync(id);

            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }
    }
}
