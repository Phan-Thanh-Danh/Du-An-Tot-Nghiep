using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.StudentAssignments;
using Backend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/assignments")]
public class StudentAssignmentsController : ControllerBase
{
    private readonly IR2StorageService _r2StorageService;
    private readonly Backend.Data.ApplicationDbContext _context;

    public StudentAssignmentsController(
        IR2StorageService r2StorageService,
        Backend.Data.ApplicationDbContext context)
    {
        _r2StorageService = r2StorageService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<List<StudentAssignmentDto>>>> GetAssignments()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var enrolledMonHocIds = await GetStudentSubjectIdsAsync(currentUser.UserId);

        var assignments = await _context.BaiTaps
            .Include(b => b.MonHoc)
            .Where(b => enrolledMonHocIds.Contains(b.MaMonHoc) && b.TrangThai != "nhap")
            .OrderByDescending(b => b.HanNop)
            .ToListAsync();

        var submittedIds = await _context.BaiNops
            .Where(n => n.MaHocSinh == currentUser.UserId)
            .Select(n => n.MaBaiTap)
            .Distinct()
            .ToListAsync();

        var result = assignments.Select(a =>
        {
            var hasSubmitted = submittedIds.Contains(a.MaBaiTap);
            var isOverdue = a.HanNop < DateTime.UtcNow;
            var isNearDeadline = a.HanNop <= DateTime.UtcNow.AddDays(3);

            string status, variant, priority;
            if (hasSubmitted)
            {
                status = "Đã nộp";
                variant = "success";
                priority = "medium";
            }
            else if (isOverdue)
            {
                status = "Quá hạn";
                variant = "danger";
                priority = "high";
            }
            else if (isNearDeadline)
            {
                status = "Sắp đến hạn";
                variant = "warning";
                priority = "high";
            }
            else
            {
                status = "Chưa nộp";
                variant = "secondary";
                priority = "medium";
            }

            return new StudentAssignmentDto
            {
                Id = a.MaBaiTap.ToString(),
                Course = a.MonHoc?.TenMonHoc ?? "",
                Title = a.TieuDe,
                Deadline = a.HanNop.ToString("dd/MM/yyyy"),
                Status = status,
                Variant = variant,
                Priority = priority
            };
        }).ToList();

        return Ok(ApiResponseDto<List<StudentAssignmentDto>>.Ok(result));
    }

    [HttpGet("{assignmentId}")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<StudentAssignmentDetailDto>>> GetAssignmentDetail(string assignmentId)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (!int.TryParse(assignmentId, out int aId) || aId <= 0)
        {
            return BadRequest(ApiResponseDto.Fail("Mã bài tập không hợp lệ."));
        }

        var enrolledMonHocIds = await GetStudentSubjectIdsAsync(currentUser.UserId);

        var assignment = await _context.BaiTaps
            .Include(a => a.MonHoc)
            .FirstOrDefaultAsync(a => a.MaBaiTap == aId
                && enrolledMonHocIds.Contains(a.MaMonHoc)
                && a.TrangThai != "nhap");

        if (assignment == null)
        {
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bài tập."));
        }

        var submissions = new List<Backend.Models.BaiNop>();
        submissions = await _context.BaiNops
            .Where(n => n.MaBaiTap == aId && n.MaHocSinh == currentUser.UserId)
            .OrderByDescending(n => n.ThoiDiemNop)
            .ToListAsync();

        var latestSubmission = submissions.FirstOrDefault();
        var now = DateTime.UtcNow;
        var isOverdue = assignment.HanNop < now;
        var status = latestSubmission?.DiemSo != null && latestSubmission.DaCongBo
            ? "graded"
            : latestSubmission != null
                ? "submitted"
                : isOverdue
                    ? "overdue"
                    : "pending";
        var statusLabel = status switch
        {
            "graded" => "Đã chấm",
            "submitted" => "Đã nộp",
            "overdue" => "Quá hạn",
            _ => "Chưa nộp"
        };

        var detail = new StudentAssignmentDetailDto
        {
            CourseCode = assignment.MonHoc?.MaCodeMonHoc ?? "",
            Class = assignment.MonHoc?.TenMonHoc ?? "",
            Title = assignment.TieuDe,
            Teacher = "Giảng viên phụ trách",
            DeadlineDisplay = assignment.HanNop.ToString("dd/MM/yyyy HH:mm"),
            Status = status,
            StatusLabel = statusLabel,
            Description = assignment.MoTa ?? "",
            Score = latestSubmission?.DaCongBo == true ? latestSubmission.DiemSo : null,
            Feedback = latestSubmission?.DaCongBo == true ? latestSubmission.NhanXet : null,
            Rules = new SubmissionRulesDto
            {
                AllowedFormats = ParseAllowedFormats(assignment.DinhDangChoPhep),
                MaxSizeMB = 50,
                MaxAttempts = assignment.SoLanNopToiDa > 0 ? assignment.SoLanNopToiDa : 3,
                CurrentAttempt = submissions.Count,
                Note = "Lưu ý: Không chấp nhận nộp bài qua email."
            },
            Submissions = submissions.Select((s, index) => new SubmissionHistoryDto
            {
                Id = s.MaBaiNop.ToString(),
                Attempt = s.SoLanNop,
                SubmittedAt = s.ThoiDiemNop.ToString("dd/MM/yyyy HH:mm"),
                Status = s.DiemSo.HasValue ? "graded" : "checking",
                StatusLabel = s.DiemSo.HasValue ? "Đã chấm" : "Đang kiểm tra",
                OnTime = !s.NopTre,
                TimeLabel = s.NopTre ? "Nộp trễ" : "Đúng hạn",
                File = Path.GetFileName(s.UrlTapTin) ?? "file",
                FileSize = "N/A",
                Note = s.NhanXet ?? "",
                IsLatest = index == 0,
                FileUrl = s.UrlTapTin,
                Score = s.DaCongBo ? s.DiemSo : null,
                Feedback = s.DaCongBo ? s.NhanXet : null
            }).ToList()
        };

        return Ok(ApiResponseDto<StudentAssignmentDetailDto>.Ok(detail));
    }

    [HttpPost("{assignmentId}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApiResponseDto<AssignmentSubmissionResultDto>>> SubmitAssignment(
        string assignmentId, [FromForm] IFormFile file)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Vui lòng chọn file để nộp." });
        }

        if (!int.TryParse(assignmentId, out int aId) || aId <= 0)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Mã bài tập không hợp lệ." });
        }

        var enrolledMonHocIds = await GetStudentSubjectIdsAsync(currentUser.UserId);

        var assignment = await _context.BaiTaps
            .FirstOrDefaultAsync(a => a.MaBaiTap == aId
                && enrolledMonHocIds.Contains(a.MaMonHoc)
                && a.TrangThai != "nhap");

        if (assignment == null)
        {
            return NotFound(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Không tìm thấy bài tập." });
        }

        if (assignment.TrangThai == "da_dong")
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Bài tập đã đóng." });
        }

        var now = DateTime.UtcNow;
        if (assignment.HanNop < now)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Đã quá hạn nộp bài." });
        }

        string fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var uploadResult = await _r2StorageService.UploadFileAsync(
            file.OpenReadStream(),
            fileName,
            file.ContentType,
            "student-assignments");

        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url))
        {
            return StatusCode(500, new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Lỗi khi tải file lên hệ thống lưu trữ." });
        }

        var previousSubmissions = await _context.BaiNops
            .Where(n => n.MaBaiTap == aId && n.MaHocSinh == currentUser.UserId)
            .OrderByDescending(n => n.SoLanNop)
            .ToListAsync();

        if (previousSubmissions.Count >= assignment.SoLanNopToiDa)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Bạn đã hết lượt nộp bài." });
        }

        int nextAttempt = previousSubmissions.Count > 0 ? previousSubmissions.First().SoLanNop + 1 : 1;

        var baiNop = new Backend.Models.BaiNop
        {
            MaBaiTap = aId,
            MaHocSinh = currentUser.UserId,
            UrlTapTin = uploadResult.Url,
            SoLanNop = nextAttempt,
            NopTre = assignment.HanNop < now,
            ThoiDiemNop = now,
            DaCongBo = false
        };

        _context.BaiNops.Add(baiNop);
        await _context.SaveChangesAsync();

        var result = new AssignmentSubmissionResultDto
        {
            Success = true,
            Message = "Nộp bài thành công.",
            Submission = new SubmissionHistoryDto
            {
                Id = baiNop.MaBaiNop.ToString(),
                Attempt = baiNop.SoLanNop,
                SubmittedAt = baiNop.ThoiDiemNop.ToString("dd/MM/yyyy HH:mm"),
                Status = "checking",
                StatusLabel = "Đang kiểm tra",
                OnTime = !baiNop.NopTre,
                TimeLabel = baiNop.NopTre ? "Nộp trễ" : "Đúng hạn",
                File = file.FileName,
                FileSize = $"{file.Length / 1024} KB",
                Note = "",
                IsLatest = true,
                FileUrl = uploadResult.Url
            }
        };

        return Ok(ApiResponseDto<AssignmentSubmissionResultDto>.Ok(result));
    }

    private static List<string> ParseAllowedFormats(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [".zip", ".rar", ".pdf", ".doc", ".docx"];
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

    private async Task<List<int>> GetStudentSubjectIdsAsync(int studentId)
    {
        var classId = await _context.NguoiDungs
            .AsNoTracking()
            .Where(u => u.MaNguoiDung == studentId)
            .Select(u => u.MaLop)
            .FirstOrDefaultAsync();

        var classSubjectIds = classId.HasValue
            ? await _context.KhoaHocs
                .AsNoTracking()
                .Where(k => k.MaLop == classId.Value && k.TrangThai == "da_xuat_ban")
                .Select(k => k.MaMonHoc)
                .ToListAsync()
            : [];

        var registeredSubjectIds = await _context.DangKyHocPhans
            .AsNoTracking()
            .Where(d => d.MaHocSinh == studentId && d.TrangThai == "da_dang_ky")
            .Select(d => d.LopHocPhan!.MaMonHoc)
            .ToListAsync();

        return classSubjectIds
            .Concat(registeredSubjectIds)
            .Distinct()
            .ToList();
    }
}
