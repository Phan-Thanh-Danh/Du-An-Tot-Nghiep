using Backend.DTOs.Applications;
using Backend.DTOs.Common;

namespace Backend.Services.Applications;

public interface IStudentApplicationService
{
    Task<StudentApplicationDetailDto> CreateAsync(CreateStudentApplicationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResultDto<StudentApplicationListItemDto>> GetOwnAsync(StudentApplicationQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<StudentApplicationDetailDto> GetOwnDetailAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<StudentApplicationDetailDto> UpdateAsync(int applicationId, UpdateStudentApplicationRequest request, CancellationToken cancellationToken = default);
    Task<StudentApplicationDetailDto> SubmitAsync(int applicationId, SubmitStudentApplicationRequest request, CancellationToken cancellationToken = default);
    Task<StudentApplicationDetailDto> ResubmitAsync(int applicationId, ResubmitStudentApplicationRequest request, CancellationToken cancellationToken = default);
    Task<StudentApplicationDetailDto> CancelAsync(int applicationId, CancelStudentApplicationRequest request, CancellationToken cancellationToken = default);
}
