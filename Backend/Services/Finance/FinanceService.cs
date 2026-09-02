using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Finance;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Finance;

public class FinanceService : IFinanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FinanceService> _logger;

    public FinanceService(
        ApplicationDbContext context,
        ILogger<FinanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<HoaDonDto> Items, int Total)> GetInvoicesAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.HoaDons
            .AsNoTracking()
            .Where(hd => hd.MaDonVi == maDonVi)
            .OrderByDescending(hd => hd.NgayTao);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(hd => new HoaDonDto
            {
                MaHoaDon = hd.MaHoaDon,
                SoHoaDon = hd.MaHoaDonCode,
                MaHocSinh = hd.MaHocSinh,
                HoTenHocSinh = hd.HocSinh != null ? hd.HocSinh.HoTen : "",
                MaHocKy = hd.MaHocKy ?? 0,
                TenHocKy = hd.HocKy != null ? hd.HocKy.TenHocKy : "",
                SoTien = hd.SoTien,
                DaThu = hd.DaThanhToan,
                ConNo = hd.SoTien - hd.DaThanhToan,
                TrangThai = hd.TrangThai,
                NgayTao = hd.NgayTao,
                NgayThanhToan = hd.NgayCapNhat,
                GhiChu = hd.GhiChu ?? ""
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<HoaDonDetailDto?> GetInvoiceDetailAsync(
        int maHoaDon, int maDonVi,
        CancellationToken cancellationToken = default)
    {
        var hoaDon = await _context.HoaDons
            .AsNoTracking()
            .Include(hd => hd.HocSinh)
            .Include(hd => hd.HocKy)
            .Where(hd => hd.MaHoaDon == maHoaDon && hd.MaDonVi == maDonVi)
            .FirstOrDefaultAsync(cancellationToken);

        if (hoaDon == null)
            return null;

        var giaoDiches = await _context.GiaoDichs
            .AsNoTracking()
            .Include(gd => gd.TaiKhoanNhanTien)
            .Where(gd => gd.MaHoaDon == maHoaDon)
            .OrderByDescending(gd => gd.NgayTao)
            .Select(gd => new GiaoDichDto
            {
                MaGiaoDich = gd.MaGiaoDich,
                SoTien = gd.SoTien,
                LoaiGiaoDich = gd.LoaiGiaoDich,
                TrangThai = gd.TrangThai,
                NgayGiaoDich = gd.NgayTao,
                MaTaiKhoan = MaskAccountNumber(gd.TaiKhoanNhanTien != null ? gd.TaiKhoanNhanTien.SoTaiKhoan : ""),
                GhiChu = gd.ChuThich ?? gd.NoiDungChuyenKhoan ?? ""
            })
            .ToListAsync(cancellationToken);

        var dto = new HoaDonDetailDto
        {
            MaHoaDon = hoaDon.MaHoaDon,
            SoHoaDon = hoaDon.MaHoaDonCode,
            MaHocSinh = hoaDon.MaHocSinh,
            HoTenHocSinh = hoaDon.HocSinh?.HoTen ?? "",
            MaHocKy = hoaDon.MaHocKy ?? 0,
            TenHocKy = hoaDon.HocKy?.TenHocKy ?? "",
            SoTien = hoaDon.SoTien,
            DaThu = hoaDon.DaThanhToan,
            ConNo = hoaDon.SoTien - hoaDon.DaThanhToan,
            TrangThai = hoaDon.TrangThai,
            NgayTao = hoaDon.NgayTao,
            NgayThanhToan = hoaDon.NgayCapNhat,
            GhiChu = hoaDon.GhiChu ?? "",
            ChiTiets = new List<HoaDonChiTietDto>
            {
                new HoaDonChiTietDto
                {
                    MaChiTiet = 1,
                    TenKhoanHoc = hoaDon.GhiChu ?? $"Học phí kỳ {hoaDon.HocKy?.TenHocKy ?? ""}",
                    SoTien = hoaDon.SoTien,
                    GhiChu = hoaDon.LoaiHoaDon
                }
            },
            GiaoDiches = giaoDiches
        };

        return dto;
    }

    public async Task<FinanceMonitorDto> GetMonitorAsync(
        int maDonVi, DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        fromDate ??= now.AddMonths(-6);
        toDate ??= now.AddDays(1);

        var query = _context.HoaDons
            .AsNoTracking()
            .Where(hd => hd.MaDonVi == maDonVi
                      && hd.NgayTao >= fromDate
                      && hd.NgayTao <= toDate);

        var tongDoanhThu = await query
            .SumAsync(hd => (decimal?)hd.SoTien, cancellationToken) ?? 0m;

        var daThu = await query
            .SumAsync(hd => (decimal?)hd.DaThanhToan, cancellationToken) ?? 0m;

        var conNo = tongDoanhThu - daThu;

        var soHoaDonChuaThu = await query
            .CountAsync(hd => hd.TrangThai != "da_thanh_toan", cancellationToken);

        var soHoaDonQuaHan = await query
            .CountAsync(hd => hd.TrangThai != "da_thanh_toan" && hd.TrangThai != "da_huy", cancellationToken);

        var topDebtors = await _context.HoaDons
            .AsNoTracking()
            .Where(hd => hd.MaDonVi == maDonVi && hd.TrangThai != "da_thanh_toan" && (hd.SoTien - hd.DaThanhToan) > 0)
            .OrderByDescending(hd => hd.SoTien - hd.DaThanhToan)
            .Take(10)
            .Select(hd => new TopDebtorsDto
            {
                MaHocSinh = hd.MaHocSinh,
                HoTenHocSinh = hd.HocSinh != null ? hd.HocSinh.HoTen : "",
                ConNo = hd.SoTien - hd.DaThanhToan,
                SoNgayQuaHan = 0
            })
            .ToListAsync(cancellationToken);

        return new FinanceMonitorDto
        {
            TongDoanhThu = tongDoanhThu,
            DaThu = daThu,
            ConNo = conNo,
            SoHoaDonChuaThu = soHoaDonChuaThu,
            SoHoaDonQuaHan = soHoaDonQuaHan,
            DailyRevenue = new List<DailyRevenueDto>(),
            TopDebtors = topDebtors
        };
    }

    public async Task<(List<GiaoDichDto> Items, int Total)> GetTransactionsAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.GiaoDichs
            .AsNoTracking()
            .Include(gd => gd.HoaDon)
            .Include(gd => gd.TaiKhoanNhanTien)
            .Where(gd => gd.HoaDon != null && gd.HoaDon.MaDonVi == maDonVi)
            .OrderByDescending(gd => gd.NgayTao);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(gd => new GiaoDichDto
            {
                MaGiaoDich = gd.MaGiaoDich,
                SoTien = gd.SoTien,
                LoaiGiaoDich = gd.LoaiGiaoDich,
                TrangThai = gd.TrangThai,
                NgayGiaoDich = gd.NgayTao,
                MaTaiKhoan = MaskAccountNumber(gd.TaiKhoanNhanTien != null ? gd.TaiKhoanNhanTien.SoTaiKhoan : ""),
                GhiChu = gd.ChuThich ?? gd.NoiDungChuyenKhoan ?? ""
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<InvoiceDetailDto> CreateInvoiceAsync(
        int maDonVi, CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SoTien <= 0)
            throw new ArgumentException("Số tiền hóa đơn phải lớn hơn 0.");

        if (request.GiamTru < 0)
            throw new ArgumentException("Giảm trừ không được âm.");

        if (request.GiamTru > request.SoTien)
            throw new ArgumentException("Giảm trừ không thể lớn hơn số tiền hóa đơn.");

        var hocSinh = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MaNguoiDung == request.MaHocSinh, cancellationToken);

        if (hocSinh == null)
            throw new ArgumentException($"Không tìm thấy học sinh với mã {request.MaHocSinh}.");

        var hocKy = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(hk => hk.MaHocKy == request.MaHocKy, cancellationToken);

        if (hocKy == null)
            throw new ArgumentException($"Không tìm thấy học kỳ với mã {request.MaHocKy}.");

        var loaiHoaDon = string.IsNullOrWhiteSpace(request.LoaiHoaDon) ? "hoc_phi" : request.LoaiHoaDon;
        var hanThanhToan = request.HanThanhToan == default
            ? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
            : request.HanThanhToan;

        var exists = await _context.HoaDons.AnyAsync(
            hd => hd.MaHocSinh == request.MaHocSinh && hd.MaHocKy == request.MaHocKy && hd.LoaiHoaDon == loaiHoaDon,
            cancellationToken);
        if (exists)
            throw new ArgumentException($"Học sinh {request.MaHocSinh} đã có hóa đơn loại '{loaiHoaDon}' trong học kỳ {request.MaHocKy}.");

        var code = $"INV-{DateTime.UtcNow.Year}-{request.MaHocSinh}-{Random.Shared.Next(1000, 9999)}";
        while (await _context.HoaDons.AnyAsync(hd => hd.MaHoaDonCode == code, cancellationToken))
        {
            code = $"INV-{DateTime.UtcNow.Year}-{request.MaHocSinh}-{Random.Shared.Next(1000, 9999)}";
        }

        var hoaDon = new HoaDon
        {
            MaDonVi = maDonVi,
            MaHocSinh = request.MaHocSinh,
            MaHocKy = request.MaHocKy,
            MaHoaDonCode = code,
            LoaiHoaDon = loaiHoaDon,
            SoTien = request.SoTien,
            GiamTru = request.GiamTru,
            DaThanhToan = 0m,
            TrangThai = FinanceConstants.InvoiceStatuses.Unpaid,
            HanThanhToan = hanThanhToan,
            GhiChu = request.GhiChu,
            NgayTao = DateTime.UtcNow
        };

        _context.HoaDons.Add(hoaDon);
        await _context.SaveChangesAsync(cancellationToken);

        var audit = new NhatKyKiemToan
        {
            MaDonVi = maDonVi,
            LoaiDoiTuong = "HoaDon",
            MaDoiTuong = hoaDon.MaHoaDon.ToString(),
            HanhDong = "tao_hoa_don",
            GiaTriMoi = JsonSerializer.Serialize(new
            {
                hoaDon.MaHoaDon,
                hoaDon.MaHoaDonCode,
                hoaDon.MaHocSinh,
                hoaDon.MaHocKy,
                hoaDon.SoTien,
                hoaDon.GiamTru,
                hoaDon.TrangThai
            }),
            ThoiDiemThayDoi = DateTime.UtcNow,
            MoTa = $"Tạo hóa đơn mới {hoaDon.MaHoaDonCode} cho học sinh {hocSinh.HoTen} ({request.MaHocSinh})"
        };
        _context.NhatKyKiemToans.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        var donVi = await _context.DonVis.AsNoTracking().FirstOrDefaultAsync(d => d.MaDonVi == maDonVi, cancellationToken);

        return new InvoiceDetailDto
        {
            MaHoaDon = hoaDon.MaHoaDon,
            MaHoaDonCode = hoaDon.MaHoaDonCode,
            MaDonVi = hoaDon.MaDonVi,
            TenDonVi = donVi?.TenDonVi,
            MaHocSinh = hoaDon.MaHocSinh,
            HoTenHocSinh = hocSinh.HoTen,
            EmailHocSinh = hocSinh.Email,
            MaHocKy = hoaDon.MaHocKy,
            TenHocKy = hocKy.TenHocKy,
            LoaiHoaDon = hoaDon.LoaiHoaDon,
            SoTien = hoaDon.SoTien,
            GiamTru = hoaDon.GiamTru,
            DaThanhToan = hoaDon.DaThanhToan,
            TrangThai = hoaDon.TrangThai,
            HanThanhToan = hoaDon.HanThanhToan,
            NgayTao = hoaDon.NgayTao,
            GhiChu = hoaDon.GhiChu
        };
    }

    public async Task<bool> UpdateInvoiceStatusAsync(
        int maHoaDon, int maDonVi, UpdateInvoiceStatusRequest request,
        int maNguoiThucHien,
        CancellationToken cancellationToken = default)
    {
        var hoaDon = await _context.HoaDons
            .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon && hd.MaDonVi == maDonVi, cancellationToken);

        if (hoaDon == null)
            return false;

        var validStatuses = FinanceConstants.InvoiceStatuses.All;
        if (!validStatuses.Contains(request.TrangThaiMoi))
            throw new ArgumentException($"Trạng thái '{request.TrangThaiMoi}' không hợp lệ.");

        var oldStatus = hoaDon.TrangThai;
        var oldDaThanhToan = hoaDon.DaThanhToan;

        hoaDon.TrangThai = request.TrangThaiMoi;
        hoaDon.NgayCapNhat = DateTime.UtcNow;
        hoaDon.NguoiCapNhat = maNguoiThucHien;

        if (request.TrangThaiMoi == FinanceConstants.InvoiceStatuses.Paid)
        {
            hoaDon.DaThanhToan = hoaDon.SoTien - hoaDon.GiamTru;
        }
        else if (request.SoTienThanhToan.HasValue)
        {
            hoaDon.DaThanhToan = request.SoTienThanhToan.Value;
        }

        if (request.TrangThaiMoi == FinanceConstants.InvoiceStatuses.Canceled)
        {
            hoaDon.LyDoHuy = request.LyDo ?? "Hủy hóa đơn";
            hoaDon.NgayHuy = DateTime.UtcNow;
            hoaDon.NguoiHuy = maNguoiThucHien;
        }

        var audit = new NhatKyKiemToan
        {
            MaDonVi = maDonVi,
            LoaiDoiTuong = "HoaDon",
            MaDoiTuong = hoaDon.MaHoaDon.ToString(),
            HanhDong = "cap_nhat_hoa_don",
            GiaTriCu = JsonSerializer.Serialize(new { TrangThai = oldStatus, DaThanhToan = oldDaThanhToan }),
            GiaTriMoi = JsonSerializer.Serialize(new { TrangThai = hoaDon.TrangThai, DaThanhToan = hoaDon.DaThanhToan, LyDo = request.LyDo }),
            NguoiThayDoi = maNguoiThucHien,
            ThoiDiemThayDoi = DateTime.UtcNow,
            MoTa = $"Cập nhật trạng thái hóa đơn {hoaDon.MaHoaDonCode} từ '{oldStatus}' sang '{hoaDon.TrangThai}'. Lý do: {request.LyDo}"
        };
        _context.NhatKyKiemToans.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<RefundRequestDto> CreateRefundRequestAsync(
        int maDonVi, CreateRefundRequest request, int maNguoiTao,
        CancellationToken cancellationToken = default)
    {
        var hoaDon = await _context.HoaDons
            .Include(hd => hd.HocSinh)
            .FirstOrDefaultAsync(hd => hd.MaHoaDon == request.MaHoaDon && hd.MaDonVi == maDonVi, cancellationToken);

        if (hoaDon == null)
            throw new ArgumentException("Không tìm thấy hóa đơn hoặc không có quyền truy cập.");

        if (hoaDon.TrangThai == FinanceConstants.InvoiceStatuses.Canceled)
            throw new ArgumentException("Không thể tạo yêu cầu hoàn phí cho hóa đơn đã hủy.");

        if (request.SoTienYeuCau <= 0)
            throw new ArgumentException("Số tiền yêu cầu hoàn phí phải lớn hơn 0.");

        if (request.SoTienYeuCau > (hoaDon.SoTien - hoaDon.GiamTru))
            throw new ArgumentException("Số tiền yêu cầu hoàn phí vượt quá giá trị hóa đơn.");

        var loaiHoanPhi = request.LoaiHoanPhi switch
        {
            "hoan_toan" or "toan_phan" => FinanceConstants.RefundTypes.Full,
            "hoan_phan" or "mot_phan" => FinanceConstants.RefundTypes.Partial,
            "dieu_chinh" or "ghi_co" => FinanceConstants.RefundTypes.Credit,
            _ => FinanceConstants.RefundTypes.Partial
        };

        var yeuCau = new YeuCauHoanPhi
        {
            MaHoaDon = hoaDon.MaHoaDon,
            MaHocSinh = hoaDon.MaHocSinh,
            MaDonVi = maDonVi,
            SoTienYeuCau = request.SoTienYeuCau,
            LoaiHoanPhi = loaiHoanPhi,
            TrangThai = FinanceConstants.RefundRequestStatuses.PendingApproval,
            LyDoYeuCau = request.LyDoYeuCau,
            NguoiTao = maNguoiTao,
            NgayTao = DateTime.UtcNow
        };

        _context.YeuCauHoanPhis.Add(yeuCau);
        await _context.SaveChangesAsync(cancellationToken);

        var audit = new NhatKyKiemToan
        {
            MaDonVi = maDonVi,
            LoaiDoiTuong = "YeuCauHoanPhi",
            MaDoiTuong = yeuCau.MaHoanPhi.ToString(),
            HanhDong = "yeu_cau_hoan_phi",
            GiaTriMoi = JsonSerializer.Serialize(new
            {
                yeuCau.MaHoanPhi,
                yeuCau.MaHoaDon,
                yeuCau.SoTienYeuCau,
                yeuCau.LoaiHoanPhi,
                yeuCau.TrangThai,
                yeuCau.LyDoYeuCau
            }),
            NguoiThayDoi = maNguoiTao,
            ThoiDiemThayDoi = DateTime.UtcNow,
            MoTa = $"Tạo yêu cầu hoàn phí #{yeuCau.MaHoanPhi} cho hóa đơn {hoaDon.MaHoaDonCode}, số tiền {request.SoTienYeuCau:N0} đ. Lý do: {request.LyDoYeuCau}"
        };
        _context.NhatKyKiemToans.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        return new RefundRequestDto
        {
            MaHoanPhi = yeuCau.MaHoanPhi,
            MaHoaDon = yeuCau.MaHoaDon,
            SoHoaDon = hoaDon.MaHoaDonCode,
            MaHocSinh = yeuCau.MaHocSinh,
            HoTenHocSinh = hoaDon.HocSinh?.HoTen ?? "",
            SoTienYeuCau = yeuCau.SoTienYeuCau,
            LoaiHoanPhi = yeuCau.LoaiHoanPhi,
            TrangThai = yeuCau.TrangThai,
            LyDoYeuCau = yeuCau.LyDoYeuCau,
            NgayTao = yeuCau.NgayTao
        };
    }

    public async Task<(List<RefundRequestDto> Items, int Total)> GetRefundRequestsAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.YeuCauHoanPhis
            .AsNoTracking()
            .Include(y => y.HoaDon)
            .Include(y => y.HocSinh)
            .Include(y => y.NguoiDuyetNavigation)
            .Where(y => y.MaDonVi == maDonVi)
            .OrderByDescending(y => y.NgayTao);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(y => new RefundRequestDto
            {
                MaHoanPhi = y.MaHoanPhi,
                MaHoaDon = y.MaHoaDon,
                SoHoaDon = y.HoaDon != null ? y.HoaDon.MaHoaDonCode : "",
                MaHocSinh = y.MaHocSinh,
                HoTenHocSinh = y.HocSinh != null ? y.HocSinh.HoTen : "",
                SoTienYeuCau = y.SoTienYeuCau,
                LoaiHoanPhi = y.LoaiHoanPhi,
                TrangThai = y.TrangThai,
                LyDoYeuCau = y.LyDoYeuCau,
                LyDoTuChoi = y.LyDoTuChoi,
                NgayTao = y.NgayTao,
                NgayDuyet = y.XuLyLuc,
                NguoiDuyetTen = y.NguoiDuyetNavigation != null ? y.NguoiDuyetNavigation.HoTen : null
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ApproveRefundRequestAsync(
        int maHoanPhi, int maDonVi, ApproveRefundRequest request,
        int maNguoiDuyet,
        CancellationToken cancellationToken = default)
    {
        var yeuCau = await _context.YeuCauHoanPhis
            .Include(y => y.HoaDon)
            .FirstOrDefaultAsync(y => y.MaHoanPhi == maHoanPhi && y.MaDonVi == maDonVi, cancellationToken);

        if (yeuCau == null)
            return false;

        if (yeuCau.TrangThai != FinanceConstants.RefundRequestStatuses.PendingApproval && yeuCau.TrangThai != "dang_xu_ly")
            throw new ArgumentException("Yêu cầu hoàn phí đã được xử lý trước đó.");

        yeuCau.NguoiDuyet = maNguoiDuyet;
        yeuCau.XuLyLuc = DateTime.UtcNow;
        yeuCau.NgayCapNhat = DateTime.UtcNow;
        yeuCau.NguoiCapNhat = maNguoiDuyet;

        if (request.DuaVao)
        {
            yeuCau.TrangThai = FinanceConstants.RefundRequestStatuses.Approved;

            if (yeuCau.HoaDon != null)
            {
                if (yeuCau.LoaiHoanPhi == FinanceConstants.RefundTypes.Full || yeuCau.LoaiHoanPhi == "hoan_toan")
                {
                    yeuCau.HoaDon.TrangThai = FinanceConstants.InvoiceStatuses.Canceled;
                    yeuCau.HoaDon.LyDoHuy = request.LyDo ?? "Hoàn toàn bộ học phí";
                    yeuCau.HoaDon.NgayHuy = DateTime.UtcNow;
                    yeuCau.HoaDon.NguoiHuy = maNguoiDuyet;
                    yeuCau.HoaDon.NgayCapNhat = DateTime.UtcNow;
                    yeuCau.HoaDon.NguoiCapNhat = maNguoiDuyet;
                }
                else if (yeuCau.LoaiHoanPhi == FinanceConstants.RefundTypes.Partial || yeuCau.LoaiHoanPhi == "hoan_phan")
                {
                    yeuCau.HoaDon.DaThanhToan = Math.Max(0, yeuCau.HoaDon.DaThanhToan - yeuCau.SoTienYeuCau);
                    yeuCau.HoaDon.NgayCapNhat = DateTime.UtcNow;
                    yeuCau.HoaDon.NguoiCapNhat = maNguoiDuyet;
                }
            }
        }
        else
        {
            yeuCau.TrangThai = FinanceConstants.RefundRequestStatuses.Rejected;
            yeuCau.LyDoTuChoi = request.LyDo ?? "Từ chối yêu cầu hoàn phí";
        }

        var audit = new NhatKyKiemToan
        {
            MaDonVi = maDonVi,
            LoaiDoiTuong = "YeuCauHoanPhi",
            MaDoiTuong = yeuCau.MaHoanPhi.ToString(),
            HanhDong = request.DuaVao ? "duyet_hoan_phi" : "tu_choi_hoan_phi",
            GiaTriMoi = JsonSerializer.Serialize(new
            {
                yeuCau.MaHoanPhi,
                yeuCau.TrangThai,
                request.DuaVao,
                request.LyDo,
                yeuCau.LyDoTuChoi
            }),
            NguoiThayDoi = maNguoiDuyet,
            ThoiDiemThayDoi = DateTime.UtcNow,
            MoTa = $"{(request.DuaVao ? "Duyệt" : "Từ chối")} yêu cầu hoàn phí #{yeuCau.MaHoanPhi}. Lý do: {request.LyDo}"
        };
        _context.NhatKyKiemToans.Add(audit);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return "";
        var trimmed = accountNumber.Trim();
        if (trimmed.Length <= 4)
            return new string('*', trimmed.Length);
        return "****" + trimmed[^4..];
    }
}
