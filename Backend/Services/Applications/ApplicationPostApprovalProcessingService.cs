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

public class ApplicationPostApprovalProcessingService : IApplicationPostApprovalProcessingService
{
    private const int LockTimeoutMs = 10000;
    private const int PublicNoteMinLength = 10;
    private const int PublicNoteMaxLength = 2000;
    private const int InternalNoteMaxLength = 2000;
    private const string ManualRequiredNote = "Đơn đã được chuyển sang bộ phận phụ trách để xử lý thủ công.";

    private static readonly string[] AutoNoOpStatuses =
    [
        ApplicationProcessingStatuses.Recorded,
        ApplicationProcessingStatuses.Succeeded,
        ApplicationProcessingStatuses.ManualRequired
    ];

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly IApplicationProcessingStateMachine _stateMachine;
    private readonly IApplicationProcessingPermissionEvaluator _permissionEvaluator;
    private readonly IApplicationProcessingResultSanitizer _resultSanitizer;
    private readonly IApplicationAdminQueueService _queueService;
    private readonly IReadOnlyList<IApplicationPostApprovalHandler> _handlers;
    private readonly IApplicationNotificationService _applicationNotificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationPostApprovalProcessingService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IApplicationProcessingStateMachine stateMachine,
        IApplicationProcessingPermissionEvaluator permissionEvaluator,
        IApplicationProcessingResultSanitizer resultSanitizer,
        IApplicationAdminQueueService queueService,
        IEnumerable<IApplicationPostApprovalHandler> handlers,
        IApplicationNotificationService applicationNotificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _scopeService = scopeService;
        _stateMachine = stateMachine;
        _permissionEvaluator = permissionEvaluator;
        _resultSanitizer = resultSanitizer;
        _queueService = queueService;
        _handlers = handlers.ToList();
        _applicationNotificationService = applicationNotificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AdminApplicationDetailDto> ProcessAsync(
        int applicationId,
        AdminApplicationProcessRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        _permissionEvaluator.EnsureCanOperate(actor);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        DonTu? changedApplication = null;
        string? outcomeStatus = null;
        string? outcomePublicNote = null;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureApproved(application);
                EnsureRowVersion(application.RowVersion, rowVersion);

                if (AutoNoOpStatuses.Contains(application.TrangThaiXuLyNghiepVu))
                {
                    return;
                }

                if (application.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.NotProcessed)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Đơn chưa sẵn sàng để xử lý nghiệp vụ sau duyệt.");
                }

                if (application.TrangThaiXuLyNghiepVu is not (ApplicationProcessingStatuses.Pending or ApplicationProcessingStatuses.Failed))
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Trạng thái xử lý nghiệp vụ hiện tại không cho phép xử lý tự động.");
                }

                var oldSnapshot = BuildAuditSnapshot(application);
                var oldProcessingStatus = application.TrangThaiXuLyNghiepVu;
                var now = DateTime.UtcNow;
                var attemptId = Guid.NewGuid();
                var outcome = await ResolveAutomaticOutcomeAsync(application, actor, cancellationToken);

                _stateMachine.EnsureTransitionAllowed(oldProcessingStatus, outcome.Outcome);
                application.TrangThaiXuLyNghiepVu = outcome.Outcome;
                application.KetQuaXuLyJson = SerializeResultEnvelope(
                    "automatic",
                    outcome.Outcome,
                    application.LoaiDon,
                    now,
                    actor.User.MaNguoiDung,
                    outcome.Data,
                    outcome.Handler);
                application.NhatKyTuDong = SerializeAutomaticAttempt(
                    attemptId,
                    outcome.Handler,
                    now,
                    actor.User.MaNguoiDung,
                    outcome.Outcome);
                application.NgayCapNhat = now;
                application.NguoiXuLyCuoi = actor.User.MaNguoiDung;
                SetOriginalRowVersion(application, rowVersion);

                AddTimeline(
                    application,
                    actor,
                    "system",
                    outcome.PublicNote,
                    BuildInternalNote(outcome.Handler, outcome.InternalCode),
                    new
                    {
                        operation = "auto_process",
                        handler = outcome.Handler,
                        processingStatusFrom = oldProcessingStatus,
                        processingStatusTo = outcome.Outcome,
                        outcome = outcome.Outcome,
                        processorId = actor.User.MaNguoiDung
                    },
                    now);
                AddAudit(application, actor, "P0-DT6 auto process", oldSnapshot, BuildAuditSnapshot(application), now);
                await _context.SaveChangesAsync(cancellationToken);
                changedApplication = application;
                outcomeStatus = outcome.Outcome;
                outcomePublicNote = outcome.PublicNote;
            }, cancellationToken);
        });

        if (changedApplication is not null && outcomeStatus is not null)
        {
            await NotifyProcessingOutcomeAsync(changedApplication, outcomeStatus, outcomePublicNote, cancellationToken);
        }

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    public async Task<AdminApplicationDetailDto> RecordProcessingResultAsync(
        int applicationId,
        AdminApplicationRecordProcessingResultRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        _permissionEvaluator.EnsureCanOperate(actor);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        var outcome = NormalizeManualOutcome(request.Outcome);
        var publicNote = NormalizeRequiredText(request.PublicNote, "Ghi chú công khai", PublicNoteMinLength, PublicNoteMaxLength);
        var internalNote = NormalizeOptionalText(request.InternalNote, InternalNoteMaxLength, "Ghi chú nội bộ");
        var result = _resultSanitizer.Sanitize(request.Result);
        DonTu? changedApplication = null;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureApproved(application);
                EnsureRowVersion(application.RowVersion, rowVersion);

                var oldProcessingStatus = application.TrangThaiXuLyNghiepVu;
                if (_stateMachine.IsTerminal(oldProcessingStatus))
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Không được ghi đè kết quả xử lý nghiệp vụ đã kết thúc.");
                }

                _stateMachine.EnsureTransitionAllowed(oldProcessingStatus, outcome);

                var oldSnapshot = BuildAuditSnapshot(application);
                var now = DateTime.UtcNow;
                application.TrangThaiXuLyNghiepVu = outcome;
                application.KetQuaXuLyJson = SerializeResultEnvelope(
                    "manual",
                    outcome,
                    application.LoaiDon,
                    now,
                    actor.User.MaNguoiDung,
                    result);
                application.NgayCapNhat = now;
                application.NguoiXuLyCuoi = actor.User.MaNguoiDung;
                SetOriginalRowVersion(application, rowVersion);

                AddTimeline(
                    application,
                    actor,
                    "user",
                    publicNote,
                    internalNote,
                    new
                    {
                        operation = "manual_processing_result",
                        processingStatusFrom = oldProcessingStatus,
                        processingStatusTo = outcome,
                        outcome,
                        processorId = actor.User.MaNguoiDung
                    },
                    now);
                AddAudit(application, actor, "P0-DT6 manual processing result", oldSnapshot, BuildAuditSnapshot(application), now);
                await _context.SaveChangesAsync(cancellationToken);
                changedApplication = application;
            }, cancellationToken);
        });

        if (changedApplication is not null)
        {
            await NotifyProcessingOutcomeAsync(changedApplication, outcome, publicNote, cancellationToken);
        }

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    private async Task NotifyProcessingOutcomeAsync(
        DonTu application,
        string outcome,
        string? publicNote,
        CancellationToken cancellationToken)
    {
        switch (outcome)
        {
            case ApplicationProcessingStatuses.Recorded:
                await _applicationNotificationService.NotifyProcessingRecordedAsync(application, cancellationToken);
                break;
            case ApplicationProcessingStatuses.Succeeded:
                await _applicationNotificationService.NotifyProcessingSucceededAsync(application, cancellationToken);
                break;
            case ApplicationProcessingStatuses.Failed:
                await _applicationNotificationService.NotifyProcessingFailedAsync(application, publicNote, cancellationToken);
                break;
            case ApplicationProcessingStatuses.ManualRequired:
                await _applicationNotificationService.NotifyManualProcessingRequiredAsync(application, cancellationToken);
                break;
        }
    }

    private async Task<ApplicationPostApprovalOutcome> ResolveAutomaticOutcomeAsync(
        DonTu application,
        ApplicationActorContext actor,
        CancellationToken cancellationToken)
    {
        var handler = _handlers.FirstOrDefault(x => x.CanHandle(application.LoaiDon));
        if (handler is not null)
        {
            return await handler.ProcessAsync(application, actor, cancellationToken);
        }

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            reason = "manual_processor_required",
            applicationType = application.LoaiDon
        }));

        return new ApplicationPostApprovalOutcome
        {
            Outcome = ApplicationProcessingStatuses.ManualRequired,
            Handler = "manual_required_fallback",
            PublicNote = ManualRequiredNote,
            InternalCode = "manual_processor_required",
            Data = document.RootElement.Clone()
        };
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
        string source,
        string publicNote,
        string? internalNote,
        object snapshot,
        DateTime now)
    {
        _context.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = actor.User.MaNguoiDung,
            NguonThucHien = source,
            HanhDong = ApplicationActions.BusinessProcess,
            TrangThaiCu = application.TrangThai,
            TrangThaiMoi = application.TrangThai,
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
        string description,
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
            HanhDong = ApplicationActions.BusinessProcess,
            GiaTriCu = JsonSerializer.Serialize(oldValue),
            GiaTriMoi = JsonSerializer.Serialize(newValue),
            NguoiThayDoi = actor.User.MaNguoiDung,
            ThoiDiemThayDoi = now,
            DiaChiIp = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            TraceId = httpContext?.TraceIdentifier,
            MoTa = description
        });
    }

    private static string SerializeResultEnvelope(
        string mode,
        string outcome,
        string applicationType,
        DateTime processedAt,
        int processedBy,
        JsonElement data,
        string? handler = null)
    {
        return handler is null
            ? JsonSerializer.Serialize(new
            {
                version = 1,
                mode,
                outcome,
                applicationType,
                processedAt,
                processedBy,
                data
            })
            : JsonSerializer.Serialize(new
            {
                version = 1,
                mode,
                outcome,
                applicationType,
                handler,
                processedAt,
                processedBy,
                data
            });
    }

    private static string SerializeAutomaticAttempt(
        Guid attemptId,
        string handler,
        DateTime attemptedAt,
        int triggeredBy,
        string outcome)
    {
        return JsonSerializer.Serialize(new
        {
            version = 1,
            attemptId,
            handler,
            attemptedAt,
            triggeredBy,
            outcome
        });
    }

    private static string? BuildInternalNote(string handler, string? code)
    {
        return string.IsNullOrWhiteSpace(code)
            ? $"handler={handler}"
            : $"handler={handler}; code={code}";
    }

    private static object BuildAuditSnapshot(DonTu application)
    {
        return new
        {
            status = application.TrangThai,
            processingStatus = application.TrangThaiXuLyNghiepVu,
            lastProcessorId = application.NguoiXuLyCuoi,
            hasProcessingResult = !string.IsNullOrWhiteSpace(application.KetQuaXuLyJson)
        };
    }

    private static void EnsureApproved(DonTu application)
    {
        if (application.TrangThai != ApplicationStatuses.Approved)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được xử lý nghiệp vụ sau khi đơn đã được duyệt.");
        }
    }

    private static string NormalizeManualOutcome(string? outcome)
    {
        if (string.IsNullOrWhiteSpace(outcome))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý không hợp lệ.");
        }

        var canonical = ApplicationProcessingStatuses.All.FirstOrDefault(x =>
            x.Equals(outcome.Trim(), StringComparison.OrdinalIgnoreCase));
        if (canonical is not (ApplicationProcessingStatuses.Recorded or ApplicationProcessingStatuses.Succeeded or ApplicationProcessingStatuses.Failed))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Kết quả xử lý không hợp lệ.");
        }

        return canonical;
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
