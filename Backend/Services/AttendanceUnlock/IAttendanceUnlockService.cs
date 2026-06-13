using Backend.DTOs.AttendanceUnlock;
using Backend.DTOs.Common;

namespace Backend.Services.AttendanceUnlock;

public interface IAttendanceUnlockService
{
    Task<AttendanceUnlockRequestDto> CreateAsync(
        int sessionId,
        CreateAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AttendanceUnlockRequestDto>> GetTeacherRequestsAsync(
        AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AttendanceUnlockRequestDto>> GetAdminRequestsAsync(
        AttendanceUnlockQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AttendanceUnlockRequestDto> GetAdminRequestByIdAsync(
        int requestId,
        CancellationToken cancellationToken = default);

    Task<AttendanceUnlockRequestDto> ApproveAsync(
        int requestId,
        ApproveAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default);

    Task<AttendanceUnlockRequestDto> RejectAsync(
        int requestId,
        RejectAttendanceUnlockRequest request,
        CancellationToken cancellationToken = default);
}
