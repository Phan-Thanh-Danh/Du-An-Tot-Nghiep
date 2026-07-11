using Backend.DTOs.Courses.AssignmentSuggestions;

namespace Backend.Services.Courses;

public interface ICourseAssignmentSuggestionService
{
    Task<CourseAssignmentSuggestionResultDto> GetSuggestionsAsync(
        CourseAssignmentSuggestionRequestDto request,
        int campusId,
        CancellationToken cancellationToken = default);
}
