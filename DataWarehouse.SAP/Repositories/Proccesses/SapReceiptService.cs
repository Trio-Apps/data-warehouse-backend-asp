using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
    public class SapReceiptService : ISapReceiptService
    {
        private readonly IBaseSap<SapPurchaseOrderDto> sap;
        private readonly IDynamicBarCodeRepository dynamicBarCodeRepository;
        private readonly ISapSyncStatusRepository _syncRepo;
        private readonly ILogger<SapPurchaseService> logger;
        private readonly DataWarehouseDbContext _context;

        public SapReceiptService(IBaseSap<SapPurchaseOrderDto> sap, IDynamicBarCodeRepository dynamicBarCodeRepository,
            ISapSyncStatusRepository syncRepo, DataWarehouseDbContext context, ILogger<SapPurchaseService> logger)
        {
            this.sap = sap;
            this.dynamicBarCodeRepository = dynamicBarCodeRepository;
            _syncRepo = syncRepo;
            this.logger = logger;
            _context = context;
        }

        public Task<string> SyncReceiptAsync(int sapId)
        {
            throw new NotImplementedException();
        }
    }
}
