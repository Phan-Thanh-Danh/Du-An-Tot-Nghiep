using Backend.DTOs.Common;
using Backend.DTOs.TrainingProgramSubjects;

namespace Backend.Services.TrainingProgramSubjects;

public interface ITrainingProgramSubjectService
{
    Task<PagedResultDto<TrainingProgramSubjectDto>> GetAsync(
        TrainingProgramSubjectQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<List<TrainingProgramSubjectDto>> GetByProgramAsync(
        int programId,
        CancellationToken cancellationToken = default);

    Task<TrainingProgramSubjectDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TrainingProgramSubjectDto> CreateAsync(
        CreateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<TrainingProgramSubjectDto> UpdateAsync(
        int id,
        UpdateTrainingProgramSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
