using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.TeacherPersonnel;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.TeacherPersonnel;

public class TeacherPersonnelService : ITeacherPersonnelService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public TeacherPersonnelService(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    private bool IsAdminOrSuperAdmin(CurrentUserContext currentUser)
    {
        var r = (currentUser.Role ?? "").ToLower();
        return r == "superadmin" || r == "sieu_quan_tri" || r == "admin" || r == "quan_tri" || r == "principal" || r == "hieu_truong" || r == "bgh";
    }

    private void EnsureAccessScope(CurrentUserContext currentUser, int targetCampusId)
    {
        if (IsAdminOrSuperAdmin(currentUser))
            return;

        if (currentUser.CampusId > 0 && currentUser.CampusId != targetCampusId)
        {
            throw new ApiException(403, "Bạn không có quyền truy cập hoặc quản trị nhân sự thuộc cơ sở khác.");
        }
    }

    public async Task<PagedResultDto<TeacherPersonnelListDto>> GetTeachersAsync(
        CurrentUserContext currentUser,
        TeacherPersonnelQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var targetCampusId = query.MaDonVi ?? (currentUser.CampusId > 0 ? currentUser.CampusId : (int?)null);
        if (targetCampusId.HasValue && !IsAdminOrSuperAdmin(currentUser))
        {
            EnsureAccessScope(currentUser, targetCampusId.Value);
        }

        var teachersQuery = _context.NguoiDungs
            .Include(n => n.DonVi)
            .Where(n => n.VaiTroChinh == "giao_vien" || n.VaiTroChinh == "Teacher")
            .AsQueryable();

        if (targetCampusId.HasValue && targetCampusId.Value > 0)
        {
            teachersQuery = teachersQuery.Where(n => n.MaDonVi == targetCampusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLower();
            teachersQuery = teachersQuery.Where(n =>
                n.HoTen.ToLower().Contains(kw) ||
                n.Email.ToLower().Contains(kw) ||
                (n.SoDienThoai != null && n.SoDienThoai.Contains(kw)));
        }

        if (!string.IsNullOrWhiteSpace(query.TrangThai))
        {
            teachersQuery = teachersQuery.Where(n => n.TrangThai == query.TrangThai);
        }

        if (query.MaChuyenNganh.HasValue)
        {
            var teacherIdsWithMajor = await _context.GiaoVienChuyenNganhs
                .Where(g => g.MaChuyenNganh == query.MaChuyenNganh.Value && g.ConHoatDong)
                .Select(g => g.MaGiaoVien)
                .Distinct()
                .ToListAsync(cancellationToken);

            teachersQuery = teachersQuery.Where(n => teacherIdsWithMajor.Contains(n.MaNguoiDung));
        }

        if (query.MaMonHoc.HasValue)
        {
            var teacherIdsWithSub = await _context.GiaoVienMonHocs
                .Where(g => g.MaMonHoc == query.MaMonHoc.Value && g.ConHoatDong)
                .Select(g => g.MaGiaoVien)
                .Distinct()
                .ToListAsync(cancellationToken);

            teachersQuery = teachersQuery.Where(n => teacherIdsWithSub.Contains(n.MaNguoiDung));
        }

        var totalItems = await teachersQuery.CountAsync(cancellationToken);

        var pagedTeachers = await teachersQuery
            .OrderByDescending(n => n.NgayTao)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var teacherIds = pagedTeachers.Select(t => t.MaNguoiDung).ToList();

        // 1. Majors
        var teacherMajors = await _context.GiaoVienChuyenNganhs
            .Include(g => g.ChuyenNganh)
            .Where(g => teacherIds.Contains(g.MaGiaoVien) && g.ConHoatDong)
            .ToListAsync(cancellationToken);

        // 2. Subjects count (union of GiaoVienMonHoc and active KhoaHoc)
        var subjectTeacherItems = await _context.GiaoVienMonHocs
            .Where(g => teacherIds.Contains(g.MaGiaoVien) && g.ConHoatDong)
            .Select(g => new { g.MaGiaoVien, g.MaMonHoc })
            .ToListAsync(cancellationToken);

        var taughtCourseTeacherItems = await _context.KhoaHocs
            .Where(k => teacherIds.Contains(k.MaGiaoVien) && k.MonHoc != null)
            .Select(k => new { MaGiaoVien = k.MaGiaoVien, MaMonHoc = k.MonHoc!.MaMonHoc })
            .ToListAsync(cancellationToken);

        var subjectCounts = subjectTeacherItems
            .Concat(taughtCourseTeacherItems)
            .GroupBy(x => x.MaGiaoVien)
            .ToDictionary(g => g.Key, g => g.Select(x => x.MaMonHoc).Distinct().Count());

        // 3. Current active classes
        var activeClassCounts = await _context.KhoaHocs
            .Where(k => teacherIds.Contains(k.MaGiaoVien))
            .GroupBy(k => k.MaGiaoVien)
            .Select(g => new { TeacherId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Count, cancellationToken);

        // 4. Ratings
        var evalRatings = await _context.DanhGiaGiaoViens
            .Where(d => teacherIds.Contains(d.MaGiaoVien))
            .GroupBy(d => d.MaGiaoVien)
            .Select(g => new { TeacherId = g.Key, Avg = g.Average(d => (double)d.DiemSo) })
            .ToDictionaryAsync(x => x.TeacherId, x => x.Avg, cancellationToken);

        var items = pagedTeachers.Select(t =>
        {
            var mainMajor = teacherMajors
                .Where(m => m.MaGiaoVien == t.MaNguoiDung)
                .OrderByDescending(m => m.LaChuyenMonChinh)
                .Select(m => m.ChuyenNganh != null ? m.ChuyenNganh.TenChuyenNganh : "")
                .FirstOrDefault() ?? "Công nghệ thông tin";

            var classCount = activeClassCounts.GetValueOrDefault(t.MaNguoiDung);
            var subCount = subjectCounts.GetValueOrDefault(t.MaNguoiDung);
            var avgRating = evalRatings.TryGetValue(t.MaNguoiDung, out var r) ? r : 5.0;

            return new TeacherPersonnelListDto
            {
                MaNguoiDung = t.MaNguoiDung,
                MaGiangVien = $"GV{t.MaNguoiDung:D4}",
                HoTen = t.HoTen,
                Email = t.Email,
                SoDienThoai = t.SoDienThoai,
                MaDonVi = t.MaDonVi,
                TenDonVi = t.DonVi != null ? t.DonVi.TenDonVi : "Cơ sở chính",
                TrangThai = t.TrangThai,
                ChuyenNganhChinh = mainMajor,
                SoMonDuocPhepDay = subCount,
                SoLopHocKyHienTai = classCount,
                SoCaMoiTuan = classCount * 2,
                DiemDanhGiaTrungBinh = Math.Round((decimal)avgRating, 1),
                NgayTao = t.NgayTao
            };
        }).ToList();

        return new PagedResultDto<TeacherPersonnelListDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<TeacherPersonnelDetailDto> GetTeacherDetailAsync(
        CurrentUserContext currentUser,
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs
            .Include(n => n.DonVi)
            .FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);

        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy thông tin giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        // Majors
        var majors = await _context.GiaoVienChuyenNganhs
            .Include(g => g.ChuyenNganh)
            .Where(g => g.MaGiaoVien == teacherId)
            .Select(g => new TeacherMajorDto
            {
                MaChuyenNganh = g.MaChuyenNganh,
                TenChuyenNganh = g.ChuyenNganh != null ? g.ChuyenNganh.TenChuyenNganh : "",
                MaCode = g.ChuyenNganh != null ? $"CN{g.ChuyenNganh.MaChuyenNganh:D3}" : "",
                LaChuyenMonChinh = g.LaChuyenMonChinh,
                MucDoPhuHop = g.MucDoPhuHop,
                SoNamKinhNghiem = g.SoNamKinhNghiem
            })
            .ToListAsync(cancellationToken);

        // Subjects
        var subjects = await _context.GiaoVienMonHocs
            .Include(g => g.MonHoc)
            .Where(g => g.MaGiaoVien == teacherId)
            .Select(g => new TeacherSubjectCapabilityDto
            {
                MaMonHoc = g.MaMonHoc,
                MaCodeMonHoc = g.MonHoc != null ? g.MonHoc.MaCodeMonHoc : "",
                TenMonHoc = g.MonHoc != null ? g.MonHoc.TenMonHoc : "",
                SoTinChi = g.MonHoc != null ? g.MonHoc.SoTinChi : 3,
                MucDoPhuHop = g.MucDoPhuHop,
                PhuHopChuyenMon = g.PhuHopChuyenMon,
                DiemDanhGia = g.DiemDanhGia,
                SoNamKinhNghiem = g.SoNamKinhNghiem,
                SoLanDaDay = g.SoLanDaDay,
                LaMonChinh = g.LaMonChinh,
                ConHoatDong = g.ConHoatDong
            })
            .ToListAsync(cancellationToken);

        // Also query distinct MonHoc from courses currently taught by this teacher
        var taughtCourses = await _context.KhoaHocs
            .Include(k => k.MonHoc)
            .Where(k => k.MaGiaoVien == teacherId && k.MonHoc != null)
            .ToListAsync(cancellationToken);

        var existingSubjectIds = subjects.Select(s => s.MaMonHoc).ToHashSet();
        foreach (var c in taughtCourses)
        {
            if (c.MonHoc != null && !existingSubjectIds.Contains(c.MonHoc.MaMonHoc))
            {
                existingSubjectIds.Add(c.MonHoc.MaMonHoc);
                subjects.Add(new TeacherSubjectCapabilityDto
                {
                    MaMonHoc = c.MonHoc.MaMonHoc,
                    MaCodeMonHoc = c.MonHoc.MaCodeMonHoc ?? "",
                    TenMonHoc = c.MonHoc.TenMonHoc ?? c.TieuDe,
                    SoTinChi = c.MonHoc.SoTinChi,
                    MucDoPhuHop = 90,
                    SoNamKinhNghiem = 2,
                    SoLanDaDay = 1,
                    LaMonChinh = true,
                    ConHoatDong = true
                });
            }
        }

        // Workload & Evaluations
        var workload = await GetTeacherWorkloadAsync(currentUser, teacherId, null, cancellationToken);
        var evaluations = await GetTeacherEvaluationsAsync(currentUser, teacherId, cancellationToken);

        // Preferences
        var pref = await _context.GiaoVienNguyenVongHocKys
            .Include(p => p.HocKy)
            .Include(p => p.ChiTietNguyenVong)
                .ThenInclude(c => c.CaHoc)
            .Where(p => p.MaGiaoVien == teacherId)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        TeacherPreferenceSummaryDto? prefDto = null;
        if (pref != null)
        {
            prefDto = new TeacherPreferenceSummaryDto
            {
                MaHocKy = pref.MaHocKy,
                TenHocKy = pref.HocKy != null ? pref.HocKy.TenHocKy : "Học kỳ hiện tại",
                SoLopToiDaMongMuon = pref.SoLopToiDaMongMuon,
                SoCaToiDaMoiTuan = pref.SoCaToiDaMoiTuan,
                GhiChu = pref.GhiChu,
                TrangThai = pref.TrangThai,
                CaUuTien = pref.ChiTietNguyenVong
                    .Where(c => c.CaHoc != null)
                    .Select(c => $"Thứ {c.ThuTrongTuan}: {c.CaHoc.TenCa} ({c.MucDo})")
                    .ToList()
            };
        }

        return new TeacherPersonnelDetailDto
        {
            MaNguoiDung = teacher.MaNguoiDung,
            MaGiangVien = $"GV{teacher.MaNguoiDung:D4}",
            HoTen = teacher.HoTen,
            Email = teacher.Email,
            SoDienThoai = teacher.SoDienThoai,
            MaDonVi = teacher.MaDonVi,
            TenDonVi = teacher.DonVi != null ? teacher.DonVi.TenDonVi : "Cơ sở",
            TrangThai = teacher.TrangThai,
            NgayTao = teacher.NgayTao,
            LanDangNhapCuoi = teacher.LanDangNhapCuoi,
            ChuyenNganhList = majors,
            MonHocList = subjects,
            TuanNayWorkload = workload,
            EvaluationSummary = evaluations,
            NguyenVongGanNhat = prefDto
        };
    }

    public async Task<TeacherWorkloadSummaryDto> GetTeacherWorkloadAsync(
        CurrentUserContext currentUser,
        int teacherId,
        int? semesterId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);
        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        var coursesQuery = _context.KhoaHocs
            .Include(k => k.MonHoc)
            .Include(k => k.Lop)
            .Include(k => k.HocKy)
            .Where(k => k.MaGiaoVien == teacherId);

        if (semesterId.HasValue)
        {
            coursesQuery = coursesQuery.Where(k => k.MaHocKy == semesterId.Value);
        }

        var courses = await coursesQuery.ToListAsync(cancellationToken);
        var courseIds = courses.Select(c => c.MaKhoaHoc).ToList();

        var sessions = await _context.BuoiHocs
            .Where(b => courseIds.Contains(b.MaKhoaHoc))
            .ToListAsync(cancellationToken);

        var sessionsByCourse = sessions.GroupBy(s => s.MaKhoaHoc).ToDictionary(g => g.Key, g => g.ToList());

        var classItems = courses.Select(c =>
        {
            var cSessions = sessionsByCourse.GetValueOrDefault(c.MaKhoaHoc, []);
            var completedCount = cSessions.Count(s => s.TrangThaiBuoi == "da_dien_ra" || s.NgayHoc < DateOnly.FromDateTime(DateTime.UtcNow));
            var studentCount = _context.NguoiDungs.Count(n => n.MaLop == c.MaLop && n.VaiTroChinh == "hoc_sinh");

            return new TeacherCourseWorkloadItemDto
            {
                MaKhoaHoc = c.MaKhoaHoc,
                TieuDe = c.TieuDe,
                TenMonHoc = c.MonHoc != null ? c.MonHoc.TenMonHoc : c.TieuDe,
                MaCodeMonHoc = c.MonHoc != null ? c.MonHoc.MaCodeMonHoc : "",
                TenLopHanhChinh = c.Lop != null ? c.Lop.TenLop : "",
                SoLuongSinhVien = studentCount,
                SoCaMoiTuan = 2,
                TongSoBuoi = cSessions.Count > 0 ? cSessions.Count : 15,
                SoBuoiHoanThanh = completedCount
            };
        }).ToList();

        var sem = courses.FirstOrDefault()?.HocKy;

        return new TeacherWorkloadSummaryDto
        {
            MaHocKy = sem?.MaHocKy ?? (semesterId ?? 1),
            TenHocKy = sem?.TenHocKy ?? "Học kỳ 1 năm 2026",
            TongSoLopHocPhan = courses.Count,
            TongSoCaDayTrongTuan = courses.Count * 2,
            TongSoGioGiangDayQuyDoi = courses.Count * 45,
            TongSoBuoiDaDienRa = sessions.Count(s => s.TrangThaiBuoi == "da_dien_ra"),
            TongSoBuoiChuaDienRa = sessions.Count(s => s.TrangThaiBuoi == "chua_dien_ra"),
            TongSoBuoiBiHuy = sessions.Count(s => s.TrangThaiBuoi == "da_huy"),
            TongSoBuoiDayThay = sessions.Count(s => s.MaGiaoVien != teacherId),
            DanhSachLop = classItems
        };
    }

    public async Task<TeacherSessionLogsSummaryDto> GetTeacherSessionLogsAsync(
        CurrentUserContext currentUser,
        int teacherId,
        int? semesterId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);
        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        var sessionsQuery = _context.BuoiHocs
            .Include(b => b.KhoaHoc)
                .ThenInclude(k => k!.MonHoc)
            .Include(b => b.KhoaHoc)
                .ThenInclude(k => k!.Lop)
            .Include(b => b.CaHoc)
            .Include(b => b.Phong)
            .Include(b => b.GiaoVien)
            .Where(b => b.MaGiaoVien == teacherId || (b.KhoaHoc != null && b.KhoaHoc.MaGiaoVien == teacherId));

        if (semesterId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(b => b.KhoaHoc != null && b.KhoaHoc.MaHocKy == semesterId.Value);
        }

        var sessions = await sessionsQuery
            .OrderByDescending(b => b.NgayHoc)
            .ThenByDescending(b => b.MaBuoiHoc)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.MaBuoiHoc).ToList();

        var attendances = await _context.DiemDanhs
            .Where(d => sessionIds.Contains(d.MaBuoiHoc))
            .GroupBy(d => d.MaBuoiHoc)
            .Select(g => new
            {
                SessionId = g.Key,
                Total = g.Count(),
                Present = g.Count(x => x.TrangThai == "co_mat"),
                Absent = g.Count(x => x.TrangThai == "vang" || x.TrangThai == "co_phep"),
                Late = g.Count(x => x.TrangThai == "di_muon")
            })
            .ToDictionaryAsync(x => x.SessionId, x => x, cancellationToken);

        var items = sessions.Select(b =>
        {
            var isSubstitute = b.KhoaHoc != null && b.KhoaHoc.MaGiaoVien != teacherId;
            var att = attendances.GetValueOrDefault(b.MaBuoiHoc);
            var isSubmitted = b.TrangThaiDiemDanh == "da_gui";
            var isOnTime = isSubmitted && b.DiemDanhHanChinhSuaLuc.HasValue;

            return new TeacherSessionLogDto
            {
                MaBuoiHoc = b.MaBuoiHoc,
                MaKhoaHoc = b.MaKhoaHoc,
                TenMonHoc = b.KhoaHoc?.MonHoc?.TenMonHoc ?? b.KhoaHoc?.TieuDe ?? "Chưa rõ môn",
                MaCodeMonHoc = b.KhoaHoc?.MonHoc?.MaCodeMonHoc ?? "",
                TenLopHanhChinh = b.KhoaHoc?.Lop?.TenLop ?? "",
                NgayHoc = b.NgayHoc,
                TenCaHoc = b.CaHoc != null ? b.CaHoc.TenCa : $"Ca {b.MaCaHoc}",
                GioBatDau = b.CaHoc != null ? b.CaHoc.GioBatDau.ToString("HH:mm") : "07:30",
                GioKetThuc = b.CaHoc != null ? b.CaHoc.GioKetThuc.ToString("HH:mm") : "09:30",
                TenPhong = b.Phong?.TenPhong ?? "Phòng học",
                TrangThaiBuoi = b.TrangThaiBuoi,
                LaDayThay = isSubstitute,
                TenGiangVienChinh = isSubstitute ? (b.KhoaHoc?.GiaoVien?.HoTen ?? "Giảng viên bộ môn") : teacher.HoTen,
                TrangThaiDiemDanh = b.TrangThaiDiemDanh,
                ThoiDiemGuiDiemDanh = isSubmitted ? (b.DiemDanhDaGuiLuc ?? b.DiemDanhHanChinhSuaLuc ?? b.NgayTao) : null,
                HanDiemDanh = b.DiemDanhHanChinhSuaLuc,
                DungHanDiemDanh = isOnTime,
                SoLuongSinhVien = att?.Total ?? 29,
                SoCoMat = att?.Present ?? 29,
                SoVang = att?.Absent ?? 0,
                SoDiMuon = att?.Late ?? 0
            };
        }).ToList();

        var totalSessions = items.Count;
        var completedSessions = items.Count(i => i.TrangThaiBuoi == "da_dien_ra");
        var onTimeAtt = items.Count(i => i.TrangThaiDiemDanh == "da_gui" && i.DungHanDiemDanh);
        var lateAtt = items.Count(i => i.TrangThaiDiemDanh == "da_gui" && !i.DungHanDiemDanh);
        var unsubmittedAtt = items.Count(i => i.TrangThaiDiemDanh == "chua_gui");

        return new TeacherSessionLogsSummaryDto
        {
            TongSoCa = totalSessions,
            SoCaDaDienRa = completedSessions,
            SoCaDayThay = items.Count(i => i.LaDayThay),
            SoCaBiHuy = items.Count(i => i.TrangThaiBuoi == "da_huy"),
            SoCaDiemDanhDungHan = onTimeAtt,
            SoCaDiemDanhTreHan = lateAtt,
            SoCaChuaDiemDanh = unsubmittedAtt,
            TyLeDiemDanhDungHan = (completedSessions > 0) ? Math.Round((decimal)onTimeAtt / completedSessions * 100, 1) : 100m,
            Items = items
        };
    }

    public async Task<TeacherEvaluationSummaryDto> GetTeacherEvaluationsAsync(
        CurrentUserContext currentUser,
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);
        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        var evaluations = await _context.DanhGiaGiaoViens
            .Include(d => d.HocKy)
            .Where(d => d.MaGiaoVien == teacherId)
            .OrderByDescending(d => d.NgayTao)
            .ToListAsync(cancellationToken);

        if (evaluations.Count == 0)
        {
            return new TeacherEvaluationSummaryDto
            {
                DiemTrungBinhChung = 5.0m,
                TongSoLuotDanhGia = 0,
                TongSoHocSinhDanhGia = 0,
                TheoHocKy = [],
                NhanXetGanNhat = []
            };
        }

        var avgScore = Math.Round((decimal)evaluations.Average(d => (double)d.DiemSo), 2);

        var termGroups = evaluations
            .GroupBy(d => d.MaHocKy)
            .Select(g => new TeacherEvaluationTermDto
            {
                MaHocKy = g.Key,
                TenHocKy = g.FirstOrDefault()?.HocKy?.TenHocKy ?? $"Học kỳ {g.Key}",
                DiemTrungBinh = Math.Round((decimal)g.Average(d => (double)d.DiemSo), 2),
                SoLuotDanhGia = g.Count(),
                SoKhoaHoc = 1
            })
            .ToList();

        var recentFeedbacks = evaluations
            .Where(d => !string.IsNullOrWhiteSpace(d.NhanXetTuDo))
            .Take(10)
            .Select(d => new TeacherEvaluationFeedbackDto
            {
                MaDanhGia = d.MaDanhGia,
                TenKhoaHoc = d.HocKy?.TenHocKy ?? "Lớp học phần",
                DiemSo = d.DiemSo,
                NhanXet = d.NhanXetTuDo ?? "Giảng viên giảng dạy nhiệt tình, bài giảng dễ hiểu.",
                NgayDanhGia = d.NgayTao
            })
            .ToList();

        return new TeacherEvaluationSummaryDto
        {
            DiemTrungBinhChung = avgScore,
            TongSoLuotDanhGia = evaluations.Count,
            TongSoHocSinhDanhGia = evaluations.Count,
            TheoHocKy = termGroups,
            NhanXetGanNhat = recentFeedbacks
        };
    }

    public async Task<TeacherPersonnelDetailDto> CreateTeacherAsync(
        CurrentUserContext currentUser,
        CreateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        EnsureAccessScope(currentUser, request.MaDonVi);

        var existingUser = await _context.NguoiDungs
            .FirstOrDefaultAsync(n => n.Email.ToLower() == request.Email.ToLower().Trim(), cancellationToken);

        if (existingUser != null)
        {
            throw new ApiException(400, "Email này đã được sử dụng trong hệ thống.");
        }

        var newTeacher = new NguoiDung
        {
            HoTen = request.HoTen.Trim(),
            Email = request.Email.Trim().ToLower(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            MatKhauHash = PasswordHelper.HashPassword(request.MatKhau),
            MaDonVi = request.MaDonVi,
            VaiTroChinh = "giao_vien",
            TrangThai = "hoat_dong",
            NgayTao = DateTime.UtcNow
        };

        _context.NguoiDungs.Add(newTeacher);
        await _context.SaveChangesAsync(cancellationToken);

        // Add main major if specified
        if (request.MaChuyenNganhChinh.HasValue)
        {
            _context.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh
            {
                MaGiaoVien = newTeacher.MaNguoiDung,
                MaChuyenNganh = request.MaChuyenNganhChinh.Value,
                LaChuyenMonChinh = true,
                MucDoPhuHop = 90,
                SoNamKinhNghiem = 3,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
        }

        // Add allowed subjects
        if (request.DanhSachMonDuocPhepDay.Count > 0)
        {
            foreach (var subId in request.DanhSachMonDuocPhepDay)
            {
                var score = await CalculateSubjectSuitabilityAsync(request.MaChuyenNganhChinh, subId, false, cancellationToken);
                _context.GiaoVienMonHocs.Add(new GiaoVienMonHoc
                {
                    MaGiaoVien = newTeacher.MaNguoiDung,
                    MaMonHoc = subId,
                    MucDoPhuHop = score,
                    SoNamKinhNghiem = score >= 80 ? 2 : 1,
                    SoLanDaDay = 0,
                    LaMonChinh = false,
                    ConHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.AddAsync(
            campusId: newTeacher.MaDonVi,
            entityName: "GiangVien",
            entityId: newTeacher.MaNguoiDung,
            action: "CREATE_TEACHER",
            actorUserId: currentUser.UserId,
            oldValue: null,
            newValue: new { newTeacher.HoTen, newTeacher.Email, newTeacher.MaDonVi },
            cancellationToken: cancellationToken);

        return await GetTeacherDetailAsync(currentUser, newTeacher.MaNguoiDung, cancellationToken);
    }

    public async Task<TeacherPersonnelDetailDto> UpdateTeacherAsync(
        CurrentUserContext currentUser,
        int teacherId,
        UpdateTeacherPersonnelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs
            .FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);

        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        var oldValues = new
        {
            teacher.HoTen,
            teacher.SoDienThoai,
            teacher.TrangThai
        };

        teacher.HoTen = request.HoTen.Trim();
        teacher.SoDienThoai = request.SoDienThoai?.Trim();
        teacher.TrangThai = request.TrangThai;

        // Update Major
        int? activeMajorId = request.MaChuyenNganhChinh;
        if (request.MaChuyenNganhChinh.HasValue)
        {
            var currentMajors = await _context.GiaoVienChuyenNganhs
                .Where(g => g.MaGiaoVien == teacherId)
                .ToListAsync(cancellationToken);

            foreach (var m in currentMajors)
            {
                m.LaChuyenMonChinh = (m.MaChuyenNganh == request.MaChuyenNganhChinh.Value);
            }

            if (!currentMajors.Any(m => m.MaChuyenNganh == request.MaChuyenNganhChinh.Value))
            {
                _context.GiaoVienChuyenNganhs.Add(new GiaoVienChuyenNganh
                {
                    MaGiaoVien = teacherId,
                    MaChuyenNganh = request.MaChuyenNganhChinh.Value,
                    LaChuyenMonChinh = true,
                    MucDoPhuHop = 90,
                    ConHoatDong = true,
                    NgayTao = DateTime.UtcNow
                });
            }
        }
        else
        {
            var mainMajor = await _context.GiaoVienChuyenNganhs
                .FirstOrDefaultAsync(g => g.MaGiaoVien == teacherId && g.LaChuyenMonChinh, cancellationToken);
            activeMajorId = mainMajor?.MaChuyenNganh;
        }

        // Update Subjects (supports removing all subjects or updating assigned list)
        if (request.DanhSachMonHoc != null)
        {
            var existingSubs = await _context.GiaoVienMonHocs
                .Where(g => g.MaGiaoVien == teacherId)
                .ToListAsync(cancellationToken);

            var requestedIds = request.DanhSachMonHoc.Select(s => s.MaMonHoc).ToHashSet();
            foreach (var oldSub in existingSubs.Where(s => !requestedIds.Contains(s.MaMonHoc)))
            {
                _context.GiaoVienMonHocs.Remove(oldSub);
            }

            foreach (var subItem in request.DanhSachMonHoc)
            {
                var existing = existingSubs.FirstOrDefault(s => s.MaMonHoc == subItem.MaMonHoc);
                var autoScore = await CalculateSubjectSuitabilityAsync(activeMajorId, subItem.MaMonHoc, subItem.LaMonChinh, cancellationToken);
                var finalScore = (subItem.MucDoPhuHop > 0 && subItem.MucDoPhuHop != 90) ? subItem.MucDoPhuHop : autoScore;

                if (existing != null)
                {
                    existing.MucDoPhuHop = finalScore;
                    existing.SoNamKinhNghiem = subItem.SoNamKinhNghiem;
                    existing.LaMonChinh = subItem.LaMonChinh;
                    existing.ConHoatDong = subItem.ConHoatDong;
                    existing.NgayCapNhat = DateTime.UtcNow;
                }
                else
                {
                    _context.GiaoVienMonHocs.Add(new GiaoVienMonHoc
                    {
                        MaGiaoVien = teacherId,
                        MaMonHoc = subItem.MaMonHoc,
                        MucDoPhuHop = finalScore,
                        SoNamKinhNghiem = subItem.SoNamKinhNghiem,
                        LaMonChinh = subItem.LaMonChinh,
                        ConHoatDong = subItem.ConHoatDong,
                        NgayTao = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.AddAsync(
            campusId: teacher.MaDonVi,
            entityName: "GiangVien",
            entityId: teacherId,
            action: "UPDATE_TEACHER",
            actorUserId: currentUser.UserId,
            oldValue: oldValues,
            newValue: new { teacher.HoTen, teacher.SoDienThoai, teacher.TrangThai, request.LyDo },
            cancellationToken: cancellationToken);

        return await GetTeacherDetailAsync(currentUser, teacherId, cancellationToken);
    }

    public async Task<bool> ToggleLockTeacherAsync(
        CurrentUserContext currentUser,
        int teacherId,
        ToggleTeacherLockRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.NguoiDungs
            .FirstOrDefaultAsync(n => n.MaNguoiDung == teacherId, cancellationToken);

        if (teacher == null)
            throw new ApiException(404, "Không tìm thấy giảng viên.");

        EnsureAccessScope(currentUser, teacher.MaDonVi);

        var isLocking = (teacher.TrangThai == "hoat_dong");
        var oldStatus = teacher.TrangThai;
        teacher.TrangThai = isLocking ? "bi_khoa" : "hoat_dong";

        await _context.SaveChangesAsync(cancellationToken);

        // Audit Log
        await _auditLogService.AddAsync(
            campusId: teacher.MaDonVi,
            entityName: "GiangVien",
            entityId: teacherId,
            action: isLocking ? "LOCK_TEACHER" : "UNLOCK_TEACHER",
            actorUserId: currentUser.UserId,
            oldValue: new { TrangThai = oldStatus },
            newValue: new { teacher.TrangThai, request.LyDo },
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<List<OrganizationHierarchyNodeDto>> GetHierarchyTreeAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var targetCampusId = currentUser.CampusId;

        var orgsQuery = _context.DonVis.AsQueryable();
        if (targetCampusId > 0 && !IsAdminOrSuperAdmin(currentUser))
        {
            orgsQuery = orgsQuery.Where(d => d.MaDonVi == targetCampusId || d.MaDonViCha == targetCampusId);
        }

        var orgs = await orgsQuery.ToListAsync(cancellationToken);
        var orgIds = orgs.Select(o => o.MaDonVi).ToList();

        var users = await _context.NguoiDungs
            .Where(u => orgIds.Contains(u.MaDonVi))
            .ToListAsync(cancellationToken);

        var roles = await _context.VaiTros.ToListAsync(cancellationToken);

        var result = new List<OrganizationHierarchyNodeDto>();

        foreach (var org in orgs.Where(o => o.MaDonViCha == null || !orgIds.Contains(o.MaDonViCha.Value)))
        {
            var orgNode = new OrganizationHierarchyNodeDto
            {
                Id = $"org-{org.MaDonVi}",
                Label = org.TenDonVi,
                Type = "organization",
                EntityId = org.MaDonVi,
                Code = $"CS{org.MaDonVi:D2}",
                IsManageable = true,
                TotalMembers = users.Count(u => u.MaDonVi == org.MaDonVi)
            };

            // Roles under this organization
            var orgUsers = users.Where(u => u.MaDonVi == org.MaDonVi).ToList();

            var roleGroups = orgUsers.GroupBy(u => u.VaiTroChinh);
            foreach (var rGroup in roleGroups)
            {
                var roleInfo = roles.FirstOrDefault(r => r.MaCodeVaiTro == rGroup.Key);
                var roleName = roleInfo?.TenVaiTro ?? (rGroup.Key == "giao_vien" ? "Giảng viên" : (rGroup.Key == "hoc_sinh" ? "Sinh viên" : rGroup.Key));
                var isTeacherRole = (rGroup.Key == "giao_vien" || rGroup.Key == "Teacher");

                var roleNode = new OrganizationHierarchyNodeDto
                {
                    Id = $"org-{org.MaDonVi}-role-{rGroup.Key}",
                    Label = roleName,
                    Type = "role",
                    Code = rGroup.Key,
                    IsManageable = isTeacherRole,
                    TotalMembers = rGroup.Count()
                };

                foreach (var u in rGroup.Take(50))
                {
                    roleNode.Children.Add(new OrganizationHierarchyNodeDto
                    {
                        Id = $"user-{u.MaNguoiDung}",
                        Label = $"{u.HoTen} ({u.Email})",
                        Type = "user",
                        EntityId = u.MaNguoiDung,
                        Status = u.TrangThai,
                        IsManageable = isTeacherRole
                    });
                }

                orgNode.Children.Add(roleNode);
            }

            result.Add(orgNode);
        }

        return result;
    }

    private async Task<int> CalculateSubjectSuitabilityAsync(int? teacherMajorId, int subjectId, bool isMainSubject, CancellationToken cancellationToken = default)
    {
        if (!teacherMajorId.HasValue) return 70;

        var subject = await _context.DanhMucMonHocs.FirstOrDefaultAsync(s => s.MaMonHoc == subjectId, cancellationToken);
        if (subject == null) return 50;

        var teacherMajor = await _context.ChuyenNganhs.FirstOrDefaultAsync(c => c.MaChuyenNganh == teacherMajorId.Value, cancellationToken);
        if (teacherMajor == null) return 50;

        // 1. Cùng chuyên ngành đào tạo trực tiếp
        if (subject.MaChuyenNganh.HasValue && subject.MaChuyenNganh.Value == teacherMajorId.Value)
        {
            return isMainSubject ? 100 : 95;
        }

        // 2. Cùng ngành đào tạo lớn (ví dụ cùng khối ngành CNTT hoặc cùng khối Marketing)
        if (subject.MaNganh.HasValue && subject.MaNganh.Value == teacherMajor.MaNganh)
        {
            return 80;
        }

        // 3. Môn cơ sở đại cương chung
        if (!subject.MaNganh.HasValue && !subject.MaChuyenNganh.HasValue)
        {
            return 70;
        }

        // 4. Trái ngành đào tạo hoàn toàn (ví dụ GV Marketing dạy REST API / Phần mềm CNTT)
        return 35;
    }
}
