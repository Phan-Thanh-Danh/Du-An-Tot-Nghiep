using Backend.Configuration;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.Courses;

public class CourseTeacherEligibilityService : ICourseTeacherEligibilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IOptions<SmartTimetableScoringOptions> _scoringOptions;

    public CourseTeacherEligibilityService(
        ApplicationDbContext context,
        IOptions<SmartTimetableScoringOptions> scoringOptions)
    {
        _context = context;
        _scoringOptions = scoringOptions;
    }

    public async Task<TeacherEligibilityResultDto> ValidateTeacherForSubjectAsync(
        int campusId,
        int termId,
        int subjectId,
        int teacherId,
        int? excludeCourseId = null,
        int? targetStartBlockId = null,
        int? targetSoBlockHoc = null,
        CancellationToken cancellationToken = default)
    {
        var result = new TeacherEligibilityResultDto { IsEligible = true };

        // 1. Kiểm tra giảng viên tồn tại, đúng vai trò, không bị khóa, đúng cơ sở
        var teacher = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == teacherId, cancellationToken);

        if (teacher == null)
        {
            return Fail(result, "TEACHER_NOT_FOUND", "Giảng viên không tồn tại.");
        }

        if (teacher.VaiTroChinh != AuthRoles.ToDatabaseCode(AuthRoles.Teacher))
        {
            return Fail(result, "INVALID_ROLE", "Người dùng không phải giảng viên.");
        }

        if (teacher.TrangThai == UserStatuses.DbLocked)
        {
            return Fail(result, "TEACHER_LOCKED", "Giảng viên đang bị khóa.");
        }

        if (teacher.MaDonVi != campusId)
        {
            return Fail(result, "INVALID_CAMPUS", "Giảng viên không thuộc cơ sở của khóa học.");
        }

        // 2. Ràng buộc cứng Chuyên môn: MonHocChuyenNganh phải giao với GiaoVienChuyenNganh
        var subjectSpecs = await _context.MonHocChuyenNganhs
            .AsNoTracking()
            .Where(m => m.MaMonHoc == subjectId)
            .Include(m => m.ChuyenNganh)
            .ToListAsync(cancellationToken);

        var subjectSpecIds = subjectSpecs.Select(m => m.MaChuyenNganh).ToHashSet();
        var subjectSpecNames = subjectSpecs
            .Select(m => m.ChuyenNganh?.TenChuyenNganh)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();

        var teacherSpecs = await _context.GiaoVienChuyenNganhs
            .AsNoTracking()
            .Where(g => g.MaGiaoVien == teacherId)
            .Include(g => g.ChuyenNganh)
            .ToListAsync(cancellationToken);

        var teacherSpecIds = teacherSpecs.Select(g => g.MaChuyenNganh).ToHashSet();
        var teacherSpecNames = teacherSpecs
            .Select(g => g.ChuyenNganh?.TenChuyenNganh)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();

        var commonSpecs = subjectSpecIds.Intersect(teacherSpecIds).ToList();
        if (!commonSpecs.Any())
        {
            var subjectSpecsStr = subjectSpecNames.Any() ? string.Join(", ", subjectSpecNames) : "Chưa xác định";
            var teacherSpecsStr = teacherSpecNames.Any() ? string.Join(", ", teacherSpecNames) : "Chưa có chuyên ngành";
            return Fail(result, "SPECIALIZATION_MISMATCH",
                $"Giảng viên không có chuyên ngành phù hợp với môn học này. Chuyên ngành môn học: [{subjectSpecsStr}]. Chuyên ngành giảng viên: [{teacherSpecsStr}].");
        }

        // 3. Ràng buộc cứng Trần tải giảng dạy (WeeklyCapCa từ SmartTimetableScoringOptions)
        if (termId > 0)
        {
            var cap = _scoringOptions.Value?.WeeklyCapCa ?? 6;

            var subject = await _context.DanhMucMonHocs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaMonHoc == subjectId, cancellationToken);

            var quyDoiList = await _context.QuyDoiTinChis
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var newCourseShifts = GetWeeklyShiftsForCredits(subject?.SoTinChi ?? 3, quyDoiList);

            // Tải danh sách Block của học kỳ để xác định thứ tự thời gian các block
            var termBlocks = await _context.Blocks
                .AsNoTracking()
                .Where(b => b.MaHocKy == termId)
                .OrderBy(b => b.ThuTuBlock)
                .ToListAsync(cancellationToken);

            var blockSeqDict = termBlocks.ToDictionary(b => b.MaBlock, b => b.ThuTuBlock);

            // Xác định khoảng block của khóa học sắp gán [targetStartSeq, targetEndSeq]
            int targetStartSeq = 1;
            if (targetStartBlockId.HasValue && blockSeqDict.TryGetValue(targetStartBlockId.Value, out var startSeq))
            {
                targetStartSeq = startSeq;
            }
            else if (termBlocks.Any())
            {
                targetStartSeq = termBlocks.Min(b => b.ThuTuBlock);
            }

            var subjectQuyDoi = quyDoiList.FirstOrDefault(q => q.SoTinChi == (subject?.SoTinChi ?? 3));
            int targetDuration = targetSoBlockHoc.HasValue && targetSoBlockHoc.Value > 0
                ? targetSoBlockHoc.Value
                : (subjectQuyDoi?.SoBlockHoc ?? 1);
            int targetEndSeq = targetStartSeq + targetDuration - 1;

            var existingCoursesQuery = _context.KhoaHocs
                .AsNoTracking()
                .Where(c => c.MaGiaoVien == teacherId && c.MaHocKy == termId && c.TrangThai != "luu_tru" && c.TrangThai != "Archived");

            if (excludeCourseId.HasValue && excludeCourseId.Value > 0)
            {
                existingCoursesQuery = existingCoursesQuery.Where(c => c.MaKhoaHoc != excludeCourseId.Value);
            }

            var existingCourses = await existingCoursesQuery
                .Select(c => new 
                { 
                    c.MaKhoaHoc, 
                    c.MaBlockBatDau, 
                    c.SoBlockHoc, 
                    c.MonHoc!.SoTinChi 
                })
                .ToListAsync(cancellationToken);

            int overlappingWeeklyShifts = 0;
            int overlappingCount = 0;

            foreach (var ec in existingCourses)
            {
                int ecStartSeq = 1;
                if (ec.MaBlockBatDau.HasValue && blockSeqDict.TryGetValue(ec.MaBlockBatDau.Value, out var seq))
                {
                    ecStartSeq = seq;
                }
                else if (termBlocks.Any())
                {
                    ecStartSeq = termBlocks.Min(b => b.ThuTuBlock);
                }

                int ecDuration = ec.SoBlockHoc > 0 ? ec.SoBlockHoc : 1;
                int ecEndSeq = ecStartSeq + ecDuration - 1;

                // Hai khóa học chồng lấn thời gian nếu khoảng Block của chúng giao nhau
                bool isOverlapping = targetStartSeq <= ecEndSeq && ecStartSeq <= targetEndSeq;
                if (isOverlapping)
                {
                    overlappingWeeklyShifts += GetWeeklyShiftsForCredits(ec.SoTinChi, quyDoiList);
                    overlappingCount++;
                }
            }

            int totalWeeklyShifts = overlappingWeeklyShifts + newCourseShifts;
            if (totalWeeklyShifts > cap)
            {
                string blockScope = termBlocks.Any()
                    ? $"trong khoảng Block {targetStartSeq}{(targetDuration > 1 ? $"-{targetEndSeq}" : "")}"
                    : "trong học kỳ";

                return Fail(result, "WORKLOAD_EXCEEDED",
                    $"Giảng viên vượt trần tải giảng dạy {blockScope}. Tải hiện tại trong khoảng block này: {overlappingWeeklyShifts} ca/tuần ({overlappingCount} khóa học trùng block), khóa học sắp gán: {newCourseShifts} ca/tuần, tổng dự kiến: {totalWeeklyShifts} ca/tuần (Trần cho phép tối đa: {cap} ca/tuần).");
            }
        }

        return result;
    }

    private static int GetWeeklyShiftsForCredits(int credits, IReadOnlyList<QuyDoiTinChi> quyDoiList)
    {
        var quyDoi = quyDoiList.FirstOrDefault(q => q.SoTinChi == credits);
        if (quyDoi != null)
        {
            return quyDoi.SoBuoiMoiTuan * quyDoi.SoCaMoiBuoi;
        }

        return credits switch
        {
            <= 2 => 2,
            3 => 3,
            4 => 4,
            _ => 5
        };
    }

    private TeacherEligibilityResultDto Fail(TeacherEligibilityResultDto result, string code, string message)
    {
        result.IsEligible = false;
        result.ReasonCode = code;
        result.ReasonMessage = message;
        return result;
    }
}
