using Backend.DTOs.Common;
using Backend.DTOs.Subjects;

namespace Backend.Services.Subjects;

public interface ISubjectService
{
    Task<PagedResultDto<SubjectDto>> GetSubjectsAsync(SubjectQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<SubjectDto> GetByIdAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);
    Task<SubjectDto> UpdateAsync(int subjectId, UpdateSubjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<SubjectDto> ActivateAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<SubjectDto> DeactivateAsync(int subjectId, CancellationToken cancellationToken = default);
}
