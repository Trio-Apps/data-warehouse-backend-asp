using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Core.Interfaces.Company;
using DataWarehouse.Core.Interfaces.IsProgress;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using DataWarehouse.Services.Repository.Based;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.IsProgress
{
    internal class ProcessSettingApprovalRepository : BaseRepository<ProcessSettingApproval>, IProcessSettingApprovalRepository
    {
        private readonly ICompanyCache companyCache;

        public ProcessSettingApprovalRepository(ICompanyCache companyCache, DataWarehouseDbContext context) : base(context)
        {
            this.companyCache = companyCache;
        }

        // get all by company Id
        public async Task<IReadOnlyList<ProcessSettingApprovalDto>> GetProcessSettingsAsync(
     CancellationToken cancellationToken = default)
        {
            var companyId = await companyCache.Get()??0;

            await EnsureAllProcessTypesExist(companyId, cancellationToken);

            return await _context.Set<ProcessSettingApproval>()
                .Where(x => x.CompanyId == companyId)
                .Include(x => x.ApprovalSteps.OrderBy(s => s.StepOrder))
                .AsNoTracking()
                .OrderBy(x => x.ProcessType)
                .Select(e => new ProcessSettingApprovalDto
                {
                    ProcessSettingApprovalId
                     = e.ProcessSettingApprovalId,
                    CompanyId = companyId,
                    IgnoreSteps = e.IgnoreSteps,
                    ProcessType = e.ProcessType.ToString(),
                })
                .ToListAsync(cancellationToken);
            
        }

     

        // get by id
        public async Task<ProcessSettingApprovalDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
              var e = await _context.Set<ProcessSettingApproval>()
                .Include(x => x.ApprovalSteps.OrderBy(s => s.StepOrder))
                
                .FirstOrDefaultAsync(x => x.ProcessSettingApprovalId == id, cancellationToken);

            return new ProcessSettingApprovalDto
            {
                ProcessSettingApprovalId
                     = e.ProcessSettingApprovalId,
                CompanyId = e.CompanyId,
                IgnoreSteps = e.IgnoreSteps,
                ProcessType = e.ProcessType.ToString(),
                ApprovalSteps = e.ApprovalSteps.Select(res => new ApprovalStepDto
                {
                    ApprovalStepId = res.ApprovalStepId,
                    StepName = res.StepName,
                    StepOrder = res.StepOrder,
                    RoleId = res.RoleId,
                    IsFinalStep = res.IsFinalStep,
                    CompanyId = res.CompanyId
                }).ToList()
            };
        }


        public async Task<bool> ToggleIgnoreStepsAsync(
           int processSettingApprovalId,
           CancellationToken cancellationToken = default)
        {
            var setting = await _context.Set<ProcessSettingApproval>()
                .FirstOrDefaultAsync(
                    x => x.ProcessSettingApprovalId == processSettingApprovalId,
                    cancellationToken);

            if (setting == null)
                return false;

            setting.IgnoreSteps = !setting.IgnoreSteps;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }



        public async Task<ProcessSettingApproval?> GetByProcessTypeAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProcessSettingApproval>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CompanyId == companyId && x.ProcessType == processType,
                    cancellationToken);
        }

        public async Task<ProcessSettingApproval?> GetByProcessTypeWithStepsAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProcessSettingApproval>()
                .Include(x => x.ApprovalSteps.OrderBy(s => s.StepOrder))
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CompanyId == companyId && x.ProcessType == processType, cancellationToken);
        }

        public async Task<IReadOnlyList<ProcessSettingApproval>> GetAllAsync(
            int companyId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProcessSettingApproval>()
                .Where(x => x.CompanyId == companyId)
                .Include(x => x.ApprovalSteps.OrderBy(s => s.StepOrder))
                .AsNoTracking()
                .OrderBy(x => x.ProcessType)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProcessSettingApproval>()
                .AnyAsync(
                    x => x.CompanyId == companyId && x.ProcessType == processType,
                    cancellationToken);
        }

        public async Task AddAsync(
            ProcessSettingApproval entity,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<ProcessSettingApproval>().AddAsync(entity, cancellationToken);
        }

        public void Update(ProcessSettingApproval entity)
        {
            _context.Set<ProcessSettingApproval>().Update(entity);
        }

        public void Remove(ProcessSettingApproval entity)
        {
            _context.Set<ProcessSettingApproval>().Remove(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        // helper
        private async Task EnsureAllProcessTypesExist(
    int companyId,
    CancellationToken cancellationToken)
        {
            var dbSet = _context.Set<ProcessSettingApproval>();

            var existingProcessTypes = await dbSet
                .Where(x => x.CompanyId == companyId)
                .Select(x => x.ProcessType)
                .ToListAsync(cancellationToken);

            var existingSet = existingProcessTypes.ToHashSet();

            var allProcessTypes = Enum.GetValues<ProcessType>();

            var newSettings = new List<ProcessSettingApproval>();

            foreach (var processType in allProcessTypes)
            {
                if (existingSet.Contains(processType))
                    continue;

                newSettings.Add(new ProcessSettingApproval
                {
                    CompanyId = companyId,
                    ProcessType = processType,
                    IgnoreSteps = true
                });
            }

            if (newSettings.Count == 0)
                return;

            await dbSet.AddRangeAsync(newSettings, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // في حالة concurrency لو request تاني أضافهم
            }
        }

    }
}
