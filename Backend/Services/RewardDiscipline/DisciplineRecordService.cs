using System.Security.Claims;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class DisciplineRecordService : IDisciplineRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DisciplineRecordService(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User 
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Không tìm thấy thông tin đăng nhập.");
    }

    private bool IsSuperAdmin(ClaimsPrincipal user) => user.IsInRole(AuthRoles.SuperAdmin);

    private int? GetUserCampusId(ClaimsPrincipal user)
    {
        var campusClaim = user.FindFirst("MaDonVi")?.Value;
        if (int.TryParse(campusClaim, out var campusId))
            return campusId;
        return null;
    }

    private int GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var userId))
            return userId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "User ID claim is missing or invalid.");
    }

    private void ValidateEvidenceJson(JsonElement? json)
    {
        if (json == null || json.Value.ValueKind == JsonValueKind.Null) return;

        if (json.Value.ValueKind != JsonValueKind.Object)
            throw new ApiException(StatusCodes.Status400BadRequest, "EvidenceJson phải là Object.");

        string rawJson = json.Value.GetRawText();
        if (System.Text.Encoding.UTF8.GetByteCount(rawJson) > 16384)
            throw new ApiException(StatusCodes.Status400BadRequest, "EvidenceJson không được vượt quá 16KB.");

        CheckJsonSafety(json.Value, 0);
    }

    private void CheckJsonSafety(JsonElement element, int depth)
    {
        if (depth > 5) throw new ApiException(StatusCodes.Status400BadRequest, "EvidenceJson lồng nhau quá 5 mức.");

        if (element.ValueKind == JsonValueKind.Array)
        {
            if (element.GetArrayLength() > 100)
                throw new ApiException(StatusCodes.Status400BadRequest, "EvidenceJson mảng không được vượt quá 100 phần tử.");
            foreach (var item in element.EnumerateArray())
                CheckJsonSafety(item, depth + 1);
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            var sensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
            { 
                "password", "token", "secret", "storageKey", "fileHash" 
            };

            foreach (var prop in element.EnumerateObject())
            {
                if (sensitiveKeys.Contains(prop.Name))
                    throw new ApiException(StatusCodes.Status400BadRequest, $"EvidenceJson chứa trường nhạy cảm không được phép: {prop.Name}");
                
                CheckJsonSafety(prop.Value, depth + 1);
            }
        }
    }

    public async Task<PagedResultDto<DisciplineRecordListItemDto>> GetDisciplineRecordsAsync(
        DisciplineRecordQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);

        var query = _context.HoSoKyLuats
            .Include(h => h.HocSinh)
            .Include(h => h.DonVi)
            .Include(h => h.HocKy)
            .AsNoTracking()
            .AsQueryable();

        if (!isSuperAdmin)
        {
            if (campusId == null) throw new ApiException(StatusCodes.Status403Forbidden, "Không xác định được cơ sở của người dùng.");
            query = query.Where(h => h.MaDonVi == campusId.Value);
        }

        if (parameters.MaDonVi.HasValue)
            query = query.Where(h => h.MaDonVi == parameters.MaDonVi.Value);
        if (parameters.MaHocKy.HasValue)
            query = query.Where(h => h.MaHocKy == parameters.MaHocKy.Value);
        if (parameters.MaHocSinh.HasValue)
            query = query.Where(h => h.MaHocSinh == parameters.MaHocSinh.Value);
        if (!string.IsNullOrEmpty(parameters.MucDoKyLuat))
            query = query.Where(h => h.MucDoViPham == parameters.MucDoKyLuat);
        if (!string.IsNullOrEmpty(parameters.HinhThucXuLy))
            query = query.Where(h => h.HinhThucXuLy == parameters.HinhThucXuLy);
        if (!string.IsNullOrEmpty(parameters.TrangThai))
            query = query.Where(h => h.TrangThai == parameters.TrangThai);
        
        if (parameters.TuNgay.HasValue)
            query = query.Where(h => h.NgayViPham >= parameters.TuNgay.Value);
        if (parameters.DenNgay.HasValue)
            query = query.Where(h => h.NgayViPham <= parameters.DenNgay.Value);
            
        if (!string.IsNullOrEmpty(parameters.Keyword))
        {
            var kw = parameters.Keyword.ToLower();
            query = query.Where(h => 
                h.TieuDe.ToLower().Contains(kw) || 
                (h.HocSinh != null && (h.HocSinh.HoTen.ToLower().Contains(kw) || h.HocSinh.Email.ToLower().Contains(kw)))
            );
        }

        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(h => h.NgayViPham)
            .ThenByDescending(h => h.MaKyLuat)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(h => new DisciplineRecordListItemDto
            {
                MaHoSoKyLuat = h.MaKyLuat,
                MaHocSinh = h.MaHocSinh,
                HoTenHocSinh = h.HocSinh != null ? h.HocSinh.HoTen : "",
                Mssv = h.HocSinh != null ? h.HocSinh.Email : "",
                MaDonVi = h.MaDonVi,
                TenDonVi = h.DonVi != null ? h.DonVi.TenDonVi : "",
                MaHocKy = h.MaHocKy,
                TenHocKy = h.HocKy != null ? h.HocKy.TenHocKy : "",
                MucDoKyLuat = h.MucDoViPham,
                HinhThucXuLy = h.HinhThucXuLy,
                TrangThai = h.TrangThai,
                TieuDe = h.TieuDe,
                NgayViPham = h.NgayViPham,
                NgayTao = h.NgayTao,
                NguoiTao = h.NguoiTao
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<DisciplineRecordListItemDto>
        {
            Items = items,
            TotalItems = total,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize
        };
    }

    public async Task<DisciplineRecordDetailDto> GetDisciplineRecordDetailAsync(
        int recordId,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);

        var hoso = await _context.HoSoKyLuats
            .Include(h => h.HocSinh)
            .Include(h => h.DonVi)
            .Include(h => h.HocKy)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.MaKyLuat == recordId, cancellationToken);

        if (hoso == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");

        if (!isSuperAdmin && hoso.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem hồ sơ kỷ luật của cơ sở khác.");

        JsonElement? evidence = null;
        if (!string.IsNullOrEmpty(hoso.ChungTuJson))
        {
            try { evidence = JsonSerializer.Deserialize<JsonElement>(hoso.ChungTuJson); }
            catch { }
        }

        return new DisciplineRecordDetailDto
        {
            MaHoSoKyLuat = hoso.MaKyLuat,
            MaHocSinh = hoso.MaHocSinh,
            HoTenHocSinh = hoso.HocSinh != null ? hoso.HocSinh.HoTen : "",
            Mssv = hoso.HocSinh != null ? hoso.HocSinh.Email : "",
            MaDonVi = hoso.MaDonVi,
            TenDonVi = hoso.DonVi != null ? hoso.DonVi.TenDonVi : "",
            MaHocKy = hoso.MaHocKy,
            TenHocKy = hoso.HocKy != null ? hoso.HocKy.TenHocKy : "",
            MucDoKyLuat = hoso.MucDoViPham,
            HinhThucXuLy = hoso.HinhThucXuLy,
            TrangThai = hoso.TrangThai,
            TieuDe = hoso.TieuDe,
            NgayViPham = hoso.NgayViPham,
            NgayTao = hoso.NgayTao,
            NguoiTao = hoso.NguoiTao,
            MoTaViPham = hoso.MoTa,
            CanCuXuLy = hoso.CanCuXuLy,
            GhiChuNoiBo = hoso.GhiChuNoiBo,
            EvidenceJson = evidence,
            LyDoHuy = hoso.LyDoHuy,
            NguoiHuy = hoso.NguoiHuy,
            NgayHuy = hoso.NgayHuy,
            NgayHieuLuc = hoso.NgayHieuLuc,
            NgayHetHieuLuc = hoso.NgayHetHieuLuc
        };
    }

    public async Task<DisciplineRecordResultDto> CreateDisciplineRecordAsync(
        CreateDisciplineRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var userId = GetUserId(user);

        ValidateEvidenceJson(request.EvidenceJson);

        if (!RewardDisciplineConstants.DisciplineLevels.All.Contains(request.MucDoKyLuat))
            throw new ApiException(StatusCodes.Status400BadRequest, "Mức độ kỷ luật không hợp lệ.");
        if (!RewardDisciplineConstants.DisciplineActions.All.Contains(request.HinhThucXuLy))
            throw new ApiException(StatusCodes.Status400BadRequest, "Hình thức xử lý không hợp lệ.");

        var student = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == request.MaHocSinh, cancellationToken);
        
        if (student == null || student.VaiTroChinh != "hoc_sinh")
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy học sinh hoặc user không phải học sinh.");

        if (!isSuperAdmin && student.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không thể tạo kỷ luật cho học sinh ngoài cơ sở của bạn.");

        if (request.MaHocKy.HasValue)
        {
            var term = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(hk => hk.MaHocKy == request.MaHocKy.Value, cancellationToken);
            if (term == null) throw new ApiException(StatusCodes.Status404NotFound, "Học kỳ không tồn tại.");
        }

        // Duplicate check (naive)
        var isDup = await _context.HoSoKyLuats.AnyAsync(h => 
            h.MaHocSinh == request.MaHocSinh && 
            h.MaHocKy == request.MaHocKy && 
            h.NgayViPham == request.NgayViPham && 
            h.TieuDe == request.TieuDe &&
            h.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Cancelled, 
            cancellationToken);

        if (isDup)
            throw new ApiException(StatusCodes.Status409Conflict, "Hồ sơ kỷ luật tương tự đã tồn tại.");

        var hoso = new HoSoKyLuat
        {
            MaHocSinh = request.MaHocSinh,
            MaDonVi = student.MaDonVi,
            MaHocKy = request.MaHocKy,
            TieuDe = request.TieuDe,
            MoTa = request.MoTaViPham,
            NgayViPham = request.NgayViPham,
            MucDoViPham = request.MucDoKyLuat,
            HinhThucXuLy = request.HinhThucXuLy,
            CanCuXuLy = request.CanCuXuLy,
            GhiChuNoiBo = request.GhiChuNoiBo,
            ChungTuJson = request.EvidenceJson != null && request.EvidenceJson.Value.ValueKind != JsonValueKind.Null ? JsonSerializer.Serialize(request.EvidenceJson) : null,
            TrangThai = RewardDisciplineConstants.DisciplineStatuses.Draft,
            NguoiTao = userId,
            NgayTao = DateTime.UtcNow
        };

        _context.HoSoKyLuats.Add(hoso);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            hoso.MaDonVi,
            "HoSoKyLuat",
            hoso.MaKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.CreateDisciplineRecord,
            userId,
            null,
            new 
            {
                maHoSoKyLuat = hoso.MaKyLuat,
                maHocSinh = hoso.MaHocSinh,
                maHocKy = hoso.MaHocKy,
                mucDoKyLuat = hoso.MucDoViPham,
                hinhThucXuLy = hoso.HinhThucXuLy
            },
            cancellationToken
        );

        return new DisciplineRecordResultDto
        {
            MaHoSoKyLuat = hoso.MaKyLuat,
            TrangThai = hoso.TrangThai
        };
    }

    public async Task<DisciplineRecordResultDto> UpdateDisciplineRecordAsync(
        int recordId,
        UpdateDisciplineRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var userId = GetUserId(user);

        var hoso = await _context.HoSoKyLuats
            .FirstOrDefaultAsync(h => h.MaKyLuat == recordId, cancellationToken);

        if (hoso == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");

        if (!isSuperAdmin && hoso.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền sửa hồ sơ ở cơ sở khác.");

        if (hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Draft && 
            hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.PendingApproval)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được sửa hồ sơ ở trạng thái nháp hoặc chờ duyệt.");
        }

        ValidateEvidenceJson(request.EvidenceJson);
        if (!RewardDisciplineConstants.DisciplineLevels.All.Contains(request.MucDoKyLuat))
            throw new ApiException(StatusCodes.Status400BadRequest, "Mức độ kỷ luật không hợp lệ.");
        if (!RewardDisciplineConstants.DisciplineActions.All.Contains(request.HinhThucXuLy))
            throw new ApiException(StatusCodes.Status400BadRequest, "Hình thức xử lý không hợp lệ.");

        hoso.TieuDe = request.TieuDe;
        hoso.MoTa = request.MoTaViPham;
        hoso.NgayViPham = request.NgayViPham;
        hoso.MucDoViPham = request.MucDoKyLuat;
        hoso.HinhThucXuLy = request.HinhThucXuLy;
        hoso.CanCuXuLy = request.CanCuXuLy;
        hoso.GhiChuNoiBo = request.GhiChuNoiBo;
        hoso.ChungTuJson = request.EvidenceJson != null && request.EvidenceJson.Value.ValueKind != JsonValueKind.Null ? JsonSerializer.Serialize(request.EvidenceJson) : null;
        hoso.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            hoso.MaDonVi,
            "HoSoKyLuat",
            hoso.MaKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.UpdateDisciplineRecord,
            userId,
            null,
            new 
            {
                maHoSoKyLuat = hoso.MaKyLuat,
                maHocSinh = hoso.MaHocSinh,
                maHocKy = hoso.MaHocKy,
                mucDoKyLuat = hoso.MucDoViPham,
                hinhThucXuLy = hoso.HinhThucXuLy
            },
            cancellationToken
        );

        return new DisciplineRecordResultDto
        {
            MaHoSoKyLuat = hoso.MaKyLuat,
            TrangThai = hoso.TrangThai
        };
    }

    public async Task<DisciplineRecordResultDto> SubmitDisciplineRecordAsync(
        int recordId,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var userId = GetUserId(user);

        var hoso = await _context.HoSoKyLuats
            .FirstOrDefaultAsync(h => h.MaKyLuat == recordId, cancellationToken);

        if (hoso == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");

        if (!isSuperAdmin && hoso.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền gửi duyệt hồ sơ ở cơ sở khác.");

        if (hoso.TrangThai == RewardDisciplineConstants.DisciplineStatuses.PendingApproval)
            throw new ApiException(StatusCodes.Status409Conflict, "Hồ sơ đang ở trạng thái chờ duyệt.");

        if (hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Draft)
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hồ sơ nháp mới có thể gửi duyệt.");

        var oldStatus = hoso.TrangThai;
        hoso.TrangThai = RewardDisciplineConstants.DisciplineStatuses.PendingApproval;
        hoso.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            hoso.MaDonVi,
            "HoSoKyLuat",
            hoso.MaKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.SubmitDisciplineRecord,
            userId,
            new { status = oldStatus },
            new 
            {
                status = hoso.TrangThai,
                mucDoKyLuat = hoso.MucDoViPham,
                hinhThucXuLy = hoso.HinhThucXuLy
            },
            cancellationToken
        );

        return new DisciplineRecordResultDto
        {
            MaHoSoKyLuat = hoso.MaKyLuat,
            TrangThai = hoso.TrangThai
        };
    }

    public async Task<DisciplineRecordResultDto> CancelDisciplineRecordAsync(
        int recordId,
        CancelDisciplineRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var userId = GetUserId(user);

        var hoso = await _context.HoSoKyLuats
            .FirstOrDefaultAsync(h => h.MaKyLuat == recordId, cancellationToken);

        if (hoso == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");

        if (!isSuperAdmin && hoso.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền hủy hồ sơ ở cơ sở khác.");

        if (hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Draft && 
            hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.PendingApproval)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được hủy hồ sơ ở trạng thái nháp hoặc chờ duyệt.");
        }

        var oldStatus = hoso.TrangThai;
        hoso.TrangThai = RewardDisciplineConstants.DisciplineStatuses.Cancelled;
        hoso.LyDoHuy = request.Reason;
        hoso.NguoiHuy = userId;
        hoso.NgayHuy = DateTime.UtcNow;
        hoso.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            hoso.MaDonVi,
            "HoSoKyLuat",
            hoso.MaKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.CancelDisciplineRecord,
            userId,
            new { status = oldStatus },
            new 
            {
                status = hoso.TrangThai,
                reason = request.Reason
            },
            cancellationToken
        );

        return new DisciplineRecordResultDto
        {
            MaHoSoKyLuat = hoso.MaKyLuat,
            TrangThai = hoso.TrangThai
        };
    }
}
