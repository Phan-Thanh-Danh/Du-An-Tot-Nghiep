using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class CourseTeacherEligibilityService : ICourseTeacherEligibilityService
{
    private readonly ApplicationDbContext _context;

    public CourseTeacherEligibilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherEligibilityResultDto> ValidateTeacherForSubjectAsync(
        int campusId,
        int termId,
        int subjectId,
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        var result = new TeacherEligibilityResultDto { IsEligible = true };

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

        var capability = await _context.GiaoVienMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaGiaoVien == teacherId && x.MaMonHoc == subjectId, cancellationToken);

        if (capability == null)
        {
            return Fail(result, "MISSING_CAPABILITY", "Giảng viên chưa có chuyên môn phù hợp cho môn học này.");
        }

        if (!capability.ConHoatDong)
        {
            return Fail(result, "CAPABILITY_INACTIVE", "Chuyên môn môn học của giảng viên đã không còn hoạt động.");
        }

        return result;
    }

    private TeacherEligibilityResultDto Fail(TeacherEligibilityResultDto result, string code, string message)
    {
        result.IsEligible = false;
        result.ReasonCode = code;
        result.ReasonMessage = message;
        return result;
    }
}
