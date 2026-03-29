using DataWarehouse.Domain.Enums.Approval;

namespace DataWarehouse.Core.Interfaces.Notifications;

public interface IAppNotificationTrigger
{
    Task TriggerProcessStatusNotificationAsync(ProcessType processType, int referenceId, string actionType);
    Task TriggerOrderCreatedNotificationAsync(ProcessType processType, int referenceId, string userId, bool isDraft);
}
