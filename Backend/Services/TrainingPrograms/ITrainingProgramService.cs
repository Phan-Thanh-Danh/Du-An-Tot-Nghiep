using Backend.DTOs.Common;
using Backend.DTOs.TrainingPrograms;

namespace Backend.Services.TrainingPrograms;

public interface ITrainingProgramService
{
    Task<PagedResultDto<TrainingProgramDto>> GetAsync(TrainingProgramQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> CreateAsync(CreateTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> CloneAsync(int sourceProgramId, CloneTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> UpdateAsync(int id, UpdateTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> SubmitAsync(int id, SubmitTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> ApproveAsync(int id, ApproveTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> RejectAsync(int id, RejectTrainingProgramRequest request, CancellationToken cancellationToken = default);
    Task<TrainingProgramDto> ArchiveAsync(int id, CancellationToken cancellationToken = default);
}
