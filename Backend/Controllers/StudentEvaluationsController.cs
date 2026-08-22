using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/evaluations")]
[Authorize(Roles = AuthRoles.Student)]
public class StudentEvaluationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StudentEvaluationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<object>>> GetEvaluations(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var currentTerm = await _db.HocKys
            .OrderByDescending(h => h.MaHocKy)
            .FirstOrDefaultAsync(ct);

        if (currentTerm == null)
            return Ok(ApiResponseDto<object>.Ok(new List<object>()));

        var student = await _db.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == userId, ct);

        // 1. Lấy môn học từ đăng ký học phần
        var enrollments = await _db.DangKyHocPhans
            .Include(d => d.LopHocPhan!)
                .ThenInclude(l => l.MonHoc)
            .Where(d => d.MaHocSinh == userId && (d.TrangThai == "da_duyet" || d.TrangThai == "da_dang_ky"))
            .Select(d => d.LopHocPhan)
            .Where(l => l != null)
            .Distinct()
            .ToListAsync(ct);

        // 2. Lấy khóa học phân công cho lớp của sinh viên
        var classCourses = student?.MaLop != null
            ? await _db.KhoaHocs
                .AsNoTracking()
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .Where(k => k.MaLop == student.MaLop.Value && k.MaGiaoVien != null)
                .ToListAsync(ct)
            : new List<KhoaHoc>();

        // 3. Lấy danh sách các khóa học sinh viên đã hoàn thành đánh giá
        var evaluatedHashes = await _db.DanhGiaGiaoViens
            .AsNoTracking()
            .Where(d => d.CohortHash != null && d.CohortHash.StartsWith($"student-{userId}-course-"))
            .Select(d => d.CohortHash!)
            .Distinct()
            .ToListAsync(ct);

        var evaluatedCourseIds = new HashSet<int>();
        foreach (var hash in evaluatedHashes)
        {
            var parts = hash.Split('-');
            if (parts.Length >= 4 && int.TryParse(parts[3], out var cid))
            {
                evaluatedCourseIds.Add(cid);
            }
        }

        var result = new List<object>();

        // Thêm từ class courses (loại bỏ môn đã đánh giá)
        foreach (var course in classCourses.GroupBy(c => c.MaMonHoc).Select(g => g.First()))
        {
            if (evaluatedCourseIds.Contains(course.MaKhoaHoc))
                continue;

            result.Add(new
            {
                Id = $"EVAL-COURSE-{course.MaKhoaHoc}",
                EnrollmentId = course.MaKhoaHoc,
                Subject = course.MonHoc?.TenMonHoc ?? course.TieuDe,
                Teacher = course.GiaoVien?.HoTen ?? "Giảng viên phụ trách",
                Status = "Pending",
                EditsLeft = 2,
                Ratings = new { r1 = 0, r2 = 0, r3 = 0, r4 = 0, r5 = 0, r6 = 0 },
                Feedback = ""
            });
        }

        // Thêm từ enrollments nếu chưa có và chưa được đánh giá
        foreach (var l in enrollments)
        {
            if (l == null) continue;
            if (evaluatedCourseIds.Contains(l.MaLopHocPhan)) continue;
            var evalId = $"EVAL-LHP-{l.MaLopHocPhan}";
            if (result.Any(r => ((dynamic)r).Subject == (l.MonHoc?.TenMonHoc ?? ""))) continue;

            result.Add(new
            {
                Id = evalId,
                EnrollmentId = l.MaLopHocPhan,
                Subject = l.MonHoc?.TenMonHoc ?? "",
                Teacher = "Giảng viên bộ môn",
                Status = "Pending",
                EditsLeft = 2,
                Ratings = new { r1 = 0, r2 = 0, r3 = 0, r4 = 0, r5 = 0, r6 = 0 },
                Feedback = ""
            });
        }

        return Ok(ApiResponseDto<object>.Ok(result));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ApiResponseDto<object>>> SubmitEvaluation(
        [FromBody] SubmitEvaluationRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var currentTerm = await _db.HocKys
            .OrderByDescending(h => h.MaHocKy)
            .FirstOrDefaultAsync(ct);
        if (currentTerm == null)
            return BadRequest(ApiResponseDto.Fail("Không tìm thấy học kỳ hiện tại."));

        int enrollmentId = request.EnrollmentId ?? 0;
        if (enrollmentId <= 0 && !string.IsNullOrWhiteSpace(request.Id))
        {
            var parts = request.Id.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[^1], out var parsedId))
            {
                enrollmentId = parsedId;
            }
        }

        var course = await _db.KhoaHocs
            .Where(k => k.MaLopHocPhan == enrollmentId || k.MaKhoaHoc == enrollmentId)
            .FirstOrDefaultAsync(ct);

        int teacherId = course?.MaGiaoVien ?? 0;
        int courseId = course?.MaKhoaHoc ?? enrollmentId;

        if (teacherId == 0)
        {
            var student = await _db.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.MaNguoiDung == userId, ct);
            if (student?.MaLop != null)
            {
                var fallbackCourse = await _db.KhoaHocs
                    .Where(k => k.MaLop == student.MaLop.Value && k.MaGiaoVien != null && (k.MaKhoaHoc == enrollmentId || k.MaMonHoc == enrollmentId))
                    .FirstOrDefaultAsync(ct);
                if (fallbackCourse != null)
                {
                    teacherId = fallbackCourse.MaGiaoVien;
                    courseId = fallbackCourse.MaKhoaHoc;
                }
            }
        }

        if (teacherId == 0)
            return BadRequest(ApiResponseDto.Fail("Không tìm thấy lớp học hoặc giảng viên."));

        var existing = await _db.NopBaiDanhGias
            .FirstOrDefaultAsync(n => n.MaHocSinh == userId && n.MaGiaoVien == teacherId && n.MaHocKy == currentTerm.MaHocKy, ct);

        if (existing != null)
        {
            existing.SoLanNop = Math.Min(existing.SoLanNop + 1, 2);
            existing.SoLanSua = Math.Min(existing.SoLanSua + 1, 2);
            existing.CapNhatLuc = DateTime.UtcNow;
        }
        else
        {
            _db.NopBaiDanhGias.Add(new Models.NopBaiDanhGia
            {
                MaHocSinh = userId,
                MaGiaoVien = teacherId,
                MaHocKy = currentTerm.MaHocKy,
                SoLanNop = 1,
                SoLanSua = 0,
                CapNhatLuc = DateTime.UtcNow
            });
        }

        // Lưu từng tiêu chí đánh giá chuẩn (r1..r6)
        if (request.Ratings != null)
        {
            foreach (var kvp in request.Ratings)
            {
                int questionId = 1;
                if (kvp.Key.StartsWith("r", StringComparison.OrdinalIgnoreCase) && int.TryParse(kvp.Key.Substring(1), out var qNum))
                {
                    questionId = qNum;
                }
                else if (int.TryParse(kvp.Key, out var parsedQ))
                {
                    questionId = parsedQ;
                }

                int score = 5;
                if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    score = kvp.Value.GetInt32();
                }
                else if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(kvp.Value.GetString(), out var parsedScore))
                {
                    score = parsedScore;
                }

                if (score < 1) score = 1;
                if (score > 5) score = 5;

                _db.DanhGiaGiaoViens.Add(new Models.DanhGiaGiaoVien
                {
                    MaGiaoVien = teacherId,
                    MaHocKy = currentTerm.MaHocKy,
                    MaCauHoiDg = questionId,
                    DiemSo = score,
                    NhanXetTuDo = request.Feedback,
                    NgayTao = DateTime.UtcNow,
                    CohortHash = $"student-{userId}-course-{courseId}-term-{currentTerm.MaHocKy}"
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponseDto<object>.Ok(new { Success = true }));
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}

public class SubmitEvaluationRequest
{
    public int? EnrollmentId { get; set; }
    public string? Id { get; set; }
    public Dictionary<string, System.Text.Json.JsonElement>? Ratings { get; set; }
    public string? Feedback { get; set; }
}
