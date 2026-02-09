using DataWarehouse.Core.Interfaces.Based;
using DataWarehouse.Domain.Entities.Processes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes;

public interface IProcessesTypesDateRepository : IBaseRepository<ProcessesTypesDate>
{
    Task<IEnumerable<ProcessesTypesDate>> GetByProcessesTypeIdAsync(int processesTypeId);
    Task<IEnumerable<ProcessesType>> GetByProcessesTypesAsync();
    Task<IEnumerable<ProcessesTypesDate>> GetByProcessesTypeForProductionAsync();
}

