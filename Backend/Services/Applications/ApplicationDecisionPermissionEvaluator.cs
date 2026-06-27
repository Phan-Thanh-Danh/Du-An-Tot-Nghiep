using Backend.Constants;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services.Applications;

public class ApplicationDecisionPermissionEvaluator : IApplicationDecisionPermissionEvaluator
{
    public AdminApplicationAllowedActionsDto BuildAllowedActions(DonTu application, ApplicationActorContext actor)
    {
        var canReceive = actor.Role is AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin or AuthRoles.AcademicStaff &&
                         application.TrangThai == ApplicationStatuses.Submitted &&
                         application.NguoiDuyetHienTai is null;
        var canAssign = actor.Role is AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin &&
                        application.TrangThai is ApplicationStatuses.Submitted or ApplicationStatuses.InReview or ApplicationStatuses.NeedSupplement;
        var hasAssignee = application.NguoiDuyetHienTai.HasValue;
        var inReview = application.TrangThai == ApplicationStatuses.InReview;

        return new AdminApplicationAllowedActionsDto
        {
            CanReceive = canReceive,
            CanAssign = canAssign && !hasAssignee,
            CanReassign = canAssign && hasAssignee,
            CanRequestSupplement = inReview && hasAssignee && CanRequestSupplementForAssignedApplication(application, actor),
            CanApprove = inReview && hasAssignee && CanSensitiveDecisionForAssignedApplication(application, actor),
            CanReject = inReview && hasAssignee && CanSensitiveDecisionForAssignedApplication(application, actor),
            CanDownloadEvidence = true
        };
    }

    public void EnsureCanRequestSupplement(DonTu application, ApplicationActorContext actor)
    {
        EnsureAssigned(application);
        if (!CanRequestSupplementForAssignedApplication(application, actor))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền yêu cầu bổ sung đơn này.");
        }
    }

    public void EnsureCanApprove(DonTu application, ApplicationActorContext actor)
    {
        EnsureAssigned(application);
        if (!CanSensitiveDecisionForAssignedApplication(application, actor))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền phê duyệt đơn này.");
        }
    }

    public void EnsureCanReject(DonTu application, ApplicationActorContext actor)
    {
        EnsureAssigned(application);
        if (!CanSensitiveDecisionForAssignedApplication(application, actor))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền từ chối đơn này.");
        }
    }

    private static bool CanRequestSupplementForAssignedApplication(DonTu application, ApplicationActorContext actor)
    {
        return actor.Role switch
        {
            AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin => true,
            AuthRoles.AcademicStaff => application.NguoiDuyetHienTai == actor.User.MaNguoiDung,
            _ => false
        };
    }

    private static bool CanSensitiveDecisionForAssignedApplication(DonTu application, ApplicationActorContext actor)
    {
        return actor.Role switch
        {
            AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.Principal => true,
            _ => false
        };
    }

    private static void EnsureAssigned(DonTu application)
    {
        if (!application.NguoiDuyetHienTai.HasValue)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Đơn chưa được tiếp nhận hoặc phân công người xử lý.");
        }
    }
}
