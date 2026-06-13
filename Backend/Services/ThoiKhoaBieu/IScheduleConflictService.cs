using Backend.DTOs.ThoiKhoaBieu;

namespace Backend.Services.ThoiKhoaBieu;

public interface IScheduleConflictService
{
    Task<ScheduleConflictResultDto> CheckConflictsAsync(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken = default);

    Task EnsureNoConflictAsync(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken = default);
}
