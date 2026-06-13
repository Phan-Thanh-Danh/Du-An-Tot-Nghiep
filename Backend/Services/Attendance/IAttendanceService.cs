using Backend.DTOs.Attendance;
using Backend.DTOs.Common;

namespace Backend.Services.Attendance;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceSessionDto>> GetTeacherTodayAsync(CancellationToken cancellationToken = default);

    Task<AttendanceDetailDto> StartAsync(int sessionId, CancellationToken cancellationToken = default);

    Task<AttendanceDetailDto> GetSessionAttendanceAsync(int sessionId, CancellationToken cancellationToken = default);

    Task<AttendanceStudentDto> UpdateStudentAsync(
        int sessionId,
        int studentId,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<AttendanceDetailDto> BulkUpdateAsync(
        int sessionId,
        BulkUpdateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<AttendanceDetailDto> SubmitAsync(int sessionId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<StudentAttendanceDto>> GetStudentAttendanceAsync(
        StudentAttendanceQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
