using Backend.DTOs.AttendancePolicy;

namespace Backend.Services.AttendancePolicy;

public interface IAttendancePolicyService
{
    Task<QuyDinhChuyenCanDto> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuyDinhChuyenCanDto>> GetHistoryAsync(CancellationToken cancellationToken = default);
    Task<QuyDinhChuyenCanDto> UpdateAsync(UpdateQuyDinhChuyenCanRequest request, CancellationToken cancellationToken = default);
}
