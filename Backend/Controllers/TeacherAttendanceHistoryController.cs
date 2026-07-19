using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherAttendanceHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherAttendanceHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("attendance/history")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAttendanceHistory()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var sessions = await _context.BuoiHocs
                .Where(b => b.MaGiaoVien == userId && b.NgayHoc < today)
                .OrderByDescending(b => b.NgayHoc)
                .Select(b => new
                {
                    id = b.MaBuoiHoc,
                    date = b.NgayHoc,
                    subject = b.KhoaHoc != null ? b.KhoaHoc.TieuDe : "",
                    courseCode = b.KhoaHoc != null ? b.KhoaHoc.MaKhoaHoc.ToString() : "",
                    className = b.KhoaHoc != null && b.KhoaHoc.Lop != null ? b.KhoaHoc.Lop.TenLop : "",
                    shift = new { label = b.CaHoc != null ? b.CaHoc.TenCa : "" },
                    room = b.Phong != null ? b.Phong.TenPhong : "",
                    total = _context.DiemDanhs.Count(d => d.MaBuoiHoc == b.MaBuoiHoc),
                    present = _context.DiemDanhs.Count(d => d.MaBuoiHoc == b.MaBuoiHoc && d.TrangThai == "co_mat"),
                    late = _context.DiemDanhs.Count(d => d.MaBuoiHoc == b.MaBuoiHoc && d.TrangThai == "di_muon"),
                    excused = _context.DiemDanhs.Count(d => d.MaBuoiHoc == b.MaBuoiHoc && d.TrangThai == "co_phep"),
                    absent = _context.DiemDanhs.Count(d => d.MaBuoiHoc == b.MaBuoiHoc && d.TrangThai == "vang"),
                    status = b.TrangThaiBuoi,
                    submittedAt = (DateTime?)null,
                    lockedAt = (DateTime?)null,
                    lockReason = (string)null
                })
                .ToListAsync();

            return Ok(ApiResponseDto<object>.Ok(sessions));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải lịch sử điểm danh: " + ex.Message));
        }
    }
}
