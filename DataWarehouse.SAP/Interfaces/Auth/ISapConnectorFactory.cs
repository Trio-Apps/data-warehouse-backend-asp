using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Interfaces.Auth
{
    public interface ISapConnectorFactory
    {
        Task<HttpClient> Create(int sapId);
    }
}
