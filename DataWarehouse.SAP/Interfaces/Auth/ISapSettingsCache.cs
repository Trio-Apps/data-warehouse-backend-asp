using DataWarehouse.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Auth
{
    public interface ISapSettingsCache
    {
        Task<SapDto> GetOrSetAsync(int sapId);
        void Clear(int sapId);
    }
}
