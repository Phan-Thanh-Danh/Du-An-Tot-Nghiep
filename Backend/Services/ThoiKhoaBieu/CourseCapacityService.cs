using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.ThoiKhoaBieu;

public sealed record RequiredCapacity(int Value, string Status, string Source);

public interface ICourseCapacityService
{
    Task<IReadOnlyDictionary<int, RequiredCapacity>> GetRequiredCapacitiesAsync(
        IEnumerable<KhoaHoc> courses,
        CancellationToken cancellationToken = default);
}

/// <summary>Single source of truth for room-capacity decisions. A missing count is never treated as zero.</summary>
public sealed class CourseCapacityService : ICourseCapacityService
{
    private readonly ApplicationDbContext _db;

    public CourseCapacityService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyDictionary<int, RequiredCapacity>> GetRequiredCapacitiesAsync(
        IEnumerable<KhoaHoc> courses,
        CancellationToken cancellationToken = default)
    {
        var source = courses.DistinctBy(x => x.MaKhoaHoc).ToList();
        var classIds = source.Where(x => x.MaLop > 0).Select(x => x.MaLop).Distinct().ToList();
        var sectionIds = source.Where(x => x.MaLopHocPhan.HasValue).Select(x => x.MaLopHocPhan!.Value).Distinct().ToList();

        var classCounts = await _db.NguoiDungs.AsNoTracking()
            .Where(x => x.MaLop.HasValue && classIds.Contains(x.MaLop.Value)
                && x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Student)
                && x.TrangThai == "hoat_dong")
            .GroupBy(x => new { ClassId = x.MaLop!.Value, x.MaDonVi })
            .Select(g => new { g.Key.ClassId, g.Key.MaDonVi, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var sections = await _db.LopHocPhans.AsNoTracking()
            .Where(x => sectionIds.Contains(x.MaLopHocPhan))
            .Select(x => new { x.MaLopHocPhan, x.SoDaDangKy })
            .ToDictionaryAsync(x => x.MaLopHocPhan, cancellationToken);
        var registrations = await _db.DangKyHocPhans.AsNoTracking()
            .Where(x => sectionIds.Contains(x.MaLopHocPhan) && x.TrangThai == "da_dang_ky")
            .GroupBy(x => x.MaLopHocPhan)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SectionId, x => x.Count, cancellationToken);
        var expected = await _db.LopHanhChinhs.AsNoTracking()
            .Where(x => classIds.Contains(x.MaLop))
            .Select(x => new { x.MaLop, x.SiSoDuKien })
            .ToDictionaryAsync(x => x.MaLop, x => x.SiSoDuKien, cancellationToken);

        var result = new Dictionary<int, RequiredCapacity>();
        foreach (var course in source)
        {
            var active = classCounts.FirstOrDefault(x => x.ClassId == course.MaLop && x.MaDonVi == course.MaDonVi)?.Count ?? 0;
            var enrolled = course.MaLopHocPhan.HasValue
                ? Math.Max(registrations.GetValueOrDefault(course.MaLopHocPhan.Value), sections.GetValueOrDefault(course.MaLopHocPhan.Value)?.SoDaDangKy ?? 0)
                : 0;
            var actual = Math.Max(active, enrolled);
            if (actual > 0)
                result[course.MaKhoaHoc] = new RequiredCapacity(actual, "ready", active > 0 && enrolled > 0 ? "active_students_and_enrollments" : active > 0 ? "active_students" : "section_enrollment");
            else if (expected.GetValueOrDefault(course.MaLop) is int fallback && fallback > 0)
                result[course.MaKhoaHoc] = new RequiredCapacity(fallback, "warning", "class_expected_count");
            else
                result[course.MaKhoaHoc] = new RequiredCapacity(0, "blocked", "DATA_INCOMPLETE");
        }
        return result;
    }
}
