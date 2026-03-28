using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes;

public class ProcessesTypesDateRepository : BaseRepository<ProcessesTypesDate>, IProcessesTypesDateRepository
{
    public ProcessesTypesDateRepository(DataWarehouseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProcessesTypesDate>> GetByProcessesTypeIdAsync(int processesTypeId)
    {
        return await QueryIncluding(false, ptd => ptd.ProcessesType)
            .Where(ptd => ptd.ProcessesTypeId == processesTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessesType>> GetByProcessesTypesAsync()
    {
        return await _context.ProcessesTypes.ToListAsync();
    }

    public async Task<IEnumerable<ProcessesTypesDate>> GetByProcessesTypeForProductionAsync()
    {
        var processe = await _context.ProcessesTypes.Where(p => p.ProcessesName == ProcessesTypes.Production.ToString()).FirstOrDefaultAsync();

        var typeDates = await QueryIncluding(false, ptd => ptd.ProcessesType)
            .Where(ptd => ptd.ProcessesTypeId ==  processe.ProcessesTypeId)
            .ToListAsync();
        return typeDates;
    }
}

