using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin + "," + AuthRoles.AcademicStaff)]
public class BghAcademicController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghAcademicController(ApplicationDbContext db)
    {
        _db = db;
    }

    private (int CampusId, bool IsGlobal) GetUserScope()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var campusId = user?.CampusId ?? 0;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin || user?.Role == AuthRoles.Admin;
        return (campusId, isGlobal);
    }

    [HttpGet("academic/overview")]
    public async Task<ActionResult<ApiResponseDto<AcademicOverviewDto>>> GetAcademicOverview()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId));
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => isGlobal || l.MaDonVi == campusId);
        var activeCourses = await _db.KhoaHocs.CountAsync(k =>
            (k.TrangThai == "dang_mo" || k.TrangThai == "da_xuat_ban") &&
            (isGlobal || k.MaDonVi == campusId));

        var avgGpa = await _db.DiemSos.Where(d => isGlobal || d.MaDonVi == campusId).AverageAsync(d => (decimal?)d.GpaMonHoc) ?? 0;
        var passCount = await _db.DiemSos.CountAsync(d => d.GpaMonHoc >= 4 && (isGlobal || d.MaDonVi == campusId));
        var totalGrades = await _db.DiemSos.CountAsync(d => isGlobal || d.MaDonVi == campusId);
        var passRate = totalGrades > 0 ? (double)passCount / totalGrades * 100 : 0;

        var atRiskCount = await _db.DiemSos
            .Where(d => d.GpaMonHoc < 4 && (isGlobal || d.MaDonVi == campusId))
            .Select(d => d.MaHocSinh)
            .Distinct()
            .CountAsync();

        var distribution = await _db.DiemSos
            .Where(d => isGlobal || d.MaDonVi == campusId)
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A" :
                          d.GpaMonHoc >= 7 ? "B" :
                          d.GpaMonHoc >= 5.5m ? "C" :
                          d.GpaMonHoc >= 4 ? "D" : "F")
            .Select(g => new GradeDistributionDto
            {
                Grade = g.Key,
                Count = g.Count(),
                Percent = totalGrades > 0 ? Math.Round((double)g.Count() / totalGrades * 100, 1) : 0
            })
            .OrderByDescending(g => g.Grade == "A" ? 5 : g.Grade == "B" ? 4 : g.Grade == "C" ? 3 : g.Grade == "D" ? 2 : 1)
            .ToListAsync();

        var topSubjects = await _db.DiemSos
            .Where(d => d.MonHoc != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaMonHoc, TenMon = d.MonHoc!.TenMonHoc })
            .Select(g => new SubjectPassFailDto
            {
                SubjectName = g.Key.TenMon,
                Total = g.Count(),
                Pass = g.Count(d => d.GpaMonHoc >= 4),
                FailRate = Math.Round((double)g.Count(d => d.GpaMonHoc < 4) / g.Count() * 100, 1)
            })
            .OrderByDescending(s => s.FailRate)
            .Take(10)
            .ToListAsync();

        var totalMonHoc = await _db.DanhMucMonHocs.CountAsync();

        var semesterTrend = await _db.DiemSos
            .Where(d => d.HocKy != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaHocKy, TenHocKy = d.HocKy!.TenHocKy ?? "" })
            .Select(g => new GpaTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.Semester)
            .Take(5)
            .ToListAsync();

        var data = new AcademicOverviewDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            ActiveCourses = activeCourses,
            AvgGpa = Math.Round(avgGpa, 2),
            PassRate = Math.Round(passRate, 1),
            AtRiskCount = atRiskCount,
            TotalSubjects = totalMonHoc,
            GradeDistribution = distribution,
            TopSubjects = topSubjects,
            SemesterTrend = semesterTrend
        };

        return Ok(ApiResponseDto<AcademicOverviewDto>.Ok(data));
    }

    [HttpGet("academic/gpa")]
    public async Task<ActionResult<ApiResponseDto<GpaReportDto>>> GetGpaReports()
    {
        var (campusId, isGlobal) = GetUserScope();

        var semesterGroups = await _db.DiemSos
            .Where(d => d.HocKy != null && (isGlobal || d.MaDonVi == campusId))
            .GroupBy(d => new { d.MaHocKy, TenHocKy = d.HocKy!.TenHocKy ?? "" })
            .Select(g => new GpaTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.Semester)
            .ToListAsync();

        var distribution = await _db.DiemSos
            .Where(d => isGlobal || d.MaDonVi == campusId)
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A (8.5-10)" :
                          d.GpaMonHoc >= 7 ? "B (7.0-8.4)" :
                          d.GpaMonHoc >= 5.5m ? "C (5.5-6.9)" :
                          d.GpaMonHoc >= 4 ? "D (4.0-5.4)" : "F (< 4.0)")
            .Select(g => new GradeDistributionDto
            {
                Grade = g.Key,
                Count = g.Count(),
                Percent = 0
            })
            .ToListAsync();

        var total = distribution.Sum(d => d.Count);
        foreach (var d in distribution)
            d.Percent = total > 0 ? Math.Round((double)d.Count / total * 100, 1) : 0;

        var data = new GpaReportDto
        {
            Trends = semesterGroups,
            Distribution = distribution.OrderByDescending(d => d.Grade).ToList()
        };

        return Ok(ApiResponseDto<GpaReportDto>.Ok(data));
    }

    [HttpGet("academic/at-risk")]
    public async Task<ActionResult<ApiResponseDto<AtRiskReportDto>>> GetAtRiskStudents()
    {
        var (campusId, isGlobal) = GetUserScope();

        var atRiskStudentIds = await _db.DiemSos
            .Where(d => d.GpaMonHoc < 4 && (isGlobal || d.MaDonVi == campusId))
            .Select(d => d.MaHocSinh)
            .Distinct()
            .ToListAsync();

        var students = await _db.NguoiDungs
            .Where(u => atRiskStudentIds.Contains(u.MaNguoiDung))
            .Select(u => new AtRiskStudentDto
            {
                Id = u.MaNguoiDung,
                Name = u.HoTen,
                Email = u.Email,
                ClassCode = u.Lop != null ? u.Lop.MaCodeLop : "",
                AvgGpa = _db.DiemSos.Where(d => d.MaHocSinh == u.MaNguoiDung).Average(d => (decimal?)d.GpaMonHoc) ?? 0,
                FailCount = _db.DiemSos.Count(d => d.MaHocSinh == u.MaNguoiDung && d.GpaMonHoc < 4)
            })
            .OrderBy(u => u.AvgGpa)
            .ToListAsync();

        var data = new AtRiskReportDto
        {
            TotalAtRisk = students.Count,
            Students = students,
            Summary = new AtRiskSummaryDto
            {
                TotalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId)),
                AvgGpaAtRisk = students.Count > 0 ? Math.Round((decimal)students.Average(s => (double)s.AvgGpa), 2) : 0,
                CriticalCount = students.Count(s => s.FailCount >= 3)
            }
        };

        return Ok(ApiResponseDto<AtRiskReportDto>.Ok(data));
    }

    [HttpGet("academic/reports")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAcademicReports()
    {
        var (campusId, isGlobal) = GetUserScope();

        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId));
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (isGlobal || u.MaDonVi == campusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => isGlobal || l.MaDonVi == campusId);
        var activeCourses = await _db.KhoaHocs.CountAsync(k =>
            (k.TrangThai == "dang_mo" || k.TrangThai == "da_xuat_ban") &&
            (isGlobal || k.MaDonVi == campusId));
        var avgGpa = await _db.DiemSos.Where(d => isGlobal || d.MaDonVi == campusId).AverageAsync(d => (decimal?)d.GpaMonHoc) ?? 0;

        var data = new
        {
            Summary = new
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                ActiveCourses = activeCourses,
                AvgGpa = Math.Round(avgGpa, 2)
            },
            MonthlyStats = new object[] { },
            DepartmentStats = new object[] { }
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("academic/pass-fail/filters")]
    public async Task<ActionResult<ApiResponseDto<PassFailFilterOptionsDto>>> GetPassFailFilterOptions(
        [FromQuery] int? majorId = null,
        [FromQuery] int? specializationId = null,
        [FromQuery] int? programSubjectId = null)
    {
        var (campusId, isGlobal) = GetUserScope();

        var majors = await (
                from major in _db.NganhDaoTaos.AsNoTracking()
                join specialization in _db.ChuyenNganhs.AsNoTracking()
                    on major.MaNganh equals specialization.MaNganh
                join program in _db.ChuongTrinhDaoTaos.AsNoTracking()
                    on specialization.MaChuyenNganh equals program.MaChuyenNganh
                join academicClass in _db.LopHanhChinhs.AsNoTracking()
                    on program.MaChuongTrinh equals academicClass.MaChuongTrinh
                where major.ConHoatDong &&
                      specialization.ConHoatDong &&
                      program.ConHoatDong &&
                      academicClass.ConHoatDong &&
                      (isGlobal || academicClass.MaDonVi == campusId)
                select new { major.MaNganh, major.TenNganh })
            .Distinct()
            .OrderBy(x => x.TenNganh)
            .Select(x => new PassFailFilterOptionDto
            {
                Id = x.MaNganh,
                Label = x.TenNganh
            })
            .ToListAsync();

        var specializations = await (
                from specialization in _db.ChuyenNganhs.AsNoTracking()
                join major in _db.NganhDaoTaos.AsNoTracking()
                    on specialization.MaNganh equals major.MaNganh
                join program in _db.ChuongTrinhDaoTaos.AsNoTracking()
                    on specialization.MaChuyenNganh equals program.MaChuyenNganh
                join academicClass in _db.LopHanhChinhs.AsNoTracking()
                    on program.MaChuongTrinh equals academicClass.MaChuongTrinh
                where major.ConHoatDong &&
                      specialization.ConHoatDong &&
                      program.ConHoatDong &&
                      academicClass.ConHoatDong &&
                      (isGlobal || academicClass.MaDonVi == campusId) &&
                      (!majorId.HasValue || major.MaNganh == majorId.Value)
                select new { specialization.MaChuyenNganh, specialization.TenChuyenNganh })
            .Distinct()
            .OrderBy(x => x.TenChuyenNganh)
            .Select(x => new PassFailFilterOptionDto
            {
                Id = x.MaChuyenNganh,
                Label = x.TenChuyenNganh
            })
            .ToListAsync();

        var programSubjects = await _db.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Where(p => p.ConHoatDong &&
                        p.ChuongTrinhDaoTao != null &&
                        p.ChuongTrinhDaoTao.ConHoatDong &&
                        p.ChuongTrinhDaoTao.ChuyenNganh != null &&
                        p.ChuongTrinhDaoTao.ChuyenNganh.ConHoatDong &&
                        p.ChuongTrinhDaoTao.ChuyenNganh.NganhDaoTao != null &&
                        p.ChuongTrinhDaoTao.ChuyenNganh.NganhDaoTao.ConHoatDong &&
                        p.DanhMucMonHoc != null &&
                        p.DanhMucMonHoc.ConHoatDong &&
                        (!majorId.HasValue ||
                         p.ChuongTrinhDaoTao.ChuyenNganh.MaNganh == majorId.Value) &&
                        (!specializationId.HasValue ||
                         p.ChuongTrinhDaoTao.MaChuyenNganh == specializationId.Value) &&
                        _db.LopHanhChinhs.Any(l =>
                            l.ConHoatDong &&
                            l.MaChuongTrinh == p.MaChuongTrinh &&
                            (isGlobal || l.MaDonVi == campusId)))
            .OrderBy(p => p.DanhMucMonHoc!.TenMonHoc)
            .ThenBy(p => p.ChuongTrinhDaoTao!.MaCodeChuongTrinh)
            .Select(p => new ProgramSubjectFilterOptionDto
            {
                Id = p.MaChuongTrinhMonHoc,
                SubjectId = p.MaMonHoc,
                Label = p.DanhMucMonHoc!.TenMonHoc,
                SubjectCode = p.DanhMucMonHoc.MaCodeMonHoc,
                ProgramCode = p.ChuongTrinhDaoTao!.MaCodeChuongTrinh,
                ExpectedSemester = p.HocKyDuKien
            })
            .ToListAsync();

        var gradeQuery = BuildPassFailGradeQuery(
            campusId,
            isGlobal,
            majorId,
            specializationId,
            programSubjectId);

        var availableSemesterIds = gradeQuery.Select(d => d.SemesterId).Distinct();
        var semesters = await _db.HocKys
            .AsNoTracking()
            .Where(h => availableSemesterIds.Contains(h.MaHocKy))
            .OrderBy(h => h.NamHoc)
            .ThenBy(h => h.ThuTuTrongNam)
            .Select(h => new SemesterFilterOptionDto
            {
                Id = h.MaHocKy,
                Label = h.TenHocKy,
                AcademicYear = h.NamHoc
            })
            .ToListAsync();

        return Ok(ApiResponseDto<PassFailFilterOptionsDto>.Ok(new PassFailFilterOptionsDto
        {
            Majors = majors,
            Specializations = specializations,
            ProgramSubjects = programSubjects,
            Semesters = semesters
        }));
    }

    [HttpGet("academic/pass-fail")]
    public async Task<ActionResult<ApiResponseDto<PassFailReportDto>>> GetPassFailRates(
        [FromQuery] int? majorId = null,
        [FromQuery] int? specializationId = null,
        [FromQuery] int? programSubjectId = null,
        [FromQuery] int? semesterId = null)
    {
        var (campusId, isGlobal) = GetUserScope();

        var gradeQuery = BuildPassFailGradeQuery(
            campusId,
            isGlobal,
            majorId,
            specializationId,
            programSubjectId);

        if (semesterId.HasValue)
            gradeQuery = gradeQuery.Where(d => d.SemesterId == semesterId.Value);

        var courseStats = await gradeQuery
            .GroupBy(d => new { d.SubjectId, d.SubjectName })
            .Select(g => new CoursePassFailDto
            {
                SubjectName = g.Key.SubjectName,
                Total = g.Count(),
                Pass = g.Count(d => d.Gpa >= 4),
                Fail = g.Count(d => d.Gpa < 4),
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.Gpa) ?? 0, 2)
            })
            .OrderByDescending(s => s.Fail)
            .Take(20)
            .ToListAsync();

        foreach (var c in courseStats)
            c.FailRate = c.Total > 0 ? Math.Round((double)c.Fail / c.Total * 100, 1) : 0;

        var semesterTrend = await gradeQuery
            .GroupBy(d => new
            {
                d.SemesterId,
                d.SemesterName,
                d.AcademicYear,
                d.SemesterOrder
            })
            .Select(g => new PassFailTrendDto
            {
                SemesterId = g.Key.SemesterId,
                SemesterName = g.Key.SemesterName,
                AcademicYear = g.Key.AcademicYear,
                SemesterOrder = g.Key.SemesterOrder,
                Total = g.Count(),
                Pass = g.Count(d => d.Gpa >= 4),
                Fail = g.Count(d => d.Gpa < 4)
            })
            .OrderBy(t => t.AcademicYear)
            .ThenBy(t => t.SemesterOrder)
            .ToListAsync();

        foreach (var point in semesterTrend)
        {
            point.PassRate = point.Total > 0
                ? Math.Round((double)point.Pass / point.Total * 100, 1)
                : 0;
            point.FailRate = point.Total > 0
                ? Math.Round((double)point.Fail / point.Total * 100, 1)
                : 0;
        }

        var totals = await gradeQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pass = g.Count(d => d.Gpa >= 4),
                Fail = g.Count(d => d.Gpa < 4)
            })
            .SingleOrDefaultAsync();
        var totalGrades = totals?.Total ?? 0;
        var totalPass = totals?.Pass ?? 0;
        var totalFail = totals?.Fail ?? 0;

        var data = new PassFailReportDto
        {
            CourseStats = courseStats,
            SemesterTrend = semesterTrend,
            TotalResults = totalGrades,
            TotalPass = totalPass,
            TotalFail = totalFail,
            OverallPassRate = totalGrades > 0
                ? Math.Round((double)totalPass / totalGrades * 100, 1)
                : 0,
            OverallFailRate = totalGrades > 0
                ? Math.Round((double)totalFail / totalGrades * 100, 1)
                : 0
        };

        return Ok(ApiResponseDto<PassFailReportDto>.Ok(data));
    }

    private IQueryable<PassFailGradeRow> BuildPassFailGradeQuery(
        int campusId,
        bool isGlobal,
        int? majorId,
        int? specializationId,
        int? programSubjectId)
    {
        return
            from grade in _db.DiemSos.AsNoTracking()
            join student in _db.NguoiDungs.AsNoTracking()
                on grade.MaHocSinh equals student.MaNguoiDung
            join academicClass in _db.LopHanhChinhs.AsNoTracking()
                on student.MaLop equals (int?)academicClass.MaLop
            join program in _db.ChuongTrinhDaoTaos.AsNoTracking()
                on academicClass.MaChuongTrinh equals (int?)program.MaChuongTrinh
            join specialization in _db.ChuyenNganhs.AsNoTracking()
                on program.MaChuyenNganh equals specialization.MaChuyenNganh
            join major in _db.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            join programSubject in _db.MonHocTrongChuongTrinhs.AsNoTracking()
                on new { program.MaChuongTrinh, grade.MaMonHoc }
                equals new { programSubject.MaChuongTrinh, programSubject.MaMonHoc }
            join subject in _db.DanhMucMonHocs.AsNoTracking()
                on grade.MaMonHoc equals subject.MaMonHoc
            join semester in _db.HocKys.AsNoTracking()
                on grade.MaHocKy equals semester.MaHocKy
            where program.ConHoatDong &&
                  specialization.ConHoatDong &&
                  major.ConHoatDong &&
                  programSubject.ConHoatDong &&
                  subject.ConHoatDong &&
                  (isGlobal || grade.MaDonVi == campusId) &&
                  (!majorId.HasValue || major.MaNganh == majorId.Value) &&
                  (!specializationId.HasValue || specialization.MaChuyenNganh == specializationId.Value) &&
                  (!programSubjectId.HasValue || programSubject.MaChuongTrinhMonHoc == programSubjectId.Value)
            select new PassFailGradeRow
            {
                Gpa = grade.GpaMonHoc,
                SubjectId = subject.MaMonHoc,
                SubjectName = subject.TenMonHoc,
                SemesterId = semester.MaHocKy,
                SemesterName = semester.TenHocKy,
                AcademicYear = semester.NamHoc,
                SemesterOrder = semester.ThuTuTrongNam,
                MajorId = major.MaNganh,
                MajorName = major.TenNganh,
                SpecializationId = specialization.MaChuyenNganh,
                SpecializationName = specialization.TenChuyenNganh,
                ProgramSubjectId = programSubject.MaChuongTrinhMonHoc
            };
    }

    private sealed class PassFailGradeRow
    {
        public decimal Gpa { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = "";
        public int SemesterId { get; init; }
        public string SemesterName { get; init; } = "";
        public string AcademicYear { get; init; } = "";
        public int SemesterOrder { get; init; }
        public int MajorId { get; init; }
        public string MajorName { get; init; } = "";
        public int SpecializationId { get; init; }
        public string SpecializationName { get; init; } = "";
        public int ProgramSubjectId { get; init; }
    }

    [HttpGet("schedule/changes")]
    public async Task<ActionResult<ApiResponseDto<List<ScheduleChangeDto>>>> GetScheduleChanges()
    {
        var (campusId, isGlobal) = GetUserScope();

        var changes = await _db.BuoiHocs
            .Where(b => (b.LoaiThayDoi != null || b.TrangThaiBuoi == "da_huy") && (isGlobal || (b.KhoaHoc != null && b.KhoaHoc.MaDonVi == campusId)))
            .OrderByDescending(b => b.NgayCapNhat)
            .Take(50)
            .Select(b => new ScheduleChangeDto
            {
                Id = b.MaBuoiHoc,
                ChangeType = b.LoaiThayDoi ?? (b.TrangThaiBuoi == "da_huy" ? "da_huy" : ""),
                Reason = b.LyDoThayDoi ?? "",
                Date = b.NgayHoc,
                SubjectName = b.KhoaHoc != null && b.KhoaHoc.MonHoc != null ? b.KhoaHoc.MonHoc.TenMonHoc : "",
                ClassCode = b.KhoaHoc != null && b.KhoaHoc.Lop != null ? b.KhoaHoc.Lop.MaCodeLop : "",
                TeacherName = b.KhoaHoc != null && b.KhoaHoc.GiaoVien != null ? b.KhoaHoc.GiaoVien.HoTen : "",
                SubstituteTeacherName = b.MaGiaoVienDayThay != null
                    ? _db.NguoiDungs.Where(n => n.MaNguoiDung == b.MaGiaoVienDayThay).Select(n => n.HoTen).FirstOrDefault() ?? ""
                    : "",
                UpdatedAt = b.NgayCapNhat ?? b.NgayTao
            })
            .ToListAsync();

        return Ok(ApiResponseDto<List<ScheduleChangeDto>>.Ok(changes));
    }

    // ===== Grade Unlock Request Approval =====

    [HttpPost("grade-unlock-requests/{requestId}/approve")]
    public async Task<ActionResult<ApiResponseDto<object>>> ApproveGradeUnlockRequest(int requestId)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = user!.UserId;

        var yeuCau = await _db.YeuCauSuaDiems
            .Include(y => y.DiemSo)
            .FirstOrDefaultAsync(y => y.MaYcSuaDiem == requestId);

        if (yeuCau == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy yêu cầu mở khoá."));

        if (yeuCau.LoaiYeuCau != "mo_khoa_bang_diem")
            return BadRequest(ApiResponseDto.Fail("Yêu cầu này không phải loại mở khoá bảng điểm."));

        if (yeuCau.TrangThai != "cho_duyet")
            return Conflict(ApiResponseDto.Fail($"Yêu cầu đã được xử lý (trạng thái: {yeuCau.TrangThai})."));

        // Approve the request
        yeuCau.TrangThai = "da_duyet";
        yeuCau.NguoiDuyet = userId;

        // Unlock the grade record
        if (yeuCau.DiemSo != null)
        {
            yeuCau.DiemSo.DaKhoa = false;

            // Audit log
            var auditLog = new NhatKyThayDoiDiem
            {
                MaDiemSo = yeuCau.MaDiemSo,
                NguoiThayDoi = userId,
                GiaTriCu = JsonSerializer.Serialize(new { DaKhoa = true }),
                GiaTriMoi = JsonSerializer.Serialize(new { DaKhoa = false }),
                LyDo = $"Duyệt yêu cầu mở khoá #{requestId}: {yeuCau.LyDo}",
                NguoiDuyet = userId,
                ThayDoiLuc = DateTime.UtcNow
            };
            _db.NhatKyThayDoiDiems.Add(auditLog);
        }

        await _db.SaveChangesAsync();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            message = "Đã duyệt yêu cầu mở khoá bảng điểm.",
            requestId = yeuCau.MaYcSuaDiem,
            diemSoId = yeuCau.MaDiemSo
        }));
    }

    [HttpPost("grade-unlock-requests/{requestId}/reject")]
    public async Task<ActionResult<ApiResponseDto<object>>> RejectGradeUnlockRequest(int requestId, [FromBody] RejectGradeUnlockRequest request)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = user!.UserId;

        var yeuCau = await _db.YeuCauSuaDiems
            .FirstOrDefaultAsync(y => y.MaYcSuaDiem == requestId);

        if (yeuCau == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy yêu cầu mở khoá."));

        if (yeuCau.LoaiYeuCau != "mo_khoa_bang_diem")
            return BadRequest(ApiResponseDto.Fail("Yêu cầu này không phải loại mở khoá bảng điểm."));

        if (yeuCau.TrangThai != "cho_duyet")
            return Conflict(ApiResponseDto.Fail($"Yêu cầu đã được xử lý (trạng thái: {yeuCau.TrangThai})."));

        // Reject — do not change DaKhoa
        yeuCau.TrangThai = "tu_choi";
        yeuCau.NguoiDuyet = userId;

        await _db.SaveChangesAsync();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            message = "Đã từ chối yêu cầu mở khoá bảng điểm.",
            requestId = yeuCau.MaYcSuaDiem
        }));
    }
}

// DTOs
public class AcademicOverviewDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int ActiveCourses { get; set; }
    public decimal AvgGpa { get; set; }
    public double PassRate { get; set; }
    public int AtRiskCount { get; set; }
    public int TotalSubjects { get; set; }
    public List<GradeDistributionDto> GradeDistribution { get; set; } = [];
    public List<SubjectPassFailDto> TopSubjects { get; set; } = [];
    public List<GpaTrendDto> SemesterTrend { get; set; } = [];
}

public class GradeDistributionDto
{
    public string Grade { get; set; } = "";
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class SubjectPassFailDto
{
    public string SubjectName { get; set; } = "";
    public int Total { get; set; }
    public int Pass { get; set; }
    public double FailRate { get; set; }
}

public class GpaReportDto
{
    public List<GpaTrendDto> Trends { get; set; } = [];
    public List<GradeDistributionDto> Distribution { get; set; } = [];
}

public class GpaTrendDto
{
    public string Semester { get; set; } = "";
    public decimal AvgGpa { get; set; }
    public int StudentCount { get; set; }
}

public class AtRiskReportDto
{
    public int TotalAtRisk { get; set; }
    public AtRiskSummaryDto Summary { get; set; } = new();
    public List<AtRiskStudentDto> Students { get; set; } = [];
}

public class AtRiskSummaryDto
{
    public int TotalStudents { get; set; }
    public decimal AvgGpaAtRisk { get; set; }
    public int CriticalCount { get; set; }
}

public class AtRiskStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public decimal AvgGpa { get; set; }
    public int FailCount { get; set; }
}

public class PassFailReportDto
{
    public List<CoursePassFailDto> CourseStats { get; set; } = [];
    public List<PassFailTrendDto> SemesterTrend { get; set; } = [];
    public int TotalResults { get; set; }
    public int TotalPass { get; set; }
    public int TotalFail { get; set; }
    public double OverallPassRate { get; set; }
    public double OverallFailRate { get; set; }
}

public class CoursePassFailDto
{
    public string SubjectName { get; set; } = "";
    public int Total { get; set; }
    public int Pass { get; set; }
    public int Fail { get; set; }
    public double FailRate { get; set; }
    public decimal AvgGpa { get; set; }
}

public class PassFailFilterOptionsDto
{
    public List<PassFailFilterOptionDto> Majors { get; set; } = [];
    public List<PassFailFilterOptionDto> Specializations { get; set; } = [];
    public List<ProgramSubjectFilterOptionDto> ProgramSubjects { get; set; } = [];
    public List<SemesterFilterOptionDto> Semesters { get; set; } = [];
}

public class PassFailFilterOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
}

public class ProgramSubjectFilterOptionDto : PassFailFilterOptionDto
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = "";
    public string ProgramCode { get; set; } = "";
    public int ExpectedSemester { get; set; }
}

public class SemesterFilterOptionDto : PassFailFilterOptionDto
{
    public string AcademicYear { get; set; } = "";
}

public class PassFailTrendDto
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = "";
    public string AcademicYear { get; set; } = "";
    public int SemesterOrder { get; set; }
    public int Total { get; set; }
    public int Pass { get; set; }
    public int Fail { get; set; }
    public double PassRate { get; set; }
    public double FailRate { get; set; }
}

public class ScheduleChangeDto
{
    public int Id { get; set; }
    public string ChangeType { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateOnly Date { get; set; }
    public string SubjectName { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string SubstituteTeacherName { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}

public class RejectGradeUnlockRequest
{
    public string? LyDoTuChoi { get; set; }
}
