using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Auth
{
    public interface ISapSessionCache
    {
        SapSession? Get(int sapId);
        void Set(int sapId, SapSession session);
        void Clear(int sapId);
    }

}
