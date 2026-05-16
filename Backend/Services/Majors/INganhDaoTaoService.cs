using Backend.DTOs.Common;
using Backend.DTOs.Majors;

namespace Backend.Services.Majors;

public interface INganhDaoTaoService
{
    Task<PagedResultDto<MajorDto>> GetMajorsAsync(MajorQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<MajorDto> GetByIdAsync(int majorId, CancellationToken cancellationToken = default);
    Task<MajorDto> CreateAsync(CreateMajorRequest request, CancellationToken cancellationToken = default);
    Task<MajorDto> UpdateAsync(int majorId, UpdateMajorRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int majorId, CancellationToken cancellationToken = default);
    Task<MajorDto> ActivateAsync(int majorId, CancellationToken cancellationToken = default);
    Task<MajorDto> DeactivateAsync(int majorId, CancellationToken cancellationToken = default);
}
