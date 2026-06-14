using Backend.DTOs.AttendanceAutomation;

namespace Backend.Services.AttendanceAutomation;

public interface IAttendanceAutomationService
{
    Task<AttendanceAutomationRunResultDto> ProcessDueAttendanceAsync(
        DateTime? now = null,
        CancellationToken cancellationToken = default);
}
