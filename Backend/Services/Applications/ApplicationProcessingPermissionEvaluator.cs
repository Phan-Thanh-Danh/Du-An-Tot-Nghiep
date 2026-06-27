using Backend.Constants;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services.Applications;

public class ApplicationProcessingPermissionEvaluator : IApplicationProcessingPermissionEvaluator
{
    private static readonly string[] OperatorRoles =
    [
        AuthRoles.SuperAdmin,
        AuthRoles.Admin,
        AuthRoles.CampusAdmin,
        AuthRoles.SubCampusAdmin,
        AuthRoles.AcademicStaff
    ];

    private static readonly string[] AutoProcessStatuses =
    [
        ApplicationProcessingStatuses.Pending,
        ApplicationProcessingStatuses.Failed
    ];

    private static readonly string[] RecordResultStatuses =
    [
        ApplicationProcessingStatuses.Pending,
        ApplicationProcessingStatuses.ManualRequired,
        ApplicationProcessingStatuses.Failed
    ];

    public void EnsureCanOperate(ApplicationActorContext actor)
    {
        if (!CanOperate(actor))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xử lý nghiệp vụ sau duyệt.");
        }
    }

    public bool CanOperate(ApplicationActorContext actor)
    {
        return OperatorRoles.Contains(actor.Role);
    }

    public bool CanProcessAutomatically(DonTu application, ApplicationActorContext actor)
    {
        return CanOperate(actor) &&
               application.TrangThai == ApplicationStatuses.Approved &&
               AutoProcessStatuses.Contains(application.TrangThaiXuLyNghiepVu);
    }

    public bool CanRecordProcessingResult(DonTu application, ApplicationActorContext actor)
    {
        return CanOperate(actor) &&
               application.TrangThai == ApplicationStatuses.Approved &&
               RecordResultStatuses.Contains(application.TrangThaiXuLyNghiepVu);
    }

    public void ApplyAllowedActions(AdminApplicationAllowedActionsDto allowedActions, DonTu application, ApplicationActorContext actor)
    {
        allowedActions.CanProcessAutomatically = CanProcessAutomatically(application, actor);
        allowedActions.CanRecordProcessingResult = CanRecordProcessingResult(application, actor);
    }
}
