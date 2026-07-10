using Backend.DTOs.AcademicSchedulingContext;

namespace Backend.Services.AcademicSchedulingContext;

public interface IAcademicSchedulingContextService
{
    Task<AcademicSchedulingContextDto> GetContextAsync(int campusId, CancellationToken cancellationToken = default);
    Task ValidateSchedulableTermAsync(int campusId, int requestedTermId, CancellationToken cancellationToken = default);
}
