using System.Threading;
using System.Threading.Tasks;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;

namespace Backend.Services.AI;

public interface IBghAiAnalyticsService
{
    Task<GpaAnalyticsContextDto> GetGpaAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<AtRiskAnalyticsContextDto> GetAtRiskAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<PassFailAnalyticsContextDto> GetPassFailAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<TeacherEvaluationContextDto> GetTeacherEvaluationContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default);

    Task<AwardsAnalyticsContextDto> GetAwardsAnalyticsContextAsync(
        int campusId,
        int? semesterId,
        CancellationToken cancellationToken = default);

    Task<FacilitiesAnalyticsContextDto> GetFacilitiesAnalyticsContextAsync(
        int campusId,
        CancellationToken cancellationToken = default);

    Task<AiCertificateTemplateEditResponse> EditCertificateTemplateWithAiAsync(
        AiCertificateTemplateEditRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<BghAiReportResponse> GenerateBghAiReportAsync(
        BghAiReportRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);
}
