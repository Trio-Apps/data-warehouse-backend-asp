using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.BarCode
{
    public interface ISapBarCodeService
    {
        Task<string> SyncBarCodeAsync(int sapId);
        Task<string> SyncItemUomGroupAsync(int sapId);
        Task<string> SyncDeleteBarCodeAsync(int sapId);
    }
}
