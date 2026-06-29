using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Notifications;
using Backend.Exceptions;
using Backend.Services.Audit;
using Backend.Services.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class RewardDisciplineNotificationService : IRewardDisciplineNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RewardDisciplineNotificationService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<bool> ShouldSkipNotificationAsync(string refType, int refId, string eventCode, CancellationToken cancellationToken)
    {
        return await _context.ThongBaos.AnyAsync(x => 
            x.LoaiDoiTuongLienKet == refType && 
            x.MaDoiTuongLienKet == refId && 
            x.LoaiSuKien == eventCode, cancellationToken);
    }

    private int? GetCurrentUserId()
    {
        var item = _httpContextAccessor.HttpContext?.Items["CurrentUser"];
        if (item != null && item.GetType().GetProperty("UserId")?.GetValue(item) is int userId)
        {
            return userId;
        }
        return null;
    }

    // --- REWARD ---

    public async Task NotifyRewardApprovedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.Reward, rewardId, RewardDisciplineConstants.NotificationEvents.RewardApproved, cancellationToken))
            return;

        var reward = await _context.KhenThuongs
            .Include(x => x.DotKhenThuong)
            .FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);

        if (reward == null ) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Bạn đã được phê duyệt khen thưởng",
            TomTat = "Khen thưởng của bạn đã được ban giám hiệu phê duyệt.",
            NoiDungText = $"Khen thưởng danh hiệu {reward.DanhHieuSnapshot} thuộc đợt {reward.DotKhenThuong?.TenDot} đã được duyệt. Hãy chờ thông báo nhận bằng khen.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardApproved,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
            MaDoiTuongLienKet = rewardId,
            DuongDan = $"/student/rewards/{rewardId}",
            MaDonVi = reward.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyRewardCertificateReadyAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.Reward, rewardId, RewardDisciplineConstants.NotificationEvents.RewardCertificateReady, cancellationToken))
            return;

        var reward = await _context.KhenThuongs
            .Include(x => x.DotKhenThuong)
            .FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);

        if (reward == null ) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Bằng khen của bạn đã sẵn sàng",
            TomTat = "Bạn có thể tải xuống bằng khen định dạng PDF.",
            NoiDungText = $"Bằng khen cho danh hiệu {reward.DanhHieuSnapshot} thuộc đợt {reward.DotKhenThuong?.TenDot} đã sẵn sàng. Vui lòng vào chi tiết để tải xuống.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardCertificateReady,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
            MaDoiTuongLienKet = rewardId,
            DuongDan = $"/student/rewards/{rewardId}",
            MaDonVi = reward.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyRewardIssuedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.Reward, rewardId, RewardDisciplineConstants.NotificationEvents.RewardIssued, cancellationToken))
            return;

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null ) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Khen thưởng của bạn đã được cấp phát",
            TomTat = "Khen thưởng đã chính thức được cấp phát thành công.",
            NoiDungText = $"Chúc mừng! Khen thưởng {reward.DanhHieuSnapshot} của bạn đã được cấp phát chính thức.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Important,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardIssued,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
            MaDoiTuongLienKet = rewardId,
            DuongDan = $"/student/rewards/{rewardId}",
            MaDonVi = reward.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyRewardCanceledAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.Reward, rewardId, RewardDisciplineConstants.NotificationEvents.RewardCanceled, cancellationToken))
            return;

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null ) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Khen thưởng đã bị hủy",
            TomTat = "Hồ sơ khen thưởng của bạn đã bị hủy.",
            NoiDungText = $"Hồ sơ khen thưởng {reward.DanhHieuSnapshot} của bạn đã bị hủy bỏ bởi ban quản trị.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Warning,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardCanceled,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
            MaDoiTuongLienKet = rewardId,
            DuongDan = $"/student/rewards/{rewardId}",
            MaDonVi = reward.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyRewardRestoredAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.Reward, rewardId, RewardDisciplineConstants.NotificationEvents.RewardRestored, cancellationToken))
            return;

        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null ) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Khen thưởng đã được khôi phục",
            TomTat = "Hồ sơ khen thưởng của bạn đã được khôi phục lại.",
            NoiDungText = $"Hồ sơ khen thưởng {reward.DanhHieuSnapshot} của bạn đã được khôi phục trạng thái hoạt động.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardRestored,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
            MaDoiTuongLienKet = rewardId,
            DuongDan = $"/student/rewards/{rewardId}",
            MaDonVi = reward.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
    }

    // --- DISCIPLINE ---

    public async Task NotifyDisciplineActivatedAsync(int recordId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord, recordId, RewardDisciplineConstants.NotificationEvents.DisciplineActivated, cancellationToken))
            return;

        var record = await _context.HoSoKyLuats.FirstOrDefaultAsync(x => x.MaKyLuat == recordId, cancellationToken);
        if (record == null) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Quyết định kỷ luật đã có hiệu lực",
            TomTat = "Hồ sơ kỷ luật của bạn đã bắt đầu có hiệu lực.",
            NoiDungText = $"Hồ sơ kỷ luật: {record.TieuDe} (Hình thức: {record.HinhThucXuLy}) đã chính thức có hiệu lực.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Important,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineActivated,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord,
            MaDoiTuongLienKet = recordId,
            DuongDan = $"/student/disciplines/{recordId}",
            MaDonVi = record.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { record.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyDisciplineExpiredAsync(int recordId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord, recordId, RewardDisciplineConstants.NotificationEvents.DisciplineExpired, cancellationToken))
            return;

        var record = await _context.HoSoKyLuats.FirstOrDefaultAsync(x => x.MaKyLuat == recordId, cancellationToken);
        if (record == null) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Quyết định kỷ luật đã hết hiệu lực",
            TomTat = "Thời gian áp dụng kỷ luật của bạn đã kết thúc.",
            NoiDungText = $"Hồ sơ kỷ luật: {record.TieuDe} của bạn đã hết hạn hiệu lực.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineExpired,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord,
            MaDoiTuongLienKet = recordId,
            DuongDan = $"/student/disciplines/{recordId}",
            MaDonVi = record.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { record.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyDisciplineEffectRemovedAsync(int recordId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord, recordId, RewardDisciplineConstants.NotificationEvents.DisciplineEffectRemoved, cancellationToken))
            return;

        var record = await _context.HoSoKyLuats.FirstOrDefaultAsync(x => x.MaKyLuat == recordId, cancellationToken);
        if (record == null) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Hồ sơ kỷ luật đã được gỡ hiệu lực",
            TomTat = "Quyết định kỷ luật của bạn đã được gỡ bỏ hiệu lực.",
            NoiDungText = $"Hồ sơ kỷ luật: {record.TieuDe} đã được ban quản trị gỡ hiệu lực.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineEffectRemoved,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord,
            MaDoiTuongLienKet = recordId,
            DuongDan = $"/student/disciplines/{recordId}",
            MaDonVi = record.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { record.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyDisciplineVoidedAsync(int recordId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord, recordId, RewardDisciplineConstants.NotificationEvents.DisciplineVoided, cancellationToken))
            return;

        var record = await _context.HoSoKyLuats.FirstOrDefaultAsync(x => x.MaKyLuat == recordId, cancellationToken);
        if (record == null) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Hồ sơ kỷ luật đã bị hủy",
            TomTat = "Hồ sơ kỷ luật của bạn đã bị hủy bỏ.",
            NoiDungText = $"Hồ sơ kỷ luật: {record.TieuDe} đã bị hủy bỏ bởi ban quản trị.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineVoided,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord,
            MaDoiTuongLienKet = recordId,
            DuongDan = $"/student/disciplines/{recordId}",
            MaDonVi = record.MaDonVi,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { record.MaHocSinh }, cancellationToken);
    }

    public async Task NotifyDisciplineAppealSubmittedAsync(int appealId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineAppeal, appealId, RewardDisciplineConstants.NotificationEvents.DisciplineAppealSubmitted, cancellationToken))
            return;

        var appeal = await _context.KhieuNaiKyLuats
            .Include(x => x.HoSoKyLuat)
            .FirstOrDefaultAsync(x => x.MaKhieuNaiKyLuat == appealId, cancellationToken);
        
        if (appeal == null) return;

        var req = new SystemNotificationRequest
        {
            TieuDe = "Có khiếu nại kỷ luật mới",
            TomTat = $"Sinh viên đã gửi khiếu nại cho hồ sơ kỷ luật #{appeal.MaHoSoKyLuat}",
            NoiDungText = $"Vui lòng xem xét khiếu nại của sinh viên đối với hồ sơ kỷ luật: {appeal.HoSoKyLuat?.TieuDe}.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Warning,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineAppealSubmitted,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineAppeal,
            MaDoiTuongLienKet = appealId,
            DuongDan = $"/admin/disciplines/appeals/{appealId}",
            MaDonVi = appeal.MaDonVi ?? 0,
            NguoiTao = appeal.MaHocSinh
        };

        // Gửi cho Admin scope (CampusAdmin)
        await _notificationService.SendToCampusAsync(req, appeal.MaDonVi ?? 0, cancellationToken);
    }

    public async Task NotifyDisciplineAppealResolvedAsync(int appealId, CancellationToken cancellationToken = default)
    {
        if (await ShouldSkipNotificationAsync(RewardDisciplineConstants.NotificationRefTypes.DisciplineAppeal, appealId, RewardDisciplineConstants.NotificationEvents.DisciplineAppealResolved, cancellationToken))
            return;

        var appeal = await _context.KhieuNaiKyLuats
            .Include(x => x.HoSoKyLuat)
            .FirstOrDefaultAsync(x => x.MaKhieuNaiKyLuat == appealId, cancellationToken);
        
        if (appeal == null) return;

        string decisionStr = appeal.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Accepted ? "ĐÃ ĐƯỢC CHẤP NHẬN" : "BỊ TỪ CHỐI";

        var req = new SystemNotificationRequest
        {
            TieuDe = "Kết quả khiếu nại kỷ luật",
            TomTat = $"Khiếu nại của bạn {decisionStr}.",
            NoiDungText = $"Khiếu nại đối với hồ sơ kỷ luật: {appeal.HoSoKyLuat?.TieuDe} {decisionStr}. Vui lòng xem chi tiết để biết thêm.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineAppealResolved,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineAppeal,
            MaDoiTuongLienKet = appealId,
            DuongDan = $"/student/disciplines/appeals/{appealId}",
            MaDonVi = appeal.MaDonVi ?? 0,
            NguoiTao = GetCurrentUserId()
        };

        await _notificationService.SendToUsersAsync(req, new[] { appeal.MaHocSinh }, cancellationToken);
    }

    // --- MANUAL RESEND ---

    public async Task ResendRewardNotificationAsync(int rewardId, string reason, CancellationToken cancellationToken = default)
    {
        var reward = await _context.KhenThuongs.FirstOrDefaultAsync(x => x.MaKhenThuong == rewardId, cancellationToken);
        if (reward == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khen thưởng.");
        
        var userId = GetCurrentUserId();
        await _auditLogService.LogAsync(
            "KhenThuong", rewardId.ToString(),
            RewardDisciplineConstants.RewardAuditActions.ResendRewardNotification,
            null, new { Reason = reason },
            userId ?? 0, reward.MaDonVi, "Gửi lại thông báo khen thưởng thủ công.", cancellationToken);

        // Forced resend (ignore idempotency check)
        if (true)
        {
            var req = new SystemNotificationRequest
            {
                TieuDe = "Thông báo Khen thưởng (Gửi lại)",
                TomTat = "Ban quản trị gửi lại thông báo khen thưởng.",
                NoiDungText = $"Cập nhật trạng thái khen thưởng {reward.DanhHieuSnapshot}. Trạng thái hiện tại: {reward.TrangThai}.",
                LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
                MucDo = NotificationConstants.Levels.Info,
                LoaiSuKien = RewardDisciplineConstants.NotificationEvents.RewardIssued, // default or generic event
                DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.Reward,
                MaDoiTuongLienKet = rewardId,
                DuongDan = $"/student/rewards/{rewardId}",
                MaDonVi = reward.MaDonVi,
                NguoiTao = userId
            };

            await _notificationService.SendToUsersAsync(req, new[] { reward.MaHocSinh }, cancellationToken);
        }
    }

    public async Task ResendDisciplineRecordNotificationAsync(int recordId, string reason, CancellationToken cancellationToken = default)
    {
        var record = await _context.HoSoKyLuats.FirstOrDefaultAsync(x => x.MaKyLuat == recordId, cancellationToken);
        if (record == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hồ sơ kỷ luật.");
        
        var userId = GetCurrentUserId();
        await _auditLogService.LogAsync(
            "HoSoKyLuat", recordId.ToString(),
            RewardDisciplineConstants.DisciplineAuditActions.ResendDisciplineNotification,
            null, new { Reason = reason },
            userId ?? 0, record.MaDonVi, "Gửi lại thông báo kỷ luật thủ công.", cancellationToken);

        var req = new SystemNotificationRequest
        {
            TieuDe = "Thông báo Kỷ luật (Gửi lại)",
            TomTat = "Ban quản trị gửi lại thông báo kỷ luật.",
            NoiDungText = $"Cập nhật hồ sơ kỷ luật: {record.TieuDe}. Trạng thái hiện tại: {record.TrangThai}.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineActivated, // default
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineRecord,
            MaDoiTuongLienKet = recordId,
            DuongDan = $"/student/disciplines/{recordId}",
            MaDonVi = record.MaDonVi,
            NguoiTao = userId
        };

        await _notificationService.SendToUsersAsync(req, new[] { record.MaHocSinh }, cancellationToken);
    }

    public async Task ResendDisciplineAppealNotificationAsync(int appealId, string reason, CancellationToken cancellationToken = default)
    {
        var appeal = await _context.KhieuNaiKyLuats.FirstOrDefaultAsync(x => x.MaKhieuNaiKyLuat == appealId, cancellationToken);
        if (appeal == null) throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khiếu nại.");
        
        var userId = GetCurrentUserId();
        await _auditLogService.LogAsync(
            "KhieuNaiKyLuat", appealId.ToString(),
            RewardDisciplineConstants.DisciplineAuditActions.ResendDisciplineNotification,
            null, new { Reason = reason },
            userId ?? 0, (appeal.MaDonVi ?? 0), "Gửi lại thông báo khiếu nại kỷ luật thủ công.", cancellationToken);

        var req = new SystemNotificationRequest
        {
            TieuDe = "Thông báo Khiếu nại Kỷ luật (Gửi lại)",
            TomTat = "Ban quản trị gửi lại thông báo khiếu nại kỷ luật.",
            NoiDungText = $"Cập nhật khiếu nại kỷ luật #{appeal.MaKhieuNaiKyLuat}. Trạng thái hiện tại: {appeal.TrangThai}.",
            LoaiThongBao = NotificationConstants.Types.RewardDiscipline,
            MucDo = NotificationConstants.Levels.Info,
            LoaiSuKien = RewardDisciplineConstants.NotificationEvents.DisciplineAppealResolved,
            DoiTuongLienKet = RewardDisciplineConstants.NotificationRefTypes.DisciplineAppeal,
            MaDoiTuongLienKet = appealId,
            DuongDan = $"/student/disciplines/appeals/{appealId}",
            MaDonVi = appeal.MaDonVi ?? 0,
            NguoiTao = userId
        };

        await _notificationService.SendToUsersAsync(req, new[] { appeal.MaHocSinh }, cancellationToken);
    }
}
