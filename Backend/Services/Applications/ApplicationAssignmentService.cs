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

public class ApplicationAssignmentService : IApplicationAssignmentService
{
    private const int LockTimeoutMs = 10000;
    private const int ReassignReasonMinLength = 10;
    private const int ReassignReasonMaxLength = 1000;

    private static readonly string[] AssignableStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview,
        ApplicationStatuses.NeedSupplement
    ];

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly IApplicationAdminQueueService _queueService;

    public ApplicationAssignmentService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IApplicationAdminQueueService queueService)
    {
        _context = context;
        _scopeService = scopeService;
        _queueService = queueService;
    }

    public async Task<AdminApplicationDetailDto> ReceiveAsync(
        int applicationId,
        AdminApplicationReceiveRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        EnsureCanReceive(actor);
        var rowVersion = DecodeRowVersion(request.RowVersion);

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                EnsureRowVersion(application.RowVersion, rowVersion);
                if (application.TrangThai != ApplicationStatuses.Submitted)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được tiếp nhận đơn đã nộp.");
                }

                if (application.NguoiDuyetHienTai.HasValue)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Đơn đã được tiếp nhận bởi người xử lý khác.");
                }

                var oldStatus = application.TrangThai;
                var now = DateTime.UtcNow;
                application.TrangThai = ApplicationStatuses.InReview;
                application.NguoiDuyetHienTai = actor.User.MaNguoiDung;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, rowVersion);
                _context.NhatKyDuyetDons.Add(CreateLog(
                    application,
                    actor.User.MaNguoiDung,
                    ApplicationActions.Receive,
                    oldStatus,
                    application.TrangThai,
                    "Đơn đã được tiếp nhận để xử lý.",
                    true,
                    null,
                    new
                    {
                        fromAssigneeId = (int?)null,
                        toAssigneeId = actor.User.MaNguoiDung
                    }));

                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        });

        return await _queueService.GetDetailAsync(applicationId, cancellationToken);
    }

    public async Task<AdminApplicationDetailDto> AssignAsync(
        int applicationId,
        AdminApplicationAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        EnsureCanAssign(actor);
        var rowVersion = DecodeRowVersion(request.RowVersion);
        var target = await _scopeService.GetAssignableUserAsync(actor, request.AssigneeId, cancellationToken);

        var changed = false;
        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                await AcquireWorkflowLockAsync(applicationId, cancellationToken);
                var application = await GetApplicationInScopeTrackedAsync(applicationId, actor, cancellationToken);
                if (target.MaDonVi != application.MaDonVi)
                {
                    throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy người xử lý phù hợp.");
                }

                EnsureRowVersion(application.RowVersion, rowVersion);
                EnsureAssignableStatus(application.TrangThai);

                if (application.NguoiDuyetHienTai == target.MaNguoiDung)
                {
                    return;
                }

                var fromAssigneeId = application.NguoiDuyetHienTai;
                var oldStatus = application.TrangThai;
                var action = fromAssigneeId.HasValue ? ApplicationActions.Reassign : ApplicationActions.Assign;
                var reason = fromAssigneeId.HasValue ? NormalizeReassignReason(request.LyDo ?? request.Reason) : null;
                if (application.TrangThai == ApplicationStatuses.Submitted)
                {
                    application.TrangThai = ApplicationStatuses.InReview;
                }

                application.NguoiDuyetHienTai = target.MaNguoiDung;
                application.NgayCapNhat = DateTime.UtcNow;
                SetOriginalRowVersion(application, rowVersion);
                _context.NhatKyDuyetDons.Add(CreateLog(
                    application,
                    actor.User.MaNguoiDung,
                    action,
                    oldStatus,
                    application.TrangThai,
                    fromAssigneeId.HasValue ? null : "Đơn đã được tiếp nhận để xử lý.",
                    !fromAssigneeId.HasValue,
                    reason,
                    new
                    {
                        fromAssigneeId,
                        toAssigneeId = target.MaNguoiDung,
                        reasonProvided = fromAssigneeId.HasValue
                    }));

                changed = true;
                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        });

        _ = changed;
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

    private static void EnsureCanReceive(ApplicationActorContext actor)
    {
        if (actor.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền tiếp nhận đơn từ.");
        }
    }

    private static void EnsureCanAssign(ApplicationActorContext actor)
    {
        if (actor.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền phân công đơn từ.");
        }
    }

    private static void EnsureAssignableStatus(string status)
    {
        if (!AssignableStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái hiện tại không cho phép phân công đơn.");
        }
    }

    private static string NormalizeReassignReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lý do phân công lại là bắt buộc.");
        }

        var normalized = reason.Trim();
        if (normalized.Length is < ReassignReasonMinLength or > ReassignReasonMaxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lý do phân công lại phải từ 10 đến 1000 ký tự.");
        }

        return normalized;
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

    private static NhatKyDuyetDon CreateLog(
        DonTu application,
        int actorId,
        string action,
        string? oldStatus,
        string? newStatus,
        string? publicNote,
        bool showToStudent,
        string? internalNote,
        object snapshot)
    {
        return new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = actorId,
            NguonThucHien = "user",
            HanhDong = action,
            TrangThaiCu = oldStatus,
            TrangThaiMoi = newStatus,
            GhiChuCongKhai = publicNote,
            GhiChuNoiBo = internalNote,
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            HienThiChoHocSinh = showToStudent,
            NgayTao = DateTime.UtcNow
        };
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
