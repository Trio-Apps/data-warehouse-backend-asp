using DataWarehouse.SAP.Models.Based;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Based
{
    public  interface IBaseSap<T>
    {

        Task<string> AddSapAsync(int sapId, string entityType,T entity);
        Task DeleteSap(int sapId, string entityType);
         Task<int> GetByIdSap(int sapId, string entityType, string id);


        Task UpdateSap(int sapId, string entityType, T entity, string id);

        Task<bool> AddPatchSapAsync(int sapId, string entityType, T entity);
        Task<string> GetAllSap(int sapId, string entityType);

        Task<string> GetAllSapPrivate(int sapId, string entityType);

    }
}
