using DataWarehouse.Api.Controllers.admin.Processes.BulkProductions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Services.Repository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes.OutSide
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinishedGoodPurchaseOrderController : ControllerBase
    {
        private readonly IFinishedGoodPurchaseOrderRepository _repository;
        private readonly ILogger<FinishedGoodPurchaseOrderController> _logger;

        public FinishedGoodPurchaseOrderController(
            IFinishedGoodPurchaseOrderRepository repository,
            ILogger<FinishedGoodPurchaseOrderController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        [HttpGet("search-pagination/warehouse/{warehouseId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Items_Get}")]
        public async Task<IActionResult> GetByWarehouseId(
int warehouseId,
[FromQuery] string? itemCodeOrItemName,
[FromQuery] int pageNumber = 1,
[FromQuery] int pageSize = 20)
        {
            var result = await _repository.GetByWarehouseIdAsync(
                warehouseId,
                itemCodeOrItemName,
                pageNumber,
                pageSize);

            return Ok(result);
        }


        [HttpGet]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetAll()
        {
            var finishedGoodItems = await _repository.GetAllAsync();
            return Ok(finishedGoodItems);
        }

        [HttpGet("GetFinishedGoodBomItemsByWarehouseId/{warehouseId}/{skip}/{pageSize}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetFinishedGoodBomItemsByWarehouseId(
            int warehouseId, int skip, int pageSize, int? itemId)
        {
            var res = await _repository.GetFinishedGoodBomItemsByWarehouseIdAsync(warehouseId, itemId, skip, pageSize);
            return Ok(res);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetById(int id)
        {
            var finishedGoodItem = await _repository.GetByIdAsync(id);
            if (finishedGoodItem == null)
                return NotFound($"FinishedGoodItem with ID {id} not found.");

            return Ok(finishedGoodItem);
        }

        [HttpGet("warehouse/{warehouseId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetByWarehouseId(int warehouseId)
        {
            var finishedGoodItems = await _repository.GetByWarehouseIdAsync(warehouseId);
            return Ok(finishedGoodItems);
        }
     

        [HttpGet("item/{itemId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetByItemId(int itemId)
        {
            var finishedGoodItems = await _repository.GetByItemIdAsync(itemId);
            return Ok(finishedGoodItems);
        }

        [HttpGet("item/{itemId}/warehouse/{warehouseId}")]
        [Authorize(Policy = $"{PermissionPolicyProvider.Prefix}{AppPermissions.Purchases_Get}")]
        public async Task<IActionResult> GetByItemAndWarehouse(int itemId, int warehouseId)
        {
            var finishedGoodItem = await _repository.GetByItemAndWarehouseAsync(itemId, warehouseId);
            if (finishedGoodItem == null)
                return NotFound($"FinishedGoodItem with ItemId {itemId} and WarehouseId {warehouseId} not found.");

            return Ok(finishedGoodItem);
        }
    }


}
