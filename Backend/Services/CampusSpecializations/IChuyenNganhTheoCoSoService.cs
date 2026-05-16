using Backend.DTOs.CampusSpecializations;
using Backend.DTOs.Common;

namespace Backend.Services.CampusSpecializations;

public interface IChuyenNganhTheoCoSoService
{
    Task<PagedResultDto<CampusSpecializationDto>> GetCampusSpecializationsAsync(CampusSpecializationQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> GetByIdAsync(int campusSpecializationId, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> CreateAsync(CreateCampusSpecializationRequest request, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> UpdateAsync(int campusSpecializationId, UpdateCampusSpecializationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int campusSpecializationId, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> ActivateAsync(int campusSpecializationId, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> DeactivateAsync(int campusSpecializationId, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> ApproveAsync(int campusSpecializationId, CancellationToken cancellationToken = default);
    Task<CampusSpecializationDto> RejectAsync(int campusSpecializationId, RejectCampusSpecializationRequest request, CancellationToken cancellationToken = default);
}
