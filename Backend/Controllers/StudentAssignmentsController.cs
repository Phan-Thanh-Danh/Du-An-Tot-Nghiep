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
    public async Task<ActionResult<ApiResponseDto<List<StudentAssignmentDto>>>> GetAssignments(
        [FromQuery] int? courseId = null,
        [FromQuery] string? course = null)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var enrolledMonHocIds = await GetStudentSubjectIdsAsync(currentUser.UserId);

        var query = _context.BaiTaps
            .Include(b => b.MonHoc)
            .Where(b => enrolledMonHocIds.Contains(b.MaMonHoc) && b.TrangThai != "nhap");

        if (courseId.HasValue && courseId.Value > 0)
        {
            query = query.Where(b => b.MaMonHoc == courseId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(course))
        {
            var normalizedCourse = course.Trim().ToLower();
            query = query.Where(b => b.MonHoc != null &&
                (b.MonHoc.TenMonHoc.ToLower().Contains(normalizedCourse) ||
                 b.MonHoc.MaCodeMonHoc.ToLower().Contains(normalizedCourse)));
        }

        var assignments = await query
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
                CourseId = a.MaMonHoc,
                CourseCode = a.MonHoc?.MaCodeMonHoc ?? "",
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
            .OrderByDescending(n => n.SoLanNop)
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

        int currentAttempts = submissions.Count > 0 ? submissions.Max(s => s.SoLanNop) : 0;
        int maxAttempts = assignment.SoLanNopToiDa > 0 ? assignment.SoLanNopToiDa : 3;

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
                MinSizeKB = assignment.DungLuongToiThieuKB > 0 ? assignment.DungLuongToiThieuKB : 10,
                MaxSizeMB = assignment.DungLuongToiDaMB > 0 ? assignment.DungLuongToiDaMB : 50,
                MaxAttempts = maxAttempts,
                CurrentAttempt = currentAttempts,
                Note = "Lưu ý: Mỗi lần nộp bài sẽ được cộng dồn vào lịch sử. Hệ thống không cho phép nộp vượt quá số lần quy định."
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
                File = ExtractDisplayFileName(s.UrlTapTin),
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

        var minSizeKB = assignment.DungLuongToiThieuKB > 0 ? assignment.DungLuongToiThieuKB : 10;
        if (file.Length < minSizeKB * 1024)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = $"File của bạn không có nội dung hoặc dung lượng quá nhỏ (Tối thiểu: {minSizeKB} KB)." });
        }

        var maxSizeMB = assignment.DungLuongToiDaMB > 0 ? assignment.DungLuongToiDaMB : 50;
        if (file.Length > maxSizeMB * 1024 * 1024)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = $"Dung lượng file vượt quá giới hạn (Tối đa: {maxSizeMB} MB)." });
        }

        // KIỂM TRA SỐ LẦN NỘP VÀ CỘNG DỒN
        var previousSubmissions = await _context.BaiNops
            .Where(n => n.MaBaiTap == aId && n.MaHocSinh == currentUser.UserId)
            .OrderByDescending(n => n.SoLanNop)
            .ToListAsync();

        int currentAttempts = previousSubmissions.Count > 0 ? previousSubmissions.Max(n => n.SoLanNop) : 0;
        int maxAllowedAttempts = assignment.SoLanNopToiDa > 0 ? assignment.SoLanNopToiDa : 3;

        if (currentAttempts >= maxAllowedAttempts)
        {
            return BadRequest(new ApiResponseDto<AssignmentSubmissionResultDto>
            {
                Success = false,
                Message = $"Bạn đã hết lượt nộp bài. Đã nộp {currentAttempts}/{maxAllowedAttempts} lần tối đa cho phép."
            });
        }

        int nextAttempt = currentAttempts + 1;

        // XỬ LÝ TÊN FILE TỰ ĐỘNG ĐÁNH SỐ (1), (2)... NẾU TRÙNG TÊN GIỐNG WINDOWS
        var rawOriginalName = Path.GetFileName(file.FileName);
        var ext = Path.GetExtension(rawOriginalName);
        var rawNameWithoutExt = Path.GetFileNameWithoutExtension(rawOriginalName);

        var baseNameMatch = System.Text.RegularExpressions.Regex.Match(rawNameWithoutExt, @"^(.*?)(?:\s*\(\d+\))?$");
        var cleanBaseName = baseNameMatch.Success && !string.IsNullOrWhiteSpace(baseNameMatch.Groups[1].Value)
            ? baseNameMatch.Groups[1].Value.Trim()
            : rawNameWithoutExt;

        int duplicateCount = 0;
        foreach (var prevSub in previousSubmissions)
        {
            var prevDisplayName = ExtractDisplayFileName(prevSub.UrlTapTin);
            var prevBase = Path.GetFileNameWithoutExtension(prevDisplayName);
            var prevExt = Path.GetExtension(prevDisplayName);
            if (prevExt.Equals(ext, StringComparison.OrdinalIgnoreCase) &&
                (prevBase.Equals(cleanBaseName, StringComparison.OrdinalIgnoreCase) || prevBase.StartsWith($"{cleanBaseName} (", StringComparison.OrdinalIgnoreCase)))
            {
                duplicateCount++;
            }
        }

        string finalDisplayFileName = duplicateCount > 0
            ? $"{cleanBaseName} ({duplicateCount}){ext}"
            : $"{cleanBaseName}{ext}";

        var safeFinalFileName = System.Text.RegularExpressions.Regex.Replace(finalDisplayFileName, @"[^a-zA-Z0-9_\-\.\(\)\s]", "_");
        string storageFileName = $"{currentUser.UserId}_ASM{aId}_L{nextAttempt}_{safeFinalFileName}";

        var uploadResult = await _r2StorageService.UploadFileAsync(
            file.OpenReadStream(),
            storageFileName,
            file.ContentType,
            "student-assignments",
            keepOriginalFileName: true);

        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url))
        {
            return StatusCode(500, new ApiResponseDto<AssignmentSubmissionResultDto> { Success = false, Message = "Lỗi khi tải file lên hệ thống lưu trữ." });
        }

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
            Message = $"Nộp bài thành công (Lần {nextAttempt}/{maxAllowedAttempts}).",
            Submission = new SubmissionHistoryDto
            {
                Id = baiNop.MaBaiNop.ToString(),
                Attempt = baiNop.SoLanNop,
                SubmittedAt = baiNop.ThoiDiemNop.ToString("dd/MM/yyyy HH:mm"),
                Status = "checking",
                StatusLabel = "Đang kiểm tra",
                OnTime = !baiNop.NopTre,
                TimeLabel = baiNop.NopTre ? "Nộp trễ" : "Đúng hạn",
                File = finalDisplayFileName,
                FileSize = $"{file.Length / 1024} KB",
                Note = "",
                IsLatest = true,
                FileUrl = uploadResult.Url
            }
        };

        return Ok(ApiResponseDto<AssignmentSubmissionResultDto>.Ok(result));
    }

    private static string ExtractDisplayFileName(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "file";
        var rawName = Path.GetFileName(url);
        if (string.IsNullOrEmpty(rawName)) return "file";

        // Format: {userId}_ASM{assignmentId}_L{attempt}_{originalFileName}
        var match = System.Text.RegularExpressions.Regex.Match(rawName, @"^\d+_ASM\d+_L\d+_(.+)$");
        if (match.Success)
        {
            return Uri.UnescapeDataString(match.Groups[1].Value);
        }

        // Legacy format: {userId}_{studentName}_{fileName}
        var legacyMatch = System.Text.RegularExpressions.Regex.Match(rawName, @"^\d+_[^_]+_(.+)$");
        if (legacyMatch.Success)
        {
            return Uri.UnescapeDataString(legacyMatch.Groups[1].Value);
        }

        return Uri.UnescapeDataString(rawName);
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
                .Where(k => k.MaLop == classId.Value)
                .Select(k => k.MaMonHoc)
                .ToListAsync()
            : [];

        var registeredSubjectIds = await _context.DangKyHocPhans
            .AsNoTracking()
            .Where(d => d.MaHocSinh == studentId)
            .Select(d => d.LopHocPhan!.MaMonHoc)
            .ToListAsync();

        return classSubjectIds
            .Concat(registeredSubjectIds)
            .Distinct()
            .ToList();
    }
}
