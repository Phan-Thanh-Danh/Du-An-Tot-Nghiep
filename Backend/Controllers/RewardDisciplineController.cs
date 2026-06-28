using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/reward-discipline/schema")]
[Authorize]
public class RewardDisciplineController : ControllerBase
{
    [HttpGet("options")]
    public ActionResult<ApiResponseDto<RewardDisciplineSchemaOptionsDto>> GetOptions()
    {
        return Ok(ApiResponseDto<RewardDisciplineSchemaOptionsDto>.Ok(
            CreateOptions(),
            "Lấy cấu hình khen thưởng/kỷ luật thành công."));
    }

    private static RewardDisciplineSchemaOptionsDto CreateOptions()
    {
        return new RewardDisciplineSchemaOptionsDto
        {
            RewardCampaignTypes = RewardCampaignTypes,
            RewardCampaignStatuses = RewardCampaignStatuses,
            CertificateTemplateTypes = CertificateTemplateTypes,
            PaperOrientations = PaperOrientations,
            RewardTypes = RewardTypes,
            RewardStatuses = RewardStatuses,
            DisciplineLevels = DisciplineLevels,
            DisciplineStatuses = DisciplineStatuses,
            DisciplineActions = DisciplineActions
        };
    }

    private static readonly RewardDisciplineOptionDto[] RewardCampaignTypes =
    [
        new(RewardDisciplineConstants.RewardCampaignTypes.Top100Semester, "Top 100 học kỳ")
    ];

    private static readonly RewardDisciplineOptionDto[] RewardCampaignStatuses =
    [
        new(RewardDisciplineConstants.RewardCampaignStatuses.Draft, "Nháp"),
        new(RewardDisciplineConstants.RewardCampaignStatuses.Evaluating, "Đang xét"),
        new(RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval, "Chờ duyệt"),
        new(RewardDisciplineConstants.RewardCampaignStatuses.Approved, "Đã duyệt"),
        new(RewardDisciplineConstants.RewardCampaignStatuses.Published, "Đã công bố"),
        new(RewardDisciplineConstants.RewardCampaignStatuses.Cancelled, "Đã hủy")
    ];

    private static readonly RewardDisciplineOptionDto[] CertificateTemplateTypes =
    [
        new(RewardDisciplineConstants.CertificateTemplateTypes.Top100Semester, "Bằng khen Top 100 học kỳ")
    ];

    private static readonly RewardDisciplineOptionDto[] PaperOrientations =
    [
        new(RewardDisciplineConstants.PaperOrientations.A4Landscape, "A4 ngang"),
        new(RewardDisciplineConstants.PaperOrientations.A4Portrait, "A4 dọc")
    ];

    private static readonly RewardDisciplineOptionDto[] RewardTypes =
    [
        new(RewardDisciplineConstants.RewardTypes.Top100Semester, "Top 100 học kỳ"),
        new(RewardDisciplineConstants.RewardTypes.Other, "Khác"),
        new(RewardDisciplineConstants.RewardTypes.AcademicLegacy, "Học lực"),
        new(RewardDisciplineConstants.RewardTypes.SpecialLegacy, "Đặc biệt"),
        new(RewardDisciplineConstants.RewardTypes.CompetitionLegacy, "Thi đấu")
    ];

    private static readonly RewardDisciplineOptionDto[] RewardStatuses =
    [
        new(RewardDisciplineConstants.RewardStatuses.Draft, "Nháp"),
        new(RewardDisciplineConstants.RewardStatuses.PendingApproval, "Chờ duyệt"),
        new(RewardDisciplineConstants.RewardStatuses.Approved, "Đã duyệt"),
        new(RewardDisciplineConstants.RewardStatuses.Issued, "Đã cấp"),
        new(RewardDisciplineConstants.RewardStatuses.PdfGenerated, "Đã sinh PDF"),
        new(RewardDisciplineConstants.RewardStatuses.PdfFailed, "Lỗi sinh PDF"),
        new(RewardDisciplineConstants.RewardStatuses.Cancelled, "Đã hủy")
    ];

    private static readonly RewardDisciplineOptionDto[] DisciplineLevels =
    [
        new(RewardDisciplineConstants.DisciplineLevels.Minor, "Nhẹ"),
        new(RewardDisciplineConstants.DisciplineLevels.Moderate, "Trung bình"),
        new(RewardDisciplineConstants.DisciplineLevels.Severe, "Nghiêm trọng")
    ];

    private static readonly RewardDisciplineOptionDto[] DisciplineStatuses =
    [
        new(RewardDisciplineConstants.DisciplineStatuses.Draft, "Nháp"),
        new(RewardDisciplineConstants.DisciplineStatuses.PendingApproval, "Chờ duyệt"),
        new(RewardDisciplineConstants.DisciplineStatuses.Approved, "Đã duyệt"),
        new(RewardDisciplineConstants.DisciplineStatuses.Rejected, "Từ chối"),
        new(RewardDisciplineConstants.DisciplineStatuses.Active, "Đang hiệu lực"),
        new(RewardDisciplineConstants.DisciplineStatuses.Expired, "Hết hiệu lực"),
        new(RewardDisciplineConstants.DisciplineStatuses.Removed, "Đã gỡ hiệu lực"),
        new(RewardDisciplineConstants.DisciplineStatuses.Cancelled, "Đã hủy")
    ];

    private static readonly RewardDisciplineOptionDto[] DisciplineActions =
    [
        new(RewardDisciplineConstants.DisciplineActions.Reminder, "Nhắc nhở"),
        new(RewardDisciplineConstants.DisciplineActions.Reprimand, "Khiển trách"),
        new(RewardDisciplineConstants.DisciplineActions.Warning, "Cảnh cáo"),
        new(RewardDisciplineConstants.DisciplineActions.Suspension, "Đình chỉ"),
        new(RewardDisciplineConstants.DisciplineActions.Other, "Khác")
    ];
}
