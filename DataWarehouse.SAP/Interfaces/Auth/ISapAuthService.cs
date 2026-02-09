using DataWarehouse.SAP.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Auth
{
    public interface ISapAuthService
    {
        Task<SapSession> GetSessionIdAsync(int sapId);
        Task<SapSession> ForceReLoginAsync(int sapId);
    }

}
