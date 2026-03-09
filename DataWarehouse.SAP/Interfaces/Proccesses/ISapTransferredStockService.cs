using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
  
    public interface ISapTransferredStockService
    {
        Task<string> SyncTransferredStockAsync(int sapId);
    }
}
