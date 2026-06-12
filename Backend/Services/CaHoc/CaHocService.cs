using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.CaHoc;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.CaHoc;

public class CaHocService : ICaHocService
{
    private static readonly HashSet<string> ValidSessions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sang",
        "chieu",
        "toi"
    };

    private static readonly string[] TimeFormats = ["HH:mm", "H:mm", "HH:mm:ss", "H:mm:ss"];

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public CaHocService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<CaHocDto>> GetAsync(
        CaHocQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CaHocs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.TenCa.ToLower().Contains(keyword) ||
                x.Buoi.ToLower().Contains(keyword));
        }

        if (parameters.ConHoatDong.HasValue)
        {
            query = query.Where(x => x.ConHoatDong == parameters.ConHoatDong.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.ThuTu)
            .ThenBy(x => x.GioBatDau)
            .ThenBy(x => x.MaCaHoc)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CaHocDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<IReadOnlyList<CaHocDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CaHocs
            .AsNoTracking()
            .Where(x => x.ConHoatDong)
            .OrderBy(x => x.ThuTu)
            .ThenBy(x => x.GioBatDau)
            .ThenBy(x => x.MaCaHoc)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CaHocDto> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        var shift = await _context.CaHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaCaHoc == shiftId, cancellationToken);

        if (shift is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy ca học.");
        }

        return ToDto(shift);
    }

    public async Task<CaHocDto> CreateAsync(
        CreateCaHocRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageShifts(currentUser);

        var shiftName = NormalizeRequiredText(request.TenCa, "Tên ca");
        var session = NormalizeSession(request.Buoi);
        var startTime = ParseTime(request.GioBatDau, "Giờ bắt đầu");
        var endTime = ParseTime(request.GioKetThuc, "Giờ kết thúc");
        ValidateTimeRange(startTime, endTime);
        ValidateDisplayOrder(request.ThuTu);
        await EnsureUniqueShiftNameAsync(shiftName, null, cancellationToken);

        var shift = new Models.CaHoc
        {
            TenCa = shiftName,
            Buoi = session,
            GioBatDau = startTime,
            GioKetThuc = endTime,
            ThuTu = request.ThuTu,
            ConHoatDong = true
        };

        _context.CaHocs.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(
            "CREATE_CA_HOC",
            shift,
            null,
            ToAuditSnapshot(shift),
            currentUser,
            "Tạo ca học.",
            cancellationToken);

        return ToDto(shift);
    }

    public async Task<CaHocDto> UpdateAsync(
        int shiftId,
        UpdateCaHocRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageShifts(currentUser);

        var shift = await GetManagedShiftAsync(shiftId, cancellationToken);
        var oldSnapshot = ToAuditSnapshot(shift);
        var oldStartTime = shift.GioBatDau;
        var oldEndTime = shift.GioKetThuc;

        var shiftName = NormalizeRequiredText(request.TenCa, "Tên ca");
        var session = NormalizeSession(request.Buoi);
        var startTime = ParseTime(request.GioBatDau, "Giờ bắt đầu");
        var endTime = ParseTime(request.GioKetThuc, "Giờ kết thúc");
        ValidateTimeRange(startTime, endTime);
        ValidateDisplayOrder(request.ThuTu);
        await EnsureUniqueShiftNameAsync(shiftName, shiftId, cancellationToken);

        if ((oldStartTime != startTime || oldEndTime != endTime) &&
            await HasScheduleOrSessionUsageAsync(shiftId, cancellationToken))
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Ca học đã được dùng trong thời khóa biểu hoặc buổi học, không được sửa giờ để tránh làm sai lịch cũ.");
        }

        shift.TenCa = shiftName;
        shift.Buoi = session;
        shift.GioBatDau = startTime;
        shift.GioKetThuc = endTime;
        shift.ThuTu = request.ThuTu;
        shift.ConHoatDong = request.ConHoatDong;

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(
            "UPDATE_CA_HOC",
            shift,
            oldSnapshot,
            ToAuditSnapshot(shift),
            currentUser,
            "Cập nhật ca học.",
            cancellationToken);

        return ToDto(shift);
    }

    public async Task<CaHocDto> ToggleActiveAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageShifts(currentUser);

        var shift = await GetManagedShiftAsync(shiftId, cancellationToken);
        var oldSnapshot = ToAuditSnapshot(shift);
        shift.ConHoatDong = !shift.ConHoatDong;

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(
            shift.ConHoatDong ? "ACTIVATE_CA_HOC" : "DEACTIVATE_CA_HOC",
            shift,
            oldSnapshot,
            ToAuditSnapshot(shift),
            currentUser,
            shift.ConHoatDong ? "Mở ca học." : "Tắt ca học.",
            cancellationToken);

        return ToDto(shift);
    }

    private async Task<Models.CaHoc> GetManagedShiftAsync(int shiftId, CancellationToken cancellationToken)
    {
        var shift = await _context.CaHocs.FirstOrDefaultAsync(x => x.MaCaHoc == shiftId, cancellationToken);
        if (shift is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy ca học.");
        }

        return shift;
    }

    private async Task EnsureUniqueShiftNameAsync(
        string shiftName,
        int? excludedShiftId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.CaHocs
            .AsNoTracking()
            .AnyAsync(x =>
                x.TenCa == shiftName &&
                (!excludedShiftId.HasValue || x.MaCaHoc != excludedShiftId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Tên ca học đã tồn tại.");
        }
    }

    private async Task<bool> HasScheduleOrSessionUsageAsync(int shiftId, CancellationToken cancellationToken)
    {
        return await _context.ThoiKhoaBieus.AnyAsync(x => x.MaCaHoc == shiftId, cancellationToken) ||
               await _context.BuoiHocs.AnyAsync(x => x.MaCaHoc == shiftId, cancellationToken);
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

    private static void EnsureCanManageShifts(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý ca học.");
        }
    }

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalizedValue;
    }

    private static string NormalizeSession(string value)
    {
        var session = NormalizeRequiredText(value, "Buổi học").ToLowerInvariant();
        if (!ValidSessions.Contains(session))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học chỉ nhận: sang, chieu hoặc toi.");
        }

        return session;
    }

    private static TimeOnly ParseTime(string value, string fieldName)
    {
        var normalizedValue = NormalizeRequiredText(value, fieldName);
        if (!TimeOnly.TryParseExact(
                normalizedValue,
                TimeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} phải có định dạng HH:mm, ví dụ 07:30.");
        }

        return time;
    }

    private static void ValidateTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giờ kết thúc phải lớn hơn giờ bắt đầu.");
        }
    }

    private static void ValidateDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thứ tự phải lớn hơn 0.");
        }
    }

    private async Task WriteAuditAsync(
        string action,
        Models.CaHoc shift,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken)
    {
        await _auditLogService.LogAsync(
            "CaHoc",
            shift.MaCaHoc.ToString(),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            currentUser.CampusId,
            description,
            cancellationToken);
    }

    private static object ToAuditSnapshot(Models.CaHoc shift)
    {
        return new
        {
            shift.MaCaHoc,
            shift.TenCa,
            shift.Buoi,
            GioBatDau = FormatTime(shift.GioBatDau),
            GioKetThuc = FormatTime(shift.GioKetThuc),
            shift.ThuTu,
            shift.ConHoatDong
        };
    }

    private static CaHocDto ToDto(Models.CaHoc shift)
    {
        return new CaHocDto
        {
            MaCaHoc = shift.MaCaHoc,
            TenCa = shift.TenCa,
            Buoi = shift.Buoi,
            GioBatDau = FormatTime(shift.GioBatDau),
            GioKetThuc = FormatTime(shift.GioKetThuc),
            ThuTu = shift.ThuTu,
            ConHoatDong = shift.ConHoatDong
        };
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
