using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.ThoiKhoaBieu;

public sealed record RequiredCapacity(
    int Value,
    string Status,
    string Source,
    bool IsKnown = true,
    string? WarningCode = null)
{
    public const string SourceRegistered = "registered_students";
    public const string SourceClassStudents = "administrative_class_students";
    public const string SourceExpected = "class_expected_count";
    public const string SourceMissing = "STUDENT_CAPACITY_DATA_MISSING";

    public const string StatusReady = "ready";
    public const string StatusWarning = "warning";
    public const string StatusBlocked = "blocked";
    public const string StatusUnknown = "unknown";
}

public interface ICourseCapacityService
{
    Task<IReadOnlyDictionary<int, RequiredCapacity>> GetRequiredCapacitiesAsync(
        IEnumerable<KhoaHoc> courses,
        CancellationToken cancellationToken = default);

    bool IsRoomEligible(PhongHoc room, RequiredCapacity capacity, int expectedCampusId);
}

/// <summary>Single source of truth for room-capacity decisions. A missing count is never treated as zero.</summary>
public sealed class CourseCapacityService : ICourseCapacityService
{
    private readonly ApplicationDbContext _db;

    public CourseCapacityService(ApplicationDbContext db) => _db = db;

    public bool IsRoomEligible(PhongHoc room, RequiredCapacity capacity, int expectedCampusId)
    {
        if (room == null || capacity == null) return false;
        if (!capacity.IsKnown || capacity.Status == RequiredCapacity.StatusBlocked || capacity.Value <= 0)
            return false;
        if (room.MaDonVi != expectedCampusId || room.TrangThaiPhong != "hoat_dong")
            return false;
        if (room.SucChua < capacity.Value)
            return false;
        return true;
    }

    public async Task<IReadOnlyDictionary<int, RequiredCapacity>> GetRequiredCapacitiesAsync(
        IEnumerable<KhoaHoc> courses,
        CancellationToken cancellationToken = default)
    {
        var source = courses.DistinctBy(x => x.MaKhoaHoc).ToList();
        var classIds = source.Where(x => x.MaLop > 0).Select(x => x.MaLop).Distinct().ToList();
        var sectionIds = source.Where(x => x.MaLopHocPhan.HasValue).Select(x => x.MaLopHocPhan!.Value).Distinct().ToList();
        var studentRoleCode = AuthRoles.ToDatabaseCode(AuthRoles.Student);

        // 1. Đếm distinct sinh viên đăng ký hợp lệ theo từng Lớp học phần/cơ sở.
        // CK_DangKyHocPhan_trang_thai_1 chỉ công nhận da_dang_ky là đăng ký chính thức.
        // Campus phải được đối chiếu theo từng khóa học ở dưới, không được suy ra từ LHP.
        var registrations = new List<(int SectionId, int StudentId, int CampusId)>();
        if (sectionIds.Count > 0)
        {
            var rawRegistrations = await _db.DangKyHocPhans.AsNoTracking()
                .Where(x => sectionIds.Contains(x.MaLopHocPhan)
                    && x.TrangThai == "da_dang_ky"
                    && x.HocSinh != null
                    && x.HocSinh.VaiTroChinh == studentRoleCode
                    && x.HocSinh.TrangThai == UserStatuses.DbActive)
                .Select(x => new { x.MaLopHocPhan, x.MaHocSinh, x.HocSinh!.MaDonVi })
                .ToListAsync(cancellationToken);

            registrations = rawRegistrations
                .Select(x => (x.MaLopHocPhan, x.MaHocSinh, x.MaDonVi))
                .ToList();
        }

        // 2. Đếm sinh viên hoạt động trong Lớp hành chính theo MaLop & MaDonVi:
        var classCounts = await _db.NguoiDungs.AsNoTracking()
            .Where(x => x.MaLop.HasValue && classIds.Contains(x.MaLop.Value)
                && x.VaiTroChinh == studentRoleCode
                && x.TrangThai == UserStatuses.DbActive)
            .GroupBy(x => new { ClassId = x.MaLop!.Value, x.MaDonVi })
            .Select(g => new { g.Key.ClassId, g.Key.MaDonVi, Count = g.Select(u => u.MaNguoiDung).Distinct().Count() })
            .ToListAsync(cancellationToken);

        // 3. Sĩ số dự kiến từ Lớp hành chính:
        var expected = await _db.LopHanhChinhs.AsNoTracking()
            .Where(x => classIds.Contains(x.MaLop))
            .Select(x => new { x.MaLop, x.MaDonVi, x.SiSoDuKien })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<int, RequiredCapacity>();
        foreach (var course in source)
        {
            // Ưu tiên 1: Đăng ký học phần thực tế
            var regCount = course.MaLopHocPhan.HasValue
                ? registrations
                    .Where(x => x.SectionId == course.MaLopHocPhan.Value && x.CampusId == course.MaDonVi)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count()
                : 0;
            if (regCount > 0)
            {
                result[course.MaKhoaHoc] = new RequiredCapacity(
                    regCount,
                    RequiredCapacity.StatusReady,
                    RequiredCapacity.SourceRegistered,
                    IsKnown: true);
                continue;
            }

            // Ưu tiên 2: Sinh viên hoạt động trong Lớp hành chính
            var active = classCounts.FirstOrDefault(x => x.ClassId == course.MaLop && x.MaDonVi == course.MaDonVi)?.Count ?? 0;
            if (active > 0)
            {
                result[course.MaKhoaHoc] = new RequiredCapacity(
                    active,
                    RequiredCapacity.StatusReady,
                    RequiredCapacity.SourceClassStudents,
                    IsKnown: true);
                continue;
            }

            // Ưu tiên 3: Sĩ số dự kiến
            var fallback = expected.FirstOrDefault(x => x.MaLop == course.MaLop && x.MaDonVi == course.MaDonVi)?.SiSoDuKien ?? 0;
            if (fallback > 0)
            {
                result[course.MaKhoaHoc] = new RequiredCapacity(
                    fallback,
                    RequiredCapacity.StatusWarning,
                    RequiredCapacity.SourceExpected,
                    IsKnown: true);
                continue;
            }

            // Ưu tiên 4: Thiếu dữ liệu - Tuyệt đối không fallback về 0
            result[course.MaKhoaHoc] = new RequiredCapacity(
                0,
                RequiredCapacity.StatusBlocked,
                RequiredCapacity.SourceMissing,
                IsKnown: false,
                WarningCode: "STUDENT_CAPACITY_DATA_MISSING");
        }
        return result;
    }
}
