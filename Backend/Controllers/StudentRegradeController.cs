using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Constants;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/student/regrade")]
[Authorize]
public class StudentRegradeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentRegradeController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items.TryGetValue("CurrentUser", out var userObj) && userObj is Backend.DTOs.Auth.CurrentUserContext currentUser)
        {
            return currentUser.UserId;
        }
        return 0;
    }

    [HttpGet("available-scores")]
    public async Task<IActionResult> GetAvailableScores(CancellationToken cancellationToken)
    {
        var studentId = GetCurrentUserId();
        if (studentId == 0) return Unauthorized();

        var scores = await _context.DiemSos.AsNoTracking()
            .Include(d => d.MonHoc)
            .Include(d => d.HocKy)
            .Where(d => d.MaHocSinh == studentId)
            .Select(d => new
            {
                id = d.MaDiemSo,
                subjectName = d.MonHoc != null ? d.MonHoc.TenMonHoc : "Unknown",
                termName = d.HocKy != null ? d.HocKy.TenHocKy : "Unknown",
                diem_qua_trinh = d.DiemQuaTrinh,
                diem_giua_ky = d.DiemGiuaKy,
                diem_cuoi_ky = d.DiemCuoiKy,
                gpa_mon_hoc = d.GpaMonHoc,
                label = $"{d.MonHoc!.TenMonHoc} - {d.HocKy!.TenHocKy} - Năm {d.NamNhapHoc}"
            })
            .ToListAsync(cancellationToken);

        return Ok(scores);
    }
}
