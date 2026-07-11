using Backend.Data;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class TeachingPreferenceCoverageService : ITeachingPreferenceCoverageService
{
    private readonly ApplicationDbContext _context;

    public TeachingPreferenceCoverageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PreferenceCoverageDto> EvaluateCoverageAsync(
        int teacherId,
        int campusId,
        int termId,
        List<PlannedTeachingSlotDto> plannedSlots,
        CancellationToken cancellationToken = default)
    {
        var coverages = await EvaluateCoveragesAsync(new[] { teacherId }, campusId, termId, plannedSlots, cancellationToken);
        return coverages.GetValueOrDefault(teacherId) ?? new PreferenceCoverageDto();
    }

    public async Task<Dictionary<int, PreferenceCoverageDto>> EvaluateCoveragesAsync(
        IEnumerable<int> teacherIds,
        int campusId,
        int termId,
        List<PlannedTeachingSlotDto> plannedSlots,
        CancellationToken cancellationToken = default)
    {
        var ids = teacherIds.Distinct().ToList();
        var result = ids.ToDictionary(id => id, id => new PreferenceCoverageDto());

        var preferences = await _context.GiaoVienNguyenVongHocKys
            .Include(x => x.ChiTietNguyenVong)
            .AsNoTracking()
            .Where(x => x.MaDonVi == campusId && x.MaHocKy == termId && ids.Contains(x.MaGiaoVien))
            .ToListAsync(cancellationToken);

        foreach (var pref in preferences)
        {
            var teacherId = pref.MaGiaoVien;
            var coverage = result[teacherId];

            if (pref.TrangThai != "submitted")
            {
                coverage.Warnings.Add("Nguyện vọng giảng dạy chưa được chốt (không ở trạng thái submitted). Không tính điểm nguyện vọng.");
                continue;
            }

            if (!plannedSlots.Any())
            {
                // If there are no planned slots, we can't do exact matching.
                // Just return the general preference score as a fallback or 0.
                continue;
            }

            int preferred = 0, available = 0, neutral = 0, unavailable = 0;
            bool hasHardConflict = false;

            foreach (var slot in plannedSlots)
            {
                var prefDetail = pref.ChiTietNguyenVong
                    .FirstOrDefault(x => x.ThuTrongTuan == slot.ThuTrongTuan && x.MaCaHoc == slot.MaCaHoc);

                if (prefDetail == null)
                {
                    neutral++;
                }
                else
                {
                    if (prefDetail.MucDo == "preferred") preferred++;
                    else if (prefDetail.MucDo == "available") available++;
                    else if (prefDetail.MucDo == "unavailable")
                    {
                        unavailable++;
                        hasHardConflict = true;
                    }
                }
            }

            coverage.PreferredCount = preferred;
            coverage.AvailableCount = available;
            coverage.NeutralCount = neutral;
            coverage.UnavailableCount = unavailable;
            coverage.HasHardConflicts = hasHardConflict;

            if (hasHardConflict)
            {
                coverage.Warnings.Add("Ca học dự kiến trùng với lịch Không thể dạy của giảng viên.");
            }

            if (plannedSlots.Count > 0)
            {
                coverage.CoverageRatio = (double)(preferred + available) / plannedSlots.Count;
                
                // Score rule: Preferred gets max weight, Available gets half weight. Max total score is 10.
                // We'll normalize it so 100% preferred = 10 points.
                double maxScore = 10.0;
                double slotScore = (preferred * 1.0 + available * 0.5) / plannedSlots.Count;
                coverage.Score = slotScore * maxScore;
            }
        }

        foreach (var kvp in result)
        {
            if (!preferences.Any(p => p.MaGiaoVien == kvp.Key))
            {
                kvp.Value.Warnings.Add("Giảng viên chưa gửi nguyện vọng giảng dạy trong học kỳ này.");
            }
        }

        return result;
    }
}
