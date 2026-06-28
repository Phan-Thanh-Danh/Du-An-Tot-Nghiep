using System.Data;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationDecisionService : IApplicationDecisionService
{
    private const int LockTimeoutMs = 10000;
    private const int RequiredNoteMinLength = 10;
    private const int RequiredNoteMaxLength = 2000;
    private const int PublicNoteMaxLength = 1000;
    private const int InternalNoteMaxLength = 2000;
    private const string DefaultApprovalNote = "Đơn đã được phê duyệt.";

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly IApplicationStateMachine _stateMachine;
    private readonly IApplicationDecisionPermissionEvaluator _permissionEvaluator;
    private readonly IApplicationApprovalPreconditionValidator _approvalPreconditionValidator;
    private readonly IApplicationAdminQueueService _queueService;
    private readonly IApplicationNotificationService _applicationNotificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDecisionService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IApplicationStateMachine stateMachine,
        IApplicationDecisionPermissionEvaluator permissionEvaluator,
        IApplicationApprovalPreconditionValidator approvalPreconditionValidator,
        IApplicationAdminQueueService queueService,
        IApplicationNotificationService applicationNotificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _scopeService = scopeService;
        _stateMachine = stateMachine;
        _permissionEvaluator = permissionEvaluator;
        _approvalPreconditionValidator = approvalPreconditionValidator;
        _queueService = queueService;
        _applicationNotificationService = applicationNotificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AdminApplicationDetailDto> RequestSupplementAsync(
        int applicationId,
        AdminApplicationRequestSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        var publicRequest = NormalizeRequiredText(request.Request, "Nội dung yêu cầu bổ sung", RequiredNoteMinLength, RequiredNoteMaxLength);
        var internalNote = NormalizeOptionalText(request.InternalNote, InternalNoteMaxLength, "Ghi chú nội bộ");
        DonTu? changedApplication = null;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureRowVersion(application.RowVersion, rowVersion);
                _stateMachine.EnsureTransitionAllowed(application.TrangThai, ApplicationStatuses.NeedSupplement);
                _permissionEvaluator.EnsureCanRequestSupplement(application, actor);

                var oldSnapshot = BuildAuditSnapshot(application);
                var oldStatus = application.TrangThai;
                var now = DateTime.UtcNow;
                var previousAssigneeId = application.NguoiDuyetHienTai;
                application.TrangThai = ApplicationStatuses.NeedSupplement;
                application.NoiDungYeuCauBoSung = publicRequest;
                application.LyDoTuChoi = null;
                application.NgayDuyet = null;
                application.TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, rowVersion);
                AddTimeline(application, actor, ApplicationActions.RequestSupplement, oldStatus, application.TrangThai, publicRequest, internalNote, new
                {
                    decision = "request_supplement",
                    previousAssigneeId,
                    processorId = actor.User.MaNguoiDung
                }, now);
                AddAudit(application, actor, ApplicationActions.RequestSupplement, oldSnapshot, BuildAuditSnapshot(application), now);
                await _context.SaveChangesAsync(cancellationToken);
                changedApplication = application;
            }, cancellationToken);
        });

        if (changedApplication is not null)
        {
            await _applicationNotificationService.NotifySupplementRequestedAsync(changedApplication, publicRequest, cancellationToken);
        }

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    public async Task<AdminApplicationDetailDto> ApproveAsync(
        int applicationId,
        AdminApplicationApproveRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        var publicNote = NormalizeApprovalNote(request.PublicNote);
        var internalNote = NormalizeOptionalText(request.InternalNote, InternalNoteMaxLength, "Ghi chú nội bộ");
        DonTu? changedApplication = null;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureRowVersion(application.RowVersion, rowVersion);
                _stateMachine.EnsureTransitionAllowed(application.TrangThai, ApplicationStatuses.Approved);
                _permissionEvaluator.EnsureCanApprove(application, actor);
                await _approvalPreconditionValidator.ValidateAsync(application, cancellationToken);

                var oldSnapshot = BuildAuditSnapshot(application);
                var oldStatus = application.TrangThai;
                var now = DateTime.UtcNow;
                var previousAssigneeId = application.NguoiDuyetHienTai;
                application.TrangThai = ApplicationStatuses.Approved;
                application.NgayDuyet = now;
                application.NguoiXuLyCuoi = actor.User.MaNguoiDung;
                application.NguoiDuyetHienTai = null;
                application.LyDoTuChoi = null;
                application.NoiDungYeuCauBoSung = null;
                application.TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.Pending;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, rowVersion);
                AddTimeline(application, actor, ApplicationActions.Approve, oldStatus, application.TrangThai, publicNote, internalNote, new
                {
                    decision = "approve",
                    previousAssigneeId,
                    processorId = actor.User.MaNguoiDung
                }, now);
                AddAudit(application, actor, ApplicationActions.Approve, oldSnapshot, BuildAuditSnapshot(application), now);
                await _context.SaveChangesAsync(cancellationToken);
                changedApplication = application;
            }, cancellationToken);
        });

        if (changedApplication is not null)
        {
            await _applicationNotificationService.NotifyApprovedAsync(changedApplication, cancellationToken);
        }

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    public async Task<AdminApplicationDetailDto> RejectAsync(
        int applicationId,
        AdminApplicationRejectRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        var reason = NormalizeRequiredText(request.Reason, "Lý do từ chối", RequiredNoteMinLength, RequiredNoteMaxLength);
        var internalNote = NormalizeOptionalText(request.InternalNote, InternalNoteMaxLength, "Ghi chú nội bộ");
        DonTu? changedApplication = null;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureRowVersion(application.RowVersion, rowVersion);
                _stateMachine.EnsureTransitionAllowed(application.TrangThai, ApplicationStatuses.Rejected);
                _permissionEvaluator.EnsureCanReject(application, actor);

                var oldSnapshot = BuildAuditSnapshot(application);
                var oldStatus = application.TrangThai;
                var now = DateTime.UtcNow;
                var previousAssigneeId = application.NguoiDuyetHienTai;
                application.TrangThai = ApplicationStatuses.Rejected;
                application.LyDoTuChoi = reason;
                application.NoiDungYeuCauBoSung = null;
                application.NgayDuyet = null;
                application.NguoiXuLyCuoi = actor.User.MaNguoiDung;
                application.NguoiDuyetHienTai = null;
                application.TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, rowVersion);
                AddTimeline(application, actor, ApplicationActions.Reject, oldStatus, application.TrangThai, reason, internalNote, new
                {
                    decision = "reject",
                    previousAssigneeId,
                    processorId = actor.User.MaNguoiDung
                }, now);
                AddAudit(application, actor, ApplicationActions.Reject, oldSnapshot, BuildAuditSnapshot(application), now);
                await _context.SaveChangesAsync(cancellationToken);
                changedApplication = application;
            }, cancellationToken);
        });

        if (changedApplication is not null)
        {
            await _applicationNotificationService.NotifyRejectedAsync(changedApplication, reason, cancellationToken);
        }

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    private async Task<DonTu> GetApplicationInScopeTrackedAsync(
        int applicationId,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var application = await _context.DonTus
            .FirstOrDefaultAsync(x => x.MaDonTu == applicationId, cancellationToken);
        if (application is null ||
            !await _scopeService.CanAccessCampusAsync(actor, application.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        return application;
    }

    private async Task AcquireWorkflowLockAsync(int applicationId, CancellationToken cancellationToken)
    {
        var result = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        var resource = new SqlParameter("@resource", SqlDbType.NVarChar, 255)
        {
            Value = $"ApplicationWorkflow:{applicationId}"
        };
        var timeout = new SqlParameter("@timeout", SqlDbType.Int)
        {
            Value = LockTimeoutMs
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @timeout",
            [result, resource, timeout],
            cancellationToken);

        if (result.Value is not int code || code < 0)
        {
            throw ConcurrencyException();
        }
    }

    private void AddTimeline(
        DonTu application,
        ApplicationActorContext actor,
        string action,
        string oldStatus,
        string newStatus,
        string publicNote,
        string? internalNote,
        object snapshot,
        DateTime now)
    {
        _context.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = actor.User.MaNguoiDung,
            NguonThucHien = "user",
            HanhDong = action,
            TrangThaiCu = oldStatus,
            TrangThaiMoi = newStatus,
            GhiChuCongKhai = publicNote,
            GhiChuNoiBo = internalNote,
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            HienThiChoHocSinh = true,
            NgayTao = now
        });
    }

    private void AddAudit(
        DonTu application,
        ApplicationActorContext actor,
        string action,
        object oldValue,
        object newValue,
        DateTime now)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        _context.NhatKyKiemToans.Add(new NhatKyKiemToan
        {
            MaDonVi = application.MaDonVi,
            LoaiDoiTuong = nameof(DonTu),
            MaDoiTuong = application.MaDonTu.ToString(),
            HanhDong = action,
            GiaTriCu = JsonSerializer.Serialize(oldValue),
            GiaTriMoi = JsonSerializer.Serialize(newValue),
            NguoiThayDoi = actor.User.MaNguoiDung,
            ThoiDiemThayDoi = now,
            DiaChiIp = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            TraceId = httpContext?.TraceIdentifier,
            MoTa = $"P0-DT5 {action}"
        });
    }

    private static object BuildAuditSnapshot(DonTu application)
    {
        return new
        {
            status = application.TrangThai,
            assigneeId = application.NguoiDuyetHienTai,
            lastProcessorId = application.NguoiXuLyCuoi,
            businessProcessingStatus = application.TrangThaiXuLyNghiepVu
        };
    }

    private static string NormalizeApprovalNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return DefaultApprovalNote;
        }

        return NormalizeOptionalText(note, PublicNoteMaxLength, "Ghi chú công khai")!;
    }

    private static string NormalizeRequiredText(string? text, string fieldName, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} là bắt buộc.");
        }

        var normalized = text.Trim();
        if (normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} phải từ {minLength} đến {maxLength} ký tự.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? text, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} tối đa {maxLength} ký tự.");
        }

        return normalized;
    }

    private static byte[] DecodeRowVersion(string rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }

        try
        {
            var decoded = Convert.FromBase64String(rowVersion);
            if (decoded.Length != 8)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
            }

            return decoded;
        }
        catch (FormatException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }
    }

    private static void EnsureRowVersion(byte[] actual, byte[] expected)
    {
        if (!actual.SequenceEqual(expected))
        {
            throw ConcurrencyException();
        }
    }

    private void SetOriginalRowVersion(DonTu application, byte[] rowVersion)
    {
        _context.Entry(application).Property(x => x.RowVersion).OriginalValue = rowVersion;
    }

    private static ApiException ConcurrencyException()
    {
        return new ApiException(
            StatusCodes.Status409Conflict,
            "Đơn đã được thay đổi bởi một thao tác khác. Vui lòng tải lại dữ liệu.");
    }

    private static async Task ExecuteConcurrencyAwareAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConcurrencyException();
        }
        catch (SqlException exception) when (exception.Number is -2 or 1205 or 1222 or 2601 or 2627 or 3960 or 3961 or 3962 or 3963)
        {
            throw ConcurrencyException();
        }
    }
}
