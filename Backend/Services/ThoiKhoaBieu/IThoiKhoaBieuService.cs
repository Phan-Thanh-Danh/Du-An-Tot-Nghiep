using Backend.DTOs.Common;
using Backend.DTOs.ThoiKhoaBieu;

namespace Backend.Services.ThoiKhoaBieu;

public interface IThoiKhoaBieuService
{
    Task<PagedResultDto<ThoiKhoaBieuDto>> GetAsync(
        ThoiKhoaBieuQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ThoiKhoaBieuDetailDto> GetByIdAsync(int scheduleId, CancellationToken cancellationToken = default);

    Task<ThoiKhoaBieuDetailDto> CreateAsync(
        CreateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken = default);

    Task<ThoiKhoaBieuDetailDto> UpdateAsync(
        int scheduleId,
        UpdateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int scheduleId,
        CancellationToken cancellationToken = default);

    Task<TienDoBuoiHocDto> GetTienDoBuoiHocAsync(
        int courseId, 
        CancellationToken cancellationToken = default);

    Task<ThoiKhoaBieuDetailDto> CancelAsync(int scheduleId, CancellationToken cancellationToken = default);
}
