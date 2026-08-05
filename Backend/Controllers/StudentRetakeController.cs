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

        // Môn học rớt hoặc điểm < 5
        var failedSubjectsQuery = _context.DiemSos.AsNoTracking()
            .Where(d => d.MaHocSinh == studentId && (d.GpaMonHoc < 5.0m || d.DiemCuoiKy < 5.0m))
            .Select(d => d.MaMonHoc);

        // Lấy danh sách các môn này mà đang có ca thi mở
        var today = DateTime.Today;
        var availableSubjects = await _context.DanhMucMonHocs.AsNoTracking()
            .Where(m => failedSubjectsQuery.Contains(m.MaMonHoc))
            .Where(m => _context.CaThis.Any(c => 
                c.LichThiTong.MaMonHoc == m.MaMonHoc && 
                (c.TrangThai == "nhap" || c.TrangThai == "dang_mo") &&
                c.NgayThi >= today))
            .Select(m => new
            {
                id = m.MaMonHoc,
                name = m.TenMonHoc,
                code = m.MaCodeMonHoc
            })
            .ToListAsync(cancellationToken);

        return Ok(availableSubjects);
    }

    [HttpGet("subjects/{subjectId}/exam-sessions")]
    public async Task<IActionResult> GetExamSessions(int subjectId, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var examSessions = await _context.CaThis.AsNoTracking()
            .Include(c => c.Phong)
            .Where(c => c.LichThiTong.MaMonHoc == subjectId && 
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
