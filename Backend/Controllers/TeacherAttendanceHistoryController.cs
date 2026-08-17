using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher,giao_vien,CampusAdmin,AcademicStaff,nhan_vien,giao_vu,Admin,quan_tri,SuperAdmin,sieu_quan_tri")]
public class TeacherAttendanceHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherAttendanceHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("attendance/history")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAttendanceHistory(
        [FromQuery] int? courseId = null,
        [FromQuery] int? classId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var query = _context.BuoiHocs
                .AsNoTracking()
                .Include(b => b.KhoaHoc)
                    .ThenInclude(kh => kh!.MonHoc)
                .Include(b => b.KhoaHoc)
                    .ThenInclude(kh => kh!.Lop)
                .Include(b => b.KhoaHoc)
                    .ThenInclude(kh => kh!.HocKy)
                .Include(b => b.CaHoc)
                .Include(b => b.Phong)
                .Where(b => b.MaGiaoVien == userId || b.MaGiaoVienDayThay == userId || (b.KhoaHoc != null && b.KhoaHoc.MaGiaoVien == userId))
                .AsQueryable();

            if (courseId.HasValue)
                query = query.Where(b => b.MaKhoaHoc == courseId.Value);

            if (classId.HasValue)
                query = query.Where(b => b.KhoaHoc != null && b.KhoaHoc.MaLop == classId.Value);

            if (fromDate.HasValue)
                query = query.Where(b => b.NgayHoc >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.NgayHoc <= toDate.Value);

            var rawSessions = await query
                .OrderByDescending(b => b.NgayHoc)
                .ThenByDescending(b => b.MaBuoiHoc)
                .ToListAsync();

            var buoiHocIds = rawSessions.Select(b => b.MaBuoiHoc).ToList();
            var classIds = rawSessions.Where(b => b.KhoaHoc?.MaLop != null).Select(b => b.KhoaHoc!.MaLop).Distinct().ToList();

            var studentCountByClass = await _context.NguoiDungs
                .AsNoTracking()
                .Where(n => n.MaLop != null && classIds.Contains(n.MaLop.Value) && n.VaiTroChinh == "hoc_sinh")
                .GroupBy(n => n.MaLop!.Value)
                .Select(g => new { ClassId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClassId, x => x.Count);

            var diemDanhStats = await _context.DiemDanhs
                .AsNoTracking()
                .Where(d => buoiHocIds.Contains(d.MaBuoiHoc))
                .GroupBy(d => d.MaBuoiHoc)
                .Select(g => new
                {
                    MaBuoiHoc = g.Key,
                    Total = g.Count(),
                    Present = g.Count(d => d.TrangThai == "co_mat"),
                    Late = g.Count(d => d.TrangThai == "di_muon"),
                    Excused = g.Count(d => d.TrangThai == "co_phep" || d.TrangThai == "vang_co_phep"),
                    Absent = g.Count(d => d.TrangThai == "vang" || d.TrangThai == "vang_khong_phep")
                })
                .ToDictionaryAsync(x => x.MaBuoiHoc);

            var sessions = rawSessions.Select(b =>
            {
                var stats = diemDanhStats.GetValueOrDefault(b.MaBuoiHoc);
                int classStudents = b.KhoaHoc?.MaLop != null && studentCountByClass.TryGetValue(b.KhoaHoc.MaLop, out var c) ? c : 0;
                int totalCount = stats != null && stats.Total > 0 ? stats.Total : classStudents;

                string normalizedStatus = b.TrangThaiBuoi == "da_huy"
                    ? "da_huy"
                    : (!string.IsNullOrWhiteSpace(b.TrangThaiDiemDanh)
                        ? b.TrangThaiDiemDanh
                        : (stats != null && stats.Total > 0 ? "da_gui" : "chua_diem_danh"));

                return new
                {
                    id = b.MaBuoiHoc,
                    date = b.NgayHoc,
                    courseId = b.MaKhoaHoc,
                    subject = b.KhoaHoc?.MonHoc?.TenMonHoc ?? b.KhoaHoc?.TieuDe ?? "Môn học",
                    courseCode = b.KhoaHoc?.MonHoc?.MaCodeMonHoc ?? $"KH{b.MaKhoaHoc}",
                    classId = b.KhoaHoc?.MaLop ?? 0,
                    className = b.KhoaHoc?.Lop?.TenLop ?? "",
                    shift = new
                    {
                        label = b.CaHoc?.TenCa ?? "Ca học",
                        start = b.CaHoc?.GioBatDau.ToString(@"hh\:mm") ?? "",
                        end = b.CaHoc?.GioKetThuc.ToString(@"hh\:mm") ?? ""
                    },
                    room = b.Phong?.TenPhong ?? "",
                    total = totalCount,
                    present = stats?.Present ?? 0,
                    late = stats?.Late ?? 0,
                    excused = stats?.Excused ?? 0,
                    absent = stats?.Absent ?? 0,
                    status = normalizedStatus,
                    submittedAt = b.DiemDanhDaGuiLuc,
                    lockedAt = b.DiemDanhKhoaLuc,
                    lockReason = b.LyDoThayDoi ?? b.GhiChu
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                sessions = sessions.Where(s => s.status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(ApiResponseDto<object>.Ok(sessions));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải lịch sử điểm danh: " + ex.Message));
        }
    }
}
