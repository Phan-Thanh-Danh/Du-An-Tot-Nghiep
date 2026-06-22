using Backend.DTOs.QuizAttempts;
using Backend.Models;

namespace Backend.Services.QuizGrading;

public interface IQuizGradingService
{
    QuizGradingResultDto GradeObjectiveQuestions(
        IReadOnlyList<CauHoiDeKiemTra> questions,
        IReadOnlyList<QuizAttemptAnswerDto> answers,
        bool includeAnswerKeys);

    IReadOnlyList<QuizAttemptAnswerDto> ParseAnswersJson(string? answersJson);
}
