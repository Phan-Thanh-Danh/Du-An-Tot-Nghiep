using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Exam;
using Backend.DTOs.QuizAttempts;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.QuizGrading;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Exam;

public class ExamService : IExamService
{
    private readonly ApplicationDbContext _db;
    private readonly IQuizGradingService _gradingService;
    private readonly Backend.Services.Grading.IGradeAggregationService _gradeAggregationService;
    private readonly ILogger<ExamService> _logger;

    public ExamService(
        ApplicationDbContext db, 
        IQuizGradingService gradingService,
        Backend.Services.Grading.IGradeAggregationService gradeAggregationService,
        ILogger<ExamService> logger)
    {
        _db = db;
        _gradingService = gradingService;
        _gradeAggregationService = gradeAggregationService;
        _logger = logger;
    }

    // ===== KyThi =====

    public async Task<PagedResultDto<KyThiDto>> GetKyThisAsync(ExamQueryParameters parameters, CancellationToken ct)
    {
        var query = _db.KyThis
            .Include(k => k.HocKy)
            .Include(k => k.LichThiTongs)
            .AsQueryable();

        if (parameters.MaHocKy.HasValue)
            query = query.Where(k => k.MaHocKy == parameters.MaHocKy.Value);
        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
            query = query.Where(k => k.TrangThai == parameters.TrangThai);
        if (!string.IsNullOrWhiteSpace(parameters.Search))
            query = query.Where(k => k.TenKyThi.Contains(parameters.Search));

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(k => k.NgayTao)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(k => new KyThiDto
            {
                MaKyThi = k.MaKyThi,
                TenKyThi = k.TenKyThi,
                MaHocKy = k.MaHocKy,
                TenHocKy = k.HocKy != null ? k.HocKy.TenHocKy : null,
                LoaiKyThi = k.LoaiKyThi,
                TrangThai = k.TrangThai,
                NgayTao = k.NgayTao,
                NgayCapNhat = k.NgayCapNhat,
                SoLichThiTong = k.LichThiTongs.Count
            })
            .ToListAsync(ct);

        return new PagedResultDto<KyThiDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<KyThiDto> GetKyThiByIdAsync(int id, CancellationToken ct)
    {
        var k = await _db.KyThis
            .Include(x => x.HocKy)
            .Include(x => x.LichThiTongs)
            .FirstOrDefaultAsync(x => x.MaKyThi == id, ct)
            ?? throw new ApiException(404, "Không tìm thấy kỳ thi.");

        return new KyThiDto
        {
            MaKyThi = k.MaKyThi,
            TenKyThi = k.TenKyThi,
            MaHocKy = k.MaHocKy,
            TenHocKy = k.HocKy?.TenHocKy,
            LoaiKyThi = k.LoaiKyThi,
            TrangThai = k.TrangThai,
            NgayTao = k.NgayTao,
            NgayCapNhat = k.NgayCapNhat,
            SoLichThiTong = k.LichThiTongs.Count
        };
    }

    public async Task<KyThiDto> CreateKyThiAsync(CreateKyThiRequest request, CancellationToken ct)
    {
        var hocKy = await _db.HocKys.FindAsync(new object[] { request.MaHocKy }, ct)
            ?? throw new ApiException(400, "Học kỳ không tồn tại.");

        var entity = new KyThi
        {
            TenKyThi = request.TenKyThi,
            MaHocKy = request.MaHocKy,
            LoaiKyThi = request.LoaiKyThi,
            TrangThai = "nhap"
        };

        _db.KyThis.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetKyThiByIdAsync(entity.MaKyThi, ct);
    }

    public async Task<KyThiDto> UpdateKyThiAsync(int id, UpdateKyThiRequest request, CancellationToken ct)
    {
        var entity = await _db.KyThis.FindAsync(new object[] { id }, ct)
            ?? throw new ApiException(404, "Không tìm thấy kỳ thi.");

        if (!string.IsNullOrWhiteSpace(request.TenKyThi))
            entity.TenKyThi = request.TenKyThi;
        if (!string.IsNullOrWhiteSpace(request.TrangThai))
            entity.TrangThai = request.TrangThai;
        if (!string.IsNullOrWhiteSpace(request.LoaiKyThi))
            entity.LoaiKyThi = request.LoaiKyThi;

        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetKyThiByIdAsync(id, ct);
    }

    // ===== LichThiTong =====

    public async Task<PagedResultDto<LichThiTongDto>> GetLichThiTongsAsync(ExamQueryParameters parameters, CancellationToken ct)
    {
        var query = _db.LichThiTongs
            .Include(l => l.KyThi)
            .Include(l => l.MonHoc)
            .Include(l => l.DeKiemTra)
            .Include(l => l.CaThis)
            .AsQueryable();

        if (parameters.MaHocKy.HasValue)
            query = query.Where(l => l.KyThi != null && l.KyThi.MaHocKy == parameters.MaHocKy.Value);
        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
            query = query.Where(l => l.TrangThai == parameters.TrangThai);

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.NgayTao)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(l => new LichThiTongDto
            {
                MaLichThiTong = l.MaLichThiTong,
                MaKyThi = l.MaKyThi,
                TenKyThi = l.KyThi != null ? l.KyThi.TenKyThi : null,
                MaMonHoc = l.MaMonHoc,
                TenMonHoc = l.MonHoc != null ? l.MonHoc.TenMonHoc : null,
                MaDeKiemTra = l.MaDeKiemTra,
                TenDeKiemTra = l.DeKiemTra != null ? l.DeKiemTra.TieuDe : null,
                HinhThucThi = l.HinhThucThi,
                NgayThiDuKien = l.NgayThiDuKien,
                TrangThai = l.TrangThai,
                NgayTao = l.NgayTao,
                SoCaThi = l.CaThis.Count
            })
            .ToListAsync(ct);

        return new PagedResultDto<LichThiTongDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<LichThiTongDto> GetLichThiTongByIdAsync(int id, CancellationToken ct)
    {
        var l = await _db.LichThiTongs
            .Include(x => x.KyThi)
            .Include(x => x.MonHoc)
            .Include(x => x.DeKiemTra)
            .Include(x => x.CaThis)
            .FirstOrDefaultAsync(x => x.MaLichThiTong == id, ct)
            ?? throw new ApiException(404, "Không tìm thấy lịch thi tổng.");

        return new LichThiTongDto
        {
            MaLichThiTong = l.MaLichThiTong,
            MaKyThi = l.MaKyThi,
            TenKyThi = l.KyThi?.TenKyThi,
            MaMonHoc = l.MaMonHoc,
            TenMonHoc = l.MonHoc?.TenMonHoc,
            MaDeKiemTra = l.MaDeKiemTra,
            TenDeKiemTra = l.DeKiemTra?.TieuDe,
            HinhThucThi = l.HinhThucThi,
            NgayThiDuKien = l.NgayThiDuKien,
            TrangThai = l.TrangThai,
            NgayTao = l.NgayTao,
            SoCaThi = l.CaThis.Count
        };
    }

    public async Task<LichThiTongDto> CreateLichThiTongAsync(CreateLichThiTongRequest request, CancellationToken ct)
    {
        _ = await _db.KyThis.FindAsync(new object[] { request.MaKyThi }, ct)
            ?? throw new ApiException(400, "Kỳ thi không tồn tại.");
        _ = await _db.DanhMucMonHocs.FindAsync(new object[] { request.MaMonHoc }, ct)
            ?? throw new ApiException(400, "Môn học không tồn tại.");

        if (request.MaDeKiemTra.HasValue)
        {
            _ = await _db.DeKiemTras.FindAsync(new object[] { request.MaDeKiemTra.Value }, ct)
                ?? throw new ApiException(400, "Đề kiểm tra không tồn tại.");
        }

        var entity = new LichThiTong
        {
            MaKyThi = request.MaKyThi,
            MaMonHoc = request.MaMonHoc,
            MaDeKiemTra = request.MaDeKiemTra,
            HinhThucThi = request.HinhThucThi,
            NgayThiDuKien = request.NgayThiDuKien,
            TrangThai = "nhap"
        };

        _db.LichThiTongs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetLichThiTongByIdAsync(entity.MaLichThiTong, ct);
    }

    public async Task<LichThiTongDto> UpdateLichThiTongAsync(int id, UpdateLichThiTongRequest request, CancellationToken ct)
    {
        var entity = await _db.LichThiTongs.FindAsync(new object[] { id }, ct)
            ?? throw new ApiException(404, "Không tìm thấy lịch thi tổng.");

        if (request.MaDeKiemTra.HasValue)
            entity.MaDeKiemTra = request.MaDeKiemTra;
        if (!string.IsNullOrWhiteSpace(request.HinhThucThi))
            entity.HinhThucThi = request.HinhThucThi;
        if (request.NgayThiDuKien.HasValue)
            entity.NgayThiDuKien = request.NgayThiDuKien.Value;
        if (!string.IsNullOrWhiteSpace(request.TrangThai))
            entity.TrangThai = request.TrangThai;

        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetLichThiTongByIdAsync(id, ct);
    }

    public async Task SendToCoSoAsync(int id, CancellationToken ct)
    {
        var entity = await _db.LichThiTongs.FindAsync(new object[] { id }, ct)
            ?? throw new ApiException(404, "Không tìm thấy lịch thi tổng.");

        if (entity.TrangThai != "nhap")
            throw new ApiException(400, "Chỉ có thể gửi lịch thi ở trạng thái nháp.");

        entity.TrangThai = "da_gui_ve_co_so";
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ===== CaThi =====

    public async Task<PagedResultDto<CaThiDto>> GetCaThisAsync(CaThiQueryParameters parameters, CancellationToken ct)
    {
        var query = _db.CaThis
            .Include(c => c.Phong)
            .Include(c => c.DonVi)
            .Include(c => c.ThiSinhCaThis)
            .Include(c => c.PhanCongGiamThis)
            .AsQueryable();

        if (parameters.MaLichThiTong.HasValue)
            query = query.Where(c => c.MaLichThiTong == parameters.MaLichThiTong.Value);
        if (parameters.MaDonVi.HasValue)
            query = query.Where(c => c.MaDonVi == parameters.MaDonVi.Value);
        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
            query = query.Where(c => c.TrangThai == parameters.TrangThai);
        if (parameters.TuNgay.HasValue)
            query = query.Where(c => c.NgayThi >= parameters.TuNgay.Value);
        if (parameters.DenNgay.HasValue)
            query = query.Where(c => c.NgayThi <= parameters.DenNgay.Value);

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.NgayThi).ThenBy(c => c.ThoiGianBatDau)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(c => new CaThiDto
            {
                MaCaThi = c.MaCaThi,
                MaLichThiTong = c.MaLichThiTong,
                TenCaThi = c.TenCaThi,
                MaPhong = c.MaPhong,
                TenPhong = c.Phong != null ? c.Phong.TenPhong : null,
                NgayThi = c.NgayThi,
                ThoiGianBatDau = c.ThoiGianBatDau,
                ThoiGianKetThuc = c.ThoiGianKetThuc,
                MaDonVi = c.MaDonVi,
                TenDonVi = c.DonVi != null ? c.DonVi.TenDonVi : null,
                TrangThai = c.TrangThai,
                GhiChu = c.GhiChu,
                SoThiSinh = c.ThiSinhCaThis.Count,
                SoGiamThi = c.PhanCongGiamThis.Count
            })
            .ToListAsync(ct);

        return new PagedResultDto<CaThiDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<CaThiDto> GetCaThiByIdAsync(int id, CancellationToken ct)
    {
        var c = await _db.CaThis
            .Include(x => x.Phong)
            .Include(x => x.DonVi)
            .Include(x => x.ThiSinhCaThis)
            .Include(x => x.PhanCongGiamThis)
            .FirstOrDefaultAsync(x => x.MaCaThi == id, ct)
            ?? throw new ApiException(404, "Không tìm thấy ca thi.");

        return new CaThiDto
        {
            MaCaThi = c.MaCaThi,
            MaLichThiTong = c.MaLichThiTong,
            TenCaThi = c.TenCaThi,
            MaPhong = c.MaPhong,
            TenPhong = c.Phong?.TenPhong,
            NgayThi = c.NgayThi,
            ThoiGianBatDau = c.ThoiGianBatDau,
            ThoiGianKetThuc = c.ThoiGianKetThuc,
            MaDonVi = c.MaDonVi,
            TenDonVi = c.DonVi?.TenDonVi,
            TrangThai = c.TrangThai,
            GhiChu = c.GhiChu,
            SoThiSinh = c.ThiSinhCaThis.Count,
            SoGiamThi = c.PhanCongGiamThis.Count
        };
    }

    public async Task<CaThiDto> CreateCaThiAsync(CreateCaThiRequest request, CancellationToken ct)
    {
        _ = await _db.LichThiTongs.FindAsync(new object[] { request.MaLichThiTong }, ct)
            ?? throw new ApiException(400, "Lịch thi tổng không tồn tại.");
        _ = await _db.DonVis.FindAsync(new object[] { request.MaDonVi }, ct)
            ?? throw new ApiException(400, "Đơn vị không tồn tại.");

        if (request.MaPhong.HasValue)
        {
            _ = await _db.PhongHocs.FindAsync(new object[] { request.MaPhong.Value }, ct)
                ?? throw new ApiException(400, "Phòng thi không tồn tại.");

            // Check room conflict
            var conflict = await _db.CaThis.AnyAsync(c =>
                c.MaPhong == request.MaPhong.Value &&
                c.TrangThai != "da_huy" &&
                c.ThoiGianBatDau < request.ThoiGianKetThuc &&
                c.ThoiGianKetThuc > request.ThoiGianBatDau, ct);

            if (conflict)
                throw new ApiException(409, "Phòng thi đã bị trùng lịch trong khung giờ này.");
        }

        var entity = new CaThi
        {
            MaLichThiTong = request.MaLichThiTong,
            TenCaThi = request.TenCaThi,
            MaPhong = request.MaPhong,
            NgayThi = request.NgayThi,
            ThoiGianBatDau = request.ThoiGianBatDau,
            ThoiGianKetThuc = request.ThoiGianKetThuc,
            MaDonVi = request.MaDonVi,
            GhiChu = request.GhiChu,
            TrangThai = "nhap"
        };

        _db.CaThis.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetCaThiByIdAsync(entity.MaCaThi, ct);
    }

    public async Task<CaThiDto> UpdateCaThiAsync(int id, UpdateCaThiRequest request, CancellationToken ct)
    {
        var entity = await _db.CaThis.FindAsync(new object[] { id }, ct)
            ?? throw new ApiException(404, "Không tìm thấy ca thi.");

        if (!string.IsNullOrWhiteSpace(request.TenCaThi))
            entity.TenCaThi = request.TenCaThi;
        if (request.MaPhong.HasValue)
            entity.MaPhong = request.MaPhong;
        if (request.NgayThi.HasValue)
            entity.NgayThi = request.NgayThi.Value;
        if (request.ThoiGianBatDau.HasValue)
            entity.ThoiGianBatDau = request.ThoiGianBatDau.Value;
        if (request.ThoiGianKetThuc.HasValue)
            entity.ThoiGianKetThuc = request.ThoiGianKetThuc.Value;
        if (!string.IsNullOrWhiteSpace(request.TrangThai))
            entity.TrangThai = request.TrangThai;
        if (request.GhiChu != null)
            entity.GhiChu = request.GhiChu;
        if (!string.IsNullOrWhiteSpace(request.LyDoDieuChinh))
            entity.LyDoDieuChinh = request.LyDoDieuChinh;

        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await GetCaThiByIdAsync(id, ct);
    }

    // ===== PhanCongGiamThi =====

    public async Task<IReadOnlyList<PhanCongGiamThiDto>> GetGiamThisByCaThiAsync(int maCaThi, CancellationToken ct)
    {
        return await _db.PhanCongGiamThis
            .Include(p => p.GiamThi)
            .Where(p => p.MaCaThi == maCaThi)
            .Select(p => new PhanCongGiamThiDto
            {
                MaPhanCong = p.MaPhanCong,
                MaCaThi = p.MaCaThi,
                MaGiamThi = p.MaGiamThi,
                TenGiamThi = p.GiamThi != null ? p.GiamThi.HoTen : null,
                VaiTroGiamThi = p.VaiTroGiamThi,
                TrangThai = p.TrangThai,
                LyDoThayDoi = p.LyDoThayDoi
            })
            .ToListAsync(ct);
    }

    public async Task<PhanCongGiamThiDto> AssignGiamThiAsync(CreatePhanCongGiamThiRequest request, CancellationToken ct)
    {
        _ = await _db.CaThis.FindAsync(new object[] { request.MaCaThi }, ct)
            ?? throw new ApiException(400, "Ca thi không tồn tại.");
        _ = await _db.NguoiDungs.FindAsync(new object[] { request.MaGiamThi }, ct)
            ?? throw new ApiException(400, "Giám thị không tồn tại.");

        var exists = await _db.PhanCongGiamThis.AnyAsync(
            p => p.MaCaThi == request.MaCaThi && p.MaGiamThi == request.MaGiamThi, ct);
        if (exists)
            throw new ApiException(409, "Giám thị đã được phân công cho ca thi này.");

        var entity = new PhanCongGiamThi
        {
            MaCaThi = request.MaCaThi,
            MaGiamThi = request.MaGiamThi,
            VaiTroGiamThi = request.VaiTroGiamThi
        };

        _db.PhanCongGiamThis.Add(entity);
        await _db.SaveChangesAsync(ct);

        return (await GetGiamThisByCaThiAsync(request.MaCaThi, ct))
            .First(p => p.MaPhanCong == entity.MaPhanCong);
    }

    public async Task RemoveGiamThiAsync(int maPhanCong, CancellationToken ct)
    {
        var entity = await _db.PhanCongGiamThis.FindAsync(new object[] { maPhanCong }, ct)
            ?? throw new ApiException(404, "Không tìm thấy phân công giám thị.");

        entity.TrangThai = "huy_phan_cong";
        await _db.SaveChangesAsync(ct);
    }

    // ===== ThiSinhCaThi =====

    public async Task<IReadOnlyList<ThiSinhCaThiDto>> GetThiSinhsByCaThiAsync(int maCaThi, CancellationToken ct)
    {
        return await _db.ThiSinhCaThis
            .Include(t => t.HocSinh)
            .Where(t => t.MaCaThi == maCaThi)
            .Select(t => new ThiSinhCaThiDto
            {
                MaThiSinhCaThi = t.MaThiSinhCaThi,
                MaCaThi = t.MaCaThi,
                MaHocSinh = t.MaHocSinh,
                TenHocSinh = t.HocSinh != null ? t.HocSinh.HoTen : null,
                Email = t.HocSinh != null ? t.HocSinh.Email : null,
                TrangThaiDuThi = t.TrangThaiDuThi,
                GhiChu = t.GhiChu
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ThiSinhCaThiDto>> AddThiSinhsToCaThiAsync(AddThiSinhCaThiRequest request, CancellationToken ct)
    {
        _ = await _db.CaThis.FindAsync(new object[] { request.MaCaThi }, ct)
            ?? throw new ApiException(400, "Ca thi không tồn tại.");

        var existingIds = await _db.ThiSinhCaThis
            .Where(t => t.MaCaThi == request.MaCaThi)
            .Select(t => t.MaHocSinh)
            .ToListAsync(ct);

        var newIds = request.DanhSachMaHocSinh.Except(existingIds).ToList();

        foreach (var maHocSinh in newIds)
        {
            _db.ThiSinhCaThis.Add(new ThiSinhCaThi
            {
                MaCaThi = request.MaCaThi,
                MaHocSinh = maHocSinh,
                TrangThaiDuThi = "cho_thi"
            });
        }

        await _db.SaveChangesAsync(ct);
        return await GetThiSinhsByCaThiAsync(request.MaCaThi, ct);
    }

    // ===== DiemDanhThi =====

    public async Task<IReadOnlyList<DiemDanhThiDto>> GetDiemDanhByCaThiAsync(int maCaThi, CancellationToken ct)
    {
        return await _db.DiemDanhThis
            .Include(d => d.HocSinh)
            .Include(d => d.NguoiDiemDanh)
            .Where(d => d.MaCaThi == maCaThi)
            .Select(d => new DiemDanhThiDto
            {
                MaDiemDanhThi = d.MaDiemDanhThi,
                MaCaThi = d.MaCaThi,
                MaHocSinh = d.MaHocSinh,
                TenHocSinh = d.HocSinh != null ? d.HocSinh.HoTen : null,
                TrangThaiDiemDanh = d.TrangThaiDiemDanh,
                ThoiDiemDiemDanh = d.ThoiDiemDiemDanh,
                MaNguoiDiemDanh = d.MaNguoiDiemDanh,
                TenNguoiDiemDanh = d.NguoiDiemDanh != null ? d.NguoiDiemDanh.HoTen : null,
                GhiChu = d.GhiChu
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DiemDanhThiDto>> BatchDiemDanhAsync(BatchDiemDanhRequest request, int maNguoiDiemDanh, CancellationToken ct)
    {
        _ = await _db.CaThis.FindAsync(new object[] { request.MaCaThi }, ct)
            ?? throw new ApiException(400, "Ca thi không tồn tại.");

        foreach (var item in request.DanhSachDiemDanh)
        {
            var existing = await _db.DiemDanhThis
                .FirstOrDefaultAsync(d => d.MaCaThi == request.MaCaThi && d.MaHocSinh == item.MaHocSinh, ct);

            if (existing != null)
            {
                existing.TrangThaiDiemDanh = item.TrangThaiDiemDanh;
                existing.ThoiDiemDiemDanh = DateTime.UtcNow;
                existing.MaNguoiDiemDanh = maNguoiDiemDanh;
                existing.GhiChu = item.GhiChu;
            }
            else
            {
                _db.DiemDanhThis.Add(new DiemDanhThi
                {
                    MaCaThi = request.MaCaThi,
                    MaHocSinh = item.MaHocSinh,
                    TrangThaiDiemDanh = item.TrangThaiDiemDanh,
                    ThoiDiemDiemDanh = DateTime.UtcNow,
                    MaNguoiDiemDanh = maNguoiDiemDanh,
                    GhiChu = item.GhiChu
                });
            }

            // Sync with ThiSinhCaThi
            var thiSinhCaThi = await _db.ThiSinhCaThis
                .FirstOrDefaultAsync(t => t.MaCaThi == request.MaCaThi && t.MaHocSinh == item.MaHocSinh, ct);
            if (thiSinhCaThi != null)
            {
                if (item.TrangThaiDiemDanh == "co_mat")
                {
                    thiSinhCaThi.TrangThaiDuThi = "duoc_thi";
                }
                else if (item.TrangThaiDiemDanh == "vang_mat" && thiSinhCaThi.TrangThaiDuThi != "dinh_chi")
                {
                    thiSinhCaThi.TrangThaiDuThi = "vang_thi";
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return await GetDiemDanhByCaThiAsync(request.MaCaThi, ct);
    }

    public async Task StartCaThiAsync(int id, CancellationToken ct)
    {
        var caThi = await _db.CaThis.FindAsync(new object[] { id }, ct)
            ?? throw new ApiException(404, "Ca thi không tồn tại.");

        if (caThi.TrangThai == "da_ket_thuc" || caThi.TrangThai == "da_huy")
        {
            throw new ApiException(400, "Ca thi đã kết thúc hoặc đã hủy, không thể bắt đầu.");
        }

        caThi.TrangThai = "dang_thi";
        await _db.SaveChangesAsync(ct);
    }

    // ===== NhatKyViPhamThi =====

    public async Task<IReadOnlyList<NhatKyViPhamThiDto>> GetViPhamsByCaThiAsync(int maCaThi, CancellationToken ct)
    {
        return await _db.NhatKyViPhamThis
            .Include(v => v.HocSinh)
            .Include(v => v.XuLyViPhams)
            .Where(v => v.MaCaThi == maCaThi)
            .OrderByDescending(v => v.ThoiDiem)
            .Select(v => new NhatKyViPhamThiDto
            {
                MaViPham = v.MaViPham,
                MaPhienThi = v.MaPhienThi,
                MaHocSinh = v.MaHocSinh,
                TenHocSinh = v.HocSinh != null ? v.HocSinh.HoTen : null,
                MaCaThi = v.MaCaThi,
                LoaiViPham = v.LoaiViPham,
                MucDo = v.MucDo,
                ChiTietJson = v.ChiTietJson,
                ThoiDiem = v.ThoiDiem,
                SoLanXuLy = v.XuLyViPhams.Count
            })
            .ToListAsync(ct);
    }

    public async Task<NhatKyViPhamThiDto> CreateViPhamAsync(CreateNhatKyViPhamRequest request, CancellationToken ct)
    {
        var entity = new NhatKyViPhamThi
        {
            MaPhienThi = request.MaPhienThi,
            MaHocSinh = request.MaHocSinh,
            MaCaThi = request.MaCaThi,
            LoaiViPham = request.LoaiViPham,
            MucDo = request.MucDo,
            ChiTietJson = request.ChiTietJson,
            ThoiDiem = DateTime.UtcNow
        };

        _db.NhatKyViPhamThis.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new NhatKyViPhamThiDto
        {
            MaViPham = entity.MaViPham,
            MaPhienThi = entity.MaPhienThi,
            MaHocSinh = entity.MaHocSinh,
            MaCaThi = entity.MaCaThi,
            LoaiViPham = entity.LoaiViPham,
            MucDo = entity.MucDo,
            ChiTietJson = entity.ChiTietJson,
            ThoiDiem = entity.ThoiDiem,
            SoLanXuLy = 0
        };
    }

    // ===== XuLyViPhamThi =====

    public async Task<XuLyViPhamThiDto> XuLyViPhamAsync(CreateXuLyViPhamRequest request, int maNguoiXuLy, CancellationToken ct)
    {
        var viPham = await _db.NhatKyViPhamThis
            .Include(v => v.XuLyViPhams)
            .FirstOrDefaultAsync(v => v.MaViPham == request.MaViPham, ct)
            ?? throw new ApiException(404, "Không tìm thấy vi phạm.");

        var lanNhacNho = viPham.XuLyViPhams.Count + 1;

        var entity = new XuLyViPhamThi
        {
            MaViPham = request.MaViPham,
            HanhDongXuLy = request.HanhDongXuLy,
            LanNhacNho = lanNhacNho,
            MaNguoiXuLy = maNguoiXuLy,
            ThoiDiem = DateTime.UtcNow,
            LyDo = request.LyDo,
            GhiChu = request.GhiChu
        };

        _db.XuLyViPhamThis.Add(entity);

        // Auto-escalate severity
        if (lanNhacNho >= 3 && viPham.MucDo != "nghiem_trong")
        {
            viPham.MucDo = "nghiem_trong";
        }

        await _db.SaveChangesAsync(ct);

        var nguoiXuLy = await _db.NguoiDungs.FindAsync(new object[] { maNguoiXuLy }, ct);

        return new XuLyViPhamThiDto
        {
            MaXuLy = entity.MaXuLy,
            MaViPham = entity.MaViPham,
            HanhDongXuLy = entity.HanhDongXuLy,
            LanNhacNho = entity.LanNhacNho,
            MaNguoiXuLy = entity.MaNguoiXuLy,
            TenNguoiXuLy = nguoiXuLy?.HoTen,
            ThoiDiem = entity.ThoiDiem,
            LyDo = entity.LyDo,
            GhiChu = entity.GhiChu
        };
    }

    // ===== BienBanThi =====

    public async Task<IReadOnlyList<BienBanThiDto>> GetBienBansByCaThiAsync(int maCaThi, CancellationToken ct)
    {
        return await _db.BienBanThis
            .Include(b => b.NguoiLap)
            .Where(b => b.MaCaThi == maCaThi)
            .OrderByDescending(b => b.ThoiDiemLap)
            .Select(b => new BienBanThiDto
            {
                MaBienBan = b.MaBienBan,
                MaCaThi = b.MaCaThi,
                MaPhienThi = b.MaPhienThi,
                LoaiBienBan = b.LoaiBienBan,
                NoiDung = b.NoiDung,
                MaNguoiLap = b.MaNguoiLap,
                TenNguoiLap = b.NguoiLap != null ? b.NguoiLap.HoTen : null,
                ThoiDiemLap = b.ThoiDiemLap,
                TrangThaiXuLy = b.TrangThaiXuLy
            })
            .ToListAsync(ct);
    }

    public async Task<BienBanThiDto> CreateBienBanAsync(CreateBienBanThiRequest request, int maNguoiLap, CancellationToken ct)
    {
        var entity = new BienBanThi
        {
            MaCaThi = request.MaCaThi,
            MaPhienThi = request.MaPhienThi,
            LoaiBienBan = request.LoaiBienBan,
            NoiDung = request.NoiDung,
            MaNguoiLap = maNguoiLap,
            ThoiDiemLap = DateTime.UtcNow,
            TrangThaiXuLy = "cho_xu_ly"
        };

        _db.BienBanThis.Add(entity);
        await _db.SaveChangesAsync(ct);

        var nguoiLap = await _db.NguoiDungs.FindAsync(new object[] { maNguoiLap }, ct);

        return new BienBanThiDto
        {
            MaBienBan = entity.MaBienBan,
            MaCaThi = entity.MaCaThi,
            MaPhienThi = entity.MaPhienThi,
            LoaiBienBan = entity.LoaiBienBan,
            NoiDung = entity.NoiDung,
            MaNguoiLap = entity.MaNguoiLap,
            TenNguoiLap = nguoiLap?.HoTen,
            ThoiDiemLap = entity.ThoiDiemLap,
            TrangThaiXuLy = entity.TrangThaiXuLy
        };
    }

    // ===== Signature =====

    public async Task<PhienThiDto> ConfirmSignatureAsync(ConfirmSignatureRequest request, int maNguoiXacNhan, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaPhienThi == request.MaPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        phienThi.TrangThaiKyTen = "da_ky";
        phienThi.ThoiDiemKy = DateTime.UtcNow;
        phienThi.NguoiXacNhanKyTen = maNguoiXacNhan;

        await _db.SaveChangesAsync(ct);
        return MapToPhienThiDto(phienThi);
    }

    public async Task<PhienThiDto> ReportMissingSignatureAsync(ReportMissingSignatureRequest request, int maNguoiXacNhan, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaPhienThi == request.MaPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        phienThi.TrangThaiKyTen = "quen_ky";
        phienThi.NguoiXacNhanKyTen = maNguoiXacNhan;

        // Create incident report for missing signature
        if (phienThi.MaCaThi.HasValue)
        {
            _db.BienBanThis.Add(new BienBanThi
            {
                MaCaThi = phienThi.MaCaThi.Value,
                MaPhienThi = phienThi.MaPhienThi,
                LoaiBienBan = "quen_ky_ten",
                NoiDung = request.GhiChu ?? $"Sinh viên (MaPhienThi: {request.MaPhienThi}) quên ký tên sau thi.",
                MaNguoiLap = maNguoiXacNhan,
                ThoiDiemLap = DateTime.UtcNow,
                TrangThaiXuLy = "cho_xu_ly"
            });
        }

        await _db.SaveChangesAsync(ct);
        return MapToPhienThiDto(phienThi);
    }

    public async Task<IReadOnlyList<PhienThiDto>> GetMissingSignatureSessionsAsync(int? maCaThi, CancellationToken ct)
    {
        var query = _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .Where(p => p.TrangThaiKyTen == "quen_ky");

        if (maCaThi.HasValue)
        {
            query = query.Where(p => p.MaCaThi == maCaThi.Value);
        }

        var list = await query.ToListAsync(ct);
        return list.Select(MapToPhienThiDto).ToList();
    }

    public async Task<IReadOnlyList<PhienThiDto>> GetSignedSessionsAsync(int? maCaThi, CancellationToken ct)
    {
        var query = _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .Where(p => p.TrangThaiKyTen == "da_ky");

        if (maCaThi.HasValue)
        {
            query = query.Where(p => p.MaCaThi == maCaThi.Value);
        }

        var list = await query.ToListAsync(ct);
        return list.Select(MapToPhienThiDto).ToList();
    }

    // ===== Exam Taking =====

    public async Task<IReadOnlyList<StudentExamListItemDto>> GetStudentExamsAsync(int maHocSinh, CancellationToken ct)
    {
        var student = await _db.NguoiDungs
            .AsNoTracking()
            .Include(x => x.Lop)
            .FirstOrDefaultAsync(x => x.MaNguoiDung == maHocSinh, ct)
            ?? throw new ApiException(404, "Không tìm thấy học sinh hiện tại.");

        if (student.MaLop is null || student.Lop?.MaChuongTrinh is null)
        {
            throw new ApiException(404, "Học sinh chưa được gán lớp hoặc chương trình đào tạo.");
        }

        var program = await _db.ChuongTrinhDaoTaos
            .AsNoTracking()
            .Include(x => x.ChuyenNganh)
                .ThenInclude(x => x!.NganhDaoTao)
            .FirstOrDefaultAsync(x => x.MaChuongTrinh == student.Lop.MaChuongTrinh.Value && x.ConHoatDong, ct)
            ?? throw new ApiException(404, "Không tìm thấy chương trình đào tạo của học sinh.");

        var programSubjects = await _db.MonHocTrongChuongTrinhs
            .AsNoTracking()
            .Include(x => x.DanhMucMonHoc)
            .Where(x => x.MaChuongTrinh == program.MaChuongTrinh && x.ConHoatDong)
            .ToListAsync(ct);

        var subjectIds = programSubjects.Select(x => x.MaMonHoc).Distinct().ToList();
        if (subjectIds.Count == 0)
        {
            return [];
        }

        var grades = await _db.DiemSos
            .AsNoTracking()
            .Where(x => x.MaHocSinh == maHocSinh && subjectIds.Contains(x.MaMonHoc))
            .ToListAsync(ct);
        var completedSubjectIds = grades
            .Where(x => x.TrangThai == "dat")
            .Select(x => x.MaMonHoc)
            .ToHashSet();
        var currentSemesterIndex = DetermineCurrentSemester(programSubjects, completedSubjectIds, program.SoHocKy);

        var exams = await _db.DeKiemTras
            .AsNoTracking()
            .Include(x => x.MonHoc)
            .Include(x => x.HocKy)
            .Where(x =>
                x.MaMonHoc.HasValue &&
                subjectIds.Contains(x.MaMonHoc.Value) &&
                x.TrangThai != "nhap")
            .OrderBy(x => x.MaHocKy)
            .ThenBy(x => x.MaMonHoc)
            .ThenBy(x => x.TieuDe)
            .ToListAsync(ct);

        var examIds = exams.Select(x => x.MaDeKiemTra).ToList();
        var assignments = await _db.ThiSinhCaThis
            .AsNoTracking()
            .Include(x => x.CaThi)
                .ThenInclude(x => x!.LichThiTong)
            .Where(x =>
                x.MaHocSinh == maHocSinh &&
                x.CaThi != null &&
                x.CaThi.LichThiTong != null &&
                x.CaThi.LichThiTong.MaDeKiemTra.HasValue &&
                examIds.Contains(x.CaThi.LichThiTong.MaDeKiemTra.Value))
            .ToListAsync(ct);
        var assignmentByExam = assignments
            .GroupBy(x => x.CaThi!.LichThiTong!.MaDeKiemTra!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(item => item.CaThi!.ThoiGianBatDau).First());

        var attempts = await _db.PhienThiHocSinhs
            .AsNoTracking()
            .Where(x => x.MaHocSinh == maHocSinh && examIds.Contains(x.MaDeKiemTra))
            .ToListAsync(ct);
        var attemptsByExam = attempts
            .GroupBy(x => x.MaDeKiemTra)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.LanThu).First());

        var questionCounts = await _db.CauHoiDeKiemTras
            .AsNoTracking()
            .Where(x => examIds.Contains(x.MaDeKiemTra))
            .GroupBy(x => x.MaDeKiemTra)
            .Select(x => new { MaDeKiemTra = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.MaDeKiemTra, x => x.Count, ct);

        return exams.Select(exam =>
        {
            var programSubject = programSubjects.FirstOrDefault(x => x.MaMonHoc == exam.MaMonHoc);
            assignmentByExam.TryGetValue(exam.MaDeKiemTra, out var assignment);
            attemptsByExam.TryGetValue(exam.MaDeKiemTra, out var attempt);

            var caThi = assignment?.CaThi;
            var status = ResolveStudentExamStatus(exam, caThi, attempt);
            var accessStatus = ResolveStudentExamAccessStatus(exam, caThi, attempt, programSubject?.HocKyDuKien ?? 1, currentSemesterIndex);
            var score = attempt?.DiemCuoiCung ?? attempt?.DiemTuDong;

            return new StudentExamListItemDto
            {
                Id = exam.MaDeKiemTra.ToString(),
                MaDeKiemTra = exam.MaDeKiemTra,
                MaCaThi = caThi?.MaCaThi,
                Title = exam.TieuDe,
                Subject = exam.MonHoc?.TenMonHoc ?? string.Empty,
                SubjectCode = exam.MonHoc?.MaCodeMonHoc ?? string.Empty,
                MajorName = program.ChuyenNganh?.TenChuyenNganh ?? program.TenChuongTrinh,
                FacultyName = program.ChuyenNganh?.NganhDaoTao?.TenNganh ?? string.Empty,
                SemesterName = exam.HocKy?.TenHocKy ?? $"Kỳ {programSubject?.HocKyDuKien ?? 1}",
                BlockName = "Block 1",
                PlannedSemesterIndex = programSubject?.HocKyDuKien ?? 1,
                PlannedBlockIndex = 1,
                StudentCurrentSemesterIndex = currentSemesterIndex,
                StudentCurrentBlockIndex = 1,
                DurationMinutes = exam.ThoiGianPhut,
                TotalQuestions = questionCounts.GetValueOrDefault(exam.MaDeKiemTra),
                ExamTypeLabel = ResolveExamTypeLabel(exam.LoaiDeThi),
                UsedAttempts = attempts.Count(x => x.MaDeKiemTra == exam.MaDeKiemTra),
                MaxAttempts = 1,
                Status = status,
                AccessStatus = accessStatus,
                OpenAt = caThi?.ThoiGianBatDau.ToString("O"),
                CloseAt = caThi?.ThoiGianKetThuc.ToString("O"),
                Score = score,
                ResultId = score.HasValue ? attempt?.MaPhienThi.ToString() : null,
                ClassSectionCode = student.Lop.MaCodeLop,
                TrangThaiDuThi = assignment?.TrangThaiDuThi
            };
        }).ToList();
    }

    public async Task<PhienThiDto> StartExamAsync(StartExamRequest request, int maHocSinh, CancellationToken ct)
    {
        var caThi = await _db.CaThis
            .Include(c => c.LichThiTong)
            .FirstOrDefaultAsync(c => c.MaCaThi == request.MaCaThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy ca thi.");

        if (caThi.TrangThai != "dang_thi")
            throw new ApiException(400, "Ca thi chưa bắt đầu hoặc đã kết thúc.");

        // Check if student is eligible
        var thiSinh = await _db.ThiSinhCaThis
            .FirstOrDefaultAsync(t => t.MaCaThi == request.MaCaThi && t.MaHocSinh == maHocSinh, ct)
            ?? throw new ApiException(403, "Sinh viên không có trong danh sách thi của ca này.");

        if (thiSinh.TrangThaiDuThi == "khong_duoc_thi" || thiSinh.TrangThaiDuThi == "dinh_chi")
            throw new ApiException(403, "Sinh viên không đủ điều kiện dự thi.");

        // Validate Environment Security
        if (request.EnvCheckScore >= 70)
            throw new ApiException(403, "Môi trường thi không đạt yêu cầu bảo mật. Vui lòng quét lại môi trường.");

        // We can optionally check IsAgentActive here if needed, but let's assume EnvCheckScore incorporates it
        // Or we can explicitly check if (request.EnvCheckScore > 30 && !request.IsAgentActive) etc.

        // Check if already has session
        var existingSession = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaCaThi == request.MaCaThi && p.MaHocSinh == maHocSinh, ct);

        if (existingSession != null)
        {
            if (existingSession.TrangThaiLuong == "da_dung")
                throw new ApiException(400, "Sinh viên đã nộp bài thi.");

            return MapToPhienThiDto(existingSession);
        }

        var maDeKiemTra = caThi.LichThiTong?.MaDeKiemTra
            ?? throw new ApiException(400, "Ca thi chưa được gán đề kiểm tra.");

        var phienThi = new PhienThiHocSinh
        {
            MaDeKiemTra = maDeKiemTra,
            MaHocSinh = maHocSinh,
            MaCaThi = request.MaCaThi,
            BatDauLuc = DateTime.UtcNow,
            HanNopLuc = caThi.ThoiGianKetThuc,
            LanThu = 1,
            TrangThaiLuong = "dang_hoat_dong",
            TrangThaiKyTen = "chua_ky",
            TrangThaiCongBo = "chua_co_diem"
        };

        _db.PhienThiHocSinhs.Add(phienThi);
        thiSinh.TrangThaiDuThi = "duoc_thi";
        await _db.SaveChangesAsync(ct);

        await _db.Entry(phienThi).Reference(p => p.HocSinh).LoadAsync(ct);

        return MapToPhienThiDto(phienThi);
    }

    public async Task<PhienThiDto> GetExamSessionAsync(int maPhienThi, int maHocSinh, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaPhienThi == maPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        if (phienThi.MaHocSinh != maHocSinh)
            throw new ApiException(403, "Không có quyền truy cập phiên thi này.");

        return MapToPhienThiDto(phienThi);
    }

    public async Task<IReadOnlyList<QuizAttemptQuestionDto>> GetExamQuestionsAsync(int maPhienThi, int maHocSinh, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .FirstOrDefaultAsync(p => p.MaPhienThi == maPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        if (phienThi.MaHocSinh != maHocSinh)
            throw new ApiException(403, "Không có quyền truy cập phiên thi này.");

        var questions = await _db.CauHoiDeKiemTras
            .Include(x => x.CauHoi)
            .Where(x => x.MaDeKiemTra == phienThi.MaDeKiemTra)
            .OrderBy(x => x.ThuTu)
            .ToListAsync(ct);

        return questions.Select(x => new QuizAttemptQuestionDto
        {
            MaCauHoi = x.MaCauHoi,
            LoaiCauHoi = x.CauHoi.LoaiCauHoi,
            NoiDung = x.CauHoi.NoiDung,
            KieuLuaChon = x.CauHoi.KieuLuaChon,
            LuaChon = string.IsNullOrEmpty(x.CauHoi.LuaChon) ? null : System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(x.CauHoi.LuaChon),
            DiemSo = x.DiemSo,
            ThuTu = x.ThuTu
        }).ToList();
    }

    public async Task AutoSaveAnswerAsync(AutoSaveAnswerRequest request, int maHocSinh, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs.FindAsync(new object[] { request.MaPhienThi }, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        if (phienThi.MaHocSinh != maHocSinh)
            throw new ApiException(403, "Không có quyền cập nhật phiên thi này.");

        if (phienThi.TrangThaiLuong == "da_dung")
            throw new ApiException(400, "Bài thi đã được nộp.");

        phienThi.SaoLuuCucBo = request.CauTraLoiJson;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PhienThiDto> SubmitExamAsync(SubmitExamRequest request, int maHocSinh, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaPhienThi == request.MaPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        if (phienThi.MaHocSinh != maHocSinh)
            throw new ApiException(403, "Không có quyền nộp bài thi này.");

        if (phienThi.TrangThaiLuong == "da_dung")
            throw new ApiException(400, "Bài thi đã được nộp rồi.");

        phienThi.CauTraLoiJson = request.CauTraLoiJson;
        phienThi.NopLuc = DateTime.UtcNow;
        phienThi.TrangThaiLuong = "da_dung";

        await _db.SaveChangesAsync(ct);

        return MapToPhienThiDto(phienThi);
    }

    public async Task<object> GetStudentExamResultAsync(int maPhienThi, int maHocSinh, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.CaThi).ThenInclude(c => c.LichThiTong).ThenInclude(l => l.MonHoc)
            .FirstOrDefaultAsync(p => p.MaPhienThi == maPhienThi && p.MaHocSinh == maHocSinh, ct)
            ?? throw new ApiException(404, "Không tìm thấy kết quả thi.");

        return new
        {
            SessionId = phienThi.MaPhienThi,
            SubjectName = phienThi.CaThi?.LichThiTong?.MonHoc?.TenMonHoc ?? "Unknown",
            Score = phienThi.DiemCuoiCung ?? phienThi.DiemTuDong,
            Status = phienThi.TrangThaiLuong,
            SubmittedAt = phienThi.NopLuc
        };
    }

    // ===== Grading =====

    // Lưu ý: Hàm này CHƯA đồng bộ DiemGiuaKy/DiemCuoiKy vào DiemSo, vì điểm ở bước
    // này chỉ là tạm (trạng thái "da_cham_xong"), có thể còn bị điều chỉnh (chấm tự luận,
    // phúc khảo) trước khi công bố chính thức. Việc đồng bộ CHỈ diễn ra ở PublishScoresAsync
    // (khi TrangThaiCongBo chuyển sang "da_cong_bo") — xem SyncDiemThiToDiemSoAsync.
    public async Task FinalizeAutoGradeAsync(int maCaThi, CancellationToken ct)
    {
        var phienThis = await _db.PhienThiHocSinhs
            .Where(p => p.MaCaThi == maCaThi && p.TrangThaiLuong == "da_dung")
            .ToListAsync(ct);

        foreach (var p in phienThis)
        {
            var questions = await _db.CauHoiDeKiemTras
                .Include(x => x.CauHoi)
                .Where(x => x.MaDeKiemTra == p.MaDeKiemTra)
                .OrderBy(x => x.ThuTu)
                .ToListAsync(ct);

            var answers = _gradingService.ParseAnswersJson(p.CauTraLoiJson);
            var grading = _gradingService.GradeObjectiveQuestions(questions, answers, false);

            p.DiemTuDong = grading.DiemTracNghiem;
            p.SoCauDung = grading.SoCauDung;
            if (!grading.CoCauTuLuan)
            {
                p.DiemCuoiCung = grading.DiemTracNghiem;
            }
            p.TrangThaiCongBo = grading.CoCauTuLuan ? "chua_co_diem" : "da_cham_xong";
            p.NgayCapNhat = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<PhienThiDto> GradeEssayAsync(GradeEssayRequest request, CancellationToken ct)
    {
        var phienThi = await _db.PhienThiHocSinhs
            .Include(p => p.HocSinh)
            .FirstOrDefaultAsync(p => p.MaPhienThi == request.MaPhienThi, ct)
            ?? throw new ApiException(404, "Không tìm thấy phiên thi.");

        if (phienThi.TrangThaiLuong != "da_dung")
            throw new ApiException(400, "Bài thi chưa được nộp.");

        phienThi.DiemCuoiCung = request.DiemCuoiCung;
        phienThi.TrangThaiCongBo = "da_cham_xong";
        phienThi.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Sync DiemGiuaKy/DiemCuoiKy to DiemSo
        await SyncDiemThiToDiemSoAsync(new[] { phienThi }, ct);

        // Recompute Grade (Trigger)
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == phienThi.MaDeKiemTra, ct);
        if (quiz?.MaMonHoc != null && quiz?.MaHocKy != null)
        {
            try
            {
                await _gradeAggregationService.CalculateGradeAsync(phienThi.MaHocSinh, quiz.MaMonHoc.Value, quiz.MaHocKy.Value, ct);
            }
            catch (ApiException ex) when (ex.StatusCode == 400)
            {
                _logger.LogWarning("Không thể tính lại điểm sau khi chấm tự luận (môn chưa cấu hình đầu điểm): {Message}", ex.Message);
            }
        }

        return MapToPhienThiDto(phienThi);
    }

    public async Task PublishScoresAsync(PublishScoresRequest request, CancellationToken ct)
    {
        var phienThis = await _db.PhienThiHocSinhs
            .Where(p => p.MaCaThi == request.MaCaThi && p.TrangThaiCongBo == "da_cham_xong")
            .ToListAsync(ct);

        if (!phienThis.Any())
            throw new ApiException(400, "Không có bài thi nào ở trạng thái đã chấm xong để công bố.");

        foreach (var p in phienThis)
        {
            p.TrangThaiCongBo = "da_cong_bo";
            // If no final score set, use auto-grade
            p.DiemCuoiCung ??= p.DiemTuDong;
        }

        // Update ca thi status
        var caThi = await _db.CaThis.FindAsync(new object[] { request.MaCaThi }, ct);
        if (caThi != null)
        {
            caThi.TrangThai = "da_ket_thuc";
            caThi.NgayCapNhat = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        // Sync DiemGiuaKy/DiemCuoiKy to DiemSo
        await SyncDiemThiToDiemSoAsync(phienThis, ct);

        // Recompute Grade (Trigger) for all students in published attempts
        var deKiemTraId = phienThis.FirstOrDefault()?.MaDeKiemTra;
        if (deKiemTraId.HasValue)
        {
            var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == deKiemTraId, ct);
            if (quiz?.MaMonHoc != null && quiz?.MaHocKy != null)
            {
                foreach (var p in phienThis)
                {
                    try
                    {
                        await _gradeAggregationService.CalculateGradeAsync(p.MaHocSinh, quiz.MaMonHoc.Value, quiz.MaHocKy.Value, ct);
                    }
                    catch (ApiException ex) when (ex.StatusCode == 400)
                    {
                        _logger.LogWarning("Không thể tính lại điểm sau khi công bố điểm thi (môn chưa cấu hình đầu điểm): {Message}", ex.Message);
                    }
                }
            }
        }
    }

    // ===== Reports =====

    public async Task<ExamReportSummaryDto> GetReportSummaryAsync(int? maKyThi, int? maDonVi, CancellationToken ct)
    {
        var kyThiQuery = _db.KyThis.AsQueryable();
        if (maKyThi.HasValue)
            kyThiQuery = kyThiQuery.Where(k => k.MaKyThi == maKyThi.Value);

        var caThiQuery = _db.CaThis.AsQueryable();
        if (maDonVi.HasValue)
            caThiQuery = caThiQuery.Where(c => c.MaDonVi == maDonVi.Value);
        if (maKyThi.HasValue)
            caThiQuery = caThiQuery.Where(c => c.LichThiTong != null && c.LichThiTong.MaKyThi == maKyThi.Value);

        var caThiIds = await caThiQuery.Select(c => c.MaCaThi).ToListAsync(ct);

        var diemDanhStats = await _db.DiemDanhThis
            .Where(d => caThiIds.Contains(d.MaCaThi))
            .GroupBy(d => 1)
            .Select(g => new
            {
                CoMat = g.Count(d => d.TrangThaiDiemDanh == "co_mat"),
                VangMat = g.Count(d => d.TrangThaiDiemDanh == "vang_mat")
            })
            .FirstOrDefaultAsync(ct);

        var tongViPham = await _db.NhatKyViPhamThis
            .Where(v => caThiIds.Contains(v.MaCaThi))
            .CountAsync(ct);

        var tongBienBan = await _db.BienBanThis
            .Where(b => caThiIds.Contains(b.MaCaThi))
            .CountAsync(ct);

        var diemTrungBinh = await _db.PhienThiHocSinhs
            .Where(p => p.MaCaThi.HasValue && caThiIds.Contains(p.MaCaThi.Value) && p.DiemCuoiCung.HasValue)
            .AverageAsync(p => (decimal?)p.DiemCuoiCung, ct) ?? 0;

        return new ExamReportSummaryDto
        {
            TongSoKyThi = await kyThiQuery.CountAsync(ct),
            TongSoCaThi = caThiIds.Count,
            TongSoThiSinh = await _db.ThiSinhCaThis
                .Where(t => caThiIds.Contains(t.MaCaThi))
                .CountAsync(ct),
            SoThiSinhCoMat = diemDanhStats?.CoMat ?? 0,
            SoThiSinhVangMat = diemDanhStats?.VangMat ?? 0,
            TongSoViPham = tongViPham,
            TongSoBienBan = tongBienBan,
            DiemTrungBinh = Math.Round(diemTrungBinh, 2)
        };
    }

    public async Task<IReadOnlyList<DeKiemTraDto>> GetDeKiemTrasAsync(CancellationToken ct)
    {
        return await _db.DeKiemTras
            .Select(d => new DeKiemTraDto
            {
                MaDeKiemTra = d.MaDeKiemTra,
                MaMonHoc = d.MaMonHoc,
                TieuDe = d.TieuDe,
                ThoiGianPhut = d.ThoiGianPhut,
                TrangThai = d.TrangThai,
                LoaiDeThi = d.LoaiDeThi,
                HinhThucThi = d.HinhThucThi
            })
            .ToListAsync(ct);
    }

    // ===== Helpers =====

    private static int DetermineCurrentSemester(
        IReadOnlyCollection<MonHocTrongChuongTrinh> programSubjects,
        IReadOnlySet<int> completedSubjectIds,
        int totalSemesters)
    {
        if (!programSubjects.Any())
        {
            return totalSemesters > 0 ? 1 : 0;
        }

        var firstOpenSemester = programSubjects
            .GroupBy(subject => subject.HocKyDuKien)
            .OrderBy(group => group.Key)
            .FirstOrDefault(group => group.Any(subject => !completedSubjectIds.Contains(subject.MaMonHoc)))
            ?.Key;

        return firstOpenSemester ?? totalSemesters;
    }

    private static string ResolveStudentExamStatus(DeKiemTra exam, CaThi? caThi, PhienThiHocSinh? attempt)
    {
        if (attempt?.TrangThaiCongBo == "da_cong_bo" || exam.TrangThai == "da_cong_bo")
        {
            return "result_published";
        }

        if (caThi?.TrangThai == "dang_thi" || exam.TrangThai == "dang_mo")
        {
            return "open";
        }

        if (caThi?.TrangThai == "da_ket_thuc" || exam.TrangThai == "da_dong")
        {
            return "closed";
        }

        return exam.TrangThai == "nhap" ? "draft" : "scheduled";
    }

    private static string ResolveStudentExamAccessStatus(
        DeKiemTra exam,
        CaThi? caThi,
        PhienThiHocSinh? attempt,
        int plannedSemesterIndex,
        int currentSemesterIndex)
    {
        if (attempt?.TrangThaiCongBo == "da_cong_bo" || exam.TrangThai == "da_cong_bo")
        {
            return "completed";
        }

        if (caThi?.TrangThai == "dang_thi" || exam.TrangThai == "dang_mo")
        {
            return "official";
        }

        if (caThi?.TrangThai == "da_ket_thuc" || exam.TrangThai == "da_dong")
        {
            return "completed";
        }

        return "future_locked";
    }

    private static string ResolveExamTypeLabel(string? loaiDeThi)
    {
        return loaiDeThi switch
        {
            "trac_nghiem" => "Trắc nghiệm",
            "tu_luan" => "Tự luận",
            "ket_hop" => "Kết hợp",
            "quiz_bai_hoc" => "Quiz",
            _ => "Thi"
        };
    }

    private static PhienThiDto MapToPhienThiDto(PhienThiHocSinh p)
    {
        return new PhienThiDto
        {
            MaPhienThi = p.MaPhienThi,
            MaDeKiemTra = p.MaDeKiemTra,
            MaHocSinh = p.MaHocSinh,
            TenHocSinh = p.HocSinh?.HoTen,
            BatDauLuc = p.BatDauLuc,
            NopLuc = p.NopLuc,
            CauTraLoiJson = p.CauTraLoiJson,
            TrangThaiLuong = p.TrangThaiLuong,
            DiemTuDong = p.DiemTuDong,
            DiemCuoiCung = p.DiemCuoiCung,
            DiemTuLuanAiGoiY = p.DiemTuLuanAiGoiY,
            LanThu = p.LanThu,
            HanNopLuc = p.HanNopLuc,
            SoCauDung = p.SoCauDung,
            KetQuaDat = p.KetQuaDat,
            MaCaThi = p.MaCaThi,
            TrangThaiKyTen = p.TrangThaiKyTen,
            ThoiDiemKy = p.ThoiDiemKy,
            NguoiXacNhanKyTen = p.NguoiXacNhanKyTen,
            TrangThaiCongBo = p.TrangThaiCongBo
        };
    }

    private async Task SyncDiemThiToDiemSoAsync(IEnumerable<PhienThiHocSinh> phienThis, CancellationToken ct)
    {
        var phienThiIds = phienThis.Select(p => p.MaPhienThi).ToList();
        if (!phienThiIds.Any()) return;

        var examInfos = await _db.PhienThiHocSinhs
            .Where(x => phienThiIds.Contains(x.MaPhienThi))
            .Select(x => new
            {
                MaPhienThi = x.MaPhienThi,
                MaHocSinh = x.MaHocSinh,
                DiemCuoiCung = x.DiemCuoiCung,
                MaMonHoc = x.CaThi.LichThiTong.MaMonHoc,
                MaHocKy = x.CaThi.LichThiTong.KyThi.MaHocKy,
                LoaiKyThi = x.CaThi.LichThiTong.KyThi.LoaiKyThi
            })
            .ToListAsync(ct);

        foreach (var info in examInfos)
        {
            if (info.DiemCuoiCung == null) continue;

            var diemSo = await _db.DiemSos
                .FirstOrDefaultAsync(d => d.MaHocSinh == info.MaHocSinh &&
                                          d.MaMonHoc == info.MaMonHoc &&
                                          d.MaHocKy == info.MaHocKy, ct);

            if (diemSo != null)
            {
                if (diemSo.DaKhoa)
                {
                    _logger.LogWarning("Bỏ qua đồng bộ điểm thi cho học sinh {MaHocSinh} vì bảng điểm đã bị khóa.", info.MaHocSinh);
                    continue;
                }
            }
            else
            {
                var hs = await _db.NguoiDungs.FindAsync(new object[] { info.MaHocSinh }, ct);
                if (hs == null) continue;

                diemSo = new DiemSo
                {
                    MaDonVi = hs.MaDonVi,
                    MaHocSinh = info.MaHocSinh,
                    MaMonHoc = info.MaMonHoc,
                    MaHocKy = info.MaHocKy,
                    TrangThai = "dang_hoc",
                    DaKhoa = false,
                    NamNhapHoc = hs.NamNhapHoc ?? DateTime.UtcNow.Year
                };
                _db.DiemSos.Add(diemSo);
            }

            if (info.LoaiKyThi == "giua_ky")
                diemSo.DiemGiuaKy = info.DiemCuoiCung;
            else if (info.LoaiKyThi == "cuoi_ky")
                diemSo.DiemCuoiKy = info.DiemCuoiCung;
        }

        await _db.SaveChangesAsync(ct);
    }
}
