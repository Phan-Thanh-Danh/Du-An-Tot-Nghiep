using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Exam;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Exam;

public class ExamService : IExamService
{
    private readonly ApplicationDbContext _db;

    public ExamService(ApplicationDbContext db)
    {
        _db = db;
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
        }

        await _db.SaveChangesAsync(ct);
        return await GetDiemDanhByCaThiAsync(request.MaCaThi, ct);
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

    // ===== Grading =====

    public async Task FinalizeAutoGradeAsync(int maCaThi, CancellationToken ct)
    {
        var phienThis = await _db.PhienThiHocSinhs
            .Where(p => p.MaCaThi == maCaThi && p.TrangThaiLuong == "da_dung")
            .ToListAsync(ct);

        foreach (var p in phienThis)
        {
            // Mock auto-grading: parse answers and score MCQ
            if (p.DiemTuDong == null)
            {
                // Simple mock: random score between 3 and 9 for demo
                p.DiemTuDong = Math.Round((decimal)(new Random().NextDouble() * 6 + 3), 2);
            }
            p.TrangThaiCongBo = "da_cham_xong";
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

        // Mock AI grading suggestion
        if (phienThi.DiemTuLuanAiGoiY == null)
        {
            phienThi.DiemTuLuanAiGoiY = Math.Round(request.DiemCuoiCung * 0.9m + 0.5m, 2);
            if (phienThi.DiemTuLuanAiGoiY > 10) phienThi.DiemTuLuanAiGoiY = 10;
        }

        await _db.SaveChangesAsync(ct);

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
            MaCaThi = p.MaCaThi,
            TrangThaiKyTen = p.TrangThaiKyTen,
            ThoiDiemKy = p.ThoiDiemKy,
            NguoiXacNhanKyTen = p.NguoiXacNhanKyTen,
            TrangThaiCongBo = p.TrangThaiCongBo
        };
    }
}
