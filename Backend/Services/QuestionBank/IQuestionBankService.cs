using Backend.DTOs.Common;
using Backend.DTOs.QuestionBank;
using Microsoft.AspNetCore.Http;

namespace Backend.Services.QuestionBank;

public interface IQuestionBankService
{
    Task<PagedResultDto<QuestionDto>> GetQuestionsAsync(QuestionFilterDto filter, CancellationToken cancellationToken = default);
    Task<QuestionDto> GetQuestionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto input, CancellationToken cancellationToken = default);
    Task<QuestionDto> UpdateQuestionAsync(int id, UpdateQuestionDto input, CancellationToken cancellationToken = default);
    Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default);
    Task ActivateQuestionAsync(int id, CancellationToken cancellationToken = default);
    Task DeactivateQuestionAsync(int id, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateImportTemplateAsync(CancellationToken cancellationToken = default);
    Task<int> ImportQuestionsAsync(IFormFile file, CancellationToken cancellationToken = default);
}
