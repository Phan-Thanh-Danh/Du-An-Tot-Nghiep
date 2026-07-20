using Backend.Models;

namespace Backend.Services.Grading;

public interface IGradeAggregationService
{
    Task CalculateGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
    Task CalculateFallbackGradeAsync(int studentId, int subjectId, int termId, decimal? manualAssignmentGrade, decimal? manualExamGrade, CancellationToken ct = default);

    // Read-only methods for display (Phase 3) — return null when no source data exists
    Task<decimal?> GetAttendanceGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
    Task<decimal?> GetAssignmentTypeGradeAsync(int studentId, int subjectId, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default);
    Task<decimal?> GetQuizTypeGradeAsync(int studentId, int subjectId, int termId, string loaiCode, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default);
}
