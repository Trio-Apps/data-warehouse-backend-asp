using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
    public interface ISapDeliveryNoteService
    {
        Task<string> SyncDeliveryNotesAsync(int deliveryNoteOrderId);
    }

}
