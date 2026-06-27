using Backend.DTOs.Applications;
using Backend.Models;

namespace Backend.Services.Applications;

public interface IApplicationDecisionPermissionEvaluator
{
    AdminApplicationAllowedActionsDto BuildAllowedActions(DonTu application, ApplicationActorContext actor);
    void EnsureCanRequestSupplement(DonTu application, ApplicationActorContext actor);
    void EnsureCanApprove(DonTu application, ApplicationActorContext actor);
    void EnsureCanReject(DonTu application, ApplicationActorContext actor);
}
