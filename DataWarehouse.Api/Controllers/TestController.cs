using DataWarehouse.Api.Resources;
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
using Microsoft.Extensions.Localization;
using Polly;
using System.Globalization;

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
        private readonly IStringLocalizer<SharedResource> localizer;

        public TestController(
            DataWarehouseDbContext context,
            ILogger<TestController> logger,
            ISapAuthService sap,
            ISapItemService sapItem,
            IStringLocalizer<SharedResource> localizer)
        {
            this.context = context;
            this.logger = logger;
            this.sap = sap;
            this.sapItem = sapItem;
            this.localizer = localizer;
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

        [HttpGet("localized")]
        public IActionResult LocalizedExample()
        {
            // This endpoint returns a localized string based on the current request culture.
            // Use Accept-Language header or ?culture=ar to see Arabic.
            var welcome = localizer["Welcome"].Value;
            return Ok(new { message = welcome });
        }

        public record LocalizeRequest(string Culture);

        [HttpPost("localized")]
        public IActionResult LocalizedFromBody([FromBody] LocalizeRequest request)
        {
            // If the client wants to force a culture via JSON body, we can override the current thread culture.
            if (!string.IsNullOrWhiteSpace(request?.Culture))
            {
                var culture = new CultureInfo(request.Culture);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            var welcome = localizer["Welcome"].Value;
            return Ok(new { message = welcome, culture = CultureInfo.CurrentUICulture.Name });
        }

        [HttpPost("localized/with-culture")]
        public IActionResult LocalizedFromBodyWithCulture([FromBody] LocalizeRequest request)
        {
            var culture = string.IsNullOrWhiteSpace(request?.Culture)
                ? CultureInfo.CurrentUICulture
                : new CultureInfo(request.Culture);

            CultureInfo.CurrentUICulture = culture;
            var welcome = localizer["Welcome"].Value;
            return Ok(new { message = welcome, culture = culture.Name });
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
