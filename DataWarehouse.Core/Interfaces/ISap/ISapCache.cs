using DataWarehouse.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.ISap
{
    public interface ISapCache
    {
        Task<int?> Get();
        void Set(int sapId, SapDto sap);
        void Clear(int sapId);
        Task UpdateSapUserClaimAsync(string claimValue);
    }

}
