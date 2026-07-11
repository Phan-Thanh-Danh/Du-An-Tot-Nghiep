using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Courses.AssignmentSuggestions;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class CourseAssignmentSuggestionService : ICourseAssignmentSuggestionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITeacherAcademicWorkloadService _workloadService;
    private readonly ITeachingPreferenceCoverageService _preferenceCoverageService;

    public CourseAssignmentSuggestionService(
        ApplicationDbContext context,
        ITeacherAcademicWorkloadService workloadService,
        ITeachingPreferenceCoverageService preferenceCoverageService)
    {
        _context = context;
        _workloadService = workloadService;
        _preferenceCoverageService = preferenceCoverageService;
    }

    public async Task<CourseAssignmentSuggestionResultDto> GetSuggestionsAsync(
        CourseAssignmentSuggestionRequestDto request,
        int campusId,
        CancellationToken cancellationToken = default)
    {
        var subject = await _context.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMonHoc == request.MaMonHoc, cancellationToken);

        if (subject == null)
            throw new ApiException(StatusCodes.Status400BadRequest, "Môn học không tồn tại.");

        var result = new CourseAssignmentSuggestionResultDto
        {
            MaHocKy = request.MaHocKy,
            MaMonHoc = request.MaMonHoc,
            TenMonHoc = subject.TenMonHoc
        };

        // Get all capabilities for this subject in this campus
        var capabilities = await _context.GiaoVienMonHocs
            .Include(x => x.GiaoVien)
            .Include(x => x.MonHoc)
            .AsNoTracking()
            .Where(x => x.MaMonHoc == request.MaMonHoc && x.GiaoVien.MaDonVi == campusId && x.GiaoVien.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Teacher))
            .ToListAsync(cancellationToken);

        var teacherIds = capabilities.Select(c => c.MaGiaoVien).Distinct().ToList();

        // Get workloads and preferences in batch
        var workloads = await _workloadService.GetWorkloadsAsync(teacherIds, campusId, request.MaHocKy, cancellationToken);
        var preferences = await _preferenceCoverageService.EvaluateCoveragesAsync(teacherIds, campusId, request.MaHocKy, request.PlannedSlots, cancellationToken);

        foreach (var cap in capabilities)
        {
            var teacher = cap.GiaoVien;
            var workload = workloads.GetValueOrDefault(teacher.MaNguoiDung);
            var pref = preferences.GetValueOrDefault(teacher.MaNguoiDung);

            if (teacher.TrangThai == UserStatuses.DbLocked)
            {
                result.ExcludedCandidates.Add(new ExcludedTeacherCandidateDto
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    HoTen = teacher.HoTen,
                    Email = teacher.Email,
                    ReasonCode = "TEACHER_LOCKED",
                    ReasonMessage = "Giảng viên đang bị khóa."
                });
                continue;
            }

            if (!cap.ConHoatDong)
            {
                result.ExcludedCandidates.Add(new ExcludedTeacherCandidateDto
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    HoTen = teacher.HoTen,
                    Email = teacher.Email,
                    ReasonCode = "CAPABILITY_INACTIVE",
                    ReasonMessage = "Chuyên môn môn học của giảng viên đã không còn hoạt động."
                });
                continue;
            }

            if (pref != null && pref.HasHardConflicts)
            {
                result.ExcludedCandidates.Add(new ExcludedTeacherCandidateDto
                {
                    MaGiaoVien = teacher.MaNguoiDung,
                    HoTen = teacher.HoTen,
                    Email = teacher.Email,
                    ReasonCode = "HARD_CONFLICT",
                    ReasonMessage = "Giảng viên đã đăng ký Không thể dạy vào các ca học dự kiến của môn này."
                });
                continue;
            }

            var candidate = new TeacherAssignmentCandidateDto
            {
                MaGiaoVien = teacher.MaNguoiDung,
                HoTen = teacher.HoTen,
                Email = teacher.Email,
                ChuyenNganh = "",
                IsEligible = true
            };

            // Calculate capability score (max 55)
            candidate.CapabilityScore = Math.Clamp(cap.MucDoPhuHop, 0, 100) * 0.55;
            candidate.Reasons.Add($"Mức độ phù hợp chuyên môn {cap.MucDoPhuHop}/100.");

            // Calculate main subject bonus
            if (cap.LaMonChinh)
            {
                candidate.MainSubjectBonus = 10;
                candidate.Reasons.Add("Môn học là môn chuyên môn chính của giảng viên.");
            }

            // Calculate experience score (max 15)
            int exp = cap.SoNamKinhNghiem ?? 0;
            candidate.ExperienceScore = Math.Min(exp, 15);
            if (exp > 0)
                candidate.Reasons.Add($"Có {exp} năm kinh nghiệm.");

            // Calculate history score (max 10)
            candidate.HistoryScore = Math.Min(cap.SoLanDaDay, 10);
            if (cap.SoLanDaDay > 0)
                candidate.Reasons.Add($"Đã giảng dạy môn này {cap.SoLanDaDay} lần.");

            // Workload penalties
            if (workload != null)
            {
                candidate.CurrentClassCount = workload.CurrentClassCount;
                candidate.CurrentWeeklyShiftCount = workload.CurrentWeeklyShiftCount;

                candidate.ClassWorkloadPenalty = workload.CurrentClassCount * 8;
                if (workload.CurrentClassCount > 0)
                    candidate.Warnings.Add($"Đang phụ trách {workload.CurrentClassCount} lớp trong học kỳ.");

                int maxWeeklyShifts = 8; // Predefined threshold
                int excessShifts = Math.Max(0, workload.CurrentWeeklyShiftCount - maxWeeklyShifts);
                candidate.WeeklyShiftPenalty = excessShifts * 2;
                
                if (excessShifts > 0)
                    candidate.Warnings.Add($"Số ca mỗi tuần ({workload.CurrentWeeklyShiftCount}) cao hơn ngưỡng khuyến nghị ({maxWeeklyShifts}).");
            }

            // Preference score
            if (pref != null)
            {
                candidate.PreferenceCoverage = pref.CoverageRatio;
                candidate.PreferenceScore = pref.Score;
                
                if (pref.Score > 0)
                    candidate.Reasons.Add("Ưu tiên các ca phù hợp với lịch dự kiến.");

                foreach (var w in pref.Warnings)
                {
                    candidate.Warnings.Add(w);
                }
            }

            // Final score calculation
            double rawScore = candidate.CapabilityScore + candidate.MainSubjectBonus + candidate.ExperienceScore + candidate.HistoryScore + candidate.PreferenceScore - candidate.ClassWorkloadPenalty - candidate.WeeklyShiftPenalty;
            candidate.FinalScore = Math.Max(0, rawScore);

            result.Candidates.Add(candidate);
        }

        // Sort candidates
        result.Candidates = result.Candidates
            .OrderByDescending(c => c.FinalScore)
            .ThenByDescending(c => c.CapabilityScore)
            .ThenBy(c => c.CurrentClassCount)
            .ThenBy(c => c.HoTen)
            .Take(request.CandidateLimit)
            .ToList();

        return result;
    }
}
