using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Auth;
using DataWarehouse.SAP.Interfaces.Actors;
using Microsoft.Extensions.Logging;


namespace  DataWarehouse.SAP.Jobs.Actors
    {
    public class SalesOrderSyncJob
    {
        private readonly DataWarehouseDbContext _context;
        private readonly ISapAuthService _sapAuth;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SalesOrderSyncJob> _logger;

        public SalesOrderSyncJob(
            DataWarehouseDbContext context,
            ISapAuthService sapAuth,
            IHttpClientFactory httpClientFactory,
            ILogger<SalesOrderSyncJob> logger
            )
        {
            _context = context;
            _sapAuth = sapAuth;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task SyncToSapTestAsync()
        { }

        public async Task SyncToSapAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
           
            if (warehouse == null) return;

            try
            {
                _logger.LogInformation(
     "Inside Hangfire job for warehouseId {warehouseId}",
     warehouseId);


                //                var sapSession = await _sapAuth.GetSessionIdAsync();

                //                var client = _httpClientFactory.CreateClient("SAP");
                //                client.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sapSession.SessionId};ROUTEID=.node1");

                //                var sapRequest = new
                //                {
                //                    ItemCode = "2168",
                //                    ItemName = "Classic Beef Sandwich",
                //                    ItemsGroupCode = 103,
                //                    SalesItem = "tYES",
                //                    PurchaseItem = "tNO",
                //                    VatLiable = "tYES"
                //                };


                //                var response = await client.PostAsJsonAsync(
                //                    "Items",
                //                    sapRequest);


                //                // التحقق من النتيجة
                //                /*
                //                 لو:

                //200 / 201 → تمام

                //400 / 401 / 500 → Exception

                //📌 وده مقصود
                //عشان يروح للـ catch
                //ويعمل Retry
                //                 */
                //                response.EnsureSuccessStatusCode();


          
               // order.LiveStatus = "Synced";
              //  await _context.SaveChangesAsync();

                _logger.LogError(
                   "Success to sync SalesOrder {warehouseId} to SAP",
                   warehouseId);
            }
            catch (Exception ex)
            {
              //  order.LiveStatus = "Failed";
                await _context.SaveChangesAsync();

                _logger.LogError(ex,
                    "Failed to sync SalesOrder {OrderId} to SAP",
                    warehouseId);

                throw; // 👈 مهم عشان Hangfire يعمل Retry
            }
        }
    }

}
