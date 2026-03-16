using DataWarehouse.Domain.Entities.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Proccesses
{
    public interface ISapAttachmentService
    {
        Task<int?> CreateAttachmentEntryAsync(
            int sapId,
            List<DocumentAttachment> attachments);
    }
}
