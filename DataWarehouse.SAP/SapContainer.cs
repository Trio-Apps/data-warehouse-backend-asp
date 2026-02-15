using Dataitem.SAP.Repositories.Actors;
using DataWarehouse.SAP.Auth;
using DataWarehouse.SAP.Interfaces.Actors;
using DataWarehouse.SAP.Interfaces.Auth;
using DataWarehouse.SAP.Interfaces.BarCode;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Models.Auth;
using DataWarehouse.SAP.Repositories.Actors;
using DataWarehouse.SAP.Repositories.Auth;
using DataWarehouse.SAP.Repositories.BarCode;
using DataWarehouse.SAP.Repositories.Based;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace DataWarehouse.SAP
{
    public static class SapContainer
    {
        public static IServiceCollection AddSapService(this IServiceCollection services, IConfiguration configuration)
        {


            services.AddHttpClient("SAP")
                  .ConfigurePrimaryHttpMessageHandler(() =>
                      new HttpClientHandler
                      {
                          UseCookies = true,
                          CookieContainer = new CookieContainer(),
                          ServerCertificateCustomValidationCallback =
                              HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                      })
                  .AddPolicyHandler(
                      HttpPolicyExtensions
                          .HandleTransientHttpError()
                          .WaitAndRetryAsync(3, retry =>
                              TimeSpan.FromSeconds(2 * retry)));




            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            // DI
            services.AddScoped<ISapItemService, SapItemService>();
            services.AddScoped<ISapWarehouseService, SapWarehouseService>();
            services.AddScoped<ISapBarCodeService, SapBarCodeService>();

            services.AddScoped<ISapSyncStatusRepository, SapSyncStatusRepository>();

            services.AddScoped(typeof(IBaseSap<>), typeof(BaseSap<>));


            services.AddScoped<ISapSettingsCache, SapSettingsCache>();
            services.AddScoped<ISapAuthService, SapAuthService>();
            services.AddScoped<ISapSessionCache, SapSessionCache>();
            services.AddScoped<ISapConnectorFactory, SapConnectorFactory>();
            services.AddScoped<ISapDynamicBarCodeService, SapDynamicBarCodeService>();
            services.AddScoped<IBusinessPartnersSupplierService, BusinessPartnersSupplierService>();


            //  services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            //services.AddScoped<IForgetPassword, ForgetPassword>();

            // Repo



            // Json
            services.AddControllers().AddJsonOptions(
                options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                });

            services.AddControllers()
               .AddJsonOptions(options =>
               {
                   options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                   options.JsonSerializerOptions.WriteIndented = true;
               });


            return services;

        }



    }
}
