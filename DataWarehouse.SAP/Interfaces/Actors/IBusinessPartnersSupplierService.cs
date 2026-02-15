using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Actors
{
    public interface IBusinessPartnersSupplierService
    {
        Task<string> SyncBusinessPartnersAsync(int sapId);
    }
}
