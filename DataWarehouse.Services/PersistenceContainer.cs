using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.Interfaces.BarCode;
using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.Dashboard;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Core.Interfaces.Permissions;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Core.Interfaces.Processes.OutSide;
using DataWarehouse.Core.Interfaces.Sync;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Core.IServices.Auth;
using DataWarehouse.Domain.Context;
using DataWarehouse.Services.Repository.Actors;
using DataWarehouse.Services.Repository.BarCode;
using DataWarehouse.Services.Repository.Based;
using DataWarehouse.Services.Repository.CompanyRepo;
using DataWarehouse.Services.Repository.Dashboard;
using DataWarehouse.Services.Repository.IsProgress;
using DataWarehouse.Services.Repository.Permissions;
using DataWarehouse.Services.Repository.Processes;
using DataWarehouse.Services.Repository.Processes.BulkProductions;
using DataWarehouse.Services.Repository.Processes.OutSide;
using DataWarehouse.Services.Repository.Processes.PurchaseOrderRepo;
using DataWarehouse.Services.Repository.SapRepo;
using DataWarehouse.Services.Repository.Sync;
using DataWarehouse.Services.Services.Actors;
using DataWarehouse.Services.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DataWarehouse.Services
{
    public static class PersistenceContainer
    {
        public static IServiceCollection AddPersistenceService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataWarehouseDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("connectionString")));

            #region Base Repository
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IBaseProcessesRepository<>), typeof(BaseProcessesRepository<>));
            #endregion


            #region Actors Repositories
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IBinLocationRepository, BinLocationRepository>();
            services.AddScoped<IUserWarehousesRepository, UserWarehousesRepository>();
            services.AddScoped<ISupplierItemRepository, SupplierItemRepository>();
            services.AddScoped<ISapSyncStatusFrontRepository, SapSyncStatusFrontRepository>();
            #endregion


            #region Sync Repositories
            services.AddScoped<ISapSyncResetRepository, SapSyncResetRepository>();
            #endregion

            #region Processes Repositories
            services.AddScoped<ICountStockRepository, CountStockRepository>();
            services.AddScoped<IQuantityAdjustmentStockRepository, QuantityAdjustmentStockRepository>();
            services.AddScoped<IReceivedStockRepository, ReceivedStockRepository>();
            services.AddScoped<ITransferredStockRepository, TransferredStockRepository>();
            services.AddScoped<ITransferredRequestOrderRepository, TransferredRequestOrderRepository>();
            services.AddScoped<IProcessesTypesDateRepository, ProcessesTypesDateRepository>();
            services.AddScoped<IReceiptPurchaseOrderBatchRepository, ReceiptPurchaseOrderBatchRepository>();
            services.AddScoped<IDocumentSearchRepository, DocumentSearchRepository>();
            #endregion

            #region Processes OutSide Repositories
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IReceiptPurchaseOrderRepository, ReceiptPurchaseOrderRepository>();
            services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
            services.AddScoped<IGoodsReturnOrderRepository, GoodsReturnOrderRepository>();
            services.AddScoped<ISalesReturnOrderRepository, SalesReturnOrderRepository>();
            services.AddScoped<ISalesOrderBatchRepository, SalesOrderBatchRepository>();


            services.AddScoped<IDeliveryNoteOrderRepository, DeliveryNoteOrderRepository>();
            services.AddScoped<IDeliveryNoteItemRepository, DeliveryNoteItemRepository>();
            services.AddScoped<IDeliveryNoteBatchRepository, DeliveryNoteBatchRepository>();
            #endregion

            #region Process Items Repositories
            services.AddScoped<ICountStockItemRepository, CountStockItemRepository>();
            services.AddScoped<ICountStockBatchRepository, CountStockBatchRepository>();
            services.AddScoped<IQuantityAdjustmentStockItemRepository, QuantityAdjustmentStockItemRepository>();
            services.AddScoped<IReceivedItemRepository, ReceivedItemRepository>();
            services.AddScoped<ITransferredItemRepository, TransferredItemRepository>();
            services.AddScoped<ITransferredRequestItemRepository, TransferredRequestItemRepository>();
            services.AddScoped<ITransferredStockBatchRepository, TransferredStockBatchRepository>();
            services.AddScoped<ITransferredRequestBatchRepository, TransferredRequestBatchRepository>();
            services.AddScoped<IReceivedStockBatchRepository, ReceivedStockBatchRepository>();
            services.AddScoped<IQuantityAdjustmentStockBatchRepository, QuantityAdjustmentStockBatchRepository>();
            services.AddScoped<IDocumentAttachmentRepository, DocumentAttachmentRepository>();

            
            #endregion

          
            #region Process Items OutSide Repositories
            services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
            services.AddScoped<IReceiptPurchaseOrderItemRepository, ReceiptPurchaseOrderItemRepository>();
           // services.AddScoped<IReceiptPurchaseOrderItemCommentRepository, ReceiptPurchaseOrderItemCommentRepository>();
            services.AddScoped<ISalesOrderItemRepository, SalesOrderItemRepository>();
            services.AddScoped<IFinishedGoodPurchaseOrderRepository, FinishedGoodPurchaseOrderRepository>();
            services.AddScoped<IGoodsReturnOrderItemRepository, GoodsReturnOrderItemRepository>();
            services.AddScoped<IGoodsReturnOrderBatchRepository, GoodsReturnOrderBatchRepository>();
            services.AddScoped<ISalesReturnOrderItemRepository, SalesReturnOrderItemRepository>();
            services.AddScoped<ISalesReturnOrderBatchRepository, SalesReturnOrderBatchRepository>();
            #endregion

            #region BulkProductions Repositories
            services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
            services.AddScoped<IProductionOrderItemRepository, ProductionOrderItemRepository>();
            services.AddScoped<IFinishedGoodItemRepository, FinishedGoodItemRepository>();
            services.AddScoped<IProductionReceiptRepository, ProductionReceiptRepository>();
            services.AddScoped<IProductionHeaderBatchRepository, ProductionHeaderBatchRepository>();
            #endregion

            #region Dashboard Repositories
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            #endregion

            #region BarCode Repositories
            services.AddScoped<IItemBarCodeRepository, ItemBarCodeRepository>();
            services.AddScoped<IBarCodeSettingRepository, BarCodeSettingRepository>();
            services.AddScoped<IDynamicBarCodeRepository, DynamicBarCodeRepository>();
            services.AddScoped<IBarCodeOrdersRepository, BarCodeOrdersRepository>();
            #endregion

            #region IsProgress Repositories
            services.AddScoped<IProcessItemIsProgressRepository, ProcessItemIsProgressRepository>();
            services.AddScoped<IProcessApprovalRepository, ProcessApprovalRepository>();
            services.AddScoped<IApprovalStepRepository, ApprovalStepRepository>();
            services.AddScoped<IProcessSettingApprovalRepository, ProcessSettingApprovalRepository>();

            services.AddScoped<IApprovalRepository, ApprovalRepository>();

            #endregion

            #region Permissions
            services.AddScoped<IPermissionsRepository, PermissionsRepository>();

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            services.AddAuthorization();
            #endregion

            #region Email 

            //services.Configure<OutlookMailSettings>(configuration.GetSection("MailSetting"));
            //services.AddScoped<IEmailServices, EmailServices>();

            #endregion

            #region JWT 

            services.Configure<JWT>(configuration.GetSection("JWT"));
            // Authorize for Bearer by Defualt without add to each controller

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            #endregion

            #region Auth
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IPasswordServices, PasswordServices>();
            services.AddScoped<IRoleServices, RoleServices>();
            services.AddScoped<IUserServices, UserServices>();
            #endregion
            #region Sap
            services.AddScoped<ISapCache, SapCache>();
            services.AddScoped<ISapSettingsRepository,SapSettingsRepository>();

            #endregion

            #region Company
            services.AddScoped<ICompanyCache, CompanyCache>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();

            #endregion

            #region Actors Services
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IItemService, ItemService>();
      
            services.AddScoped<IBinLocationService, BinLocationService>();
            services.AddScoped<IUserWarehousesService, UserWarehousesService>();
            services.AddScoped<ISupplierItemService, SupplierItemService>();
            services.AddScoped<ISapSyncStatusFrontService, SapSyncStatusFrontService>();
            #endregion
            // Json
            //services.AddControllers().AddJsonOptions(
            //    options =>
            //    {
            //        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            //    });

            //services.AddControllers().AddNewtonsoftJson(
            //    options =>
            //    {
            //        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //    });

            return services;

        }
    }
}
