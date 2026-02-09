using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Domain.Entities.BarCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.SeedData.BarCode
{
    public static  class BarCodeSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();

            if (!context.BarCodeSettings.Any())
            {
                var barcodeSetting = new BarCodeSetting
                {
                    TotalLength = 13,
                    StartsWith = "99",
                    SapStartPosition = 0,
                    SapLength = 7,
                    QuantityStartPosition = 7,
                    QuantityLength = 5,
                    IgnoreLastDigit = true,
                    DefaultUom = "Kg",
                    CreatedDate = DateTime.UtcNow,
                     ModifiedDate = DateTime.UtcNow,
                      CompanyId = 1
                };

                context.BarCodeSettings.Add(barcodeSetting);
            }

            await context.SaveChangesAsync();
        }

    }
}
