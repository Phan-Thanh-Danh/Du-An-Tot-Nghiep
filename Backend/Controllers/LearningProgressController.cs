using Backend.Constants;
using Backend.Data;
using Backend.DTOs.LearningProgress;
using Backend.Services.LearningProgress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/learning-progress")]
[Authorize]
public class LearningProgressController : ControllerBase
{
    private readonly ILearningProgressService _progressService;
    private readonly ApplicationDbContext _context;

    public LearningProgressController(
        ILearningProgressService progressService,
        ApplicationDbContext context)
    {
        _progressService = progressService;
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("sessions/start")]
    public async Task<IActionResult> StartSession([FromBody] ContentSessionRequestDto request)
    {
        var studentId = GetCurrentUserId();
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var response = await _progressService.StartContentSessionAsync(studentId, request, userAgent, ipAddress);
        return Ok(response);
    }

    [HttpPost("sessions/{sessionToken:guid}/end")]
    public async Task<IActionResult> EndSession(Guid sessionToken)
    {
        var studentId = GetCurrentUserId();
        await _progressService.EndContentSessionAsync(studentId, sessionToken);
        return Ok(new { message = "Phiên học đã kết thúc." });
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] ContentHeartbeatRequestDto request)
    {
        var studentId = GetCurrentUserId();
        var response = await _progressService.ProcessHeartbeatAsync(studentId, request);
        return Ok(response);
    }

    [HttpGet("contents/{contentId}")]
    public async Task<IActionResult> GetContentProgress(int contentId)
    {
        var studentId = GetCurrentUserId();
        var progress = await _context.TienDoNoiDungHocTaps
            .FirstOrDefaultAsync(p => p.MaHocSinh == studentId && p.MaNoiDung == contentId);

        if (progress == null)
        {
            return Ok(new StudentContentProgressDto
            {
                MaNoiDung = contentId,
                TrangThai = "chua_bat_dau",
                PhanTramTienDo = 0
            });
        }

        return Ok(new StudentContentProgressDto
        {
            MaNoiDung = contentId,
            TrangThai = progress.TrangThai,
            PhanTramTienDo = progress.PhanTramTienDo,
            ViTriVideoCuoiGiay = progress.ViTriVideoCuoiGiay,
            LanTuongTacCuoi = progress.LanTuongTacCuoi,
            HoanThanhLuc = progress.HoanThanhLuc
        });
    }

    [HttpGet("classes/{classId}/students")]
    public async Task<IActionResult> GetClassProgressReport(int classId)
    {
        var currentUserId = GetCurrentUserId();
        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        // Kiểm tra quyền: Chỉ GVCN của lớp hoặc Admin/GiaoVu mới được xem
        var hopLe = false;
        if (currentRole == AuthRoles.Admin || currentRole == AuthRoles.AcademicStaff)
        {
            hopLe = true;
        }
        else
        {
            var lop = await _context.LopHanhChinhs.FindAsync(classId);
            if (lop != null && lop.MaGiaoVienChuNhiem == currentUserId)
            {
                hopLe = true;
            }
        }

        if (!hopLe)
        {
            return Forbid();
        }

        // Lấy danh sách tiến độ theo học sinh
        var result = await _context.NguoiDungs
            .Where(u => u.MaLop == classId)
            .Select(u => new
            {
                u.MaNguoiDung,
                u.HoTen,
                u.Email,
                TienDoBaiHocs = _context.TienDoBaiHocs.Where(p => p.MaHocSinh == u.MaNguoiDung).Select(p => new
                {
                    p.MaBaiHoc,
                    p.PhanTramTienDo,
                    p.HoanThanhLuc
                }).ToList()
            })
            .ToListAsync();

        return Ok(result);
    }
}
