using Backend.DTOs.Applications;
using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationProcessingPermissionEvaluator
{
    void EnsureCanOperate(ApplicationActorContext actor);
    bool CanOperate(ApplicationActorContext actor);
    bool CanProcessAutomatically(DonTu application, ApplicationActorContext actor);
    bool CanRecordProcessingResult(DonTu application, ApplicationActorContext actor);
    void ApplyAllowedActions(AdminApplicationAllowedActionsDto allowedActions, DonTu application, ApplicationActorContext actor);
}
