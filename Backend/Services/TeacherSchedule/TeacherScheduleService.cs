using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.TeacherSchedule;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Services.TeacherSchedule;

public class TeacherScheduleService : ITeacherScheduleService
{
    private readonly ApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _vietnamTimeZone;

    public TeacherScheduleService(ApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
        _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    }

    private DateTime GetVietnamTimeNow()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, _vietnamTimeZone);
    }

    private DateOnly GetVietnamDateToday()
    {
        return DateOnly.FromDateTime(GetVietnamTimeNow());
    }

    public async Task<TeacherScheduleSummaryDto> GetSummaryAsync(int teacherId)
    {
        var today = GetVietnamDateToday();
        var nowTime = TimeOnly.FromDateTime(GetVietnamTimeNow());

        var currentTerm = await _context.HocKys
            .Where(hk => hk.NgayBatDau <= today && hk.NgayKetThuc >= today)
            .OrderByDescending(hk => hk.NgayBatDau)
            .FirstOrDefaultAsync();

        var summary = new TeacherScheduleSummaryDto();

        if (currentTerm != null)
        {
            summary.CurrentTerm = new TeacherScheduleTermDto
            {
                MaHocKy = currentTerm.MaHocKy,
                TenHocKy = currentTerm.TenHocKy,
                NgayBatDau = currentTerm.NgayBatDau,
                NgayKetThuc = currentTerm.NgayKetThuc,
                IsCurrent = true
            };

            var courses = await _context.KhoaHocs
                .Include(kh => kh.Lop)
                .Include(kh => kh.MonHoc)
                .Where(kh => kh.MaHocKy == currentTerm.MaHocKy && kh.MaGiaoVien == teacherId && kh.TrangThai == "da_xuat_ban")
                .ToListAsync();

            summary.AssignedCourseCount = courses.Select(c => c.MaKhoaHoc).Distinct().Count();
            summary.AssignedClassCount = courses.Select(c => c.MaLop).Distinct().Count();
            summary.SubjectCount = courses.Select(c => c.MaMonHoc).Distinct().Count();

            // Count weekly shifts from published schedules
            var schedules = await _context.ThoiKhoaBieus
                .Where(t => t.KhoaHoc!.MaHocKy == currentTerm.MaHocKy && t.KhoaHoc.MaGiaoVien == teacherId && t.TrangThai == "da_xuat_ban")
                .ToListAsync();
            summary.WeeklyShiftCount = schedules.Count;
            
            summary.AssignedCourses = courses.Select(c => new TeacherAssignedCourseDto
            {
                MaKhoaHoc = c.MaKhoaHoc,
                TieuDe = c.TieuDe,
                MaLop = c.MaLop,
                TenLop = c.Lop?.TenLop ?? "",
                MaMonHoc = c.MaMonHoc,
                TenMonHoc = c.MonHoc?.TenMonHoc ?? "",
                MaHocKy = c.MaHocKy,
                TenHocKy = currentTerm.TenHocKy,
                SessionsPerWeek = schedules.Count(s => s.MaKhoaHoc == c.MaKhoaHoc)
            }).ToList();
        }

        var todaySessions = await GetTodayScheduleAsync(teacherId);
        summary.TodaySessions = todaySessions;

        // Next session is the first session today that hasn't ended yet
        // If all today's sessions ended, look for the next days
        summary.NextSession = todaySessions
            .Where(s => s.TrangThaiBuoi != "da_huy" && s.GioKetThuc > nowTime)
            .OrderBy(s => s.GioBatDau)
            .FirstOrDefault();

        if (summary.NextSession == null)
        {
            // Find next upcoming session in the future
            summary.NextSession = await _context.BuoiHocs
                .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.Lop)
                .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.MonHoc)
                .Include(b => b.CaHoc)
                .Include(b => b.Phong)
                .Include(b => b.Tkb)
                .Where(b => (b.MaGiaoVien == teacherId || b.MaGiaoVienDayThay == teacherId) && b.NgayHoc > today && b.TrangThaiBuoi != "da_huy" && b.Tkb != null && b.Tkb.TrangThai == "da_xuat_ban")
                .OrderBy(b => b.NgayHoc).ThenBy(b => b.CaHoc!.GioBatDau)
                .Select(b => new TeacherScheduleItemDto
                {
                    MaBuoiHoc = b.MaBuoiHoc,
                    MaKhoaHoc = b.MaKhoaHoc,
                    TieuDeKhoaHoc = b.KhoaHoc!.TieuDe,
                    MaHocKy = b.KhoaHoc.MaHocKy,
                    MaLop = b.KhoaHoc.MaLop,
                    TenLop = b.KhoaHoc.Lop!.TenLop,
                    MaMonHoc = b.KhoaHoc.MaMonHoc,
                    TenMonHoc = b.KhoaHoc.MonHoc!.TenMonHoc,
                    NgayHoc = b.NgayHoc,
                    MaCaHoc = b.MaCaHoc,
                    TenCa = b.CaHoc!.TenCa,
                    GioBatDau = b.CaHoc.GioBatDau,
                    GioKetThuc = b.CaHoc.GioKetThuc,
                    MaPhong = b.MaPhong,
                    TenPhong = b.Phong!.TenPhong,
                    IsSubstitute = b.MaGiaoVienDayThay == teacherId,
                    TeacherRoleLabel = b.MaGiaoVienDayThay == teacherId ? "Dạy thay" : "Giảng viên chính",
                    TrangThaiBuoi = b.TrangThaiBuoi,
                    TrangThaiDiemDanh = b.TrangThaiDiemDanh
                })
                .FirstOrDefaultAsync();
        }

        return summary;
    }

    public async Task<List<TeacherScheduleItemDto>> GetTodayScheduleAsync(int teacherId)
    {
        var today = GetVietnamDateToday();
        
        var query = new TeacherScheduleQueryDto
        {
            NgayTu = today,
            NgayDen = today,
            PageIndex = 1,
            PageSize = 100
        };

        var pagedResult = await GetScheduleAsync(teacherId, query);
        return pagedResult.Items.ToList();
    }

    public async Task<PagedResultDto<TeacherScheduleItemDto>> GetScheduleAsync(int teacherId, TeacherScheduleQueryDto query)
    {
        var dbQuery = _context.BuoiHocs
            .AsNoTracking()
            .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.Lop)
            .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.MonHoc)
            .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.HocKy)
            .Include(b => b.CaHoc)
            .Include(b => b.Phong)
            .Include(b => b.Tkb)
            .Where(b => (b.MaGiaoVien == teacherId || b.MaGiaoVienDayThay == teacherId) && b.Tkb != null && b.Tkb.TrangThai == "da_xuat_ban");

        if (query.NgayTu.HasValue)
        {
            dbQuery = dbQuery.Where(b => b.NgayHoc >= query.NgayTu.Value);
        }

        if (query.NgayDen.HasValue)
        {
            dbQuery = dbQuery.Where(b => b.NgayHoc <= query.NgayDen.Value);
        }

        if (query.MaHocKy.HasValue)
        {
            dbQuery = dbQuery.Where(b => b.KhoaHoc!.MaHocKy == query.MaHocKy.Value);
        }

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderBy(b => b.NgayHoc)
            .ThenBy(b => b.CaHoc!.GioBatDau)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new TeacherScheduleItemDto
            {
                MaBuoiHoc = b.MaBuoiHoc,
                MaKhoaHoc = b.MaKhoaHoc,
                TieuDeKhoaHoc = b.KhoaHoc!.TieuDe,
                MaHocKy = b.KhoaHoc.MaHocKy,
                TenHocKy = b.KhoaHoc.HocKy != null ? b.KhoaHoc.HocKy.TenHocKy : "",
                MaLop = b.KhoaHoc.MaLop,
                TenLop = b.KhoaHoc.Lop!.TenLop,
                MaMonHoc = b.KhoaHoc.MaMonHoc,
                TenMonHoc = b.KhoaHoc.MonHoc!.TenMonHoc,
                NgayHoc = b.NgayHoc,
                MaCaHoc = b.MaCaHoc,
                TenCa = b.CaHoc!.TenCa,
                GioBatDau = b.CaHoc.GioBatDau,
                GioKetThuc = b.CaHoc.GioKetThuc,
                MaPhong = b.MaPhong,
                TenPhong = b.Phong!.TenPhong,
                IsSubstitute = b.MaGiaoVienDayThay == teacherId,
                TeacherRoleLabel = b.MaGiaoVienDayThay == teacherId ? "Dạy thay" : "Giảng viên chính",
                TrangThaiBuoi = b.TrangThaiBuoi,
                TrangThaiDiemDanh = b.TrangThaiDiemDanh
            })
            .ToListAsync();

        return new PagedResultDto<TeacherScheduleItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<List<TeacherScheduleTermDto>> GetTermsAsync(int teacherId)
    {
        var today = GetVietnamDateToday();
        var termIds = await _context.BuoiHocs
            .AsNoTracking()
            .Include(b => b.KhoaHoc)
            .Include(b => b.Tkb)
            .Where(b => (b.MaGiaoVien == teacherId || b.MaGiaoVienDayThay == teacherId) && b.Tkb != null && b.Tkb.TrangThai == "da_xuat_ban" && b.KhoaHoc!.MaHocKy != null)
            .Select(b => b.KhoaHoc!.MaHocKy!.Value)
            .Distinct()
            .ToListAsync();

        var terms = await _context.HocKys
            .AsNoTracking()
            .Where(hk => termIds.Contains(hk.MaHocKy))
            .OrderByDescending(hk => hk.NgayBatDau)
            .Select(hk => new TeacherScheduleTermDto
            {
                MaHocKy = hk.MaHocKy,
                TenHocKy = hk.TenHocKy,
                NgayBatDau = hk.NgayBatDau,
                NgayKetThuc = hk.NgayKetThuc,
                IsCurrent = hk.NgayBatDau <= today && hk.NgayKetThuc >= today
            })
            .ToListAsync();

        return terms;
    }
}
