using Backend.DTOs.CaHoc;
using Backend.DTOs.Common;

namespace Backend.Services.CaHoc;

public interface ICaHocService
{
    Task<PagedResultDto<CaHocDto>> GetAsync(CaHocQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CaHocDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CaHocDto> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<CaHocDto> CreateAsync(CreateCaHocRequest request, CancellationToken cancellationToken = default);
    Task<CaHocDto> UpdateAsync(int shiftId, UpdateCaHocRequest request, CancellationToken cancellationToken = default);
    Task<CaHocDto> ToggleActiveAsync(int shiftId, CancellationToken cancellationToken = default);
}
