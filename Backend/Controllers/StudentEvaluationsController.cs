using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
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

        var enrollments = await _db.DangKyHocPhans
            .Include(d => d.LopHocPhan!)
                .ThenInclude(l => l.MonHoc)
            .Where(d => d.MaHocSinh == userId && d.TrangThai == "da_duyet")
            .Select(d => d.LopHocPhan)
            .Where(l => l != null)
            .Distinct()
            .ToListAsync(ct);

        var courseTeacherMap = await _db.KhoaHocs
            .Where(k => k.MaLopHocPhan != null && enrollments.Select(e => e!.MaLopHocPhan).Contains(k.MaLopHocPhan.Value))
            .Select(k => new { k.MaLopHocPhan, k.MaGiaoVien, TeacherName = k.GiaoVien != null ? k.GiaoVien.HoTen : "" })
            .Distinct()
            .ToDictionaryAsync(k => k.MaLopHocPhan!.Value, k => new { k.MaGiaoVien, k.TeacherName }, ct);

        var submittedEvals = await _db.NopBaiDanhGias
            .Where(n => n.MaHocSinh == userId && n.MaHocKy == currentTerm.MaHocKy)
            .ToListAsync(ct);

        var result = enrollments.Select(l =>
        {
            var courseInfo = courseTeacherMap.GetValueOrDefault(l!.MaLopHocPhan);
            var teacherId = courseInfo?.MaGiaoVien ?? 0;
            var submitted = submittedEvals.FirstOrDefault(n => n.MaGiaoVien == teacherId);

            return new
            {
                Id = $"EVAL-{l.MaLopHocPhan}",
                EnrollmentId = l.MaLopHocPhan,
                Subject = l.MonHoc?.TenMonHoc ?? "",
                Teacher = courseInfo?.TeacherName ?? "",
                Status = submitted != null ? "Completed" : "Pending",
                EditsLeft = submitted != null ? Math.Max(0, 2 - submitted.SoLanSua) : 2,
                Ratings = new { r1 = 0, r2 = 0, r3 = 0 },
                Feedback = ""
            };
        }).ToList();

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

        var courseTeacher = await _db.KhoaHocs
            .Where(k => k.MaLopHocPhan == request.EnrollmentId)
            .Select(k => new { k.MaGiaoVien })
            .FirstOrDefaultAsync(ct);

        if (courseTeacher == null)
            return BadRequest(ApiResponseDto.Fail("Không tìm thấy lớp học hoặc giảng viên."));

        var existing = await _db.NopBaiDanhGias
            .FirstOrDefaultAsync(n => n.MaHocSinh == userId && n.MaGiaoVien == courseTeacher.MaGiaoVien && n.MaHocKy == currentTerm.MaHocKy, ct);

        if (existing != null)
        {
            existing.SoLanNop++;
            existing.SoLanSua++;
            existing.CapNhatLuc = DateTime.UtcNow;
        }
        else
        {
            _db.NopBaiDanhGias.Add(new Models.NopBaiDanhGia
            {
                MaHocSinh = userId,
                MaGiaoVien = courseTeacher.MaGiaoVien,
                MaHocKy = currentTerm.MaHocKy,
                SoLanNop = 1,
                SoLanSua = 0,
                CapNhatLuc = DateTime.UtcNow
            });
        }

        if (request.Ratings != null)
        {
            foreach (var rating in request.Ratings)
            {
                _db.DanhGiaGiaoViens.Add(new Models.DanhGiaGiaoVien
                {
                    MaGiaoVien = courseTeacher.MaGiaoVien,
                    MaHocKy = currentTerm.MaHocKy,
                    MaCauHoiDg = rating.Key,
                    DiemSo = rating.Value,
                    NhanXetTuDo = request.Feedback,
                    NgayTao = DateTime.UtcNow,
                    CohortHash = $"student-{userId}-term-{currentTerm.MaHocKy}"
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
    public int EnrollmentId { get; set; }
    public Dictionary<int, int>? Ratings { get; set; }
    public string? Feedback { get; set; }
}
