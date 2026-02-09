using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Domain.Entities.AllinAll;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.SeedData
{
    public static class SapSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataWarehouseDbContext>();

            var entities = context.Saps.ToList();
       
          
                if (!entities.Any())
                {
                    var dto = new Domain.Entities.AllinAll.Sap
                    {
                        SapUrl = "https://hb152.beon-it.com:50000/b1s/v1/",
                        CompanyDB = "SBODEMOGB_NEW",
                        UserName = "manager",
                        Password = "manager"
                    };
                    context.Saps.Add( dto);
                }
            
            await context.SaveChangesAsync();

        }

    }
}
