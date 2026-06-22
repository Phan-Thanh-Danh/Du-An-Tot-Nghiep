using Backend.DTOs.Common;
using Backend.DTOs.QuizManagement;

namespace Backend.Services.QuizManagement;

public interface IQuizManagementService
{
    Task<PagedResultDto<QuizDto>> GetQuizzesAsync(QuizFilterDto filter, CancellationToken ct);
    Task<QuizDetailDto> GetQuizByIdAsync(int id, CancellationToken ct);
    Task<QuizDetailDto> CreateQuizAsync(CreateQuizRequest request, int userId, CancellationToken ct);
    Task<QuizDetailDto> UpdateQuizAsync(int id, UpdateQuizRequest request, int userId, CancellationToken ct);
    Task DeleteQuizAsync(int id, int userId, CancellationToken ct);
    
    Task<IReadOnlyList<QuizQuestionDto>> GetQuizQuestionsAsync(int quizId, CancellationToken ct);
    Task<IReadOnlyList<QuizQuestionDto>> AssignQuestionsAsync(int quizId, AssignQuizQuestionsRequest request, int userId, CancellationToken ct);
    Task<IReadOnlyList<QuizQuestionDto>> ReplaceQuestionsAsync(int quizId, AssignQuizQuestionsRequest request, int userId, CancellationToken ct);
    Task<QuizQuestionDto> UpdateQuestionAsync(int quizId, int questionId, UpdateQuizQuestionRequest request, int userId, CancellationToken ct);
    Task RemoveQuestionAsync(int quizId, int questionId, int userId, CancellationToken ct);
    Task ReorderQuestionsAsync(int quizId, ReorderQuizQuestionsRequest request, int userId, CancellationToken ct);
    
    Task PublishQuizAsync(int id, int userId, CancellationToken ct);
    Task UnpublishQuizAsync(int id, int userId, CancellationToken ct);
    Task OpenQuizAsync(int id, int userId, CancellationToken ct);
    Task CloseQuizAsync(int id, int userId, CancellationToken ct);
}
