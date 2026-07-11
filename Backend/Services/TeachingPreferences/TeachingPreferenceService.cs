using Backend.Configuration;
using Backend.Data;
using Backend.DTOs.TeachingPreferences;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.TeachingPreferences;

public class TeachingPreferenceService : ITeachingPreferenceService
{
    private readonly ApplicationDbContext _db;
    private readonly IAcademicSchedulingContextService _schedulingContext;
    private readonly TimeProvider _timeProvider;
    private readonly TeachingPreferenceOptions _options;

    public TeachingPreferenceService(
        ApplicationDbContext db,
        IAcademicSchedulingContextService schedulingContext,
        TimeProvider timeProvider,
        IOptions<TeachingPreferenceOptions> options)
    {
        _db = db;
        _schedulingContext = schedulingContext;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    private DateTime GetVietnamTime()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            return TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
        }
    }

    private async Task<(DateTime? openTime, DateTime? deadline)> GetTermWindowAsync(int maHocKy)
    {
        var term = await _db.HocKys.FindAsync(maHocKy);
        if (term == null) return (null, null);

        var termStartDate = term.NgayBatDau.ToDateTime(TimeOnly.MinValue);
        var openTime = termStartDate.AddDays(-_options.OpenDaysBeforeTermStart);
        var deadline = termStartDate.AddDays(-_options.DeadlineDaysBeforeTermStart).AddHours(23).AddMinutes(59).AddSeconds(59);
        return (openTime, deadline);
    }

    public async Task<TeachingPreferenceContextDto> GetContextAsync(int teacherId)
    {
        var teacher = await _db.NguoiDungs.FindAsync(teacherId);
        if (teacher == null) throw new ApiException(404, "Không tìm thấy giảng viên");
        
        var context = await _schedulingContext.GetContextAsync(teacher.MaDonVi);
        var result = new TeachingPreferenceContextDto
        {
            SchedulingContext = context
        };

        if (context.SchedulableTerm == null)
        {
            result.ReasonCode = "NO_SCHEDULABLE_TERM";
            result.ReasonMessage = "Chưa có học kỳ tương lai phù hợp để đăng ký nguyện vọng.";
            return result;
        }

        var (openTime, deadline) = await GetTermWindowAsync(context.SchedulableTerm.MaHocKy);
        result.OpenTime = openTime;
        result.Deadline = deadline;

        var now = GetVietnamTime();
        
        result.IsOpen = openTime <= now && now <= deadline;
        result.IsPastDeadline = deadline < now;

        if (now < openTime)
        {
            result.ReasonCode = "NOT_OPEN_YET";
            result.ReasonMessage = $"Thời gian đăng ký chưa bắt đầu. Hệ thống sẽ mở vào {openTime:dd/MM/yyyy HH:mm}.";
        }

        var preference = await _db.GiaoVienNguyenVongHocKys
            .Where(x => x.MaGiaoVien == teacherId && x.MaHocKy == context.SchedulableTerm.MaHocKy)
            .FirstOrDefaultAsync();
            
        if (preference != null)
        {
            result.CurrentStatus = preference.TrangThai;
            result.LastUpdated = preference.NgayCapNhat ?? preference.NgayTao;
            result.SubmittedAt = preference.NgayGui;
        }

        result.CanEdit = result.IsOpen && result.CurrentStatus != "submitted";
        result.CanSubmit = result.IsOpen && result.CurrentStatus != "submitted";

        result.ActiveShifts = await _db.CaHocs
            .Where(c => c.ConHoatDong)
            .OrderBy(c => c.ThuTu)
            .Select(c => new TeachingPreferenceShiftDto
            {
                MaCaHoc = c.MaCaHoc,
                TenCa = c.TenCa,
                Buoi = c.Buoi,
                GioBatDau = c.GioBatDau.ToString("HH:mm"),
                GioKetThuc = c.GioKetThuc.ToString("HH:mm"),
                ThuTu = c.ThuTu
            })
            .ToListAsync();

        return result;
    }

    public async Task<TeachingPreferenceFormDto> GetTeacherFormAsync(int teacherId, int maHocKy)
    {
        var preference = await _db.GiaoVienNguyenVongHocKys
            .Include(x => x.ChiTietNguyenVong)
            .Where(x => x.MaGiaoVien == teacherId && x.MaHocKy == maHocKy)
            .FirstOrDefaultAsync();

        if (preference == null)
        {
            return new TeachingPreferenceFormDto
            {
                MaHocKy = maHocKy
            };
        }

        return new TeachingPreferenceFormDto
        {
            MaHocKy = maHocKy,
            SoLopToiDaMongMuon = preference.SoLopToiDaMongMuon,
            SoCaToiDaMoiTuan = preference.SoCaToiDaMoiTuan,
            GhiChu = preference.GhiChu,
            TrangThai = preference.TrangThai,
            NgayCapNhat = preference.NgayCapNhat ?? preference.NgayTao,
            NgayGui = preference.NgayGui,
            Slots = preference.ChiTietNguyenVong.Select(x => new TeachingPreferenceSlotDto
            {
                ThuTrongTuan = x.ThuTrongTuan,
                MaCaHoc = x.MaCaHoc,
                MucDo = x.MucDo
            }).ToList()
        };
    }

    private async Task ValidateModificationAllowedAsync(int teacherId, int maHocKy)
    {
        var ctx = await GetContextAsync(teacherId);
        if (ctx.SchedulingContext.SchedulableTerm?.MaHocKy != maHocKy)
        {
            throw new ApiException(400, "Học kỳ này không mở để đăng ký nguyện vọng.");
        }

        if (ctx.ReasonCode == "NOT_OPEN_YET")
        {
            throw new ApiException(400, ctx.ReasonMessage ?? "Chưa tới thời gian đăng ký.");
        }

        if (ctx.IsPastDeadline)
        {
            throw new ApiException(400, "Đã quá hạn đăng ký nguyện vọng cho học kỳ này.");
        }

        if (ctx.CurrentStatus == "submitted")
        {
            throw new ApiException(400, "Nguyện vọng đã được gửi và không thể chỉnh sửa.");
        }
    }

    public async Task<TeachingPreferenceFormDto> SaveDraftAsync(int teacherId, int maHocKy, UpdateTeachingPreferenceDto dto)
    {
        await ValidateModificationAllowedAsync(teacherId, maHocKy);
        
        var activeShifts = await _db.CaHocs.Where(c => c.ConHoatDong).Select(c => c.MaCaHoc).ToListAsync();
        var validMucDo = new[] { "preferred", "available", "unavailable" };

        if (dto.SoLopToiDaMongMuon < 0) throw new ApiException(400, "Số lớp mong muốn không được âm.");
        if (dto.SoCaToiDaMoiTuan < 0) throw new ApiException(400, "Số ca mong muốn không được âm.");
        if (dto.GhiChu?.Length > 1000) throw new ApiException(400, "Ghi chú vượt quá 1000 ký tự.");

        if (dto.Slots.GroupBy(x => new { x.ThuTrongTuan, x.MaCaHoc }).Any(g => g.Count() > 1))
        {
            throw new ApiException(400, "Có ca dạy bị trùng lặp trong dữ liệu gửi lên.");
        }

        foreach (var slot in dto.Slots)
        {
            if (slot.ThuTrongTuan < 2 || slot.ThuTrongTuan > 7)
                throw new ApiException(400, $"Thứ {slot.ThuTrongTuan} không hợp lệ.");
            if (!activeShifts.Contains(slot.MaCaHoc))
                throw new ApiException(400, $"Ca học {slot.MaCaHoc} không tồn tại hoặc không còn hoạt động.");
            if (!validMucDo.Contains(slot.MucDo))
                throw new ApiException(400, $"Mức độ ưu tiên '{slot.MucDo}' không hợp lệ.");
        }

        var teacher = await _db.NguoiDungs.FindAsync(teacherId);
        if (teacher == null) throw new ApiException(404, "Giảng viên không tồn tại");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var tx = await _db.Database.BeginTransactionAsync();

            var preference = await _db.GiaoVienNguyenVongHocKys
                .Include(x => x.ChiTietNguyenVong)
                .FirstOrDefaultAsync(x => x.MaGiaoVien == teacherId && x.MaHocKy == maHocKy);

            if (preference == null)
            {
                preference = new GiaoVienNguyenVongHocKy
                {
                    MaGiaoVien = teacherId,
                    MaHocKy = maHocKy,
                    MaDonVi = teacher.MaDonVi,
                    NgayTao = GetVietnamTime(),
                    TrangThai = "draft"
                };
                _db.GiaoVienNguyenVongHocKys.Add(preference);
            }
            
            preference.SoLopToiDaMongMuon = dto.SoLopToiDaMongMuon;
            preference.SoCaToiDaMoiTuan = dto.SoCaToiDaMoiTuan;
            preference.GhiChu = dto.GhiChu;
            preference.NgayCapNhat = GetVietnamTime();

            // Clear existing slots that are not in the incoming list (meaning they revert to neutral)
            var incomingKeys = dto.Slots.Select(x => $"{x.ThuTrongTuan}-{x.MaCaHoc}").ToHashSet();
            var toRemove = preference.ChiTietNguyenVong.Where(x => !incomingKeys.Contains($"{x.ThuTrongTuan}-{x.MaCaHoc}")).ToList();
            foreach (var slot in toRemove)
            {
                preference.ChiTietNguyenVong.Remove(slot);
            }

            // Upsert incoming slots
            foreach (var incoming in dto.Slots)
            {
                var existing = preference.ChiTietNguyenVong.FirstOrDefault(x => x.ThuTrongTuan == incoming.ThuTrongTuan && x.MaCaHoc == incoming.MaCaHoc);
                if (existing != null)
                {
                    if (existing.MucDo != incoming.MucDo)
                    {
                        existing.MucDo = incoming.MucDo;
                        existing.NgayCapNhat = GetVietnamTime();
                    }
                }
                else
                {
                    preference.ChiTietNguyenVong.Add(new GiaoVienNguyenVongCaDay
                    {
                        ThuTrongTuan = incoming.ThuTrongTuan,
                        MaCaHoc = incoming.MaCaHoc,
                        MucDo = incoming.MucDo,
                        NgayTao = GetVietnamTime()
                    });
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetTeacherFormAsync(teacherId, maHocKy);
        });
    }

    public async Task<TeachingPreferenceFormDto> SubmitAsync(int teacherId, int maHocKy, SubmitTeachingPreferenceDto dto)
    {
        // Must use same transaction to ensure we don't save draft successfully but fail submit
        await ValidateModificationAllowedAsync(teacherId, maHocKy);
        
        var draftResult = await SaveDraftAsync(teacherId, maHocKy, dto);
        
        var preference = await _db.GiaoVienNguyenVongHocKys
            .FirstOrDefaultAsync(x => x.MaGiaoVien == teacherId && x.MaHocKy == maHocKy);
            
        if (preference != null)
        {
            preference.TrangThai = "submitted";
            preference.NgayGui = GetVietnamTime();
            preference.NgayCapNhat = preference.NgayGui;
            await _db.SaveChangesAsync();
            
            draftResult.TrangThai = "submitted";
            draftResult.NgayGui = preference.NgayGui;
            draftResult.NgayCapNhat = preference.NgayCapNhat;
        }

        return draftResult;
    }

    public async Task<StaffTeachingPreferenceSummaryDto> GetSummaryAsync(int staffDonViId, int maHocKy)
    {
        var summary = new StaffTeachingPreferenceSummaryDto { MaHocKy = maHocKy };
        
        var totalTeachers = await _db.NguoiDungs
            .Where(x => x.MaDonVi == staffDonViId && x.VaiTroChinh == "giao_vien")
            .CountAsync();
            
        summary.TotalTeachers = totalTeachers;

        var preferences = await _db.GiaoVienNguyenVongHocKys
            .Where(x => x.MaDonVi == staffDonViId && x.MaHocKy == maHocKy)
            .ToListAsync();

        summary.SubmittedCount = preferences.Count(x => x.TrangThai == "submitted");
        summary.DraftCount = preferences.Count(x => x.TrangThai == "draft");
        summary.UnstartedCount = totalTeachers - preferences.Count;

        var details = await _db.GiaoVienNguyenVongCaDays
            .Include(x => x.NguyenVongHocKy)
            .Where(x => x.NguyenVongHocKy.MaDonVi == staffDonViId && x.NguyenVongHocKy.MaHocKy == maHocKy && x.NguyenVongHocKy.TrangThai == "submitted")
            .ToListAsync();

        foreach (var detail in details)
        {
            var key = $"{detail.ThuTrongTuan}-{detail.MaCaHoc}";
            if (detail.MucDo == "preferred")
            {
                if (!summary.PreferredCoverage.ContainsKey(key)) summary.PreferredCoverage[key] = 0;
                summary.PreferredCoverage[key]++;
            }
            else if (detail.MucDo == "available")
            {
                if (!summary.AvailableCoverage.ContainsKey(key)) summary.AvailableCoverage[key] = 0;
                summary.AvailableCoverage[key]++;
            }
        }

        return summary;
    }

    public async Task<List<StaffTeacherPreferenceDetailDto>> GetTeachersSummaryAsync(int staffDonViId, int maHocKy)
    {
        var teachers = await _db.NguoiDungs
            .Where(x => x.MaDonVi == staffDonViId && x.VaiTroChinh == "giao_vien")
            .Select(x => new
            {
                x.MaNguoiDung,
                x.HoTen,
                x.Email
            })
            .ToListAsync();

        var teacherIds = teachers.Select(x => x.MaNguoiDung).ToList();

        var teacherMajors = await _db.GiaoVienChuyenNganhs
            .Include(x => x.ChuyenNganh)
            .ThenInclude(x => x.NganhDaoTao)
            .Where(x => teacherIds.Contains(x.MaGiaoVien) && x.ConHoatDong)
            .ToListAsync();

        var majorDict = teacherMajors.GroupBy(x => x.MaGiaoVien)
            .ToDictionary(g => g.Key, g =>
            {
                var mainMajor = g.FirstOrDefault(x => x.LaChuyenMonChinh) ?? g.First();
                return mainMajor.ChuyenNganh.NganhDaoTao?.TenNganh ?? mainMajor.ChuyenNganh.TenChuyenNganh;
            });

        var preferences = await _db.GiaoVienNguyenVongHocKys
            .Include(x => x.ChiTietNguyenVong)
            .Where(x => x.MaDonVi == staffDonViId && x.MaHocKy == maHocKy)
            .ToListAsync();

        var prefDict = preferences.ToDictionary(x => x.MaGiaoVien);
        var result = new List<StaffTeacherPreferenceDetailDto>();

        foreach (var t in teachers)
        {
            var dto = new StaffTeacherPreferenceDetailDto
            {
                MaGiaoVien = t.MaNguoiDung,
                HoTen = t.HoTen,
                Email = t.Email,
                TenDonVi = majorDict.ContainsKey(t.MaNguoiDung) ? majorDict[t.MaNguoiDung] : "Chưa phân khoa"
            };

            if (prefDict.TryGetValue(t.MaNguoiDung, out var p))
            {
                dto.TrangThai = p.TrangThai;
                dto.NgayGui = p.NgayGui;
                dto.NgayCapNhat = p.NgayCapNhat ?? p.NgayTao;
                dto.Form = new TeachingPreferenceFormDto
                {
                    MaHocKy = maHocKy,
                    SoLopToiDaMongMuon = p.SoLopToiDaMongMuon,
                    SoCaToiDaMoiTuan = p.SoCaToiDaMoiTuan,
                    GhiChu = p.GhiChu,
                    TrangThai = p.TrangThai,
                    Slots = p.ChiTietNguyenVong.Select(x => new TeachingPreferenceSlotDto
                    {
                        ThuTrongTuan = x.ThuTrongTuan,
                        MaCaHoc = x.MaCaHoc,
                        MucDo = x.MucDo
                    }).ToList()
                };
            }
            
            result.Add(dto);
        }

        return result.OrderBy(x => x.HoTen).ToList();
    }
}
