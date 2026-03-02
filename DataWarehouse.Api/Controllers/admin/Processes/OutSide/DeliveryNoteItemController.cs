using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.DTOs.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes.OutSide;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryNoteItemController : ControllerBase
    {
        private readonly IDeliveryNoteItemRepository _repository;
        private readonly ILogger<DeliveryNoteItemController> _logger;

        public DeliveryNoteItemController(
            IDeliveryNoteItemRepository repository,
            ILogger<DeliveryNoteItemController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<IEnumerable<DeliveryNoteItem>>> GetAll()
        {
            var deliveryNoteItems = await _repository.GetAllAsync();
            return Ok(deliveryNoteItems);
        }


        [HttpGet("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<DeliveryNoteItem>> GetById(int id)
        {
            var deliveryNoteItem = await _repository.GetByIdAsync(id);
            if (deliveryNoteItem == null)
                return NotFound($"DeliveryNoteItem with ID {id} not found.");

            return Ok(deliveryNoteItem);
        }

        [HttpGet("delivery-note-order/{deliveryNoteOrderId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<IActionResult> GetByDeliveryNoteOrderId(int deliveryNoteOrderId)
        {
            var res = await _repository.GetByDeliveryNoteOrderIdAsync(deliveryNoteOrderId);
            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }


        [HttpGet("delivery-note-order/{deliveryNoteOrderId}/{skip}/{pageSize}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<IActionResult> GetByDeliveryNoteOrderIdWithPagination(int deliveryNoteOrderId, int skip, int pageSize)
        {
            var res = await _repository.GetByDeliveryNoteOrderIdWithPaginationAsync(deliveryNoteOrderId, skip, pageSize);
            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }

        /// <summary>
        /// Add by reference (SalesOrderItemId) into DeliveryNoteOrder
        /// </summary>
        [HttpPost("sales-order/{deliveryNoteOrderId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
        public async Task<IActionResult> Create(int deliveryNoteOrderId, AddDeliveryNoteOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var created = await _repository.AddDeliveryNoteItemBySalesOrderItemIdAsync(userId!, deliveryNoteOrderId, dto);
            if (!created.Success)
                return BadRequest(created);

            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Edit}")]
        public async Task<IActionResult> Update(int id, UpdateDeliveryNoteOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _repository.UpdateDeliveryNoteItemAsync(id, dto);
            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }

        /// <summary>
        /// Add manual or barcode (WITHOUT reference)
        /// </summary>
        [HttpPost("witout-reference/delivery-note-order/{deliveryNoteOrderId}/add-barcode-or-no/{isBarcode:bool}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Create}")]
        public async Task<IActionResult> CreateWithoutReference(
            int deliveryNoteOrderId,
            bool isBarcode,
            AddGeneralItemByManualOrBarcodeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.AddDeliveryNoteItemByDeliveryNoteOrderIdWithoutRefAsync(
                deliveryNoteOrderId,
                isBarcode,
                dto.Barcode,
                dto.Item);

            if (!created.Success)
                return BadRequest(created);

            return Ok(created);
        }

        [HttpPut("without-reference/{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Edit}")]
        public async Task<IActionResult> UpdateWithoutReference(int id, UpdateGeneralItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _repository.UpdateDeliveryNoteItemWithoutRefAsync(id, dto);
            if (!res.Success)
                return BadRequest(res);

            return Ok(res);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Delete}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deliveryNoteItem = await _repository.GetByIdAsync(id);
            if (deliveryNoteItem == null)
                return NotFound($"DeliveryNoteItem with ID {id} not found.");

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return Ok("deleted");
        }

        [HttpGet("{id}/with-batches")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<DeliveryNoteItem>> GetWithBatches(int id)
        {
            var deliveryNoteItem = await _repository.GetWithBatchesAsync(id);
            if (deliveryNoteItem == null)
                return NotFound($"DeliveryNoteItem with ID {id} not found.");

            return Ok(deliveryNoteItem);
        }

        [HttpGet("{id}/with-sales-order-item")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<DeliveryNoteItem>> GetWithSalesOrderItem(int id)
        {
            var deliveryNoteItem = await _repository.GetWithSalesOrderItemAsync(id);
            if (deliveryNoteItem == null)
                return NotFound($"DeliveryNoteItem with ID {id} not found.");

            return Ok(deliveryNoteItem);
        }

        [HttpGet("{id}/with-item")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.DeliveryNote_Get}")]
        public async Task<ActionResult<DeliveryNoteItem>> GetWithItem(int id)
        {
            var deliveryNoteItem = await _repository.GetWithItemAsync(id);
            if (deliveryNoteItem == null)
                return NotFound($"DeliveryNoteItem with ID {id} not found.");

            return Ok(deliveryNoteItem);
        }
    }
}
