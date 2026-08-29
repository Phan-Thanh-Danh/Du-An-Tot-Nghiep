using Backend.DTOs.Courses.AssignmentSuggestions;

namespace Backend.Services.Courses;

public interface ICourseTeacherEligibilityService
{
    Task<TeacherEligibilityResultDto> ValidateTeacherForSubjectAsync(
        int campusId,
        int termId,
        int subjectId,
        int teacherId,
        int? excludeCourseId = null,
        int? targetStartBlockId = null,
        int? targetSoBlockHoc = null,
        CancellationToken cancellationToken = default);
}
