using DataWarehouse.Core.Interfaces.Actors;
using DataWarehouse.Core.IServices.Actors;
using DataWarehouse.Domain.Entities.Actors.IncrementalSync;
using DataWarehouse.Services.Repository.Actors;
using DataWarehouse.Services.Services.Based;
using System;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Services.Actors;

public class SapSyncStatusFrontService : BaseService<SapSyncStatusFront>, ISapSyncStatusFrontService
{
    private readonly ISapSyncStatusFrontRepository _sapSyncStatusFrontRepository;

    public SapSyncStatusFrontService(ISapSyncStatusFrontRepository sapSyncStatusFrontRepository) 
        : base(sapSyncStatusFrontRepository)
    {
        _sapSyncStatusFrontRepository = sapSyncStatusFrontRepository;
    }

  

   
}

