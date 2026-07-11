using Backend.DTOs.QuyDoiTinChis;

namespace Backend.Services.QuyDoiTinChis;

public interface IQuyDoiTinChiService
{
    Task<IReadOnlyList<QuyDoiTinChiDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<QuyDoiTinChiDto> CreateAsync(CreateQuyDoiTinChiRequest request, CancellationToken cancellationToken = default);
    Task<QuyDoiTinChiDto> UpdateAsync(int id, UpdateQuyDoiTinChiRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
