namespace Backend.Services.Grading;

public interface IGradeAggregationService
{
    Task CalculateGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
    Task CalculateFallbackGradeAsync(int studentId, int subjectId, int termId, decimal? manualAssignmentGrade, decimal? manualExamGrade, CancellationToken ct = default);
}
