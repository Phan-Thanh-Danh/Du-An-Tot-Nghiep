using Backend.DTOs.QuizAttempts;
using Backend.Models;

namespace Backend.Services.QuizRuntime;

public interface IQuizAvailabilityService
{
    Task<DeKiemTra> SynchronizeQuizStatusAsync(int quizId, DateTime utcNow, CancellationToken ct);
    Task<int> SynchronizeScheduledQuizzesAsync(DateTime utcNow, CancellationToken ct);
    Task<QuizAvailabilityDto> GetAvailabilityAsync(int quizId, int studentId, CancellationToken ct);
    string ResolvePublishedStatus(DeKiemTra quiz, DateTime utcNow);
}
