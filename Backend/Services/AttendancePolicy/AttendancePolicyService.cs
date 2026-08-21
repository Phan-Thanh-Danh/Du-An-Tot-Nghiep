using Backend.Data;
using Backend.DTOs.AttendancePolicy;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.AttendancePolicy;

public class AttendancePolicyService : IAttendancePolicyService
{
    private const int DefaultQuyVangToiDa = 4;
    private const decimal DefaultTiLeCanhBao = 50m;
    private const decimal DefaultHeSoVangKhongPhep = 1m;
    private const decimal DefaultHeSoVangCoPhep = 0m;
    private const decimal DefaultHeSoDiMuon = 0.5m;
    private const int DefaultHanGuiPhut = 15;
    private const int DefaultHanChinhSuaPhut = 10;

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public AttendancePolicyService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<QuyDinhChuyenCanDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var policyQuery = _context.QuyDinhChuyenCans
            .AsNoTracking()
            .Include(x => x.NguoiTaoNavigation)
            .AsQueryable();
        if (currentUser.Role != "SuperAdmin")
        {
            policyQuery = policyQuery.Where(x => x.MaDonVi == currentUser.CampusId);
        }

        var policy = await policyQuery
            .OrderByDescending(x => x.NgayHieuLuc)
            .ThenByDescending(x => x.MaQuyDinh)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy is null)
        {
            return ToDefaultDto(currentUser);
        }

        return ToDto(policy);
    }

    public async Task<IReadOnlyList<QuyDinhChuyenCanDto>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var historyQuery = _context.QuyDinhChuyenCans
            .AsNoTracking()
            .Include(x => x.NguoiTaoNavigation)
            .AsQueryable();
        if (currentUser.Role != "SuperAdmin")
        {
            historyQuery = historyQuery.Where(x => x.MaDonVi == currentUser.CampusId);
        }

        return await historyQuery
            .OrderByDescending(x => x.NgayHieuLuc)
            .ThenByDescending(x => x.MaQuyDinh)
            .Select(x => new QuyDinhChuyenCanDto
            {
                MaQuyDinh = x.MaQuyDinh,
                MaDonVi = x.MaDonVi,
                NgayHieuLuc = x.NgayHieuLuc,
                QuyVangToiDa = x.QuyVangToiDa,
                TiLeCanhBao = x.TiLeCanhBao,
                HeSoVangKhongPhep = x.HeSoVangKhongPhep,
                HeSoVangCoPhep = x.HeSoVangCoPhep,
                HeSoDiMuon = x.HeSoDiMuon,
                HanGuiPhut = x.HanGuiPhut,
                HanChinhSuaPhut = x.HanChinhSuaPhut,
                GhiChu = x.GhiChu,
                NguoiTao = x.NguoiTao,
                TenNguoiTao = x.NguoiTaoNavigation != null ? x.NguoiTaoNavigation.HoTen : null,
                TaoLuc = x.TaoLuc,
                NguoiCapNhat = x.NguoiCapNhat,
                CapNhatLuc = x.CapNhatLuc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QuyDinhChuyenCanDto> UpdateAsync(UpdateQuyDinhChuyenCanRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var now = DateTime.UtcNow;

        var existingQuery = _context.QuyDinhChuyenCans.AsQueryable();
        if (currentUser.Role != "SuperAdmin")
        {
            existingQuery = existingQuery.Where(x => x.MaDonVi == currentUser.CampusId);
        }

        var existing = await existingQuery
            .OrderByDescending(x => x.NgayHieuLuc)
            .ThenByDescending(x => x.MaQuyDinh)
            .FirstOrDefaultAsync(cancellationToken);

        var oldSnapshot = existing is null
            ? ToDefaultDto(currentUser)
            : ToDto(existing);

        var entity = new Models.QuyDinhChuyenCan
        {
            MaDonVi = currentUser.CampusId,
            NgayHieuLuc = now,
            QuyVangToiDa = request.QuyVangToiDa,
            TiLeCanhBao = request.TiLeCanhBao,
            HeSoVangKhongPhep = request.HeSoVangKhongPhep,
            HeSoVangCoPhep = request.HeSoVangCoPhep,
            HeSoDiMuon = request.HeSoDiMuon,
            HanGuiPhut = request.HanGuiPhut,
            HanChinhSuaPhut = request.HanChinhSuaPhut,
            GhiChu = request.GhiChu,
            NguoiTao = currentUser.UserId,
            TaoLuc = now
        };

        _context.QuyDinhChuyenCans.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        entity.NguoiCapNhat = currentUser.UserId;
        entity.CapNhatLuc = now;
        await _context.SaveChangesAsync(cancellationToken);

        var newDto = ToDto(entity);

        await _auditLogService.LogAsync(
            "QuyDinhChuyenCan",
            entity.MaQuyDinh.ToString(),
            "UPDATE_POLICY",
            oldSnapshot,
            newDto,
            currentUser.UserId,
            currentUser.CampusId,
            "Cập nhật chính sách điểm danh (quỹ vắng & chuyên cần).",
            cancellationToken);

        return newDto;
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }

    private static QuyDinhChuyenCanDto ToDefaultDto(CurrentUserContext currentUser)
    {
        return new QuyDinhChuyenCanDto
        {
            MaQuyDinh = 0,
            MaDonVi = currentUser.CampusId,
            NgayHieuLuc = DateTime.UtcNow,
            QuyVangToiDa = DefaultQuyVangToiDa,
            TiLeCanhBao = DefaultTiLeCanhBao,
            HeSoVangKhongPhep = DefaultHeSoVangKhongPhep,
            HeSoVangCoPhep = DefaultHeSoVangCoPhep,
            HeSoDiMuon = DefaultHeSoDiMuon,
            HanGuiPhut = DefaultHanGuiPhut,
            HanChinhSuaPhut = DefaultHanChinhSuaPhut,
            NguoiTao = currentUser.UserId,
            TaoLuc = DateTime.UtcNow
        };
    }

    private static QuyDinhChuyenCanDto ToDto(Models.QuyDinhChuyenCan entity)
    {
        return new QuyDinhChuyenCanDto
        {
            MaQuyDinh = entity.MaQuyDinh,
            MaDonVi = entity.MaDonVi,
            NgayHieuLuc = entity.NgayHieuLuc,
            QuyVangToiDa = entity.QuyVangToiDa,
            TiLeCanhBao = entity.TiLeCanhBao,
            HeSoVangKhongPhep = entity.HeSoVangKhongPhep,
            HeSoVangCoPhep = entity.HeSoVangCoPhep,
            HeSoDiMuon = entity.HeSoDiMuon,
            HanGuiPhut = entity.HanGuiPhut,
            HanChinhSuaPhut = entity.HanChinhSuaPhut,
            GhiChu = entity.GhiChu,
            NguoiTao = entity.NguoiTao,
            TenNguoiTao = entity.NguoiTaoNavigation?.HoTen,
            TaoLuc = entity.TaoLuc,
            NguoiCapNhat = entity.NguoiCapNhat,
            CapNhatLuc = entity.CapNhatLuc
        };
    }
}
