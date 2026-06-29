using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class StudentRewardService : IStudentRewardService
{
    private readonly ApplicationDbContext _context;
    private readonly ICertificatePdfStorageService _storage;

    /// <summary>
    /// Reward statuses visible to students. Internal statuses like Draft, PdfFailed are hidden.
    /// </summary>
    private static readonly HashSet<string> StudentVisibleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        RewardDisciplineConstants.RewardStatuses.Approved,
        RewardDisciplineConstants.RewardStatuses.Issued,
        RewardDisciplineConstants.RewardStatuses.PdfGenerated
    };

    public StudentRewardService(
        ApplicationDbContext context,
        ICertificatePdfStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<PagedResultDto<StudentRewardListItemDto>> GetMyRewardsAsync(
        int studentUserId,
        StudentRewardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.KhenThuongs.AsNoTracking()
            .Where(k => k.MaHocSinh == studentUserId
                        && !k.DaHuy
                        && StudentVisibleStatuses.Contains(k.TrangThai));

        if (parameters.MaHocKy.HasValue)
            query = query.Where(k => k.MaHocKy == parameters.MaHocKy.Value);

        if (!string.IsNullOrWhiteSpace(parameters.LoaiKhenThuong))
            query = query.Where(k => k.LoaiKhenThuong == parameters.LoaiKhenThuong);

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
            query = query.Where(k => k.TrangThai == parameters.TrangThai);

        if (parameters.HasCertificate.HasValue)
        {
            if (parameters.HasCertificate.Value)
                query = query.Where(k => k.UrlPdfBangKhen != null && k.UrlPdfBangKhen != "");
            else
                query = query.Where(k => k.UrlPdfBangKhen == null || k.UrlPdfBangKhen == "");
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var kw = parameters.Keyword;
            query = query.Where(k =>
                (k.DanhHieuSnapshot != null && k.DanhHieuSnapshot.Contains(kw)) ||
                (k.TenHocKySnapshot != null && k.TenHocKySnapshot.Contains(kw)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(k => k.CapLuc)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(k => new StudentRewardListItemDto
            {
                MaKhenThuong = k.MaKhenThuong,
                MaDotKhenThuong = k.MaDotKhenThuong,
                LoaiKhenThuong = k.LoaiKhenThuong,
                TenLoaiKhenThuong = MapRewardTypeName(k.LoaiKhenThuong),
                DanhHieuSnapshot = k.DanhHieuSnapshot,
                TenHocKySnapshot = k.TenHocKySnapshot,
                XepHang = k.XepHang,
                DiemXet = k.DiemXet,
                GpaHocKy = k.GpaDatDuoc,
                NgayDuyet = k.NgayCapNhat,
                HasCertificate = k.UrlPdfBangKhen != null && k.UrlPdfBangKhen != "",
                TrangThai = k.TrangThai
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<StudentRewardListItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize
        };
    }

    public async Task<StudentRewardDetailDto> GetMyRewardByIdAsync(
        int studentUserId,
        int rewardId,
        CancellationToken cancellationToken = default)
    {
        var reward = await _context.KhenThuongs.AsNoTracking()
            .Where(k => k.MaKhenThuong == rewardId
                        && k.MaHocSinh == studentUserId
                        && !k.DaHuy
                        && StudentVisibleStatuses.Contains(k.TrangThai))
            .Select(k => new StudentRewardDetailDto
            {
                MaKhenThuong = k.MaKhenThuong,
                MaDotKhenThuong = k.MaDotKhenThuong,
                LoaiKhenThuong = k.LoaiKhenThuong,
                TenLoaiKhenThuong = MapRewardTypeName(k.LoaiKhenThuong),
                DanhHieuSnapshot = k.DanhHieuSnapshot,
                TenHocKySnapshot = k.TenHocKySnapshot,
                XepHang = k.XepHang,
                DiemXet = k.DiemXet,
                GpaHocKy = k.GpaDatDuoc,
                NgayDuyet = k.NgayCapNhat,
                HasCertificate = k.UrlPdfBangKhen != null && k.UrlPdfBangKhen != "",
                TrangThai = k.TrangThai,
                HoTenSnapshot = k.HoTenSnapshot,
                MssvSnapshot = k.MssvSnapshot,
                MaHocKy = k.MaHocKy,
                MaDonVi = k.MaDonVi,
                CapLuc = k.CapLuc,
                NgayCapNhat = k.NgayCapNhat
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (reward is null)
        {
            // Return 404 for both "not found" and "belongs to someone else" to avoid data leakage
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");
        }

        return reward;
    }

    public async Task<RewardCertificateDownloadDto> DownloadMyCertificateAsync(
        int studentUserId,
        int rewardId,
        CancellationToken cancellationToken = default)
    {
        var reward = await _context.KhenThuongs.AsNoTracking()
            .FirstOrDefaultAsync(k => k.MaKhenThuong == rewardId
                                      && k.MaHocSinh == studentUserId,
                cancellationToken);

        if (reward is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");
        }

        if (reward.DaHuy)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Khen thưởng đã bị hủy, không thể tải bằng khen.");
        }

        if (!StudentVisibleStatuses.Contains(reward.TrangThai))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");
        }

        if (string.IsNullOrWhiteSpace(reward.UrlPdfBangKhen))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Bằng khen chưa có file PDF.");
        }

        var file = await _storage.TryReadAsync(reward.UrlPdfBangKhen, cancellationToken);
        if (file is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy file PDF bằng khen.");
        }

        return new RewardCertificateDownloadDto
        {
            Content = file.Content,
            FileName = file.FileName,
            ContentType = "application/pdf"
        };
    }

    private static string MapRewardTypeName(string loaiKhenThuong)
    {
        return loaiKhenThuong switch
        {
            RewardDisciplineConstants.RewardTypes.Top100Semester => "Top 100 Học kỳ",
            RewardDisciplineConstants.RewardTypes.AcademicLegacy => "Học lực",
            RewardDisciplineConstants.RewardTypes.SpecialLegacy => "Đặc biệt",
            RewardDisciplineConstants.RewardTypes.CompetitionLegacy => "Thi đấu",
            RewardDisciplineConstants.RewardTypes.Other => "Khác",
            _ => loaiKhenThuong
        };
    }
}
