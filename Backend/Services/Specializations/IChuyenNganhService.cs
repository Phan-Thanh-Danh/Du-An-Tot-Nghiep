using Backend.DTOs.Common;
using Backend.DTOs.Specializations;

namespace Backend.Services.Specializations;

public interface IChuyenNganhService
{
    Task<PagedResultDto<SpecializationDto>> GetSpecializationsAsync(SpecializationQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<SpecializationDto> GetByIdAsync(int specializationId, CancellationToken cancellationToken = default);
    Task<SpecializationDto> CreateAsync(CreateSpecializationRequest request, CancellationToken cancellationToken = default);
    Task<SpecializationDto> UpdateAsync(int specializationId, UpdateSpecializationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int specializationId, CancellationToken cancellationToken = default);
    Task<SpecializationDto> ActivateAsync(int specializationId, CancellationToken cancellationToken = default);
    Task<SpecializationDto> DeactivateAsync(int specializationId, CancellationToken cancellationToken = default);
}
