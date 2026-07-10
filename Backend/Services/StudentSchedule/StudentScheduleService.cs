using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.StudentSchedule;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Services.StudentSchedule;

public class StudentScheduleService : IStudentScheduleService
{
    private readonly ApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _vietnamTimeZone;

    public StudentScheduleService(ApplicationDbContext context, TimeProvider timeProvider)
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

    public async Task<StudentScheduleSummaryDto> GetScheduleSummaryAsync(int studentId, int classId)
    {
        var today = GetVietnamDateToday();
        var nowTime = TimeOnly.FromDateTime(GetVietnamTimeNow());

        var visibleTerms = await GetVisibleTermsInternalAsync(classId, today);
        var currentTermModel = visibleTerms.FirstOrDefault(t => t.NgayBatDau <= today && t.NgayKetThuc >= today);
        var upcomingTermModel = visibleTerms.FirstOrDefault(t => t.NgayBatDau > today);

        var summary = new StudentScheduleSummaryDto();

        if (currentTermModel != null)
        {
            summary.CurrentTerm = new StudentScheduleTermDto
            {
                MaHocKy = currentTermModel.MaHocKy,
                MaCodeHocKy = currentTermModel.MaCodeHocKy,
                TenHocKy = currentTermModel.TenHocKy,
                NgayBatDau = currentTermModel.NgayBatDau,
                NgayKetThuc = currentTermModel.NgayKetThuc,
                IsCurrent = true,
                IsUpcoming = false,
                DaysUntilStart = null,
                SessionCount = await GetTermSessionCountAsync(currentTermModel.MaHocKy, classId)
            };

            var courses = await _context.KhoaHocs
                .Where(kh => kh.MaHocKy == currentTermModel.MaHocKy && kh.MaLop == classId && kh.TrangThai == "da_xuat_ban")
                .ToListAsync();

            summary.ActiveCourseCount = courses.Count;
            summary.SubjectCount = courses.Select(c => c.MaMonHoc).Distinct().Count();

            // Weekly session count for current week
            int dayOfWeek = (int)today.DayOfWeek;
            int daysSinceMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            var startOfWeek = today.AddDays(-daysSinceMonday);
            var endOfWeek = startOfWeek.AddDays(6);

            summary.WeeklySessionCount = await _context.BuoiHocs
                .Include(b => b.KhoaHoc)
                .Include(b => b.Tkb)
                .CountAsync(b => b.KhoaHoc!.MaLop == classId && 
                            b.Tkb!.TrangThai == "da_xuat_ban" && 
                            b.NgayHoc >= startOfWeek && 
                            b.NgayHoc <= endOfWeek &&
                            b.TrangThaiBuoi != "da_huy");
        }

        if (upcomingTermModel != null)
        {
            summary.UpcomingTerm = new StudentScheduleTermDto
            {
                MaHocKy = upcomingTermModel.MaHocKy,
                MaCodeHocKy = upcomingTermModel.MaCodeHocKy,
                TenHocKy = upcomingTermModel.TenHocKy,
                NgayBatDau = upcomingTermModel.NgayBatDau,
                NgayKetThuc = upcomingTermModel.NgayKetThuc,
                IsCurrent = false,
                IsUpcoming = true,
                DaysUntilStart = upcomingTermModel.NgayBatDau.DayNumber - today.DayNumber,
                SessionCount = await GetTermSessionCountAsync(upcomingTermModel.MaHocKy, classId)
            };
        }

        var todaySessions = await GetTodayScheduleAsync(studentId, classId);
        summary.TodaySessions = todaySessions;
        summary.TodaySessionCount = todaySessions.Count(s => s.TrangThaiBuoi != "da_huy");

        // Next session
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
                .Where(b => b.KhoaHoc!.MaLop == classId && b.NgayHoc > today && b.TrangThaiBuoi != "da_huy" && b.Tkb != null && b.Tkb.TrangThai == "da_xuat_ban")
                .OrderBy(b => b.NgayHoc).ThenBy(b => b.CaHoc!.GioBatDau)
                .Select(b => MapToDto(b))
                .FirstOrDefaultAsync();
        }

        return summary;
    }

    public async Task<List<StudentScheduleItemDto>> GetTodayScheduleAsync(int studentId, int classId)
    {
        var today = GetVietnamDateToday();
        
        var query = new StudentScheduleQueryDto
        {
            NgayTu = today.ToDateTime(TimeOnly.MinValue),
            NgayDen = today.ToDateTime(TimeOnly.MaxValue),
            PageIndex = 1,
            PageSize = 100
        };

        var pagedResult = await GetScheduleAsync(studentId, classId, query);
        return pagedResult.Items.ToList();
    }

    public async Task<PagedResultDto<StudentScheduleItemDto>> GetScheduleAsync(int studentId, int classId, StudentScheduleQueryDto query)
    {
        var dbQuery = _context.BuoiHocs
            .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.Lop)
            .Include(b => b.KhoaHoc!).ThenInclude(kh => kh.MonHoc)
            .Include(b => b.CaHoc)
            .Include(b => b.Phong)
            .Include(b => b.Tkb)
            .Include(b => b.GiaoVien)
            .Include(b => b.GiaoVienDayThay)
            .Where(b => b.KhoaHoc!.MaLop == classId && b.Tkb != null && b.Tkb.TrangThai == "da_xuat_ban");

        if (query.NgayTu.HasValue)
        {
            var ngayTu = DateOnly.FromDateTime(query.NgayTu.Value.Date);
            dbQuery = dbQuery.Where(b => b.NgayHoc >= ngayTu);
        }
        
        if (query.NgayDen.HasValue)
        {
            var ngayDen = DateOnly.FromDateTime(query.NgayDen.Value.Date);
            dbQuery = dbQuery.Where(b => b.NgayHoc <= ngayDen);
        }

        if (query.MaHocKy.HasValue)
            dbQuery = dbQuery.Where(b => b.KhoaHoc!.MaHocKy == query.MaHocKy);

        if (!string.IsNullOrEmpty(query.TrangThai))
            dbQuery = dbQuery.Where(b => b.TrangThaiBuoi == query.TrangThai);

        var totalItems = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderBy(b => b.NgayHoc).ThenBy(b => b.CaHoc!.GioBatDau).ThenBy(b => b.MaBuoiHoc)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return new PagedResultDto<StudentScheduleItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<List<StudentScheduleTermDto>> GetScheduleTermsAsync(int studentId, int classId)
    {
        var today = GetVietnamDateToday();
        var visibleTerms = await GetVisibleTermsInternalAsync(classId, today);
        var result = new List<StudentScheduleTermDto>();

        foreach (var t in visibleTerms)
        {
            result.Add(new StudentScheduleTermDto
            {
                MaHocKy = t.MaHocKy,
                MaCodeHocKy = t.MaCodeHocKy,
                TenHocKy = t.TenHocKy,
                NgayBatDau = t.NgayBatDau,
                NgayKetThuc = t.NgayKetThuc,
                IsCurrent = t.NgayBatDau <= today && t.NgayKetThuc >= today,
                IsUpcoming = t.NgayBatDau > today,
                DaysUntilStart = t.NgayBatDau > today ? t.NgayBatDau.DayNumber - today.DayNumber : null,
                SessionCount = await GetTermSessionCountAsync(t.MaHocKy, classId)
            });
        }

        return result;
    }

    private async Task<List<Models.HocKy>> GetVisibleTermsInternalAsync(int classId, DateOnly today)
    {
        // 1. Current term
        // 2. Upcoming term starting within 7 days
        var maxStartDate = today.AddDays(7);

        var terms = await _context.HocKys
            .Where(hk => 
                (hk.NgayBatDau <= today && hk.NgayKetThuc >= today) || 
                (hk.NgayBatDau > today && hk.NgayBatDau <= maxStartDate))
            .OrderBy(hk => hk.NgayBatDau)
            .ToListAsync();

        var visibleTerms = new List<Models.HocKy>();
        foreach (var term in terms)
        {
            // Only add if student's class has published schedules or courses in this term
            var hasContent = await _context.KhoaHocs
                .AnyAsync(kh => kh.MaHocKy == term.MaHocKy && kh.MaLop == classId && kh.TrangThai == "da_xuat_ban");
            
            if (hasContent)
            {
                visibleTerms.Add(term);
            }
            else 
            {
                var hasPublishedTkb = await _context.ThoiKhoaBieus
                    .Include(t => t.KhoaHoc)
                    .AnyAsync(t => t.KhoaHoc!.MaHocKy == term.MaHocKy && t.KhoaHoc.MaLop == classId && t.TrangThai == "da_xuat_ban");
                if (hasPublishedTkb) 
                {
                    visibleTerms.Add(term);
                }
            }
        }

        return visibleTerms;
    }

    private async Task<int> GetTermSessionCountAsync(int maHocKy, int classId)
    {
        return await _context.BuoiHocs
            .Include(b => b.KhoaHoc)
            .Include(b => b.Tkb)
            .CountAsync(b => b.KhoaHoc!.MaHocKy == maHocKy && b.KhoaHoc.MaLop == classId && b.Tkb!.TrangThai == "da_xuat_ban" && b.TrangThaiBuoi != "da_huy");
    }

    private static StudentScheduleItemDto MapToDto(Models.BuoiHoc b)
    {
        return new StudentScheduleItemDto
        {
            MaBuoiHoc = b.MaBuoiHoc,
            MaKhoaHoc = b.MaKhoaHoc,
            TieuDeKhoaHoc = b.KhoaHoc?.TieuDe,
            MaHocKy = b.KhoaHoc?.MaHocKy,
            TenHocKy = "", // Optimization: Avoid extra joins for TenHocKy if not strictly needed inside every item
            MaLop = b.KhoaHoc?.MaLop,
            TenLop = b.KhoaHoc?.Lop?.TenLop,
            MaMonHoc = b.KhoaHoc?.MaMonHoc,
            MaCodeMonHoc = b.KhoaHoc?.MonHoc?.MaCodeMonHoc,
            TenMonHoc = b.KhoaHoc?.MonHoc?.TenMonHoc,
            NgayHoc = b.NgayHoc,
            MaCaHoc = b.MaCaHoc,
            TenCa = b.CaHoc?.TenCa,
            GioBatDau = b.CaHoc?.GioBatDau,
            GioKetThuc = b.CaHoc?.GioKetThuc,
            MaPhong = b.MaPhong,
            MaCodePhong = b.Phong?.MaCodePhong,
            TenPhong = b.Phong?.TenPhong,
            MaGiaoVien = b.MaGiaoVien,
            TenGiaoVien = b.GiaoVien?.HoTen,
            MaGiaoVienDayThay = b.MaGiaoVienDayThay,
            TenGiaoVienDayThay = b.GiaoVienDayThay?.HoTen,
            IsSubstitute = b.MaGiaoVienDayThay != null,
            TrangThaiBuoi = b.TrangThaiBuoi,
            TrangThaiDiemDanh = b.TrangThaiDiemDanh
        };
    }
}
