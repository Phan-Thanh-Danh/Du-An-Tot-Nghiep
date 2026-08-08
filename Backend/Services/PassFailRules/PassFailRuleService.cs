using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.PassFailRules;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.PassFailRules;

public class PassFailRuleService : IPassFailRuleService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public PassFailRuleService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PassFailRuleListResponse> GetListAsync(
        int? maHocKy,
        string? search,
        int? maNganh,
        int? maChuyenNganh,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var termQuery = _context.HocKys.AsNoTracking();
        if (maHocKy.HasValue)
        {
            termQuery = termQuery.Where(t => t.MaHocKy == maHocKy.Value);
        }

        var termIds = await termQuery.Select(t => t.MaHocKy).ToListAsync(ct);

        var subjectQuery = _context.DanhMucMonHocs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            subjectQuery = subjectQuery.Where(m =>
                (m.TenMonHoc != null && m.TenMonHoc.Contains(keyword)) ||
                (m.MaCodeMonHoc != null && m.MaCodeMonHoc.Contains(keyword)));
        }

        if (maNganh.HasValue)
        {
            subjectQuery = subjectQuery.Where(m => m.MaNganh == maNganh.Value);
        }

        if (maChuyenNganh.HasValue)
        {
            subjectQuery = subjectQuery.Where(m => m.MaChuyenNganh == maChuyenNganh.Value);
        }

        var subjects = await subjectQuery
            .Select(m => new { m.MaMonHoc, m.MaCodeMonHoc, m.TenMonHoc, m.MaNganh, m.MaChuyenNganh })
            .ToListAsync(ct);

        var majorIds = subjects.Select(s => s.MaNganh).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var specializationIds = subjects.Select(s => s.MaChuyenNganh).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        var majors = await _context.NganhDaoTaos
            .AsNoTracking()
            .Where(n => majorIds.Contains(n.MaNganh))
            .ToDictionaryAsync(n => n.MaNganh, n => n.TenNganh, ct);
        var specializations = await _context.ChuyenNganhs
            .AsNoTracking()
            .Where(c => specializationIds.Contains(c.MaChuyenNganh))
            .ToDictionaryAsync(c => c.MaChuyenNganh, c => c.TenChuyenNganh, ct);

        var configs = await _context.CauHinhDiemMonHocs
            .AsNoTracking()
            .Include(x => x.MonHoc)
            .Include(x => x.HocKy)
            .Include(x => x.NguoiCapNhatNavigation)
            .Where(x => termIds.Contains(x.MaHocKy))
            .ToListAsync(ct);

        var items = subjects
            .Select(subject =>
            {
                var config = configs.FirstOrDefault(c => c.MaMonHoc == subject.MaMonHoc);
                var term = config?.HocKy;
                return new PassFailRuleDto
                {
                    MaCauHinhDiem = config?.MaCauHinhDiem ?? 0,
                    MaMonHoc = subject.MaMonHoc,
                    MaCodeMonHoc = subject.MaCodeMonHoc,
                    TenMonHoc = subject.TenMonHoc,
                    MaNganh = subject.MaNganh,
                    TenNganh = subject.MaNganh.HasValue && majors.TryGetValue(subject.MaNganh.Value, out var tenNganh) ? tenNganh : null,
                    MaChuyenNganh = subject.MaChuyenNganh,
                    TenChuyenNganh = subject.MaChuyenNganh.HasValue && specializations.TryGetValue(subject.MaChuyenNganh.Value, out var tenChuyenNganh) ? tenChuyenNganh : null,
                    MaHocKy = term?.MaHocKy ?? maHocKy ?? 0,
                    TenHocKy = term?.TenHocKy,
                    TrongSoQuaTrinh = config?.TrongSoQuaTrinh ?? 0m,
                    TrongSoGiuaKy = config?.TrongSoGiuaKy ?? 0m,
                    TrongSoCuoiKy = config?.TrongSoCuoiKy ?? 0m,
                    NguongDat = config?.NguongDat ?? 0m,
                    TiLeChuyenCanToiThieu = config?.TiLeChuyenCanToiThieu ?? 0m,
                    NguoiCapNhat = config?.NguoiCapNhat,
                    TenNguoiCapNhat = config?.NguoiCapNhatNavigation?.HoTen,
                    CapNhatLuc = config?.CapNhatLuc
                };
            })
            .ToList();

        var filtered = items.Where(x => x.MaCauHinhDiem > 0 || maHocKy.HasValue).ToList();
        if (!maHocKy.HasValue)
        {
            filtered = items.Where(x => x.MaCauHinhDiem > 0).ToList();
        }

        var response = new PassFailRuleListResponse
        {
            TongMonHoc = items.Count,
            DaCauHinh = items.Count(x => x.MaCauHinhDiem > 0),
            ChuaCauHinh = items.Count(x => x.MaCauHinhDiem == 0),
            Items = filtered
                .OrderByDescending(x => x.MaCauHinhDiem)
                .ThenBy(x => x.TenMonHoc)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList()
        };

        return response;
    }

    public async Task<PassFailRuleDto?> GetAsync(int maCauHinhDiem, CancellationToken ct = default)
    {
        var config = await _context.CauHinhDiemMonHocs
            .AsNoTracking()
            .Include(x => x.MonHoc)
            .Include(x => x.HocKy)
            .Include(x => x.NguoiCapNhatNavigation)
            .FirstOrDefaultAsync(x => x.MaCauHinhDiem == maCauHinhDiem, ct);

        if (config is null) return null;

        return new PassFailRuleDto
        {
            MaCauHinhDiem = config.MaCauHinhDiem,
            MaMonHoc = config.MaMonHoc,
            MaCodeMonHoc = config.MonHoc?.MaCodeMonHoc,
            TenMonHoc = config.MonHoc?.TenMonHoc,
            MaNganh = config.MonHoc?.MaNganh,
            TenNganh = config.MonHoc?.Nganh?.TenNganh,
            MaChuyenNganh = config.MonHoc?.MaChuyenNganh,
            TenChuyenNganh = config.MonHoc?.ChuyenNganh?.TenChuyenNganh,
            MaHocKy = config.MaHocKy,
            TenHocKy = config.HocKy?.TenHocKy,
            TrongSoQuaTrinh = config.TrongSoQuaTrinh,
            TrongSoGiuaKy = config.TrongSoGiuaKy,
            TrongSoCuoiKy = config.TrongSoCuoiKy,
            NguongDat = config.NguongDat,
            TiLeChuyenCanToiThieu = config.TiLeChuyenCanToiThieu,
            NguoiCapNhat = config.NguoiCapNhat,
            TenNguoiCapNhat = config.NguoiCapNhatNavigation?.HoTen,
            CapNhatLuc = config.CapNhatLuc
        };
    }

    public async Task<PassFailRuleDto> CreateAsync(UpsertPassFailRuleRequest request, int? currentUserId, CancellationToken ct = default)
    {
        ValidateRequest(request);

        var exists = await _context.CauHinhDiemMonHocs
            .AnyAsync(x => x.MaMonHoc == request.MaMonHoc && x.MaHocKy == request.MaHocKy, ct);
        if (exists)
        {
            throw new ApiException(400, "Môn học đã có cấu hình cho học kỳ này.");
        }

        var monHoc = await _context.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMonHoc == request.MaMonHoc, ct);
        if (monHoc is null)
        {
            throw new ApiException(404, "Không tìm thấy môn học.");
        }

        var hocKy = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == request.MaHocKy, ct);
        if (hocKy is null)
        {
            throw new ApiException(404, "Không tìm thấy học kỳ.");
        }

        var entity = new CauHinhDiemMonHoc
        {
            MaMonHoc = request.MaMonHoc,
            MaHocKy = request.MaHocKy,
            TrongSoQuaTrinh = request.TrongSoQuaTrinh,
            TrongSoGiuaKy = request.TrongSoGiuaKy,
            TrongSoCuoiKy = request.TrongSoCuoiKy,
            NguongDat = request.NguongDat,
            TiLeChuyenCanToiThieu = request.TiLeChuyenCanToiThieu,
            NguoiCapNhat = currentUserId,
            CapNhatLuc = DateTime.UtcNow
        };

        _context.CauHinhDiemMonHocs.Add(entity);
        await _context.SaveChangesAsync(ct);

        var dto = ToDto(entity, monHoc, hocKy);

        await _auditLogService.LogAsync(
            "CauHinhDiemMonHoc",
            entity.MaCauHinhDiem.ToString(),
            "CREATE_PASS_FAIL_RULE",
            null,
            dto,
            currentUserId,
            null,
            $"Tạo quy tắc đạt/rớt cho môn {monHoc.TenMonHoc} học kỳ {hocKy.TenHocKy}.",
            ct);

        return dto;
    }

    public async Task<PassFailRuleDto> UpdateAsync(int maCauHinhDiem, UpsertPassFailRuleRequest request, int? currentUserId, CancellationToken ct = default)
    {
        ValidateRequest(request);

        var entity = await _context.CauHinhDiemMonHocs
            .Include(x => x.MonHoc)
            .Include(x => x.HocKy)
            .FirstOrDefaultAsync(x => x.MaCauHinhDiem == maCauHinhDiem, ct);
        if (entity is null)
        {
            throw new ApiException(404, "Không tìm thấy cấu hình điểm của môn học.");
        }

        var oldSnapshot = ToDto(entity, entity.MonHoc!, entity.HocKy!);

        entity.TrongSoQuaTrinh = request.TrongSoQuaTrinh;
        entity.TrongSoGiuaKy = request.TrongSoGiuaKy;
        entity.TrongSoCuoiKy = request.TrongSoCuoiKy;
        entity.NguongDat = request.NguongDat;
        entity.TiLeChuyenCanToiThieu = request.TiLeChuyenCanToiThieu;
        entity.NguoiCapNhat = currentUserId;
        entity.CapNhatLuc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var newDto = ToDto(entity, entity.MonHoc!, entity.HocKy!);

        await _auditLogService.LogAsync(
            "CauHinhDiemMonHoc",
            entity.MaCauHinhDiem.ToString(),
            "UPDATE_PASS_FAIL_RULE",
            oldSnapshot,
            newDto,
            currentUserId,
            null,
            $"Cập nhật quy tắc đạt/rớt cho môn {entity.MonHoc.TenMonHoc} học kỳ {entity.HocKy?.TenHocKy}.",
            ct);

        return newDto;
    }

    private static void ValidateRequest(UpsertPassFailRuleRequest request)
    {
        var totalWeight = request.TrongSoQuaTrinh + request.TrongSoGiuaKy + request.TrongSoCuoiKy;
        if (totalWeight != 100m)
        {
            throw new ApiException(400, $"Tổng trọng số phải bằng 100% (hiện tại: {totalWeight}%).");
        }

        if (request.NguongDat < 0m || request.NguongDat > 10m)
        {
            throw new ApiException(400, "Ngưỡng đạt phải nằm trong khoảng 0 - 10.");
        }

        if (request.TiLeChuyenCanToiThieu < 0m || request.TiLeChuyenCanToiThieu > 100m)
        {
            throw new ApiException(400, "Tỷ lệ chuyên cần tối thiểu phải nằm trong khoảng 0 - 100%.");
        }
    }

    private static PassFailRuleDto ToDto(CauHinhDiemMonHoc entity, DanhMucMonHoc monHoc, HocKy hocKy)
    {
        return new PassFailRuleDto
        {
            MaCauHinhDiem = entity.MaCauHinhDiem,
            MaMonHoc = entity.MaMonHoc,
            MaCodeMonHoc = monHoc.MaCodeMonHoc,
            TenMonHoc = monHoc.TenMonHoc,
            MaHocKy = entity.MaHocKy,
            TenHocKy = hocKy.TenHocKy,
            TrongSoQuaTrinh = entity.TrongSoQuaTrinh,
            TrongSoGiuaKy = entity.TrongSoGiuaKy,
            TrongSoCuoiKy = entity.TrongSoCuoiKy,
            NguongDat = entity.NguongDat,
            TiLeChuyenCanToiThieu = entity.TiLeChuyenCanToiThieu,
            NguoiCapNhat = entity.NguoiCapNhat,
            CapNhatLuc = entity.CapNhatLuc
        };
    }
}
