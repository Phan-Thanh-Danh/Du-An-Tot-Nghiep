using System.Security.Claims;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class RewardLifecycleService : IRewardLifecycleService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ICertificateGenerationService _certService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RewardLifecycleService(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        ICertificateGenerationService certService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditLogService = auditLogService;
        _certService = certService;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User 
            ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Không tìm thấy thông tin đăng nhập.");
    }

    private bool IsSuperAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole(AuthRoles.SuperAdmin);
    }

    private int? GetUserCampusId(ClaimsPrincipal user)
    {
        var campusClaim = user.FindFirst("MaDonVi")?.Value;
        if (int.TryParse(campusClaim, out var campusId))
            return campusId;
        return null;
    }

    private int GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var userId))
            return userId;
        throw new ApiException(StatusCodes.Status401Unauthorized, "User ID claim is missing or invalid.");
    }

    public async Task<PagedResultDto<AdminRewardListItemDto>> GetRewardsAsync(
        AdminRewardQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var query = _context.KhenThuongs.AsNoTracking();

        if (!IsSuperAdmin(user))
        {
            var campusId = GetUserCampusId(user);
            if (campusId == null)
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không thuộc cơ sở nào.");
            }
            query = query.Where(k => k.MaDonVi == campusId.Value);
        }

        if (parameters.MaDotKhenThuong.HasValue)
            query = query.Where(k => k.MaDotKhenThuong == parameters.MaDotKhenThuong.Value);
        
        if (parameters.MaHocKy.HasValue)
            query = query.Where(k => k.MaHocKy == parameters.MaHocKy.Value);
            
        if (parameters.MaHocSinh.HasValue)
            query = query.Where(k => k.MaHocSinh == parameters.MaHocSinh.Value);
            
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
            var kw = parameters.Keyword.ToLower();
            query = query.Where(k => 
                (k.HoTenSnapshot != null && k.HoTenSnapshot.ToLower().Contains(kw)) ||
                (k.MssvSnapshot != null && k.MssvSnapshot.ToLower().Contains(kw)) ||
                (k.DanhHieuSnapshot != null && k.DanhHieuSnapshot.ToLower().Contains(kw)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var pageIndex = Math.Max(1, parameters.PageNumber);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(k => k.MaKhenThuong)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(k => new AdminRewardListItemDto
            {
                MaKhenThuong = k.MaKhenThuong,
                MaDotKhenThuong = k.MaDotKhenThuong,
                MaHocSinh = k.MaHocSinh,
                HoTenSnapshot = k.HoTenSnapshot,
                MssvSnapshot = k.MssvSnapshot,
                TenHocKySnapshot = k.TenHocKySnapshot,
                DanhHieuSnapshot = k.DanhHieuSnapshot,
                XepHang = k.XepHang,
                DiemXet = k.DiemXet,
                TrangThai = k.TrangThai,
                HasCertificate = k.UrlPdfBangKhen != null && k.UrlPdfBangKhen != "",
                NgaySinhPdf = k.NgaySinhPdf,
                SoLanSinhPdf = k.SoLanSinhPdf,
                NgayDuyet = k.NgayCapNhat,
                NgayCap = k.NgayCap,
                MaDonVi = k.MaDonVi
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminRewardListItemDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<AdminRewardDetailDto> GetRewardDetailAsync(
        int rewardId,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var query = _context.KhenThuongs.AsNoTracking().Where(k => k.MaKhenThuong == rewardId);

        if (!IsSuperAdmin(user))
        {
            var campusId = GetUserCampusId(user);
            if (campusId == null)
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không thuộc cơ sở nào.");
            }
            query = query.Where(k => k.MaDonVi == campusId.Value);
        }

        var reward = await query.Select(k => new AdminRewardDetailDto
        {
            MaKhenThuong = k.MaKhenThuong,
            MaDotKhenThuong = k.MaDotKhenThuong,
            MaHocSinh = k.MaHocSinh,
            HoTenSnapshot = k.HoTenSnapshot,
            MssvSnapshot = k.MssvSnapshot,
            TenHocKySnapshot = k.TenHocKySnapshot,
            DanhHieuSnapshot = k.DanhHieuSnapshot,
            XepHang = k.XepHang,
            DiemXet = k.DiemXet,
            TrangThai = k.TrangThai,
            HasCertificate = k.UrlPdfBangKhen != null && k.UrlPdfBangKhen != "",
            NgaySinhPdf = k.NgaySinhPdf,
            SoLanSinhPdf = k.SoLanSinhPdf,
            NgayDuyet = k.NgayCapNhat,
            NgayCap = k.NgayCap,
            MaDonVi = k.MaDonVi,
            GpaDatDuoc = k.GpaDatDuoc,
            LoaiKhenThuong = k.LoaiKhenThuong,
            UrlChungTu = k.UrlChungTu,
            UrlPdfBangKhen = k.UrlPdfBangKhen,
            LoiSinhPdf = k.LoiSinhPdf,
            CapLuc = k.CapLuc,
            NgayCapNhat = k.NgayCapNhat,
            NguoiCap = k.NguoiCap,
            NguoiDuyet = k.NguoiDuyet,
            DaHuy = k.DaHuy,
            LyDoHuy = k.LyDoHuy,
            NguoiHuy = k.NguoiHuy,
            NgayHuy = k.NgayHuy,
            GhiChuHuy = k.GhiChuHuy,
            GhiChuVongDoi = k.GhiChuVongDoi
        }).FirstOrDefaultAsync(cancellationToken);

        if (reward == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy KhenThưởng hoặc bạn không có quyền xem.");
        }

        return reward;
    }

    public async Task<RewardLifecycleResultDto> CancelRewardAsync(
        int rewardId,
        CancelRewardRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (!IsSuperAdmin(user))
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quyền hủy khen thưởng.");

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(k => k.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");

        if (reward.DaHuy)
            throw new ApiException(StatusCodes.Status409Conflict, "Khen thưởng đã bị hủy trước đó.");

        var allowedCancelStatuses = new[] 
        { 
            RewardDisciplineConstants.RewardStatuses.Approved, 
            RewardDisciplineConstants.RewardStatuses.Issued, 
            RewardDisciplineConstants.RewardStatuses.PdfGenerated 
        };

        if (!allowedCancelStatuses.Contains(reward.TrangThai))
            throw new ApiException(StatusCodes.Status400BadRequest, $"Không thể hủy khen thưởng ở trạng thái '{reward.TrangThai}'.");

        var oldStatus = reward.TrangThai;

        reward.DaHuy = true;
        reward.TrangThai = RewardDisciplineConstants.RewardStatuses.Cancelled;
        reward.LyDoHuy = request.Reason;
        reward.NguoiHuy = GetUserId(user);
        reward.NgayHuy = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            reward.MaDonVi,
            "KhenThuong",
            reward.MaKhenThuong,
            RewardDisciplineConstants.RewardAuditActions.CancelReward,
            GetUserId(user),
            new { status = oldStatus },
            new { status = reward.TrangThai, reason = request.Reason },
            cancellationToken);

        return new RewardLifecycleResultDto
        {
            Success = true,
            Message = "Hủy khen thưởng thành công.",
            NewStatus = reward.TrangThai
        };
    }

    public async Task<RewardLifecycleResultDto> RestoreRewardAsync(
        int rewardId,
        RestoreRewardRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (!IsSuperAdmin(user))
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quyền khôi phục khen thưởng.");

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(k => k.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");

        if (!reward.DaHuy)
            throw new ApiException(StatusCodes.Status400BadRequest, "Khen thưởng chưa bị hủy, không thể khôi phục.");

        var oldStatus = reward.TrangThai;

        reward.DaHuy = false;
        reward.LyDoHuy = null;
        reward.NgayHuy = null;
        reward.NguoiHuy = null;

        if (!string.IsNullOrWhiteSpace(reward.UrlPdfBangKhen))
        {
            reward.TrangThai = RewardDisciplineConstants.RewardStatuses.PdfGenerated;
        }
        else
        {
            reward.TrangThai = RewardDisciplineConstants.RewardStatuses.Approved;
        }

        reward.GhiChuVongDoi = $"Được khôi phục lúc {DateTime.UtcNow:O}. Lý do: {request.Reason}\n{reward.GhiChuVongDoi}";

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            reward.MaDonVi,
            "KhenThuong",
            reward.MaKhenThuong,
            RewardDisciplineConstants.RewardAuditActions.RestoreReward,
            GetUserId(user),
            new { status = oldStatus },
            new { status = reward.TrangThai, reason = request.Reason },
            cancellationToken);

        return new RewardLifecycleResultDto
        {
            Success = true,
            Message = "Khôi phục khen thưởng thành công.",
            NewStatus = reward.TrangThai
        };
    }

    public async Task<RewardLifecycleResultDto> MarkIssuedAsync(
        int rewardId,
        MarkRewardIssuedRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (!IsSuperAdmin(user))
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quyền đánh dấu đã cấp.");

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(k => k.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");

        if (reward.DaHuy)
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể đánh dấu đã cấp cho khen thưởng đã hủy.");

        var allowedIssuedStatuses = new[] 
        { 
            RewardDisciplineConstants.RewardStatuses.Approved, 
            RewardDisciplineConstants.RewardStatuses.PdfGenerated 
        };

        if (!allowedIssuedStatuses.Contains(reward.TrangThai))
            throw new ApiException(StatusCodes.Status400BadRequest, $"Khen thưởng ở trạng thái '{reward.TrangThai}' không thể đánh dấu đã cấp.");

        var oldStatus = reward.TrangThai;

        reward.TrangThai = RewardDisciplineConstants.RewardStatuses.Issued;
        reward.NgayCap = request.IssuedAt ?? DateTime.UtcNow;
        reward.NguoiCap = GetUserId(user);

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            reward.GhiChuVongDoi = $"Đã cấp: {request.Note}\n{reward.GhiChuVongDoi}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.AddAsync(
            reward.MaDonVi,
            "KhenThuong",
            reward.MaKhenThuong,
            RewardDisciplineConstants.RewardAuditActions.MarkRewardIssued,
            GetUserId(user),
            new { status = oldStatus },
            new { status = reward.TrangThai, issuedAt = reward.NgayCap },
            cancellationToken);

        return new RewardLifecycleResultDto
        {
            Success = true,
            Message = "Đánh dấu đã cấp thành công.",
            NewStatus = reward.TrangThai
        };
    }

    public async Task<RewardLifecycleResultDto> RegenerateCertificateAsync(
        int rewardId,
        RegenerateSingleRewardCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        if (!IsSuperAdmin(user))
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được quyền sinh lại PDF.");

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(k => k.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");

        if (reward.DaHuy)
            throw new ApiException(StatusCodes.Status400BadRequest, "Không thể sinh lại PDF cho khen thưởng đã hủy.");

        if (!reward.MaDotKhenThuong.HasValue)
            throw new ApiException(StatusCodes.Status400BadRequest, "Khen thưởng không thuộc đợt nào, không thể dùng service sinh PDF chung.");

        var oldUrl = reward.UrlPdfBangKhen;

        var genRequest = new RegenerateRewardCertificatesRequest
        {
            RewardIds = new List<int> { rewardId },
            MaMauBangKhen = request.MaMauBangKhen,
            Reason = request.Reason
        };

        var result = await _certService.RegenerateAsync(reward.MaDotKhenThuong.Value, genRequest, cancellationToken);

        if (result.FailedCount > 0)
        {
            return new RewardLifecycleResultDto
            {
                Success = false,
                Message = "Sinh lại PDF thất bại. Vui lòng kiểm tra log lỗi trong chi tiết khen thưởng."
            };
        }

        await _context.Entry(reward).ReloadAsync(cancellationToken);

        await _auditLogService.AddAsync(
            reward.MaDonVi,
            "KhenThuong",
            reward.MaKhenThuong,
            RewardDisciplineConstants.RewardAuditActions.RegenerateSingleRewardCertificate,
            GetUserId(user),
            new { oldUrlPdf = oldUrl },
            new { newUrlPdf = reward.UrlPdfBangKhen, reason = request.Reason },
            cancellationToken);

        return new RewardLifecycleResultDto
        {
            Success = true,
            Message = "Sinh lại PDF thành công.",
            NewStatus = reward.TrangThai
        };
    }
}
