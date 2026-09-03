using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.LopHanhChinhs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.LopHanhChinhs;

public class LopHanhChinhService : ILopHanhChinhService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LopHanhChinhService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<LopHanhChinhDto>> GetByChuyenNganhAsync(
        int maChuyenNganh,
        bool conHoatDong = true,
        CancellationToken cancellationToken = default
    )
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;

        var query = _context
            .LopHanhChinhs.Include(l => l.ChuongTrinh)
            .Where(l => l.ChuongTrinh != null && l.ChuongTrinh.MaChuyenNganh == maChuyenNganh);

        if (currentUser != null && currentUser.Role != AuthRoles.SuperAdmin && currentUser.CampusId > 0)
        {
            query = query.Where(l => l.MaDonVi == currentUser.CampusId);
        }

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
