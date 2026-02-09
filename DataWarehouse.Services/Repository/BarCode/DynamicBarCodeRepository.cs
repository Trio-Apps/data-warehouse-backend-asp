using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.BarCode;
using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.SapRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.BarCode
{
    public class DynamicBarCodeRepository : BaseRepository<DynamicBarCode>, IDynamicBarCodeRepository
    {
        private readonly ISapCache sapCache;

        public DynamicBarCodeRepository(DataWarehouseDbContext context,ISapCache sapCache) : base(context)
        {
            this.sapCache = sapCache;
        }

        public async Task<GeneralResponse<DynamicBarCodeDto>> AddDynamicBarCodeByBarCodeId(int barCodeId,
            AddDynamicBarCodeDto dto)
        {
            var sapId = await sapCache.Get();

            var setting = await CheckCodeValidation(dto.BarCode);

            if (!setting)
                return GeneralResponse<DynamicBarCodeDto>
                    .FailResponse("Barcode does not match any configuration");


            var barCode = await _context.ItemBarCodes.FindAsync(barCodeId);



            var mapping = new DynamicBarCode
            {
                BarCode = barCode.BarCode + dto.BarCode,
                ItemBarCodeId = barCodeId,
                IsActive = true,
                SapId = sapId ?? 0,
                SapFlag = false,
                AbsEntry = 0,
            };

            var res = await AddAsync(mapping);
            await SaveChangesAsync();

            var model = new DynamicBarCodeDto
            {
                DynamicBarCodeId = res.DynamicBarCodeId,
                ItemBarCodeId = res.ItemBarCodeId,
                IsActive = res.IsActive,
                BarCode = res.BarCode,

            };

            return GeneralResponse<DynamicBarCodeDto>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<DynamicBarCodeDto>> DeleteDynamicBarCode(int dynamicBarCodeId)
        {
            var res = await GetByIdAsync(dynamicBarCodeId);


            if (res == null)
                return GeneralResponse<DynamicBarCodeDto>
                    .FailResponse("Barcode setting not found");

            res.IsActive = false;

            if (!res.SapFlag)
                await DeleteAsync(res.DynamicBarCodeId);

            await SaveChangesAsync();

            var model = new DynamicBarCodeDto
            {
                DynamicBarCodeId = res.DynamicBarCodeId,
                ItemBarCodeId = res.ItemBarCodeId,
                IsActive = res.IsActive,
                BarCode = res.BarCode
            };

            return GeneralResponse<DynamicBarCodeDto>.SuccessResponse(model);
        }

        public async Task<GeneralResponse<PagedResult<DynamicBarCodeDto>>> GetDynamicBarCodeByBarCodeId(int barCodeId,
            int pageNumber,int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DynamicBarCodes.Where(b => b.ItemBarCodeId == barCodeId && b.IsActive);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(res => new DynamicBarCodeDto
                {
                    DynamicBarCodeId = res.DynamicBarCodeId,
                    ItemBarCodeId = res.ItemBarCodeId,
                    IsActive = res.IsActive,
                    BarCode = res.BarCode

                })
                .ToListAsync();


            var mapping = new PagedResult<DynamicBarCodeDto>()
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords

            };

            return GeneralResponse<PagedResult<DynamicBarCodeDto>>.SuccessResponse(mapping);

        }

        public async Task<GeneralResponse<DynamicBarCodeDto>> GetDynamicBarCodeById(int dynamicBarCodeId)
        {
            var res = await GetByIdAsync(dynamicBarCodeId);
            if (res == null)
                return GeneralResponse<DynamicBarCodeDto>.FailResponse("this Dynamic Barcode not found");

            return GeneralResponse<DynamicBarCodeDto>.SuccessResponse(new DynamicBarCodeDto { BarCode = res.BarCode
            , DynamicBarCodeId = res.DynamicBarCodeId,
             IsActive=res.IsActive,
              ItemBarCodeId=res.ItemBarCodeId,  
            } );
        }
        public async Task<GeneralResponse<DynamicBarCodeDto>> UpdateDynamicBarCode(UpdateDynamicBarCodeDto dto)
        {
            var setting = await CheckCodeValidation(dto.BarCode);

          
            if (!setting)
                return GeneralResponse<DynamicBarCodeDto>
                    .FailResponse("Barcode does not match any configuration");

            var entity = await GetByIdAsync(dto.DynamicBarCodeId);

            var barCode = await _context.ItemBarCodes.FindAsync(entity.ItemBarCodeId);
           
            entity.IsActive = false;

            if (!entity.SapFlag)
                await DeleteAsync(entity.ItemBarCodeId);


            await SaveChangesAsync();


            var model = new AddDynamicBarCodeDto
            {
                BarCode = dto.BarCode
            };

            return await AddDynamicBarCodeByBarCodeId(entity.ItemBarCodeId, model);
        }

        private async Task<bool> CheckCodeValidation(string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode))
                return false;

            var sapId = await sapCache.Get();

            var sap = await _context.Saps.Where(s => s.SapId == sapId).FirstOrDefaultAsync();

            var settings = await _context.BarCodeSettings.Where(e=>e.CompanyId == sap.CompanyId).ToListAsync();

            foreach (var setting in settings)
            {
                var code = barCode;

                if (code.Length != setting.QuantityLength + 1)
                    continue;

                return true;
            }

            return false;
        }

    }
}
