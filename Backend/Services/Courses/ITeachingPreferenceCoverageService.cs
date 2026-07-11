using Backend.DTOs.TeachingPreferences;
using Backend.DTOs.Courses.AssignmentSuggestions;

namespace Backend.Services.Courses;

public interface ITeachingPreferenceCoverageService
{
    Task<PreferenceCoverageDto> EvaluateCoverageAsync(
        int teacherId,
        int campusId,
        int termId,
        List<PlannedTeachingSlotDto> plannedSlots,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, PreferenceCoverageDto>> EvaluateCoveragesAsync(
        IEnumerable<int> teacherIds,
        int campusId,
        int termId,
        List<PlannedTeachingSlotDto> plannedSlots,
        CancellationToken cancellationToken = default);
}
