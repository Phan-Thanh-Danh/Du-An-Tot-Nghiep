using Backend.Services.Audit;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Services.RewardDiscipline;

public class DisciplineAppealService : IDisciplineAppealService
{
    private readonly IRewardDisciplineNotificationService _notificationService;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public DisciplineAppealService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IRewardDisciplineNotificationService notificationService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    
        _notificationService = notificationService;}

    private ClaimsPrincipal GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            throw new ApiException(StatusCodes.Status401Unauthorized, "Người dùng chưa đăng nhập.");
        return user;
    }

    private int GetUserId(ClaimsPrincipal user)
    {
        var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(nameId, out int userId))
            return userId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Id người dùng không hợp lệ.");
    }

    private int GetUserCampusId(ClaimsPrincipal user)
    {
        var campusIdClaim = user.FindFirst("MaDonVi")?.Value;
        if (int.TryParse(campusIdClaim, out int campusId))
            return campusId;
        throw new ApiException(StatusCodes.Status403Forbidden, "Không xác định được cơ sở của người dùng.");
    }

    private bool IsSuperAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole(AuthRoles.SuperAdmin);
    }

    public async Task<DisciplineAppealListItemDto> CreateDisciplineAppealAsync(
        int recordId,
        CreateDisciplineAppealRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var studentId = GetUserId(user);

        var hoso = await _context.HoSoKyLuats
            .FirstOrDefaultAsync(h => h.MaKyLuat == recordId && h.MaHocSinh == studentId, cancellationToken);

        if (hoso == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");

        // Chi duoc khieu nai khi hs dang co hieu luc hoac da duyet
        if (hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Active &&
            hoso.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Approved)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ có thể khiếu nại đối với hồ sơ đã duyệt hoặc đang có hiệu lực.");
        }

        var existingAppeal = await _context.KhieuNaiKyLuats
            .Where(k => k.MaHoSoKyLuat == recordId && k.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Pending)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAppeal != null)
            throw new ApiException(StatusCodes.Status409Conflict, "Đã có một khiếu nại đang chờ xử lý cho hồ sơ này.");

        var khieuNai = new KhieuNaiKyLuat
        {
            MaHoSoKyLuat = recordId,
            MaHocSinh = studentId,
            MaDonVi = hoso.MaDonVi,
            LyDoKhieuNai = request.Reason,
            TrangThai = RewardDisciplineConstants.DisciplineAppealStatuses.Pending,
            ChungTuJson = request.EvidenceJson?.GetRawText(),
            NgayCapNhat = DateTime.UtcNow
        };

        _context.KhieuNaiKyLuats.Add(khieuNai);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            khieuNai.MaDonVi ?? 0,
            "KhieuNaiKyLuat",
            khieuNai.MaKhieuNaiKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.CreateDisciplineAppeal,
            studentId,
            null,
            new 
            {
                status = khieuNai.TrangThai,
                reason = request.Reason
            },
            cancellationToken
        );

        return new DisciplineAppealListItemDto
        {
            MaKhieuNaiKyLuat = khieuNai.MaKhieuNaiKyLuat,
            MaHoSoKyLuat = khieuNai.MaHoSoKyLuat,
            MaHocSinh = khieuNai.MaHocSinh,
            TrangThai = khieuNai.TrangThai,
            NgayTao = khieuNai.NgayTao
        };
    }

    public async Task<PagedResultDto<DisciplineAppealListItemDto>> GetDisciplineAppealsAsync(
        DisciplineAppealQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);

        var query = _context.KhieuNaiKyLuats
            .Include(k => k.HocSinh)
            .Include(k => k.NguoiXuLyNavigation)
            .AsQueryable();

        if (!isSuperAdmin)
        {
            query = query.Where(k => k.MaDonVi == campusId);
        }
        else if (parameters.MaDonVi.HasValue)
        {
            query = query.Where(k => k.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrEmpty(parameters.TrangThai))
            query = query.Where(k => k.TrangThai == parameters.TrangThai);

        if (!string.IsNullOrEmpty(parameters.Keyword))
        {
            query = query.Where(k => 
                k.HocSinh.HoTen.Contains(parameters.Keyword) ||
                k.HocSinh.Email.Contains(parameters.Keyword) ||
                k.LyDoKhieuNai.Contains(parameters.Keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(k => k.NgayTao)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(k => new DisciplineAppealListItemDto
            {
                MaKhieuNaiKyLuat = k.MaKhieuNaiKyLuat,
                MaHoSoKyLuat = k.MaHoSoKyLuat,
                MaHocSinh = k.MaHocSinh,
                TenHocSinh = k.HocSinh.HoTen,
                Mssv = k.HocSinh.Email,
                TrangThai = k.TrangThai,
                NgayTao = k.NgayTao,
                NgayXuLy = k.NgayXuLy,
                NguoiXuLyName = k.NguoiXuLyNavigation != null ? k.NguoiXuLyNavigation.HoTen : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<DisciplineAppealListItemDto>
        {
            Items = items,
            TotalItems = totalCount,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize
        };
    }

    public async Task<DisciplineAppealDetailDto> GetDisciplineAppealDetailAsync(
        int appealId,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var studentId = GetUserId(user);
        var isStudent = user.IsInRole(AuthRoles.Student);

        var k = await _context.KhieuNaiKyLuats
            .Include(x => x.HocSinh)
            .Include(x => x.NguoiXuLyNavigation)
            .FirstOrDefaultAsync(x => x.MaKhieuNaiKyLuat == appealId, cancellationToken);

        if (k == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khiếu nại.");

        if (isStudent && k.MaHocSinh != studentId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền xem khiếu nại của học sinh khác.");
        else if (!isStudent && !isSuperAdmin && k.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền xem khiếu nại ở cơ sở khác.");

        return new DisciplineAppealDetailDto
        {
            MaKhieuNaiKyLuat = k.MaKhieuNaiKyLuat,
            MaHoSoKyLuat = k.MaHoSoKyLuat,
            MaHocSinh = k.MaHocSinh,
            TenHocSinh = k.HocSinh.HoTen,
            Mssv = k.HocSinh.Email,
            TrangThai = k.TrangThai,
            NgayTao = k.NgayTao,
            NgayXuLy = k.NgayXuLy,
            NguoiXuLyName = k.NguoiXuLyNavigation != null ? k.NguoiXuLyNavigation.HoTen : null,
            LyDoKhieuNai = k.LyDoKhieuNai,
            EvidenceJson = string.IsNullOrEmpty(k.ChungTuJson) ? null : System.Text.Json.JsonDocument.Parse(k.ChungTuJson).RootElement,
            LyDoXuLy = k.LyDoXuLy,
            GhiChuXuLy = k.GhiChuXuLy
        };
    }

    public async Task<DisciplineAppealDetailDto> ResolveDisciplineAppealAsync(
        int appealId,
        ResolveDisciplineAppealRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var isSuperAdmin = IsSuperAdmin(user);
        var campusId = GetUserCampusId(user);
        var userId = GetUserId(user);

        var khieuNai = await _context.KhieuNaiKyLuats
            .Include(k => k.HoSoKyLuat)
            .FirstOrDefaultAsync(k => k.MaKhieuNaiKyLuat == appealId, cancellationToken);

        if (khieuNai == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khiếu nại.");

        if (!isSuperAdmin && khieuNai.MaDonVi != campusId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền xử lý khiếu nại ở cơ sở khác.");

        if (khieuNai.TrangThai != RewardDisciplineConstants.DisciplineAppealStatuses.Pending)
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được xử lý khiếu nại đang chờ xử lý.");

        if (request.Decision != RewardDisciplineConstants.DisciplineAppealStatuses.Accepted &&
            request.Decision != RewardDisciplineConstants.DisciplineAppealStatuses.Rejected)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Quyết định xử lý không hợp lệ.");
        }

        var oldStatus = khieuNai.TrangThai;
        khieuNai.TrangThai = request.Decision;
        khieuNai.NguoiXuLy = userId;
        khieuNai.NgayXuLy = DateTime.UtcNow;
        khieuNai.LyDoXuLy = request.Reason;
        khieuNai.GhiChuXuLy = request.ResolutionNote;
        khieuNai.NgayCapNhat = DateTime.UtcNow;

        if (request.Decision == RewardDisciplineConstants.DisciplineAppealStatuses.Accepted && request.RemoveEffect && khieuNai.HoSoKyLuat != null)
        {
            if (khieuNai.HoSoKyLuat.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active)
            {
                khieuNai.HoSoKyLuat.TrangThai = RewardDisciplineConstants.DisciplineStatuses.Removed;
                khieuNai.HoSoKyLuat.DaGoKyLuat = true;
                khieuNai.HoSoKyLuat.NgayGoKyLuat = DateTime.UtcNow;
                khieuNai.HoSoKyLuat.NguoiGoKyLuat = userId;
                khieuNai.HoSoKyLuat.LyDoGoKyLuat = "Gỡ hiệu lực do chấp nhận khiếu nại: " + request.Reason;
                khieuNai.HoSoKyLuat.NgayCapNhat = DateTime.UtcNow;
                khieuNai.HoSoKyLuat.NgayHetHieuLuc = DateOnly.FromDateTime(DateTime.UtcNow);

                await _auditLogService.AddAsync(
                    khieuNai.HoSoKyLuat.MaDonVi,
                    "HoSoKyLuat",
                    khieuNai.HoSoKyLuat.MaKyLuat,
                    RewardDisciplineConstants.DisciplineAuditActions.RemoveDisciplineEffectByAppeal,
                    userId,
                    new { status = RewardDisciplineConstants.DisciplineStatuses.Active },
                    new 
                    {
                        status = khieuNai.HoSoKyLuat.TrangThai,
                        appealId = khieuNai.MaKhieuNaiKyLuat,
                        reason = request.Reason
                    },
                    cancellationToken
                );
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            khieuNai.MaDonVi ?? 0,
            "KhieuNaiKyLuat",
            khieuNai.MaKhieuNaiKyLuat,
            RewardDisciplineConstants.DisciplineAuditActions.ResolveDisciplineAppeal,
            userId,
            new { status = oldStatus },
            new 
            {
                status = khieuNai.TrangThai,
                reason = request.Reason,
                removeEffect = request.RemoveEffect
            },
            cancellationToken
        );

        return await GetDisciplineAppealDetailAsync(appealId, cancellationToken);
    }
}
