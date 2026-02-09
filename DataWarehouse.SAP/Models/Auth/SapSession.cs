using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Auth
{
    public class SapSession
    {
        public string SessionId { get; set; }
        public string Version { get; set; }
        public DateTime SessionTimeout { get; set; }


    }

}
