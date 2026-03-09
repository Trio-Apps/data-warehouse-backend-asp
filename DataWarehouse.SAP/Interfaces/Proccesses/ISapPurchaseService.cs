using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
    public interface ISapPurchaseService
    {
        // go planned if faild return draft
        Task<string> SyncPurchaseAsync(int purchaseOrderId);
      //  Task<string> SyncPurchaseAsync(int sapId);

    }
}
