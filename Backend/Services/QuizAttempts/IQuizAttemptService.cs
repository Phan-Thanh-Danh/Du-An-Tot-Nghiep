using Backend.DTOs.QuizAttempts;

namespace Backend.Services.QuizAttempts;

public interface IQuizAttemptService
{
    Task<QuizAvailabilityDto> GetAvailabilityAsync(int quizId, int studentId, CancellationToken ct);
    Task<StartQuizAttemptResponse> StartAsync(int quizId, int studentId, CancellationToken ct);
    Task SaveAnswersAsync(int attemptId, SaveQuizAnswersRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptResultDto> SubmitAsync(int attemptId, SubmitQuizAttemptRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptHistoryDto> GetHistoryAsync(int quizId, int studentId, CancellationToken ct);
}
