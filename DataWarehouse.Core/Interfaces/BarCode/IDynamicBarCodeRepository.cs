using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.BarCode
{
    public interface IDynamicBarCodeRepository
    {

        //       by barcode id ////
        // get dynamic 

        Task<GeneralResponse<PagedResult<DynamicBarCodeDto>>> GetDynamicBarCodeByBarCodeId(int barCodeId, int pageNumber, int pageSize);
        // add
        Task<GeneralResponse<DynamicBarCodeDto>> AddDynamicBarCodeByBarCodeId(int barCodeId, AddDynamicBarCodeDto dto);

        // update
        Task<GeneralResponse<DynamicBarCodeDto>> UpdateDynamicBarCode( UpdateDynamicBarCodeDto dto);

        // get by id
        Task<GeneralResponse<DynamicBarCodeDto>> GetDynamicBarCodeById(int dynamicBarCodeId);
        // delete

        Task<GeneralResponse<DynamicBarCodeDto>> DeleteDynamicBarCode(int dynamicBarCodeId);


    }
}
