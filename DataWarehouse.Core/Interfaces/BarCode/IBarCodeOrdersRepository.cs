using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.BarCode
{
    public interface IBarCodeOrdersRepository
    {
        Task<ICollection<ItemByBarCodeDto>> GetItemsByBarCodesAsync(BarCodeOrdersDto barCodeOrdersDto);
        Task<GeneralResponse<ItemByBarCodeDto>> GetItemByStaticBarCodeAsync(int warehouseId, DynamicBarcodesDto dto);
        Task<GeneralResponse<ItemByBarCodeDto>> GetItemByDynamicBarCodeAsync(int warehouseId, DynamicBarcodesDto dto);
  
    
    }
}
