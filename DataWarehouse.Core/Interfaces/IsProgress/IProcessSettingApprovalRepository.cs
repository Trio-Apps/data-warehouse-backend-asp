using DataWarehouse.Core.DTOs.Approval;
using DataWarehouse.Domain.Entities.IsProgress;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.IsProgress
{
    public interface IProcessSettingApprovalRepository
    {

        Task<IReadOnlyList<ProcessSettingApprovalDto>> GetProcessSettingsAsync(
     CancellationToken cancellationToken = default);
        Task<ProcessSettingApprovalDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);


        Task<bool> ToggleIgnoreStepsAsync(int processSettingApprovalId, CancellationToken cancellationToken = default);
        Task<ProcessSettingApproval?> GetByProcessTypeAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default);

        Task<ProcessSettingApproval?> GetByProcessTypeWithStepsAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProcessSettingApproval>> GetAllAsync(
            int companyId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int companyId,
            ProcessType processType,
            CancellationToken cancellationToken = default);

        Task AddAsync(ProcessSettingApproval entity, CancellationToken cancellationToken = default);
        void Update(ProcessSettingApproval entity);
        void Remove(ProcessSettingApproval entity);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
