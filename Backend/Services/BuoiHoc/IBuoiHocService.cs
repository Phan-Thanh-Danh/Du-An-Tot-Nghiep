using Backend.DTOs.BuoiHoc;
using Backend.DTOs.Common;

namespace Backend.Services.BuoiHoc;

public interface IBuoiHocService
{
    Task<PagedResultDto<BuoiHocDto>> GetAsync(
        BuoiHocQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<BuoiHocDetailDto> GetByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<GenerateSessionsResultDto> GenerateSessionsAsync(
        int scheduleId,
        CancellationToken cancellationToken = default);
}
