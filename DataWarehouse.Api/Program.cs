using DataWarehouse.Api;
using DataWarehouse.Core.DTOs.Auth;
using DataWarehouse.Core.Interfaces.Queue;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.SAP;
using DataWarehouse.SAP.SeedData;
using DataWarehouse.SeedData.Roles;
using DataWarehouse.SeedData.Users;
using DataWarehouse.Services;
using DataWarehouse.Services.Repository.Permissions;
using DataWarehouse.Services.SeedData.BarCode;
using DataWarehouse.Services.SeedData.IncrementalSync;
using DataWarehouse.Services.Services.Auth;
using FirebaseAdmin;
using FormBuilder.API.ExceptionHandlers;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(
             options =>
             {
                 // Default Password settings.
                 options.Password.RequireDigit = false;
                 options.Password.RequireLowercase = false;
                 options.Password.RequireNonAlphanumeric = false;
                 options.Password.RequireUppercase = false;
                 options.Password.RequiredLength = 3;
                 options.Password.RequiredUniqueChars = 0;
             }
             )
             .AddEntityFrameworkStores<DataWarehouseDbContext>()
             .AddDefaultTokenProviders();

builder.Services.Replace(
    ServiceDescriptor.Scoped<IRoleValidator<ApplicationRole>, CompanyRoleValidator>());


// Persistance Container

builder.Services.AddPersistenceService(builder.Configuration);
// Persistance Container

builder.Services.AddSapService(builder.Configuration);

// HangFire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("connectionString"));
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
   .WithOrigins(
                "https://datawarehouse.runasp.net",
                "http://localhost:4200",
                "https://localhost:4200"
            ).AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // خليه بس لو فعلاً بتستخدم Cookies
    });
});
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "sap", "default" }; // sap للـ SAP jobs
    options.WorkerCount = Math.Max(2, Environment.ProcessorCount); // عدّل حسب احتياجك
});


//if (FirebaseApp.DefaultInstance is null)
//{
//    FirebaseApp.Create(new AppOptions
//    {
//        Credential = GoogleCredential.GetApplicationDefault()
//    });
//}

builder.Services.AddScoped<ISapJobQueuer, SapJobQueuer>();
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();


//// seed Roles
// ✅ الطريقة الصحيحة
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // then permissions
    await PermissionSeeder.SeedAsync(services);

    // ثم Users
    await UserSeeder.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

// Enable static files serving from wwwroot
app.UseStaticFiles();

app.UseRouting();                // 1️⃣ الأول
app.UseCors("AllowAngular");

app.UseAuthentication();         // 2️⃣
app.UseAuthorization();          // 3️⃣

// باقي الميدل وير
app.UseHangfireDashboard("/jobs");

app.MapGet("/", () => "Firebase is ready");


// 🔥 تسجيل الـ Jobs بعد ما الـ DI يبقى جاهز
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = services.GetRequiredService<IConfiguration>();

    HangfireJobScheduler.RegisterJobs(services, configuration);
}

app.MapControllers();            // 5️⃣

app.Run();