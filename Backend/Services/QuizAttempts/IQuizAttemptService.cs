using Backend.DTOs.QuizAttempts;

namespace Backend.Services.QuizAttempts;

public interface IQuizAttemptService
{
    Task<QuizAvailabilityDto> GetAvailabilityAsync(int quizId, int studentId, CancellationToken ct);
    Task<StartQuizAttemptResponse> StartAsync(int quizId, int studentId, CancellationToken ct);
    Task SaveAnswersAsync(int attemptId, SaveQuizAnswersRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptResultDto> SubmitAsync(int attemptId, SubmitQuizAttemptRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptHistoryDto> GetHistoryAsync(int quizId, int studentId, CancellationToken ct);

    Task<QuizAvailabilityDto> GetProgressTestAvailabilityAsync(int quizId, int studentId, CancellationToken ct);
    Task<StartQuizAttemptResponse> StartProgressTestAsync(int quizId, int studentId, CancellationToken ct);
    Task SaveProgressTestAnswersAsync(int attemptId, SaveQuizAnswersRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptResultDto> SubmitProgressTestAsync(int attemptId, SubmitQuizAttemptRequest request, int studentId, CancellationToken ct);
    Task<QuizAttemptHistoryDto> GetProgressTestHistoryAsync(int quizId, int studentId, CancellationToken ct);
}
