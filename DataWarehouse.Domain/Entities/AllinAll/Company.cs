using DataWarehouse.Domain.Entities.Auth;
using DataWarehouse.Domain.Entities.BarCode;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Entities.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.AllinAll
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
        public ICollection<Sap> Saps { get; set; } = new List<Sap>();
        public ICollection<BarCodeSetting> BarCodeSettings { get; set; } = new List<BarCodeSetting>();
        public ICollection<ProcessesType> ProcessesTypes { get; set; } = new List<ProcessesType>();
        public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
        public ICollection<ApplicationRole> Roles { get; set; } = new HashSet<ApplicationRole>();
    }
}
