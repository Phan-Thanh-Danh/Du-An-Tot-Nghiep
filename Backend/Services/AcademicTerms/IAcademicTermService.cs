using Backend.DTOs.AcademicTerms;
using Backend.DTOs.Common;

namespace Backend.Services.AcademicTerms;

public interface IAcademicTermService
{
    Task<PagedResultDto<AcademicTermDto>> GetTermsAsync(AcademicTermQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> GetByIdAsync(int termId, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> CreateAsync(CreateAcademicTermRequest request, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> UpdateAsync(int termId, UpdateAcademicTermRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int termId, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> LockAsync(int termId, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> UnlockAsync(int termId, CancellationToken cancellationToken = default);
}
