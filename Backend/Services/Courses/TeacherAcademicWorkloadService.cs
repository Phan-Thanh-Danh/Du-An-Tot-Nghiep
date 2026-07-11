using Backend.Data;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class TeacherAcademicWorkloadService : ITeacherAcademicWorkloadService
{
    private readonly ApplicationDbContext _context;

    public TeacherAcademicWorkloadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherWorkloadDto> GetWorkloadAsync(
        int teacherId,
        int campusId,
        int termId,
        CancellationToken cancellationToken = default)
    {
        var workloads = await GetWorkloadsAsync(new[] { teacherId }, campusId, termId, cancellationToken);
        return workloads.GetValueOrDefault(teacherId) ?? new TeacherWorkloadDto
        {
            MaGiaoVien = teacherId,
            MaHocKy = termId
        };
    }

    public async Task<Dictionary<int, TeacherWorkloadDto>> GetWorkloadsAsync(
        IEnumerable<int> teacherIds,
        int campusId,
        int termId,
        CancellationToken cancellationToken = default)
    {
        var ids = teacherIds.Distinct().ToList();
        
        var courses = await _context.KhoaHocs
            .AsNoTracking()
            .Where(c => c.MaDonVi == campusId && c.MaHocKy == termId && ids.Contains(c.MaGiaoVien) && c.TrangThai != "Archived")
            .Select(c => new { c.MaGiaoVien, c.MaKhoaHoc })
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(c => c.MaKhoaHoc).ToList();

        var scheduledShifts = await _context.ThoiKhoaBieus
            .AsNoTracking()
            .Where(t => courseIds.Contains(t.MaKhoaHoc))
            .Select(t => new { t.MaKhoaHoc, t.ThuTrongTuan, t.MaCaHoc })
            .Distinct()
            .ToListAsync(cancellationToken);

        var courseShifts = scheduledShifts.GroupBy(t => t.MaKhoaHoc)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new Dictionary<int, TeacherWorkloadDto>();

        foreach (var teacherId in ids)
        {
            var teacherCourses = courses.Where(c => c.MaGiaoVien == teacherId).ToList();
            int classCount = teacherCourses.Count;
            int shiftCount = teacherCourses.Sum(c => courseShifts.GetValueOrDefault(c.MaKhoaHoc));

            result[teacherId] = new TeacherWorkloadDto
            {
                MaGiaoVien = teacherId,
                MaHocKy = termId,
                CurrentClassCount = classCount,
                CurrentWeeklyShiftCount = shiftCount
            };
        }

        return result;
    }
}
