using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Services.Based;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class SupplierItemService : BaseService<SupplierItem>, ISupplierItemService
{
    private readonly ISupplierItemRepository _supplierItemRepository;

    public SupplierItemService(ISupplierItemRepository supplierItemRepository) : base(supplierItemRepository)
    {
        _supplierItemRepository = supplierItemRepository;
    }

    public async Task<IEnumerable<SupplierItem>> GetBySupplierIdAsync(int supplierId)
    {
        return await _supplierItemRepository.GetBySupplierIdAsync(supplierId);
    }

    public async Task<IEnumerable<SupplierItem>> GetByItemIdAsync(int itemId)
    {
        return await _supplierItemRepository.GetByItemIdAsync(itemId);
    }

    public async Task<SupplierItem?> GetBySupplierAndItemAsync(int supplierId, int itemId)
    {
        return await _supplierItemRepository.GetBySupplierAndItemAsync(supplierId, itemId);
    }

    public async Task<IEnumerable<SupplierItem>> GetPreferredSuppliersByItemIdAsync(int itemId)
    {
        return await _supplierItemRepository.GetPreferredSuppliersByItemIdAsync(itemId);
    }

    public async Task<SupplierItem?> GetWithSupplierAsync(int supplierItemId)
    {
        return await _supplierItemRepository.GetWithSupplierAsync(supplierItemId);
    }

    public async Task<SupplierItem?> GetWithItemAsync(int supplierItemId)
    {
        return await _supplierItemRepository.GetWithItemAsync(supplierItemId);
    }

    public async Task<bool> ExistsBySupplierAndItemAsync(int supplierId, int itemId)
    {
        return await _supplierItemRepository.ExistsBySupplierAndItemAsync(supplierId, itemId);
    }
}
