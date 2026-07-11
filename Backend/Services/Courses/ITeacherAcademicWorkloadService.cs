using Backend.DTOs.Courses.AssignmentSuggestions;

namespace Backend.Services.Courses;

public interface ITeacherAcademicWorkloadService
{
    Task<TeacherWorkloadDto> GetWorkloadAsync(
        int teacherId,
        int campusId,
        int termId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, TeacherWorkloadDto>> GetWorkloadsAsync(
        IEnumerable<int> teacherIds,
        int campusId,
        int termId,
        CancellationToken cancellationToken = default);
}
