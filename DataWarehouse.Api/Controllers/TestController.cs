using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors;
using DataWarehouse.SAP.Auth;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Repositories.Actors;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace DataWarehouse.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DataWarehouseDbContext context;
        private readonly ILogger<TestController> logger;
        private readonly ISapAuthService sap;
        private readonly ISapItemService sapItem;

        public TestController(DataWarehouseDbContext context, ILogger<TestController> logger, ISapAuthService sap,ISapItemService sapItem)
        {
            this.context = context;
            this.logger = logger;
            this.sap = sap;
            this.sapItem = sapItem;
        }

        [HttpGet("get-el-get")]
        public async Task<IActionResult> Index()
        {
            var wi = await context.WarehouseItems.ToListAsync();
            return Ok(wi.Count);
        }
        [HttpGet("dynamic-barcode")]
        public async Task<IActionResult> DynamicBarcode()
        {
            var wi = await CheckDynamicCodeValidation(1, "9912312777777");
                                                        
            return Ok(wi==null?"nullll":wi);
        }

        private async Task<string?> CheckDynamicCodeValidation(int sapId, string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode))
                return null;

            barCode = barCode.Trim();

            var sap = await context.Saps
                .Where(s => s.SapId == sapId)
                .Select(s => new { s.CompanyId })
                .FirstOrDefaultAsync();

            if (sap == null)
                return null;

            var settings = await context.BarCodeSettings
                .Where(e => e.CompanyId == sap.CompanyId)
                .ToListAsync();

            foreach (var setting in settings)
            {

                // StartsWith check
                if (!string.IsNullOrEmpty(setting.StartsWith) &&
                    !barCode.StartsWith(setting.StartsWith, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Total length check
                if (barCode.Length != setting.TotalLength)
                    continue;

                // Safety check
                if (setting.SapLength > barCode.Length)
                    continue;

                var staticCode = barCode.Substring(0, setting.SapLength);
                return staticCode;
            }

            return null;
        }


        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var warehouse = new Warehouse()
            {
              
              
                  WarehouseName = "TrioApp",
                   




                //OrderNumber = Guid.NewGuid().ToString(),
                //Status = "Pending",
                //LiveStatus = "NotSynced"
            };

            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            // 🔥 enqueue background sync
            //BackgroundJob.Enqueue<SalesOrderSyncJob>(
            //    job => job.SyncToSapAsync(warehouse.WarehouseId));

            return Ok(warehouse);
        }


        [HttpGet]
        public async Task<IActionResult> CreateTwo(int sapId)
        {

            var res = await sap.GetSessionIdAsync(sapId);


            return Ok(res);
        }


        [HttpGet("TestIncrementalSync")]
        public async Task<IActionResult> TestIncrementalSync(int sapId)
        {

            var res = await sapItem.SyncItemsAsync(sapId);


            return Ok(res);
        }

        /**/

        [HttpGet("testQuery")]
        public async Task<IActionResult> testQuery()
        {

            var result = await context.Items.Include(e=>e.ItemUomGroups)
    .Select(i => new
    {
        i.ItemCode,
        Count = i.ItemUomGroups.Count,
        Uomgroup = i.ItemUomGroups

    })
    .ToListAsync();

            return Ok(result);
        }

    }
}
