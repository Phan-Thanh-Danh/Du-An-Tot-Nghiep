using Backend.DTOs.LopHanhChinhs;

namespace Backend.Services.LopHanhChinhs;

public interface ILopHanhChinhService
{
    Task<IEnumerable<LopHanhChinhDto>> GetByChuyenNganhAsync(
        int maChuyenNganh, 
        bool conHoatDong = true, 
        CancellationToken cancellationToken = default);
}
