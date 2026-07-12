using Backend.Data;
using Backend.DTOs.LopHanhChinhs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.LopHanhChinhs;

public class LopHanhChinhService : ILopHanhChinhService
{
    private readonly ApplicationDbContext _context;

    public LopHanhChinhService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LopHanhChinhDto>> GetByChuyenNganhAsync(
        int maChuyenNganh,
        bool conHoatDong = true,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context
            .LopHanhChinhs.Include(l => l.ChuongTrinh)
            .Where(l => l.ChuongTrinh != null && l.ChuongTrinh.MaChuyenNganh == maChuyenNganh);

        if (conHoatDong)
        {
            query = query.Where(l => l.ConHoatDong);
        }

        var lopHanhChinhs = await query
            .OrderByDescending(l => l.NamNhapHoc)
            .ThenBy(l => l.MaCodeLop)
            .Select(l => new LopHanhChinhDto
            {
                MaLop = l.MaLop,
                MaCodeLop = l.MaCodeLop,
                TenLop = l.TenLop,
                NamNhapHoc = l.NamNhapHoc,
            })
            .ToListAsync(cancellationToken);

        return lopHanhChinhs;
    }
}
