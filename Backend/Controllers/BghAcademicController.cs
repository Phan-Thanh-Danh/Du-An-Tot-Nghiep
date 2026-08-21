
using System.Text.Json;
using Backend.DTOs.Bgh;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Bgh;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.SuperAdmin + "," + AuthRoles.Admin + "," + AuthRoles.AcademicStaff)]
public class BghAcademicController : ControllerBase
{
    private const string SavedAcademicReportType = "bgh_academic_detail";
    private static readonly JsonSerializerOptions AcademicReportJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _db;
    private readonly IBghPerformanceCache _cache;

    public BghAcademicController(ApplicationDbContext db, IBghPerformanceCache cache)
    {
        _db = db;
        _cache = cache;
    }

    private async Task EnsureHasPermissionAsync(string permissionCode, CancellationToken ct = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var roleCode = currentUser?.Role ?? "hieu_truong";

        if (roleCode == "SuperAdmin" || roleCode == "sieu_quan_tri" || roleCode == "Admin" || roleCode == "quan_tri")
            return;

        var hasPerm = await _db.VaiTroQuyenHans
            .AsNoTracking()
            .AnyAsync(vp => vp.VaiTro != null &&
                           (vp.VaiTro.MaCodeVaiTro == roleCode || vp.VaiTro.MaCodeVaiTro == "hieu_truong") &&
                           vp.QuyenHan != null && vp.QuyenHan.MaCode == permissionCode, ct);

        if (!hasPerm)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, $"Vai trò của bạn chưa được cấp quyền '{permissionCode}' để thực hiện hành động này.");
        }
    }

    private (int CampusId, bool IsGlobal) GetUserScope()
    {
        var user = HttpContext.Items["CurrentUser"] as Backend.DTOs.Auth.CurrentUserContext;
        var campusId = user?.CampusId ?? 0;
        var isGlobal = user?.Role == AuthRoles.SuperAdmin ||
                       user?.Role == AuthRoles.Admin ||
                       user?.Role == AuthRoles.Principal ||
                       (user?.Email != null && (user.Email.Contains("bgh_all", StringComparison.OrdinalIgnoreCase) ||
                                                user.Email.Contains("p15", StringComparison.OrdinalIgnoreCase)));
        return (campusId, isGlobal);
    }

    [HttpGet("academic/overview")]
    [BghResponseCache(60)]
    public async Task<ActionResult<ApiResponseDto<AcademicOverviewDto>>> GetAcademicOverview(
        [FromQuery(Name = "campusId")] int? targetCampusId = null,
        [FromQuery(Name = "semesterId")] int? targetSemesterId = null,
        [FromQuery(Name = "specializationId")] int? targetSpecializationId = null)
    {
        await EnsureHasPermissionAsync("reports.read");
        var (userCampusId, isGlobal) = GetUserScope();
        var effectiveCampusId = isGlobal && targetCampusId.HasValue ? targetCampusId.Value : userCampusId;
        var useGlobalCampus = isGlobal && !targetCampusId.HasValue;

        var gradeQuery = _db.DiemSos.AsNoTracking().Where(d => useGlobalCampus || d.MaDonVi == effectiveCampusId);
        if (targetSemesterId.HasValue) gradeQuery = gradeQuery.Where(d => d.MaHocKy == targetSemesterId.Value);

        var totalStudents = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (useGlobalCampus || u.MaDonVi == effectiveCampusId));
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien" && (useGlobalCampus || u.MaDonVi == effectiveCampusId));
        var totalClasses = await _db.LopHanhChinhs.CountAsync(l => useGlobalCampus || l.MaDonVi == effectiveCampusId);
        var activeCourses = await _db.KhoaHocs.CountAsync(k =>
            (k.TrangThai == "dang_mo" || k.TrangThai == "da_xuat_ban") &&
            (useGlobalCampus || k.MaDonVi == effectiveCampusId));

        var avgGpa = await gradeQuery.AverageAsync(d => (decimal?)d.GpaMonHoc) ?? 0;
        var passCount = await gradeQuery.CountAsync(d => d.GpaMonHoc >= 4);
        var totalGrades = await gradeQuery.CountAsync();
        var passRate = totalGrades > 0 ? (double)passCount / totalGrades * 100 : 0;

        var atRiskCount = await gradeQuery
            .Where(d => d.GpaMonHoc < 4)
            .Select(d => d.MaHocSinh)
            .Distinct()
            .CountAsync();

        var distribution = await gradeQuery
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A (8.5 - 10.0)" :
                          d.GpaMonHoc >= 7 ? "B (7.0 - 8.4)" :
                          d.GpaMonHoc >= 5.5m ? "C (5.5 - 6.9)" :
                          d.GpaMonHoc >= 4 ? "D (4.0 - 5.4)" : "F (< 4.0)")
            .Select(g => new GradeDistributionDto
            {
                Grade = g.Key,
                Count = g.Count(),
                Percent = totalGrades > 0 ? Math.Round((double)g.Count() / totalGrades * 100, 1) : 0
            })
            .OrderByDescending(g => g.Grade.StartsWith("A") ? 5 : g.Grade.StartsWith("B") ? 4 : g.Grade.StartsWith("C") ? 3 : g.Grade.StartsWith("D") ? 2 : 1)
            .ToListAsync();

        var topSubjects = await gradeQuery
            .Where(d => d.MonHoc != null)
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

        var semesterTrend = await _db.DiemSos.AsNoTracking()
            .Where(d => d.HocKy != null && (useGlobalCampus || d.MaDonVi == effectiveCampusId))
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
    [BghResponseCache(120)]
    public async Task<ActionResult<ApiResponseDto<GpaReportDto>>> GetGpaReports(
        [FromQuery(Name = "campusId")] int? targetCampusId = null,
        [FromQuery(Name = "semesterId")] int? targetSemesterId = null,
        [FromQuery(Name = "specializationId")] int? targetSpecializationId = null)
    {
        await EnsureHasPermissionAsync("reports.read");
        var (userCampusId, isGlobal) = GetUserScope();
        var effectiveCampusId = isGlobal && targetCampusId.HasValue ? targetCampusId.Value : userCampusId;
        var useGlobalCampus = isGlobal && !targetCampusId.HasValue;

        var gradeQuery = _db.DiemSos.AsNoTracking().Where(d => useGlobalCampus || d.MaDonVi == effectiveCampusId);
        if (targetSemesterId.HasValue) gradeQuery = gradeQuery.Where(d => d.MaHocKy == targetSemesterId.Value);

        var semesterGroups = await gradeQuery
            .Where(d => d.HocKy != null)
            .GroupBy(d => new { d.MaHocKy, TenHocKy = d.HocKy!.TenHocKy ?? "" })
            .Select(g => new GpaTrendDto
            {
                Semester = g.Key.TenHocKy,
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.Semester)
            .ToListAsync();

        var distribution = await gradeQuery
            .GroupBy(d => d.GpaMonHoc >= 8.5m ? "A (8.5 - 10.0)" :
                          d.GpaMonHoc >= 7 ? "B (7.0 - 8.4)" :
                          d.GpaMonHoc >= 5.5m ? "C (5.5 - 6.9)" :
                          d.GpaMonHoc >= 4 ? "D (4.0 - 5.4)" : "F (< 4.0)")
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
    public async Task<ActionResult<ApiResponseDto<AtRiskReportDto>>> GetAtRiskStudents(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? studentId = null,
        [FromQuery] int? semesterId = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureHasPermissionAsync("reports.ai_analysis", cancellationToken);
        var (campusId, isGlobal) = GetUserScope();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var normalizedKeyword = keyword?.Trim();
        Response.Headers.CacheControl = "private, max-age=15, stale-while-revalidate=45";

        var cacheKey = BghCacheKey.For(
            HttpContext,
            "academic-at-risk",
            pageIndex,
            pageSize,
            studentId,
            semesterId,
            normalizedKeyword);

        var data = await _cache.GetOrCreateAsync(
            cacheKey,
            TimeSpan.FromSeconds(60),
            async ct =>
            {
                var riskAggregates = _db.DiemSos
                    .AsNoTracking()
                    .Where(d => (isGlobal || d.MaDonVi == campusId) &&
                                (!studentId.HasValue || d.MaHocSinh == studentId.Value) &&
                                (!semesterId.HasValue || d.MaHocKy == semesterId.Value))
                    .GroupBy(d => d.MaHocSinh)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        AvgGpa = g.Average(d => d.GpaMonHoc),
                        FailCount = g.Count(d => d.GpaMonHoc < 4),
                        RiskSubjectName = g
                            .Where(d => d.GpaMonHoc < 4)
                            .OrderBy(d => d.GpaMonHoc)
                            .ThenBy(d => d.MaMonHoc)
                            .Select(d => d.MonHoc != null ? d.MonHoc.TenMonHoc : "")
                            .FirstOrDefault() ?? ""
                    })
                    .Where(x => x.FailCount > 0);

                var query =
                    from risk in riskAggregates
                    join student in _db.NguoiDungs.AsNoTracking()
                        on risk.StudentId equals student.MaNguoiDung
                    join academicClass in _db.LopHanhChinhs.AsNoTracking()
                        on student.MaLop equals (int?)academicClass.MaLop into classJoin
                    from academicClass in classJoin.DefaultIfEmpty()
                    where (!studentId.HasValue || student.MaNguoiDung == studentId.Value) &&
                          (string.IsNullOrEmpty(normalizedKeyword) ||
                           student.HoTen.Contains(normalizedKeyword) ||
                           student.Email.Contains(normalizedKeyword))
                    select new AtRiskStudentDto
                    {
                        Id = student.MaNguoiDung,
                        Name = student.HoTen,
                        Email = student.Email,
                        ClassCode = academicClass != null ? academicClass.MaCodeLop : "",
                        AvgGpa = Math.Round(risk.AvgGpa, 2),
                        FailCount = risk.FailCount,
                        RiskSubjectName = risk.RiskSubjectName
                    };

                var summary = await query
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        TotalAtRisk = g.Count(),
                        AvgGpa = g.Average(x => x.AvgGpa),
                        CriticalCount = g.Count(x => x.FailCount >= 3)
                    })
                    .SingleOrDefaultAsync(ct);
                var students = await query
                    .OrderBy(x => x.AvgGpa)
                    .ThenByDescending(x => x.FailCount)
                    .ThenBy(x => x.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);
                var totalStudents = await _db.NguoiDungs
                    .AsNoTracking()
                    .CountAsync(u => u.VaiTroChinh == "hoc_sinh" && (isGlobal || u.MaDonVi == campusId), ct);

                return new AtRiskReportDto
                {
                    TotalAtRisk = summary?.TotalAtRisk ?? 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling((summary?.TotalAtRisk ?? 0) / (double)pageSize),
                    Students = students,
                    Summary = new AtRiskSummaryDto
                    {
                        TotalStudents = totalStudents,
                        AvgGpaAtRisk = Math.Round(summary?.AvgGpa ?? 0, 2),
                        CriticalCount = summary?.CriticalCount ?? 0
                    }
                };
            },
            cancellationToken);

        return Ok(ApiResponseDto<AtRiskReportDto>.Ok(data));
    }

    [HttpGet("academic/at-risk/{studentId:int}/history")]
    [BghResponseCache(60)]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAtRiskStudentHistory(int studentId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var student = await _db.NguoiDungs
            .AsNoTracking()
            .Where(x => x.MaNguoiDung == studentId &&
                        x.VaiTroChinh == "hoc_sinh" &&
                        (isGlobal || x.MaDonVi == campusId))
            .Select(x => new
            {
                Id = x.MaNguoiDung,
                Name = x.HoTen,
                x.Email,
                x.MaDonVi,
                ClassCode = x.Lop != null ? x.Lop.MaCodeLop : "",
                ProgramId = x.Lop != null ? x.Lop.MaChuongTrinh : null
            })
            .FirstOrDefaultAsync();

        if (student == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy sinh viên trong phạm vi quản lý."));

        var grades = await _db.DiemSos
            .AsNoTracking()
            .Where(x => x.MaHocSinh == studentId && (isGlobal || x.MaDonVi == campusId))
            .OrderBy(x => x.HocKy!.NgayBatDau)
            .ThenBy(x => x.MonHoc!.TenMonHoc)
            .Select(x => new
            {
                SemesterId = x.MaHocKy,
                Semester = x.HocKy != null ? x.HocKy.TenHocKy : "",
                AcademicYear = x.HocKy != null ? x.HocKy.NamHoc : "",
                SubjectId = x.MaMonHoc,
                SubjectCode = x.MonHoc != null ? x.MonHoc.MaCodeMonHoc : "",
                SubjectName = x.MonHoc != null ? x.MonHoc.TenMonHoc : "",
                Credits = student.ProgramId.HasValue
                    ? _db.MonHocTrongChuongTrinhs
                        .Where(subject => subject.MaChuongTrinh == student.ProgramId.Value &&
                                          subject.MaMonHoc == x.MaMonHoc && subject.ConHoatDong)
                        .Select(subject => (int?)subject.SoTinChi)
                        .FirstOrDefault() ?? (x.MonHoc != null ? x.MonHoc.SoTinChi : 0)
                    : (x.MonHoc != null ? x.MonHoc.SoTinChi : 0),
                x.DiemQuaTrinh,
                x.DiemGiuaKy,
                x.DiemCuoiKy,
                Grade = x.GpaMonHoc,
                x.TrangThai,
                x.LyDoRot,
                RiskProbability = _db.BaoCaoRuiRoRotMons
                    .Where(report => report.MaHocSinh == studentId &&
                                     report.MaMonHoc == x.MaMonHoc &&
                                     report.MaHocKy == x.MaHocKy)
                    .Select(report => (decimal?)report.XacSuatRotMon)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var attendance = await _db.DiemDanhs
            .AsNoTracking()
            .Where(x => x.MaHocSinh == studentId &&
                        (isGlobal || x.MaDonVi == campusId) &&
                        x.BuoiHoc != null && x.BuoiHoc.KhoaHoc != null &&
                        x.BuoiHoc.KhoaHoc.MaHocKy != null)
            .GroupBy(x => new
            {
                SemesterId = x.BuoiHoc!.KhoaHoc!.MaHocKy!.Value,
                Semester = x.BuoiHoc.KhoaHoc.HocKy != null
                    ? x.BuoiHoc.KhoaHoc.HocKy.TenHocKy
                    : ""
            })
            .Select(g => new
            {
                g.Key.SemesterId,
                g.Key.Semester,
                TotalSessions = g.Count(),
                PresentSessions = g.Count(x => x.TrangThai == "co_mat" || x.TrangThai == "di_muon"),
                AbsentSessions = g.Count(x => x.TrangThai == "vang" || x.TrangThai == "co_phep"),
                Rate = g.Count() == 0
                    ? 0
                    : Math.Round(g.Count(x => x.TrangThai == "co_mat" || x.TrangThai == "di_muon") * 100.0 / g.Count(), 1)
            })
            .OrderBy(x => x.SemesterId)
            .ToListAsync();

        var failCount = grades.Count(x => x.Grade < 4 || x.TrangThai == "rot");
        var avgGpa = grades.Count == 0 ? 0 : Math.Round(grades.Average(x => x.Grade), 2);
        var history = grades
            .GroupBy(x => new { x.SemesterId, x.Semester, x.AcademicYear })
            .Select(group => new
            {
                group.Key.SemesterId,
                group.Key.Semester,
                group.Key.AcademicYear,
                AvgGpa = Math.Round(group.Average(x => x.Grade), 2),
                Courses = group.ToList()
            })
            .ToList();

        return Ok(ApiResponseDto<object>.Ok(new
        {
            Student = student,
            Summary = new { FailCount = failCount, AvgGpa = avgGpa, TotalSubjects = grades.Count },
            AcademicHistory = history,
            AttendanceHistory = attendance
        }));
    }

    [HttpGet("academic/reports")]
    [BghResponseCache(120)]
    public async Task<ActionResult<ApiResponseDto<AcademicReportDataDto>>> GetAcademicReports(
        [FromQuery(Name = "campusId")] int? targetCampusId = null,
        [FromQuery(Name = "semesterId")] int? targetSemesterId = null,
        [FromQuery] string? reportType = null,
        CancellationToken cancellationToken = default)
    {
        var data = await BuildAcademicReportAsync(
            targetCampusId,
            targetSemesterId,
            reportType,
            cancellationToken);
        return Ok(ApiResponseDto<AcademicReportDataDto>.Ok(data));
    }

    [HttpPost("academic/reports")]
    public async Task<ActionResult<ApiResponseDto<SavedAcademicReportResultDto>>> CreateAcademicReport(
        [FromBody] SaveAcademicReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
            return Unauthorized(ApiResponseDto.Fail("Không xác định được người tạo báo cáo."));

        var reportType = NormalizeAcademicReportType(request.ReportType);
        if (reportType == null)
            return BadRequest(ApiResponseDto.Fail("Loại báo cáo chỉ nhận class, subject hoặc campus."));

        if (request.Name?.Trim().Length > 150)
            return BadRequest(ApiResponseDto.Fail("Tên báo cáo không được vượt quá 150 ký tự."));

        var (userCampusId, isGlobal) = GetUserScope();
        var effectiveCampusId = isGlobal && request.CampusId.HasValue
            ? request.CampusId.Value
            : userCampusId;
        var useGlobalCampus = isGlobal && !request.CampusId.HasValue;

        if (isGlobal && request.CampusId.HasValue &&
            !await _db.DonVis.AsNoTracking().AnyAsync(
                campus => campus.MaDonVi == request.CampusId.Value,
                cancellationToken))
            return BadRequest(ApiResponseDto.Fail("Cơ sở được chọn không tồn tại."));

        if (request.SemesterId.HasValue &&
            !await _db.HocKys.AsNoTracking().AnyAsync(
                semester => semester.MaHocKy == request.SemesterId.Value &&
                            (useGlobalCampus || semester.MaDonVi == effectiveCampusId),
                cancellationToken))
            return BadRequest(ApiResponseDto.Fail("Học kỳ được chọn không tồn tại trong phạm vi cơ sở."));

        var report = await BuildAcademicReportAsync(
            request.CampusId,
            request.SemesterId,
            reportType,
            cancellationToken);

        var ownerCampusId = await ResolveReportOwnerCampusIdAsync(
            currentUser,
            report.Filter.CampusId,
            cancellationToken);
        if (!ownerCampusId.HasValue)
            return BadRequest(ApiResponseDto.Fail("Không tìm thấy cơ sở hợp lệ để lưu báo cáo."));

        var parameters = new SavedAcademicReportParameters
        {
            Name = string.IsNullOrWhiteSpace(request.Name)
                ? BuildDefaultAcademicReportName(reportType, report.GeneratedAt)
                : request.Name.Trim(),
            ReportType = reportType,
            CampusId = report.Filter.CampusId,
            SemesterId = report.Filter.SemesterId
        };

        var entity = new XuatBaoCao
        {
            NguoiYeuCau = currentUser.UserId,
            MaDonVi = ownerCampusId.Value,
            LoaiBaoCao = SavedAcademicReportType,
            ThamSoJson = JsonSerializer.Serialize(parameters, AcademicReportJsonOptions),
            TrangThai = "hoan_thanh",
            NgayTao = report.GeneratedAt
        };

        _db.XuatBaoCaos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.RemoveByPrefix("bgh:");

        var saved = MapSavedAcademicReport(entity, parameters);
        return Ok(ApiResponseDto<SavedAcademicReportResultDto>.Ok(
            new SavedAcademicReportResultDto { SavedReport = saved, Report = report },
            "Tạo và lưu báo cáo thành công."));
    }

    [HttpGet("academic/reports/saved")]
    public async Task<ActionResult<ApiResponseDto<List<SavedAcademicReportDto>>>> GetSavedAcademicReports(
        CancellationToken cancellationToken = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
            return Unauthorized(ApiResponseDto.Fail("Không xác định được người xem báo cáo."));

        var entities = await _db.XuatBaoCaos
            .AsNoTracking()
            .Where(report => report.NguoiYeuCau == currentUser.UserId &&
                             report.LoaiBaoCao == SavedAcademicReportType)
            .OrderByDescending(report => report.NgayTao)
            .Take(200)
            .ToListAsync(cancellationToken);

        var reports = entities
            .Select(entity => MapSavedAcademicReport(entity, ReadSavedAcademicReportParameters(entity.ThamSoJson)))
            .ToList();

        return Ok(ApiResponseDto<List<SavedAcademicReportDto>>.Ok(reports));
    }

    [HttpGet("academic/reports/saved/{reportId:int}")]
    public async Task<ActionResult<ApiResponseDto<SavedAcademicReportResultDto>>> GetSavedAcademicReport(
        int reportId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
            return Unauthorized(ApiResponseDto.Fail("Không xác định được người xem báo cáo."));

        var entity = await _db.XuatBaoCaos
            .AsNoTracking()
            .FirstOrDefaultAsync(report => report.MaXuatBaoCao == reportId &&
                                           report.NguoiYeuCau == currentUser.UserId &&
                                           report.LoaiBaoCao == SavedAcademicReportType,
                cancellationToken);
        if (entity == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy báo cáo đã lưu."));

        var parameters = ReadSavedAcademicReportParameters(entity.ThamSoJson);
        var report = await BuildAcademicReportAsync(
            parameters.CampusId,
            parameters.SemesterId,
            parameters.ReportType,
            cancellationToken);

        return Ok(ApiResponseDto<SavedAcademicReportResultDto>.Ok(new SavedAcademicReportResultDto
        {
            SavedReport = MapSavedAcademicReport(entity, parameters),
            Report = report
        }));
    }

    [HttpDelete("academic/reports/saved/{reportId:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteSavedAcademicReport(
        int reportId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
            return Unauthorized(ApiResponseDto.Fail("Không xác định được người xóa báo cáo."));

        var entity = await _db.XuatBaoCaos
            .FirstOrDefaultAsync(report => report.MaXuatBaoCao == reportId &&
                                           report.NguoiYeuCau == currentUser.UserId &&
                                           report.LoaiBaoCao == SavedAcademicReportType,
                cancellationToken);
        if (entity == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy báo cáo đã lưu."));

        _db.XuatBaoCaos.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _cache.RemoveByPrefix("bgh:");

        return Ok(ApiResponseDto<object>.Ok(new { ReportId = reportId }, "Đã xóa báo cáo đã lưu."));
    }

    private async Task<AcademicReportDataDto> BuildAcademicReportAsync(
        int? targetCampusId,
        int? targetSemesterId,
        string? reportType,
        CancellationToken cancellationToken)
    {
        var (userCampusId, isGlobal) = GetUserScope();
        var effectiveCampusId = isGlobal && targetCampusId.HasValue ? targetCampusId.Value : userCampusId;
        var useGlobalFilter = isGlobal && !targetCampusId.HasValue;
        var normalizedReportType = NormalizeAcademicReportType(reportType) ?? "class";

        var totalStudents = await _db.NguoiDungs
            .AsNoTracking()
            .CountAsync(u => u.VaiTroChinh == "hoc_sinh" &&
                             (useGlobalFilter || u.MaDonVi == effectiveCampusId), cancellationToken);
        var totalTeachers = await _db.NguoiDungs
            .AsNoTracking()
            .CountAsync(u => u.VaiTroChinh == "giao_vien" &&
                             (useGlobalFilter || u.MaDonVi == effectiveCampusId), cancellationToken);
        var totalClasses = await _db.LopHanhChinhs
            .AsNoTracking()
            .CountAsync(l => useGlobalFilter || l.MaDonVi == effectiveCampusId, cancellationToken);
        var activeCourses = await _db.KhoaHocs
            .AsNoTracking()
            .CountAsync(k => (k.TrangThai == "dang_mo" || k.TrangThai == "da_xuat_ban") &&
                             (useGlobalFilter || k.MaDonVi == effectiveCampusId), cancellationToken);
        var avgGpa = await _db.DiemSos
            .AsNoTracking()
            .Where(d => (useGlobalFilter || d.MaDonVi == effectiveCampusId) &&
                        (!targetSemesterId.HasValue || d.MaHocKy == targetSemesterId.Value))
            .AverageAsync(d => (decimal?)d.GpaMonHoc, cancellationToken) ?? 0;

        var semesterStats = await _db.DiemSos
            .AsNoTracking()
            .Where(d => d.HocKy != null &&
                        (useGlobalFilter || d.MaDonVi == effectiveCampusId) &&
                        (!targetSemesterId.HasValue || d.MaHocKy == targetSemesterId.Value))
            .GroupBy(d => new
            {
                d.MaHocKy,
                TenHocKy = d.HocKy!.TenHocKy ?? "",
                d.HocKy.NgayBatDau
            })
            .Select(g => new AcademicReportSemesterStatDto
            {
                SemesterId = g.Key.MaHocKy,
                Semester = g.Key.TenHocKy,
                StartDate = g.Key.NgayBatDau,
                TotalGrades = g.Count(),
                PassCount = g.Count(d => d.GpaMonHoc >= 4),
                FailCount = g.Count(d => d.GpaMonHoc < 4),
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                StudentCount = g.Select(d => d.MaHocSinh).Distinct().Count()
            })
            .OrderBy(g => g.StartDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var departmentStats = await (
            from grade in _db.DiemSos.AsNoTracking()
            join student in _db.NguoiDungs.AsNoTracking()
                on grade.MaHocSinh equals student.MaNguoiDung
            join academicClass in _db.LopHanhChinhs.AsNoTracking()
                on student.MaLop equals (int?)academicClass.MaLop
            join program in _db.ChuongTrinhDaoTaos.AsNoTracking()
                on academicClass.MaChuongTrinh equals (int?)program.MaChuongTrinh
            join specialization in _db.ChuyenNganhs.AsNoTracking()
                on program.MaChuyenNganh equals specialization.MaChuyenNganh
            where (useGlobalFilter || grade.MaDonVi == effectiveCampusId) &&
                  (!targetSemesterId.HasValue || grade.MaHocKy == targetSemesterId.Value)
            group grade by new { specialization.MaChuyenNganh, specialization.TenChuyenNganh } into g
            select new AcademicReportDepartmentStatDto
            {
                DepartmentId = g.Key.MaChuyenNganh,
                DepartmentName = g.Key.TenChuyenNganh,
                TotalGrades = g.Count(),
                PassCount = g.Count(d => d.GpaMonHoc >= 4),
                FailCount = g.Count(d => d.GpaMonHoc < 4),
                AvgGpa = Math.Round(g.Average(d => (decimal?)d.GpaMonHoc) ?? 0, 2),
                PassRate = g.Count() > 0
                    ? Math.Round((double)g.Count(d => d.GpaMonHoc >= 4) / g.Count() * 100, 1)
                    : 0
            })
            .OrderByDescending(d => d.TotalGrades)
            .ToListAsync(cancellationToken);

        return new AcademicReportDataDto
        {
            Filter = new AcademicReportFilterDto
            {
                ReportType = normalizedReportType,
                CampusId = useGlobalFilter ? null : effectiveCampusId,
                SemesterId = targetSemesterId
            },
            GeneratedAt = DateTime.UtcNow,
            Summary = new AcademicReportSummaryDto
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                ActiveCourses = activeCourses,
                AvgGpa = Math.Round(avgGpa, 2)
            },
            MonthlyStats = semesterStats,
            DepartmentStats = departmentStats
        };
    }

    private async Task<int?> ResolveReportOwnerCampusIdAsync(
        CurrentUserContext currentUser,
        int? reportCampusId,
        CancellationToken cancellationToken)
    {
        if (currentUser.CampusId > 0 &&
            await _db.DonVis.AsNoTracking().AnyAsync(d => d.MaDonVi == currentUser.CampusId, cancellationToken))
            return currentUser.CampusId;

        if (reportCampusId.HasValue &&
            await _db.DonVis.AsNoTracking().AnyAsync(d => d.MaDonVi == reportCampusId.Value, cancellationToken))
            return reportCampusId.Value;

        return await _db.DonVis
            .AsNoTracking()
            .Where(d => d.ConHoatDong)
            .OrderBy(d => d.MaDonVi)
            .Select(d => (int?)d.MaDonVi)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? NormalizeAcademicReportType(string? reportType)
    {
        var normalized = string.IsNullOrWhiteSpace(reportType)
            ? "class"
            : reportType.Trim().ToLowerInvariant();
        return normalized is "class" or "subject" or "campus" ? normalized : null;
    }

    private static string BuildDefaultAcademicReportName(string reportType, DateTime createdAt)
    {
        var label = reportType switch
        {
            "subject" => "Báo cáo theo môn học",
            "campus" => "Báo cáo theo cơ sở",
            _ => "Báo cáo theo lớp"
        };
        return $"{label} - {createdAt:dd/MM/yyyy HH:mm}";
    }

    private static SavedAcademicReportParameters ReadSavedAcademicReportParameters(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new SavedAcademicReportParameters();

        try
        {
            return JsonSerializer.Deserialize<SavedAcademicReportParameters>(json, AcademicReportJsonOptions)
                   ?? new SavedAcademicReportParameters();
        }
        catch (JsonException)
        {
            return new SavedAcademicReportParameters();
        }
    }

    private static SavedAcademicReportDto MapSavedAcademicReport(
        XuatBaoCao entity,
        SavedAcademicReportParameters parameters) => new()
    {
        Id = entity.MaXuatBaoCao,
        Name = string.IsNullOrWhiteSpace(parameters.Name)
            ? BuildDefaultAcademicReportName(parameters.ReportType, entity.NgayTao)
            : parameters.Name,
        ReportType = NormalizeAcademicReportType(parameters.ReportType) ?? "class",
        CampusId = parameters.CampusId,
        SemesterId = parameters.SemesterId,
        Status = entity.TrangThai,
        CreatedAt = entity.NgayTao
    };

    [HttpGet("academic/pass-fail/filters")]
    [BghResponseCache(600)]
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

        var semesters = await _db.HocKys
            .AsNoTracking()
            .Where(h => isGlobal || h.MaDonVi == campusId || _db.DiemSos.Any(d => d.MaHocKy == h.MaHocKy))
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
    [BghResponseCache(120)]
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
            .GroupBy(d => new { d.SubjectId, d.SubjectName, d.ClassCode })
            .Select(g => new CoursePassFailDto
            {
                SubjectName = g.Key.SubjectName,
                ClassCode = !string.IsNullOrEmpty(g.Key.ClassCode) ? g.Key.ClassCode : "Lớp HP",
                TeacherName = "Giáo viên phụ trách",
                Reason = "Điểm tổng kết dưới ngưỡng đạt",
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
                ProgramSubjectId = programSubject.MaChuongTrinhMonHoc,
                ClassCode = academicClass.MaCodeLop
            };
    }

    private sealed class PassFailGradeRow
    {
        public decimal Gpa { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = "";
        public string ClassCode { get; init; } = "";
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
    [BghResponseCache(20)]
    public async Task<ActionResult<ApiResponseDto<List<ScheduleChangeDto>>>> GetScheduleChanges()
    {
        var (campusId, isGlobal) = GetUserScope();

        var rows = await _db.BuoiHocs
            .AsNoTracking()
            .Where(b => (b.LoaiThayDoi != null || b.TrangThaiBuoi == "da_huy") && (isGlobal || (b.KhoaHoc != null && b.KhoaHoc.MaDonVi == campusId)))
            .OrderByDescending(b => b.NgayCapNhat)
            .Take(50)
            .Select(b => new
            {
                b.MaBuoiHoc,
                b.LoaiThayDoi,
                b.TrangThaiBuoi,
                b.LyDoThayDoi,
                b.GhiChu,
                b.NgayHoc,
                SubjectName = b.KhoaHoc != null && b.KhoaHoc.MonHoc != null
                    ? b.KhoaHoc.MonHoc.TenMonHoc
                    : "",
                ClassCode = b.KhoaHoc != null && b.KhoaHoc.Lop != null
                    ? b.KhoaHoc.Lop.MaCodeLop
                    : "",
                TeacherName = b.KhoaHoc != null && b.KhoaHoc.GiaoVien != null
                    ? b.KhoaHoc.GiaoVien.HoTen
                    : "",
                SubstituteTeacherName = b.MaGiaoVienDayThay != null
                    ? b.GiaoVienDayThay!.HoTen
                    : "",
                UpdatedAt = b.NgayCapNhat ?? b.NgayTao,
                OriginalDayOfWeek = b.Tkb != null ? b.Tkb.ThuTrongTuan : (int?)null,
                OriginalShift = b.Tkb != null && b.Tkb.CaHoc != null ? b.Tkb.CaHoc.TenCa : "",
                OriginalRoom = b.Tkb != null && b.Tkb.Phong != null ? b.Tkb.Phong.MaCodePhong : "",
                NewShift = b.CaHoc != null ? b.CaHoc.TenCa : "",
                NewRoom = b.Phong != null ? b.Phong.MaCodePhong : ""
            })
            .ToListAsync();

        var changes = rows.Select(b =>
        {
            var proposal = ReadScheduleChangeProposal(b.GhiChu);
            var changeType = GetScheduleChangeType(b.LoaiThayDoi, b.LyDoThayDoi, b.TrangThaiBuoi);
            var status = GetScheduleChangeStatus(b.TrangThaiBuoi, b.LyDoThayDoi);
            var subject = b.SubjectName;
            var proposedTeacher = proposal?.NewTeacherName ?? b.SubstituteTeacherName;
            var teacher = string.IsNullOrWhiteSpace(proposedTeacher)
                ? b.TeacherName
                : $"{b.TeacherName} → {proposedTeacher}";

            return new ScheduleChangeDto
            {
                Id = b.MaBuoiHoc,
                ChangeType = changeType,
                Type = changeType == "day_bu" ? "makeup" : changeType == "huy_buoi" ? "cancel" : "swap",
                Status = status,
                Reason = CleanScheduleChangeReason(b.LyDoThayDoi),
                Date = b.NgayHoc,
                SubjectName = subject,
                Subject = subject,
                ClassCode = b.ClassCode,
                TeacherName = b.TeacherName,
                Teacher = teacher,
                SubstituteTeacherName = proposedTeacher,
                OldSlot = FormatOriginalSlot(b.OriginalDayOfWeek, b.OriginalShift, b.OriginalRoom),
                NewSlot = FormatNewSlot(
                    proposal?.NewDate ?? b.NgayHoc,
                    proposal?.NewShiftName ?? b.NewShift,
                    proposal?.NewRoomCode ?? b.NewRoom),
                UpdatedAt = b.UpdatedAt,
                Updated = b.UpdatedAt.ToString("dd/MM/yyyy HH:mm")
            };
        }).ToList();

        return Ok(ApiResponseDto<List<ScheduleChangeDto>>.Ok(changes));
    }

    [HttpPost("schedule/changes/{changeId:int}/approve")]
    public async Task<ActionResult<ApiResponseDto<object>>> ApproveScheduleChange(int changeId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var lesson = await _db.BuoiHocs
            .Include(x => x.Tkb)
            .Include(x => x.KhoaHoc)
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == changeId &&
                                      (isGlobal || (x.KhoaHoc != null && x.KhoaHoc.MaDonVi == campusId)));
        if (lesson == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy yêu cầu thay đổi lịch."));
        if (GetScheduleChangeStatus(lesson.TrangThaiBuoi, lesson.LyDoThayDoi) != "pending")
            return Conflict(ApiResponseDto.Fail("Yêu cầu thay đổi lịch đã được xử lý."));

        var proposal = ReadScheduleChangeProposal(lesson.GhiChu);
        if (proposal != null)
        {
            lesson.NgayHoc = proposal.NewDate;
            lesson.MaCaHoc = proposal.NewShiftId;
            lesson.MaPhong = proposal.NewRoomId;
            lesson.MaGiaoVienDayThay = proposal.NewTeacherId;
        }

        lesson.TrangThaiBuoi = lesson.LoaiThayDoi switch
        {
            "huy_buoi" => "da_huy",
            "doi_giang_vien" => "day_thay",
            _ => "doi_lich"
        };
        lesson.LyDoThayDoi = AddScheduleChangeDecision("[Đã duyệt]", lesson.LyDoThayDoi);
        lesson.NgayCapNhat = DateTime.UtcNow;
        AddScheduleChangeAudit(lesson, "APPROVE_SCHEDULE_CHANGE", "phê duyệt");
        await _db.SaveChangesAsync();
        _cache.RemoveByPrefix("bgh:");

        return Ok(ApiResponseDto<object>.Ok(new { id = changeId, status = "approved" }, "Đã duyệt thay đổi lịch."));
    }

    [HttpPost("schedule/changes/{changeId:int}/reject")]
    public async Task<ActionResult<ApiResponseDto<object>>> RejectScheduleChange(int changeId)
    {
        var (campusId, isGlobal) = GetUserScope();
        var lesson = await _db.BuoiHocs
            .Include(x => x.Tkb)
            .Include(x => x.KhoaHoc)
            .FirstOrDefaultAsync(x => x.MaBuoiHoc == changeId &&
                                      (isGlobal || (x.KhoaHoc != null && x.KhoaHoc.MaDonVi == campusId)));
        if (lesson == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy yêu cầu thay đổi lịch."));
        if (GetScheduleChangeStatus(lesson.TrangThaiBuoi, lesson.LyDoThayDoi) != "pending")
            return Conflict(ApiResponseDto.Fail("Yêu cầu thay đổi lịch đã được xử lý."));

        if (lesson.Tkb != null)
        {
            lesson.MaCaHoc = lesson.Tkb.MaCaHoc;
            lesson.MaPhong = lesson.Tkb.MaPhong;
        }
        lesson.MaGiaoVienDayThay = null;
        lesson.TrangThaiBuoi = "du_kien";
        lesson.LyDoThayDoi = AddScheduleChangeDecision("[Từ chối]", lesson.LyDoThayDoi);
        lesson.NgayCapNhat = DateTime.UtcNow;
        AddScheduleChangeAudit(lesson, "REJECT_SCHEDULE_CHANGE", "từ chối");
        await _db.SaveChangesAsync();
        _cache.RemoveByPrefix("bgh:");

        return Ok(ApiResponseDto<object>.Ok(new { id = changeId, status = "rejected" }, "Đã từ chối thay đổi lịch."));
    }

    private void AddScheduleChangeAudit(BuoiHoc lesson, string action, string decision)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        _db.NhatKyKiemToans.Add(new NhatKyKiemToan
        {
            MaDonVi = lesson.KhoaHoc?.MaDonVi ?? currentUser?.CampusId ?? 0,
            LoaiDoiTuong = "BuoiHoc",
            MaDoiTuong = lesson.MaBuoiHoc.ToString(),
            HanhDong = action,
            MoTa = $"BGH {decision} thay đổi lịch buổi học #{lesson.MaBuoiHoc}",
            NguoiThayDoi = currentUser?.UserId,
            ThoiDiemThayDoi = DateTime.UtcNow
        });
    }

    private static string GetScheduleChangeStatus(string status, string? reason)
    {
        if (reason?.StartsWith("[Từ chối]", StringComparison.OrdinalIgnoreCase) == true)
            return "rejected";
        if (reason?.StartsWith("[Đã duyệt]", StringComparison.OrdinalIgnoreCase) == true)
            return "approved";
        return status switch
        {
            "du_kien" => "pending",
            "da_huy" => "rejected",
            _ => "approved"
        };
    }

    private static string GetScheduleChangeType(string? type, string? reason, string status)
    {
        if (reason?.StartsWith("[Dạy bù]", StringComparison.OrdinalIgnoreCase) == true)
            return "day_bu";
        if (status == "da_huy" && string.IsNullOrWhiteSpace(type))
            return "huy_buoi";
        return type ?? "doi_lich";
    }

    private static string FormatOriginalSlot(int? dayOfWeek, string shift, string room)
    {
        var day = dayOfWeek.HasValue ? $"Thứ {dayOfWeek}" : "Lịch gốc";
        return string.Join(" · ", new[] { day, shift, room }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string FormatNewSlot(DateOnly date, string shift, string room) =>
        string.Join(" · ", new[] { date.ToString("dd/MM/yyyy"), shift, room }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static readonly JsonSerializerOptions ScheduleChangeJsonOptions = new(JsonSerializerDefaults.Web);

    private static ScheduleChangeProposal? ReadScheduleChangeProposal(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<ScheduleChangeProposal>(json, ScheduleChangeJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string CleanScheduleChangeReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return "";
        return reason
            .Replace("[Đã duyệt]", "", StringComparison.OrdinalIgnoreCase)
            .Replace("[Từ chối]", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string AddScheduleChangeDecision(string decision, string? reason) =>
        $"{decision} {CleanScheduleChangeReason(reason)}".Trim();

    // ===== Conflict Resolution Endpoints =====

    [HttpPost("schedule/conflicts/{id}/resolve")]
    public async Task<ActionResult<ApiResponseDto<object>>> ResolveScheduleConflict(string id)
    {
        var user = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var userId = user!.UserId;

        _db.NhatKyKiemToans.Add(new Models.NhatKyKiemToan
        {
            MaDonVi = user.CampusId,
            LoaiDoiTuong = "ScheduleConflict",
            MaDoiTuong = id,
            HanhDong = "RESOLVE_CONFLICT",
            MoTa = $"BGH đã đánh dấu xử lý xung đột #{id}",
            NguoiThayDoi = userId,
            ThoiDiemThayDoi = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _cache.RemoveByPrefix("bgh:");
        return Ok(ApiResponseDto<object>.Ok(new { message = "Đã đánh dấu xử lý xung đột thành công.", id }));
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
        _cache.RemoveByPrefix("bgh:");

        return Ok(ApiResponseDto<object>.Ok(new
        {
            message = "Đã từ chối yêu cầu mở khoá bảng điểm.",
            requestId = yeuCau.MaYcSuaDiem
        }));
    }
    [HttpGet("academic/campus-comparison")]
    [BghResponseCache(300)]
    public async Task<ActionResult<ApiResponseDto<List<CampusComparisonDto>>>> GetCampusComparison(CancellationToken cancellationToken)
    {
        var cacheKey = "bgh:campus-comparison:v3";
        var cachedData = await _cache.GetOrCreateAsync(
            cacheKey,
            TimeSpan.FromMinutes(30),
            async ct =>
            {
                // Include both co_so and co_so_con campuses
                var campuses = await _db.DonVis
                    .Where(d => (d.CapDonVi == "co_so" || d.CapDonVi == "co_so_con") && d.ConHoatDong)
                    .OrderBy(d => d.TenDonVi)
                    .ToListAsync(ct);

                // Run all per-campus queries in parallel for performance
                var tasks = campuses.Select(async campus =>
                {
                    var campusId = campus.MaDonVi;

                    // Students — active hoc_sinh in campus
                    var studentsTask = _db.NguoiDungs
                        .Where(u => u.MaDonVi == campusId && u.VaiTroChinh == "hoc_sinh")
                        .CountAsync(ct);

                    // GPA & PassRate — all grades (no TrangThai filter, matches other bgh endpoints)
                    var gradeStatsTask = _db.DiemSos
                        .Where(d => d.MaDonVi == campusId)
                        .GroupBy(_ => 1)
                        .Select(g => new
                        {
                            Avg = g.Average(d => (decimal?)d.GpaMonHoc) ?? 0m,
                            Total = g.Count(),
                            Pass = g.Count(d => d.GpaMonHoc >= 4)
                        })
                        .FirstOrDefaultAsync(ct);

                    // AttendanceRate — present = co_mat or di_muon, absent = vang or co_phep
                    var attendanceStatsTask = _db.DiemDanhs
                        .Where(d => d.MaDonVi == campusId)
                        .GroupBy(_ => 1)
                        .Select(g => new
                        {
                            Total = g.Count(),
                            Present = g.Count(d => d.TrangThai == "co_mat" || d.TrangThai == "di_muon")
                        })
                        .FirstOrDefaultAsync(ct);

                    // Revenue — paid invoices in billion VND
                    var revenueTask = _db.HoaDons
                        .Where(h => h.MaDonVi == campusId && h.TrangThai == "da_thanh_toan")
                        .SumAsync(h => (decimal?)h.DaThanhToan, ct);

                    // TeacherScore — avg rating of teachers in campus
                    var teacherScoreTask = _db.DanhGiaGiaoViens
                        .Where(dg => dg.GiaoVien != null && dg.GiaoVien.MaDonVi == campusId)
                        .AverageAsync(dg => (decimal?)dg.DiemSo, ct);

                    await Task.WhenAll(studentsTask, gradeStatsTask, attendanceStatsTask, revenueTask, teacherScoreTask);

                    var gradeStats = await gradeStatsTask;
                    var attStats = await attendanceStatsTask;
                    var revenue = await revenueTask ?? 0m;
                    var teacherScore = await teacherScoreTask ?? 0m;

                    return new CampusComparisonDto
                    {
                        Id = campusId.ToString(),
                        Name = campus.TenDonVi,
                        Students = await studentsTask,
                        Gpa = gradeStats != null ? Math.Round(gradeStats.Avg, 2) : 0m,
                        PassRate = gradeStats != null && gradeStats.Total > 0
                            ? Math.Round((decimal)gradeStats.Pass / gradeStats.Total * 100, 1)
                            : 0m,
                        AttendanceRate = attStats != null && attStats.Total > 0
                            ? Math.Round((decimal)attStats.Present / attStats.Total * 100, 1)
                            : 0m,
                        Revenue = Math.Round(revenue / 1_000_000_000m, 1),
                        TeacherScore = Math.Round(teacherScore, 2)
                    };
                });

                return (await Task.WhenAll(tasks)).ToList();
            });

        return Ok(ApiResponseDto<List<CampusComparisonDto>>.Ok(cachedData!));
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

public class SaveAcademicReportRequest
{
    public string? Name { get; set; }
    public string? ReportType { get; set; }
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
}

public class SavedAcademicReportParameters
{
    public string Name { get; set; } = "";
    public string ReportType { get; set; } = "class";
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
}

public class SavedAcademicReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ReportType { get; set; } = "class";
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class SavedAcademicReportResultDto
{
    public SavedAcademicReportDto SavedReport { get; set; } = new();
    public AcademicReportDataDto Report { get; set; } = new();
}

public class AcademicReportDataDto
{
    public AcademicReportFilterDto Filter { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public AcademicReportSummaryDto Summary { get; set; } = new();
    public List<AcademicReportSemesterStatDto> MonthlyStats { get; set; } = [];
    public List<AcademicReportDepartmentStatDto> DepartmentStats { get; set; } = [];
}

public class AcademicReportFilterDto
{
    public string ReportType { get; set; } = "class";
    public int? CampusId { get; set; }
    public int? SemesterId { get; set; }
}

public class AcademicReportSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int ActiveCourses { get; set; }
    public decimal AvgGpa { get; set; }
}

public class AcademicReportSemesterStatDto
{
    public int SemesterId { get; set; }
    public string Semester { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public int TotalGrades { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public decimal AvgGpa { get; set; }
    public int StudentCount { get; set; }
}

public class AcademicReportDepartmentStatDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public int TotalGrades { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public decimal AvgGpa { get; set; }
    public double PassRate { get; set; }
}

public class AtRiskReportDto
{
    public int TotalAtRisk { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
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
    public string RiskSubjectName { get; set; } = "";
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
    public string ClassCode { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string Reason { get; set; } = "";
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
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateOnly Date { get; set; }
    public string SubjectName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string Teacher { get; set; } = "";
    public string SubstituteTeacherName { get; set; } = "";
    public string OldSlot { get; set; } = "";
    public string NewSlot { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string Updated { get; set; } = "";
}

public class ScheduleChangeProposal
{
    public DateOnly NewDate { get; set; }
    public int NewShiftId { get; set; }
    public string NewShiftName { get; set; } = "";
    public int NewRoomId { get; set; }
    public string NewRoomCode { get; set; } = "";
    public int? NewTeacherId { get; set; }
    public string NewTeacherName { get; set; } = "";
}

public class RejectGradeUnlockRequest
{
    public string? LyDoTuChoi { get; set; }
}
