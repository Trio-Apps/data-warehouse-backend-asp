using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Domain.Context;
using DataWarehouse.SAP.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Repositories.Auth
{
    public class SapConnectorFactory : ISapConnectorFactory
    {
        private readonly DataWarehouseDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISapSettingsCache sapSettings;

        public SapConnectorFactory(
            DataWarehouseDbContext context,
            IHttpClientFactory httpClientFactory,
            ISapSettingsCache sapSettings)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            this.sapSettings = sapSettings;
        }

        public async Task<HttpClient> Create(int sapId)
        {
          ///  var company = _context.Saps.First(x => x.SapId == sapId);

            var sap = await sapSettings.GetOrSetAsync(sapId);
            var client = _httpClientFactory.CreateClient("SAP");

            client.BaseAddress = new Uri(sap.SapUrl);

            return client;
        }
    }

}
