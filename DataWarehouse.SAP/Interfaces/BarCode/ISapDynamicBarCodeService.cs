using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.BarCode
{
    public interface ISapDynamicBarCodeService
    {
        Task<string> SyncDynamicBarcodeAsync(int sapId);
        Task<string> SyncDeleteDynamicBarCodeAsync(int sapId);
    }
}
