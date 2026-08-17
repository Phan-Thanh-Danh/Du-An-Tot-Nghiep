using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.TeacherPersonnel;

namespace Backend.Services.TeacherPersonnel;

public interface ITeacherPersonnelService
{
    Task<PagedResultDto<TeacherPersonnelListDto>> GetTeachersAsync(
        CurrentUserContext currentUser,
        TeacherPersonnelQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<TeacherPersonnelDetailDto> GetTeacherDetailAsync(
        CurrentUserContext currentUser,
        int teacherId,
        CancellationToken cancellationToken = default);

    Task<TeacherWorkloadSummaryDto> GetTeacherWorkloadAsync(
        CurrentUserContext currentUser,
        int teacherId,
        int? semesterId,
        CancellationToken cancellationToken = default);

    Task<TeacherSessionLogsSummaryDto> GetTeacherSessionLogsAsync(
        CurrentUserContext currentUser,
        int teacherId,
        int? semesterId,
        CancellationToken cancellationToken = default);

    Task<TeacherEvaluationSummaryDto> GetTeacherEvaluationsAsync(
        CurrentUserContext currentUser,
        int teacherId,
        CancellationToken cancellationToken = default);

    Task<TeacherPersonnelDetailDto> CreateTeacherAsync(
        CurrentUserContext currentUser,
        CreateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TeacherPersonnelDetailDto> UpdateTeacherAsync(
        CurrentUserContext currentUser,
        int teacherId,
        UpdateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleLockTeacherAsync(
        CurrentUserContext currentUser,
        int teacherId,
        ToggleTeacherLockRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<OrganizationHierarchyNodeDto>> GetHierarchyTreeAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);
}
