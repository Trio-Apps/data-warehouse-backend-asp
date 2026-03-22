using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
    public  interface ISapBulkProductionService
    {


        // go planned if faild return draft
        Task<string> SyncProductionItemsPlannedAsync(int sapId);



        // go released if faild return planned
        Task<string> SyncProductionItemsReleasedAsync(int sapId);


        //  go Recieved if faild return released
        Task<string> SyncProductionItemsRecievedAsync(int sapId);


        // go closed if faild return Recieved
        Task<string> SyncProductionItemsClosedAsync(int sapId);



    }
}
