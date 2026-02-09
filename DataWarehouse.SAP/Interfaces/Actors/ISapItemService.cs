using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Models.Actors.ItemSapModel;

namespace DataWarehouse.SAP.Interfaces.Actors
{
    public interface ISapItemService
    {
        Task<string> SyncItemsAsync(int sapId);
        //Task<int> AddNewItemsAsync(List<SapItemDto> sapItems, int skip);


    }
}
