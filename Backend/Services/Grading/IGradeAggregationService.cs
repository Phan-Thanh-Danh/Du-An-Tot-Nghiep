namespace Backend.Services.Grading;

public interface IGradeAggregationService
{
    Task CalculateGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
}
