using Backend.DTOs.Common;
using Backend.DTOs.TrainingProgramTerms;

namespace Backend.Services.AcademicTerms;

public interface ITrainingProgramTermService
{
    Task<List<TrainingProgramTermDto>> GetByProgramAsync(int programId, CancellationToken cancellationToken = default);
    Task<TrainingProgramTermDto> CreateAsync(CreateTrainingProgramTermRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
