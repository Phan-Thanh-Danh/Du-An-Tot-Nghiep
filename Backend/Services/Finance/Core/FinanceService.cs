using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Finance;
using Backend.Exceptions;
using Backend.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Finance.Core;

public class FinanceService : IFinanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FinanceService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResultDto<InvoiceListItemDto>> GetInvoicesAsync(
        InvoiceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrgIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var pageIndex = Math.Max(1, parameters.PageIndex);

        var query = _context.HoaDons.AsNoTracking()
            .Include(h => h.DonVi)
            .Include(h => h.HocSinh)
            .Include(h => h.HocKy)
            .Where(h => allowedOrgIds.Contains(h.MaDonVi));

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrgIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền truy cập đơn vị này.");
            }
            query = query.Where(h => h.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            query = query.Where(h => h.TrangThai == parameters.TrangThai.Trim());
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiHoaDon))
        {
            query = query.Where(h => h.LoaiHoaDon == parameters.LoaiHoaDon.Trim());
        }

        if (parameters.MaHocKy.HasValue)
        {
            query = query.Where(h => h.MaHocKy == parameters.MaHocKy.Value);
        }

        if (parameters.MaHocSinh.HasValue)
        {
            query = query.Where(h => h.MaHocSinh == parameters.MaHocSinh.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var kw = parameters.Keyword.Trim().ToLower();
            query = query.Where(h =>
                h.MaHoaDonCode.ToLower().Contains(kw) ||
                (h.HocSinh != null && (h.HocSinh.HoTen.ToLower().Contains(kw) || h.HocSinh.Email.ToLower().Contains(kw))));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(h => h.NgayTao)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new InvoiceListItemDto
            {
                MaHoaDon = h.MaHoaDon,
                MaHoaDonCode = h.MaHoaDonCode,
                MaDonVi = h.MaDonVi,
                TenDonVi = h.DonVi != null ? h.DonVi.TenDonVi : null,
                MaHocSinh = h.MaHocSinh,
                HoTenHocSinh = h.HocSinh != null ? h.HocSinh.HoTen : null,
                EmailHocSinh = h.HocSinh != null ? h.HocSinh.Email : null,
                MaHocKy = h.MaHocKy,
                TenHocKy = h.HocKy != null ? h.HocKy.TenHocKy : null,
                LoaiHoaDon = h.LoaiHoaDon,
                SoTien = h.SoTien,
                GiamTru = h.GiamTru,
                DaThanhToan = h.DaThanhToan,
                TrangThai = h.TrangThai,
                HanThanhToan = h.HanThanhToan,
                NgayTao = h.NgayTao,
                NgayCapNhat = h.NgayCapNhat
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<InvoiceListItemDto>
        {
            Items = items,
            TotalItems = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<InvoiceDetailDto> GetInvoiceByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrgIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var invoice = await _context.HoaDons.AsNoTracking()
            .Include(h => h.DonVi)
            .Include(h => h.HocSinh)
            .Include(h => h.HocKy)
            .Include(h => h.NguoiTaoNavigation)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id, cancellationToken);

        if (invoice == null || !allowedOrgIds.Contains(invoice.MaDonVi))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hóa đơn hoặc không có quyền truy cập.");
        }

        var transactions = await _context.GiaoDichs.AsNoTracking()
            .Include(g => g.NguoiThucHien)
            .Where(g => g.MaHoaDon == id)
            .OrderByDescending(g => g.NgayTao)
            .Select(g => new TransactionSummaryDto
            {
                MaGiaoDich = g.MaGiaoDich,
                MaThamChieuNoiBo = g.MaThamChieuNoiBo,
                MaThamChieuCong = g.MaThamChieuCong,
                SoTien = g.SoTien,
                LoaiGiaoDich = g.LoaiGiaoDich,
                TrangThai = g.TrangThai,
                NhaCungCapThanhToan = g.NhaCungCapThanhToan,
                NoiDungChuyenKhoan = g.NoiDungChuyenKhoan,
                NgayTao = g.NgayTao,
                NgayThanhToan = g.NgayThanhToan,
                TenNguoiThucHien = g.NguoiThucHien != null ? g.NguoiThucHien.HoTen : null
            })
            .ToListAsync(cancellationToken);

        return new InvoiceDetailDto
        {
            MaHoaDon = invoice.MaHoaDon,
            MaHoaDonCode = invoice.MaHoaDonCode,
            MaDonVi = invoice.MaDonVi,
            TenDonVi = invoice.DonVi?.TenDonVi,
            MaHocSinh = invoice.MaHocSinh,
            HoTenHocSinh = invoice.HocSinh?.HoTen,
            EmailHocSinh = invoice.HocSinh?.Email,
            MaHocKy = invoice.MaHocKy,
            TenHocKy = invoice.HocKy?.TenHocKy,
            LoaiHoaDon = invoice.LoaiHoaDon,
            SoTien = invoice.SoTien,
            GiamTru = invoice.GiamTru,
            DaThanhToan = invoice.DaThanhToan,
            TrangThai = invoice.TrangThai,
            HanThanhToan = invoice.HanThanhToan,
            UrlHoaDonPdf = invoice.UrlHoaDonPdf,
            GhiChu = invoice.GhiChu,
            LyDoHuy = invoice.LyDoHuy,
            NgayTao = invoice.NgayTao,
            NgayCapNhat = invoice.NgayCapNhat,
            NgayHuy = invoice.NgayHuy,
            NguoiTao = invoice.NguoiTao,
            TenNguoiTao = invoice.NguoiTaoNavigation?.HoTen,
            Transactions = transactions
        };
    }

    public async Task<PagedResultDto<TransactionListItemDto>> GetTransactionsAsync(
        TransactionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrgIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var pageIndex = Math.Max(1, parameters.PageIndex);

        var query = _context.GiaoDichs.AsNoTracking()
            .Include(g => g.HoaDon)
                .ThenInclude(h => h!.DonVi)
            .Include(g => g.HoaDon)
                .ThenInclude(h => h!.HocSinh)
            .Include(g => g.TaiKhoanNhanTien)
            .Where(g => g.HoaDon != null && allowedOrgIds.Contains(g.HoaDon.MaDonVi));

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrgIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền truy cập đơn vị này.");
            }
            query = query.Where(g => g.HoaDon != null && g.HoaDon.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.MaHoaDon.HasValue)
        {
            query = query.Where(g => g.MaHoaDon == parameters.MaHoaDon.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            query = query.Where(g => g.TrangThai == parameters.TrangThai.Trim());
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiGiaoDich))
        {
            query = query.Where(g => g.LoaiGiaoDich == parameters.LoaiGiaoDich.Trim());
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(g => g.NgayTao >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(g => g.NgayTao <= parameters.ToDate.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(g => g.NgayTao)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new TransactionListItemDto
            {
                MaGiaoDich = g.MaGiaoDich,
                MaHoaDon = g.MaHoaDon,
                MaHoaDonCode = g.HoaDon != null ? g.HoaDon.MaHoaDonCode : null,
                MaDonVi = g.HoaDon != null ? g.HoaDon.MaDonVi : 0,
                TenDonVi = g.HoaDon != null && g.HoaDon.DonVi != null ? g.HoaDon.DonVi.TenDonVi : null,
                MaHocSinh = g.HoaDon != null ? g.HoaDon.MaHocSinh : null,
                HoTenHocSinh = g.HoaDon != null && g.HoaDon.HocSinh != null ? g.HoaDon.HocSinh.HoTen : null,
                MaThamChieuNoiBo = g.MaThamChieuNoiBo,
                MaThamChieuCong = g.MaThamChieuCong,
                SoTien = g.SoTien,
                LoaiGiaoDich = g.LoaiGiaoDich,
                TrangThai = g.TrangThai,
                NhaCungCapThanhToan = g.NhaCungCapThanhToan,
                NoiDungChuyenKhoan = g.NoiDungChuyenKhoan,
                SoTaiKhoanMasked = g.TaiKhoanNhanTien != null ? FinanceMaskHelper.MaskAccountNumber(g.TaiKhoanNhanTien.SoTaiKhoan) : null,
                TenNganHang = g.TaiKhoanNhanTien != null ? g.TaiKhoanNhanTien.TenNganHang : null,
                NgayTao = g.NgayTao,
                NgayThanhToan = g.NgayThanhToan,
                ChuThich = g.ChuThich
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TransactionListItemDto>
        {
            Items = items,
            TotalItems = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<List<PaymentAccountDto>> GetPaymentAccountsAsync(
        int? maDonVi = null,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrgIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);

        var query = _context.TaiKhoanNhanTiens.AsNoTracking()
            .Include(t => t.DonVi)
            .Where(t => allowedOrgIds.Contains(t.MaDonVi));

        if (maDonVi.HasValue)
        {
            if (!allowedOrgIds.Contains(maDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền truy cập đơn vị này.");
            }
            query = query.Where(t => t.MaDonVi == maDonVi.Value);
        }

        if (currentUser.Role != AuthRoles.SuperAdmin && currentUser.Role != AuthRoles.FinanceAdmin)
        {
            query = query.Where(t => t.ConHoatDong);
        }

        var accounts = await query
            .OrderByDescending(t => t.LaMacDinh)
            .ThenBy(t => t.TenNganHang)
            .Select(t => new PaymentAccountDto
            {
                MaTaiKhoanNhanTien = t.MaTaiKhoanNhanTien,
                MaDonVi = t.MaDonVi,
                TenDonVi = t.DonVi != null ? t.DonVi.TenDonVi : null,
                TenNganHang = t.TenNganHang,
                MaNganHang = t.MaNganHang,
                SoTaiKhoanMasked = FinanceMaskHelper.MaskAccountNumber(t.SoTaiKhoan),
                TenChuTaiKhoan = t.TenChuTaiKhoan,
                ChiNhanh = t.ChiNhanh,
                NhaCungCapThanhToan = t.NhaCungCapThanhToan,
                TrangThaiDuyet = t.TrangThaiDuyet,
                LaMacDinh = t.LaMacDinh,
                ConHoatDong = t.ConHoatDong,
                NgayTao = t.NgayTao,
                NgayDuyet = t.NgayDuyet
            })
            .ToListAsync(cancellationToken);

        return accounts;
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser == null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Người dùng chưa đăng nhập.");
        }
        return currentUser;
    }

    private async Task<List<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin || currentUser.Role == AuthRoles.FinanceAdmin)
        {
            return await _context.DonVis.AsNoTracking()
                .Select(d => d.MaDonVi)
                .ToListAsync(cancellationToken);
        }

        var directCampusId = currentUser.CampusId;
        var subCampuses = await _context.DonVis.AsNoTracking()
            .Where(d => d.MaDonViCha == directCampusId)
            .Select(d => d.MaDonVi)
            .ToListAsync(cancellationToken);

        subCampuses.Add(directCampusId);
        return subCampuses.Distinct().ToList();
    }
}
