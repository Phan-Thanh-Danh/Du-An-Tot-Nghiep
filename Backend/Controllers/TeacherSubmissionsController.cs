using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherSubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherSubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("courses/{courseId:int}/assignments")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TeacherAssignmentDto>>>> GetCourseAssignments(int courseId)
    {
        var userId = GetCurrentUserId();
        
        var course = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.LopHocPhan)
            .Include(k => k.Lop)
            .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId);

        if (course == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy khóa học hoặc bạn không dạy khóa này."));

        var assignments = await _context.BaiTaps
            .Include(b => b.MonHoc)
            .Where(b => b.MaMonHoc == course.MaMonHoc)
            .OrderByDescending(b => b.HanNop)
            .ToListAsync();

        var assignmentIds = assignments.Select(a => a.MaBaiTap).ToList();
        var submissionStats = await _context.BaiNops
            .Where(b => assignmentIds.Contains(b.MaBaiTap))
            .GroupBy(b => b.MaBaiTap)
            .Select(g => new
            {
                AssignmentId = g.Key,
                Submitted = g.Count(),
                Pending = g.Count(s => s.DiemSo == null)
            })
            .ToDictionaryAsync(g => g.AssignmentId);

        var items = assignments
            .Select(a => MapAssignment(a, new List<KhoaHoc> { course }, submissionStats.GetValueOrDefault(a.MaBaiTap)))
            .ToList();

        return Ok(ApiResponseDto<IEnumerable<TeacherAssignmentDto>>.Ok(items));
    }

    [HttpGet("courses/{courseId:int}/assignments/{assignmentId:int}/students-status")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<object>>>> GetCourseAssignmentStudentStatus(int courseId, int assignmentId)
    {
        var userId = GetCurrentUserId();
        
        var course = await GetTeacherCoursesQuery(userId)
            .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId);

        if (course == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy khóa học."));

        var assignment = await _context.BaiTaps.FirstOrDefaultAsync(b => b.MaBaiTap == assignmentId && b.MaMonHoc == course.MaMonHoc);
        if (assignment == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập trong khóa học này."));

        // Lấy danh sách toàn bộ học sinh trong lớp
        var students = await _context.NguoiDungs
            .Where(n => n.MaLop == course.MaLop && n.VaiTroChinh == "hoc_sinh")
            .OrderBy(n => n.HoTen)
            .Select(n => new { n.MaNguoiDung, n.HoTen })
            .ToListAsync();

        var studentIds = students.Select(s => s.MaNguoiDung).ToList();

        // Lấy bài nộp của assignment này cho các học sinh
        var submissionsList = await _context.BaiNops
            .Where(b => b.MaBaiTap == assignmentId && studentIds.Contains(b.MaHocSinh))
            .ToListAsync();

        var submissions = submissionsList
            .GroupBy(b => b.MaHocSinh)
            .ToDictionary(
                g => g.Key, 
                g => g.OrderByDescending(b => b.SoLanNop).FirstOrDefault()
            );

        var result = students.Select(s => {
            var sub = submissions.GetValueOrDefault(s.MaNguoiDung);
            return new {
                StudentId = s.MaNguoiDung,
                StudentName = s.HoTen,
                SubmissionId = sub?.MaBaiNop,
                SubmittedAt = sub?.ThoiDiemNop,
                FileUrl = sub?.UrlTapTin,
                AttemptNumber = sub?.SoLanNop ?? 0,
                IsLate = sub?.NopTre ?? false,
                Score = sub?.DiemSo,
                Feedback = sub?.NhanXet,
                Status = sub == null ? "Chưa nộp bài" : (sub.DiemSo != null ? "Đã chấm" : "Đã nộp")
            };
        });

        return Ok(ApiResponseDto<IEnumerable<object>>.Ok(result));
    }

    [HttpGet("courses/{courseId:int}/assignments/{assignmentId:int}/download-all")]
    public async Task<IActionResult> DownloadAllSubmissions(
        int courseId, 
        int assignmentId,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        var userId = GetCurrentUserId();
        
        var course = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.Lop)
            .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId);

        if (course == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy khóa học."));

        var assignment = await _context.BaiTaps.FirstOrDefaultAsync(b => b.MaBaiTap == assignmentId && b.MaMonHoc == course.MaMonHoc);
        if (assignment == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập trong khóa học này."));

        // Lấy danh sách toàn bộ học sinh trong lớp
        var students = await _context.NguoiDungs
            .Where(n => n.MaLop == course.MaLop && n.VaiTroChinh == "hoc_sinh")
            .Select(n => new { n.MaNguoiDung, n.HoTen })
            .ToListAsync();

        var studentIds = students.Select(s => s.MaNguoiDung).ToList();

        var submissionsList = await _context.BaiNops
            .Where(b => b.MaBaiTap == assignmentId && studentIds.Contains(b.MaHocSinh))
            .ToListAsync();

        var latestSubmissions = submissionsList
            .GroupBy(b => b.MaHocSinh)
            .Select(g => g.OrderByDescending(b => b.SoLanNop).FirstOrDefault())
            .Where(b => b != null && !string.IsNullOrEmpty(b.UrlTapTin))
            .ToList();

        if (!latestSubmissions.Any())
            return BadRequest(ApiResponseDto.Fail("Chưa có sinh viên nào nộp bài."));

        var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            var httpClient = httpClientFactory.CreateClient();
            string safeCourseName = System.Text.RegularExpressions.Regex.Replace(course.MonHoc?.TenMonHoc ?? "KhoaHoc", @"[^a-zA-Z0-9_\-\.]", "_");
            string safeClassName = System.Text.RegularExpressions.Regex.Replace(course.Lop?.TenLop ?? "Lop", @"[^a-zA-Z0-9_\-\.]", "_");
            
            foreach (var sub in latestSubmissions)
            {
                var student = students.FirstOrDefault(s => s.MaNguoiDung == sub.MaHocSinh);
                var studentName = student?.HoTen ?? "Unknown";
                string safeStudentName = System.Text.RegularExpressions.Regex.Replace(studentName, @"\s+", "_");
                
                string fileName = Path.GetFileName(sub.UrlTapTin) ?? "";
                if (fileName.Contains('?')) fileName = fileName.Split('?')[0];
                
                // Loại bỏ mã GUID 32 ký tự nếu có (dành cho các file cũ)
                if (fileName.Length > 33 && fileName[32] == '_' && System.Text.RegularExpressions.Regex.IsMatch(fileName.Substring(0, 32), @"^[0-9a-fA-F]{32}$"))
                {
                    fileName = fileName.Substring(33);
                }

                // Loại bỏ tiền tố MãSinhViên_HọTênSinhViên_ nếu đã có (để tránh bị nối chuỗi 2 lần)
                string studentPrefix = $"{sub.MaHocSinh}_{safeStudentName}_";
                if (fileName.StartsWith(studentPrefix))
                {
                    fileName = fileName.Substring(studentPrefix.Length);
                }

                string entryName = $"{sub.MaHocSinh}_{safeStudentName}_{fileName}";

                var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Fastest);
                using var entryStream = entry.Open();

                if (sub.UrlTapTin!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var response = await httpClient.GetAsync(sub.UrlTapTin);
                        if (response.IsSuccessStatusCode)
                        {
                            using var contentStream = await response.Content.ReadAsStreamAsync();
                            await contentStream.CopyToAsync(entryStream);
                        }
                    }
                    catch
                    {
                        // Ignore download errors for individual files
                    }
                }
                else
                {
                    // Local file
                    var localPath = Path.Combine(env.WebRootPath, sub.UrlTapTin.TrimStart('/'));
                    if (System.IO.File.Exists(localPath))
                    {
                        using var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                        await fs.CopyToAsync(entryStream);
                    }
                }
            }
        }

        memoryStream.Position = 0;
        string safeCourseNameOut = System.Text.RegularExpressions.Regex.Replace(course.MonHoc?.TenMonHoc ?? "KhoaHoc", @"[^a-zA-Z0-9_\-\.]", "_");
        string safeClassNameOut = System.Text.RegularExpressions.Regex.Replace(course.Lop?.TenLop ?? "Lop", @"[^a-zA-Z0-9_\-\.]", "_");
        return File(memoryStream, "application/zip", $"{safeCourseNameOut}_{safeClassNameOut}.zip");
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherAssignmentDto>>>> GetAssignments(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var teacherCourses = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.LopHocPhan)
            .Include(k => k.Lop)
            .ToListAsync();

        var teacherMonHocIds = teacherCourses.Select(k => k.MaMonHoc).Distinct().ToList();
        var query = _context.BaiTaps
            .Include(b => b.MonHoc)
            .Where(b => teacherMonHocIds.Contains(b.MaMonHoc));

        var totalItems = await query.CountAsync();
        var assignments = await query
            .OrderByDescending(b => b.HanNop)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var assignmentIds = assignments.Select(a => a.MaBaiTap).ToList();
        var submissionStats = await _context.BaiNops
            .Where(b => assignmentIds.Contains(b.MaBaiTap))
            .GroupBy(b => b.MaBaiTap)
            .Select(g => new
            {
                AssignmentId = g.Key,
                Submitted = g.Count(),
                Pending = g.Count(s => s.DiemSo == null)
            })
            .ToDictionaryAsync(g => g.AssignmentId);

        var items = assignments
            .Select(a => MapAssignment(a, teacherCourses, submissionStats.GetValueOrDefault(a.MaBaiTap)))
            .ToList();

        return Ok(ApiResponseDto<PagedResultDto<TeacherAssignmentDto>>.Ok(new PagedResultDto<TeacherAssignmentDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        }));
    }

    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponseDto<TeacherAssignmentDto>>> CreateAssignment(
        TeacherAssignmentRequest request)
    {
        var userId = GetCurrentUserId();
        var course = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.LopHocPhan)
            .Include(k => k.Lop)
            .FirstOrDefaultAsync(k => k.MaKhoaHoc == request.CourseId);

        if (course == null)
            return Forbid();

        var validationError = ValidateAssignmentRequest(request);
        if (validationError != null)
            return BadRequest(ApiResponseDto.Fail(validationError));

        var assignment = new BaiTap
        {
            MaMonHoc = course.MaMonHoc,
            TieuDe = request.Title.Trim(),
            MoTa = request.Description,
            HanNop = request.DueAt!.Value,
            SoLanNopToiDa = request.MaxAttempts.GetValueOrDefault(3),
            DinhDangChoPhep = SerializeAllowedFormats(request.AllowedFormats),
            HuongDanChamDiem = request.GradingGuide,
            TrangThai = NormalizeAssignmentStatus(request.Status)
        };

        _context.BaiTaps.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAssignmentDetail),
            new { id = assignment.MaBaiTap },
            ApiResponseDto<TeacherAssignmentDto>.Ok(MapAssignment(assignment, [course], null), "Tạo bài tập thành công"));
    }

    [HttpGet("assignments/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherAssignmentDto>>> GetAssignmentDetail(int id)
    {
        var userId = GetCurrentUserId();
        var teacherCourses = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.LopHocPhan)
            .Include(k => k.Lop)
            .ToListAsync();

        var teacherMonHocIds = teacherCourses.Select(k => k.MaMonHoc).Distinct().ToList();
        var assignment = await _context.BaiTaps
            .Include(b => b.MonHoc)
            .FirstOrDefaultAsync(b => b.MaBaiTap == id && teacherMonHocIds.Contains(b.MaMonHoc));

        if (assignment == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập."));

        var stat = await _context.BaiNops
            .Where(s => s.MaBaiTap == id)
            .GroupBy(s => s.MaBaiTap)
            .Select(g => new { AssignmentId = g.Key, Submitted = g.Count(), Pending = g.Count(s => s.DiemSo == null) })
            .FirstOrDefaultAsync();

        return Ok(ApiResponseDto<TeacherAssignmentDto>.Ok(MapAssignment(assignment, teacherCourses, stat)));
    }

    [HttpPut("assignments/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherAssignmentDto>>> UpdateAssignment(
        int id,
        TeacherAssignmentRequest request)
    {
        var userId = GetCurrentUserId();
        var teacherCourses = await GetTeacherCoursesQuery(userId)
            .Include(k => k.MonHoc)
            .Include(k => k.LopHocPhan)
            .Include(k => k.Lop)
            .ToListAsync();

        var teacherMonHocIds = teacherCourses.Select(k => k.MaMonHoc).Distinct().ToList();
        var assignment = await _context.BaiTaps
            .FirstOrDefaultAsync(b => b.MaBaiTap == id && teacherMonHocIds.Contains(b.MaMonHoc));

        if (assignment == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập."));

        var validationError = ValidateAssignmentRequest(request);
        if (validationError != null)
            return BadRequest(ApiResponseDto.Fail(validationError));

        if (request.CourseId.HasValue)
        {
            var course = teacherCourses.FirstOrDefault(k => k.MaKhoaHoc == request.CourseId.Value);
            if (course == null)
                return Forbid();
            assignment.MaMonHoc = course.MaMonHoc;
        }

        assignment.TieuDe = request.Title.Trim();
        assignment.MoTa = request.Description;
        assignment.HanNop = request.DueAt!.Value;
        assignment.SoLanNopToiDa = request.MaxAttempts.GetValueOrDefault(assignment.SoLanNopToiDa > 0 ? assignment.SoLanNopToiDa : 3);
        assignment.DinhDangChoPhep = SerializeAllowedFormats(request.AllowedFormats);
        assignment.HuongDanChamDiem = request.GradingGuide;
        assignment.TrangThai = NormalizeAssignmentStatus(request.Status);

        await _context.SaveChangesAsync();

        return Ok(ApiResponseDto<TeacherAssignmentDto>.Ok(MapAssignment(assignment, teacherCourses, null), "Cập nhật bài tập thành công"));
    }

    [HttpDelete("assignments/{id:int}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteAssignment(int id)
    {
        var userId = GetCurrentUserId();
        var teacherMonHocIds = await GetTeacherCoursesQuery(userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var assignment = await _context.BaiTaps
            .FirstOrDefaultAsync(b => b.MaBaiTap == id && teacherMonHocIds.Contains(b.MaMonHoc));

        if (assignment == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập."));

        var hasSubmissions = await _context.BaiNops.AnyAsync(b => b.MaBaiTap == id);
        if (hasSubmissions)
        {
            assignment.TrangThai = "da_dong";
        }
        else
        {
            _context.BaiTaps.Remove(assignment);
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponseDto.Ok(hasSubmissions ? "Bài tập đã có bài nộp nên được đóng thay vì xóa." : "Đã xóa bài tập."));
    }

    [HttpGet("assignments/{id:int}/submissions")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>>> GetAssignmentSubmissions(
        int id,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var teacherMonHocIds = await GetTeacherCoursesQuery(userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var canAccess = await _context.BaiTaps
            .AnyAsync(b => b.MaBaiTap == id && teacherMonHocIds.Contains(b.MaMonHoc));

        if (!canAccess)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập."));

        var query = _context.BaiNops
            .Include(b => b.BaiTap!).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.MaBaiTap == id);

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.ThoiDiemNop)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TeacherSubmissionDto
            {
                SubmissionId = b.MaBaiNop,
                StudentId = b.MaHocSinh,
                StudentName = b.HocSinh != null ? b.HocSinh.HoTen : "",
                AssignmentTitle = b.BaiTap != null ? b.BaiTap.TieuDe : "",
                CourseName = b.BaiTap != null && b.BaiTap.MonHoc != null ? b.BaiTap.MonHoc.TenMonHoc : "",
                SubmittedAt = b.ThoiDiemNop,
                FileUrl = b.UrlTapTin,
                AttemptNumber = b.SoLanNop,
                IsLate = b.NopTre,
                Score = b.DiemSo,
                Feedback = b.NhanXet,
                Status = b.DiemSo != null ? "da_cham" : b.NopTre ? "nop_tre" : "cho_cham"
            })
            .ToListAsync();

        return Ok(ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>.Ok(new PagedResultDto<TeacherSubmissionDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        }));
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>>> GetSubmissions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        var teacherMonHocIds = await GetTeacherCoursesQuery(userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        if (teacherMonHocIds.Count == 0)
        {
            var emptyResult = new PagedResultDto<TeacherSubmissionDto>
            {
                Items = [],
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = 0
            };
            return Ok(ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>.Ok(emptyResult));
        }

        var query = _context.BaiNops
            .Include(b => b.BaiTap!).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc));

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.ThoiDiemNop)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new TeacherSubmissionDto
            {
                SubmissionId = b.MaBaiNop,
                StudentId = b.MaHocSinh,
                StudentName = b.HocSinh != null ? b.HocSinh.HoTen : "",
                AssignmentTitle = b.BaiTap != null ? b.BaiTap.TieuDe : "",
                CourseName = b.BaiTap != null && b.BaiTap.MonHoc != null ? b.BaiTap.MonHoc.TenMonHoc : "",
                SubmittedAt = b.ThoiDiemNop,
                FileUrl = b.UrlTapTin,
                AttemptNumber = b.SoLanNop,
                IsLate = b.NopTre,
                Score = b.DiemSo,
                Feedback = b.NhanXet,
                Status = b.DiemSo != null ? "da_cham" : b.NopTre ? "nop_tre" : "cho_cham"
            })
            .ToListAsync();

        var result = new PagedResultDto<TeacherSubmissionDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return Ok(ApiResponseDto<PagedResultDto<TeacherSubmissionDto>>.Ok(result));
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<ActionResult<ApiResponseDto<TeacherSubmissionDetailDto>>> GetSubmissionDetail(int id)
    {
        var userId = GetCurrentUserId();

        var teacherMonHocIds = await GetTeacherCoursesQuery(userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var submission = await _context.BaiNops
            .Include(b => b.BaiTap!).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc))
            .FirstOrDefaultAsync(b => b.MaBaiNop == id);

        if (submission is null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài nộp"));

        var detail = new TeacherSubmissionDetailDto
        {
            SubmissionId = submission.MaBaiNop,
            StudentName = submission.HocSinh?.HoTen ?? "",
            AssignmentTitle = submission.BaiTap?.TieuDe ?? "",
            CourseName = submission.BaiTap?.MonHoc?.TenMonHoc ?? "",
            Description = submission.BaiTap?.MoTa ?? "",
            FileUrl = submission.UrlTapTin,
            SubmittedAt = submission.ThoiDiemNop,
            AttemptNumber = submission.SoLanNop,
            IsLate = submission.NopTre,
            Score = submission.DiemSo,
            PlagiarismScore = submission.DiemDaoVan,
            AiScore = submission.DiemAiDeXuat,
            Feedback = submission.NhanXet,
            IsPublished = submission.DaCongBo,
            Status = submission.DiemSo != null ? "da_cham" : "cho_cham"
        };

        return Ok(ApiResponseDto<TeacherSubmissionDetailDto>.Ok(detail));
    }

    [HttpPut("submissions/{id:int}/grade")]
    public async Task<ActionResult<ApiResponseDto<TeacherSubmissionDto>>> GradeSubmission(
        int id,
        GradeSubmissionRequest request)
    {
        var userId = GetCurrentUserId();

        var teacherMonHocIds = await GetTeacherCoursesQuery(userId)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var submission = await _context.BaiNops
            .Include(b => b.BaiTap!).ThenInclude(bt => bt.MonHoc)
            .Include(b => b.HocSinh)
            .Where(b => b.BaiTap != null && teacherMonHocIds.Contains(b.BaiTap.MaMonHoc))
            .FirstOrDefaultAsync(b => b.MaBaiNop == id);

        if (submission is null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài nộp"));

        if (request.Score is < 0 or > 10)
            return BadRequest(ApiResponseDto.Fail("Điểm phải nằm trong khoảng 0-10."));

        submission.DiemSo = request.Score;
        submission.NhanXet = request.Feedback;
        submission.DaCongBo = request.Publish ?? false;

        await _context.SaveChangesAsync();

        var dto = new TeacherSubmissionDto
        {
            SubmissionId = submission.MaBaiNop,
            StudentId = submission.MaHocSinh,
            StudentName = submission.HocSinh?.HoTen ?? "",
            AssignmentTitle = submission.BaiTap?.TieuDe ?? "",
            CourseName = submission.BaiTap?.MonHoc?.TenMonHoc ?? "",
            SubmittedAt = submission.ThoiDiemNop,
            FileUrl = submission.UrlTapTin,
            AttemptNumber = submission.SoLanNop,
            IsLate = submission.NopTre,
            Score = submission.DiemSo,
            Feedback = submission.NhanXet,
            Status = "da_cham"
        };

        return Ok(ApiResponseDto<TeacherSubmissionDto>.Ok(dto, "Chấm điểm thành công"));
    }

    private int GetCurrentUserId()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        return currentUser?.UserId ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");
    }

    private IQueryable<KhoaHoc> GetTeacherCoursesQuery(int userId)
    {
        return _context.KhoaHocs.Where(k => k.MaGiaoVien == userId);
    }

    private static TeacherAssignmentDto MapAssignment(
        BaiTap assignment,
        IReadOnlyCollection<KhoaHoc> teacherCourses,
        object? stat)
    {
        var course = teacherCourses.FirstOrDefault(k => k.MaMonHoc == assignment.MaMonHoc);
        var submitted = ReadIntProperty(stat, "Submitted");
        var pending = ReadIntProperty(stat, "Pending");
        var totalStudents = EstimateTotalStudents(course, submitted);

        return new TeacherAssignmentDto
        {
            Id = assignment.MaBaiTap,
            CourseId = course?.MaKhoaHoc,
            CourseName = assignment.MonHoc?.TenMonHoc ?? course?.MonHoc?.TenMonHoc ?? "",
            ClassName = course?.LopHocPhan?.MaCodeLopHocPhan ?? course?.Lop?.MaCodeLop ?? "",
            Title = assignment.TieuDe,
            Description = assignment.MoTa ?? "",
            Deadline = assignment.HanNop,
            Status = assignment.TrangThai,
            MaxScore = 10,
            MaxAttempts = assignment.SoLanNopToiDa,
            AllowedFormats = ParseAllowedFormats(assignment.DinhDangChoPhep),
            GradingGuide = assignment.HuongDanChamDiem,
            SubmissionsCount = submitted,
            PendingGrades = pending,
            TotalStudents = totalStudents
        };
    }

    private static int EstimateTotalStudents(KhoaHoc? course, int submitted)
    {
        if (course?.LopHocPhan?.SoDaDangKy > 0) return course.LopHocPhan.SoDaDangKy;
        return Math.Max(submitted, 0);
    }

    private static int ReadIntProperty(object? value, string propertyName)
    {
        if (value == null) return 0;
        var property = value.GetType().GetProperty(propertyName);
        return property?.GetValue(value) is int number ? number : 0;
    }

    private static string? ValidateAssignmentRequest(TeacherAssignmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return "Vui lòng nhập tiêu đề bài tập.";
        if (request.DueAt == null) return "Vui lòng nhập hạn nộp.";
        if (request.MaxAttempts is <= 0 or > 20) return "Số lần nộp tối đa phải từ 1 đến 20.";
        if (request.MaxScore is < 0 or > 10) return "Điểm tối đa phải nằm trong khoảng 0-10.";
        return null;
    }

    private static string NormalizeAssignmentStatus(string? status)
    {
        return status switch
        {
            "draft" or "Draft" or "nhap" => "nhap",
            "closed" or "Completed" or "da_dong" => "da_dong",
            _ => "da_xuat_ban"
        };
    }

    private static string SerializeAllowedFormats(IReadOnlyCollection<string>? allowedFormats)
    {
        var formats = allowedFormats is { Count: > 0 }
            ? allowedFormats
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim().StartsWith('.') ? item.Trim() : $".{item.Trim()}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [".zip", ".rar", ".pdf", ".doc", ".docx"];

        return JsonSerializer.Serialize(formats);
    }

    private static List<string> ParseAllowedFormats(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [".zip", ".rar", ".pdf", ".doc", ".docx"];

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(raw);
            if (parsed is { Count: > 0 }) return parsed;
        }
        catch
        {
            // Legacy rows may contain comma-separated formats.
        }

        return raw
            .Replace("[", "")
            .Replace("]", "")
            .Replace("\"", "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.StartsWith('.') ? item : $".{item}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

public class TeacherAssignmentDto
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public int MaxAttempts { get; set; }
    public IReadOnlyList<string> AllowedFormats { get; set; } = [];
    public string? GradingGuide { get; set; }
    public int SubmissionsCount { get; set; }
    public int PendingGrades { get; set; }
    public int TotalStudents { get; set; }
}

public class TeacherAssignmentRequest
{
    public int? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueAt { get; set; }
    public decimal? MaxScore { get; set; }
    public int? MaxAttempts { get; set; }
    public IReadOnlyList<string>? AllowedFormats { get; set; }
    public string? GradingGuide { get; set; }
    public string? Status { get; set; }
}

public class TeacherSubmissionDto
{
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public bool IsLate { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TeacherSubmissionDetailDto
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsLate { get; set; }
    public decimal? Score { get; set; }
    public decimal? PlagiarismScore { get; set; }
    public decimal? AiScore { get; set; }
    public string? Feedback { get; set; }
    public bool IsPublished { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GradeSubmissionRequest
{
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public bool? Publish { get; set; }
}
