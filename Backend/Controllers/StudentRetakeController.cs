using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs.Auth;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/retake")]
[Authorize]
public class StudentRetakeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentRetakeController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
            return currentUser.UserId;

        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }

    [HttpGet("available-subjects")]
    public async Task<IActionResult> GetAvailableRetakeSubjects(CancellationToken cancellationToken)
    {
        var studentId = GetCurrentUserId();

        // Môn học rớt: điểm GPA môn thấp hơn ngưỡng đạt đã cấu hình cho môn đó trong học kỳ
        var failedSubjectsQuery = _context.DiemSos.AsNoTracking()
            .Join(
                _context.CauHinhDiemMonHocs.AsNoTracking(),
                d => new { d.MaMonHoc, d.MaHocKy },
                c => new { c.MaMonHoc, c.MaHocKy },
                (d, c) => new { d, c })
            .Where(x => x.d.MaHocSinh == studentId && x.d.GpaMonHoc < x.c.NguongDat)
            .Select(x => x.d.MaMonHoc);

        // Chỉ giữ khóa học (lớp học phần cụ thể) của môn rớt đang có ca thi mở ('nhap'/'dang_mo') từ hôm nay
        var today = DateTime.Today;
        var availableCourses = await _context.KhoaHocs.AsNoTracking()
            .Where(k => failedSubjectsQuery.Contains(k.MaMonHoc))
            .Where(k => _context.CaThis.Any(c =>
                c.LichThiTong.MaMonHoc == k.MaMonHoc &&
                (c.TrangThai == "nhap" || c.TrangThai == "dang_mo") &&
                c.NgayThi >= today))
            .Select(k => new
            {
                id = k.MaKhoaHoc,
                name = k.TieuDe,
                code = k.MonHoc != null ? k.MonHoc.MaCodeMonHoc : ""
            })
            .OrderBy(k => k.name)
            .ToListAsync(cancellationToken);

        return Ok(availableCourses);
    }

    [HttpGet("courses/{courseId}/exam-sessions")]
    public async Task<IActionResult> GetExamSessions(int courseId, CancellationToken cancellationToken)
    {
        var course = await _context.KhoaHocs.AsNoTracking()
            .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId, cancellationToken);
        if (course == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Khóa học không tồn tại.");
        }

        var today = DateTime.Today;
        var examSessions = await _context.CaThis.AsNoTracking()
            .Include(c => c.Phong)
            .Where(c => c.LichThiTong.MaMonHoc == course.MaMonHoc &&
                        (c.TrangThai == "nhap" || c.TrangThai == "dang_mo") &&
                        c.NgayThi >= today)
            .Select(c => new
            {
                id = c.MaCaThi,
                name = $"{c.TenCaThi} - {c.NgayThi:dd/MM/yyyy} ({c.ThoiGianBatDau:HH:mm}-{c.ThoiGianKetThuc:HH:mm}) - Phòng: {(c.Phong != null ? c.Phong.TenPhong : "Chưa xếp")}"
            })
            .ToListAsync(cancellationToken);

        return Ok(examSessions);
    }
}
