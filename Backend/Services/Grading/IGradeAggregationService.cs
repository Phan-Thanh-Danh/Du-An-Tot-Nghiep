using Backend.Models;

namespace Backend.Services.Grading;

public interface IGradeAggregationService
{
    Task CalculateGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
    Task CalculateFallbackGradeAsync(int studentId, int subjectId, int termId, decimal? manualAssignmentGrade, decimal? manualExamGrade, CancellationToken ct = default);

    // Grade calculation methods — return null when no source data exists, used for both computation and display
    Task<decimal?> CalculateAttendanceGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default);
    Task<decimal?> CalculateAssignmentGradeAsync(int studentId, int subjectId, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default);
    Task<decimal?> CalculateQuizGradeAsync(int studentId, int subjectId, int termId, string loaiCode, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default);
}
