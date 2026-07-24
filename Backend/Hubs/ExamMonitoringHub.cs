using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Constants;

namespace Backend.Hubs;

// ─── Typed sub-DTOs ──────────────────────────────────────────────────────────

/// <summary>SDP session description (offer / answer).</summary>
public class RtcSessionDescriptionDto
{
    public string Type { get; set; } = string.Empty;
    public string Sdp { get; set; } = string.Empty;
}

/// <summary>ICE candidate — must match RTCIceCandidateInit shape.</summary>
public class RtcIceCandidateDto
{
    public string Candidate { get; set; } = string.Empty;
    public string? SdpMid { get; set; }
    public int? SdpMLineIndex { get; set; }
    public string? UsernameFragment { get; set; }
}

// ─── Message DTOs ─────────────────────────────────────────────────────────────

public class WebRtcOfferDto
{
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TargetConnectionId { get; set; }
    public string? FromConnectionId { get; set; }
    public RtcSessionDescriptionDto Offer { get; set; } = new();
}

public class WebRtcAnswerDto
{
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TargetConnectionId { get; set; }
    public string? FromConnectionId { get; set; }
    public RtcSessionDescriptionDto Answer { get; set; } = new();
}

public class WebRtcIceCandidateMessageDto
{
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string? TargetConnectionId { get; set; }
    public string? FromConnectionId { get; set; }
    public RtcIceCandidateDto Candidate { get; set; } = new();
}

// ─── Hub ─────────────────────────────────────────────────────────────────────

[Authorize]
public class ExamMonitoringHub : Hub
{
    private readonly ILogger<ExamMonitoringHub> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ExamMonitoringHub(ILogger<ExamMonitoringHub> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    // ── Room Management ───────────────────────────────────────────────────────

    /// <summary>
    /// Giám thị/Admin tham gia vào nhóm theo dõi ca thi.
    /// Sau khi join, gửi broadcast yêu cầu học sinh đang online phát lại connectionId.
    /// </summary>
    public async Task JoinExamRoom(int maCaThi)
    {
        if (!await IsAuthorizedProctor(maCaThi)) return;

        var groupName = $"exam-{maCaThi}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "User {ConnectionId} (UserId: {UserId}) joined exam room {Group}",
            Context.ConnectionId, Context.UserIdentifier, groupName
        );
        await Clients.Caller.SendAsync("JoinedRoom", new { maCaThi, message = $"Đã tham gia phòng thi {maCaThi}" });
        await Clients.Group(groupName).SendAsync("ProctorRequestedConnections", new { proctorConnectionId = Context.ConnectionId });
    }

    /// <summary>Rời nhóm theo dõi ca thi.</summary>
    public async Task LeaveExamRoom(int maCaThi)
    {
        var groupName = $"exam-{maCaThi}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} left exam room {Group}", Context.ConnectionId, groupName);
    }

    // ── Student Methods ───────────────────────────────────────────────────────

    /// <summary>
    /// Học sinh tham gia phòng thi và broadcast connectionId của mình cho giám thị.
    /// </summary>
    public async Task JoinAsStudent(int maCaThi, int maHocSinh)
    {
        if (!VerifyStudentIdentity(maHocSinh)) return;
        if (maCaThi <= 0 || maHocSinh <= 0)
        {
            _logger.LogWarning("JoinAsStudent: Invalid payload — maCaThi={MaCaThi}, maHocSinh={MaHocSinh}", maCaThi, maHocSinh);
            return;
        }

        var groupName = $"exam-{maCaThi}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("StudentConnectionIdBroadcast", new
        {
            maHocSinh,
            maCaThi,
            connectionId = Context.ConnectionId,
            thoiDiem = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Student {MaHocSinh} joined exam room {MaCaThi} with connectionId {ConnectionId}",
            maHocSinh, maCaThi, Context.ConnectionId
        );
    }

    public async Task ScreenShareStarted(int maCaThi, int maHocSinh)
    {
        if (!VerifyStudentIdentity(maHocSinh)) return;
        if (maCaThi <= 0 || maHocSinh <= 0) return;
        await Clients.Group($"exam-{maCaThi}").SendAsync("ScreenShareStatusChanged", new
        {
            maHocSinh, maCaThi, status = "streaming", thoiDiem = DateTime.UtcNow
        });
        _logger.LogInformation("Student {MaHocSinh} started screen share in exam {MaCaThi}", maHocSinh, maCaThi);
    }

    public async Task ScreenShareStopped(int maCaThi, int maHocSinh)
    {
        if (!VerifyStudentIdentity(maHocSinh)) return;
        if (maCaThi <= 0 || maHocSinh <= 0) return;
        await Clients.Group($"exam-{maCaThi}").SendAsync("ScreenShareStatusChanged", new
        {
            maHocSinh, maCaThi, status = "stopped", thoiDiem = DateTime.UtcNow
        });
        _logger.LogInformation("Student {MaHocSinh} stopped screen share in exam {MaCaThi}", maHocSinh, maCaThi);
    }

    /// <summary>
    /// Giám thị phản hồi lại cho học sinh để học sinh biết connectionId của giám thị.
    /// </summary>
    public async Task AcknowledgeStudent(string studentConnectionId)
    {
        if (!IsProctorOrAdmin()) return;
        if (!string.IsNullOrWhiteSpace(studentConnectionId))
        {
            await Clients.Client(studentConnectionId).SendAsync("ProctorAcknowledged", new { proctorConnectionId = Context.ConnectionId });
        }
    }

    // ── WebRTC Signaling ──────────────────────────────────────────────────────

    /// <summary>
    /// Gửi Offer SDP tới học sinh.
    /// Nếu chưa biết targetConnectionId thì fallback broadcast cả phòng (trường hợp proctor join sau student).
    /// </summary>
    public async Task SendOffer(WebRtcOfferDto dto)
    {
        if (!await IsAuthorizedProctor(dto.MaCaThi)) return;
        if (dto.MaCaThi <= 0 || dto.MaHocSinh <= 0)
        {
            _logger.LogWarning("SendOffer: Invalid maCaThi/maHocSinh — ignored.");
            return;
        }

        dto.FromConnectionId = Context.ConnectionId;

        if (!string.IsNullOrWhiteSpace(dto.TargetConnectionId))
        {
            _logger.LogInformation(
                "SendOffer maCaThi={MaCaThi} maHocSinh={MaHocSinh} target={Target}",
                dto.MaCaThi, dto.MaHocSinh, dto.TargetConnectionId
            );
            await Clients.Client(dto.TargetConnectionId).SendAsync("ReceiveOffer", dto);
            return;
        }

        // Fallback: proctor gửi offer nhưng chưa có targetConnectionId → broadcast phòng
        _logger.LogWarning(
            "SendOffer: No targetConnectionId — broadcasting to group exam-{MaCaThi}", dto.MaCaThi
        );
        await Clients.Group($"exam-{dto.MaCaThi}").SendAsync("ReceiveOffer", dto);
    }

    /// <summary>
    /// Học sinh gửi Answer SDP về giám thị.
    /// BẮT BUỘC có targetConnectionId — không broadcast.
    /// </summary>
    public async Task SendAnswer(WebRtcAnswerDto dto)
    {
        if (!VerifyStudentIdentity(dto.MaHocSinh)) return;
        if (dto.MaCaThi <= 0 || dto.MaHocSinh <= 0)
        {
            _logger.LogWarning("SendAnswer: Invalid maCaThi/maHocSinh — ignored.");
            return;
        }

        if (string.IsNullOrWhiteSpace(dto.TargetConnectionId))
        {
            _logger.LogWarning(
                "SendAnswer: Missing TargetConnectionId for student {MaHocSinh} in exam {MaCaThi} — dropped.",
                dto.MaHocSinh, dto.MaCaThi
            );
            return;
        }

        dto.FromConnectionId = Context.ConnectionId;

        _logger.LogInformation(
            "SendAnswer maCaThi={MaCaThi} maHocSinh={MaHocSinh} target={Target}",
            dto.MaCaThi, dto.MaHocSinh, dto.TargetConnectionId
        );
        await Clients.Client(dto.TargetConnectionId).SendAsync("ReceiveAnswer", dto);
    }

    /// <summary>
    /// Trao đổi ICE Candidate.
    /// BẮT BUỘC có targetConnectionId — không broadcast.
    /// Skip nếu candidate rỗng (end-of-candidates signal).
    /// </summary>
    public async Task SendIceCandidate(WebRtcIceCandidateMessageDto dto)
    {
        // Both Proctor and Student can send ICE Candidates
        // Only verify student identity if they are a student
        if (Context.User?.IsInRole(AuthRoles.Student) == true)
        {
            if (!VerifyStudentIdentity(dto.MaHocSinh)) return;
        }
        else
        {
            if (!await IsAuthorizedProctor(dto.MaCaThi)) return;
        }

        if (dto.MaCaThi <= 0 || dto.MaHocSinh <= 0)
        {
            _logger.LogWarning("SendIceCandidate: Invalid maCaThi/maHocSinh — ignored.");
            return;
        }

        // Bỏ qua end-of-candidates (candidate string rỗng) — không cần relay
        if (string.IsNullOrWhiteSpace(dto.Candidate.Candidate))
        {
            _logger.LogDebug(
                "SendIceCandidate: Empty candidate (end-of-candidates) for student {MaHocSinh} — skipped.",
                dto.MaHocSinh
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(dto.TargetConnectionId))
        {
            _logger.LogWarning(
                "SendIceCandidate: Missing TargetConnectionId for student {MaHocSinh} in exam {MaCaThi} — dropped.",
                dto.MaHocSinh, dto.MaCaThi
            );
            return;
        }

        dto.FromConnectionId = Context.ConnectionId;
        await Clients.Client(dto.TargetConnectionId).SendAsync("ReceiveIceCandidate", dto);
    }

    // ── Proctor Methods ───────────────────────────────────────────────────────

    public async Task SendViolationLog(int maCaThi, int maHocSinh, string loaiViPham, string? chiTiet)
    {
        if (!await IsAuthorizedProctor(maCaThi)) return;
        await Clients.Group($"exam-{maCaThi}").SendAsync("ViolationDetected", new
        {
            maHocSinh, loaiViPham, chiTiet,
            thoiDiem = DateTime.UtcNow,
            connectionId = Context.ConnectionId
        });
        _logger.LogWarning(
            "Violation from student {StudentId} in exam {ExamSession}: {Type}",
            maHocSinh, maCaThi, loaiViPham
        );
    }

    public async Task UpdateStudentStatus(int maCaThi, int maHocSinh, string status)
    {
        if (!await IsAuthorizedProctor(maCaThi)) return;
        await Clients.Group($"exam-{maCaThi}").SendAsync("StudentStatusUpdated", new
        {
            maHocSinh, status, thoiDiem = DateTime.UtcNow
        });
    }

    public async Task SendWarningToStudent(int maCaThi, string studentConnectionId, string message)
    {
        if (!await IsAuthorizedProctor(maCaThi)) return;
        await Clients.Client(studentConnectionId).SendAsync("WarningReceived", new
        {
            message, thoiDiem = DateTime.UtcNow
        });
    }

    public async Task UnlockStudent(int maCaThi, string studentConnectionId)
    {
        if (!await IsAuthorizedProctor(maCaThi)) return;
        await Clients.Client(studentConnectionId).SendAsync("StudentUnlocked", new
        {
            thoiDiem = DateTime.UtcNow
        });
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "User {ConnectionId} (UserId: {UserId}) disconnected. Reason: {Reason}",
            Context.ConnectionId, Context.UserIdentifier,
            exception?.Message ?? "clean disconnect"
        );
        await base.OnDisconnectedAsync(exception);
    }

    // ── Private Security Helpers ──────────────────────────────────────────────

    private bool VerifyStudentIdentity(int maHocSinh)
    {
        if (Context.UserIdentifier != maHocSinh.ToString())
        {
            _logger.LogWarning("Security Violation: User {UserId} attempted to act as student {MaHocSinh}", Context.UserIdentifier, maHocSinh);
            return false;
        }
        return true;
    }

    private bool IsProctorOrAdmin()
    {
        return Context.User?.IsInRole(AuthRoles.Teacher) == true || Context.User?.IsInRole(AuthRoles.Admin) == true || Context.User?.IsInRole(AuthRoles.SuperAdmin) == true;
    }

    private async Task<bool> IsAuthorizedProctor(int maCaThi)
    {
        if (Context.User?.IsInRole(AuthRoles.Admin) == true || Context.User?.IsInRole(AuthRoles.SuperAdmin) == true)
        {
            return true;
        }

        if (Context.User?.IsInRole(AuthRoles.Teacher) != true)
        {
            _logger.LogWarning("Security Violation: User {UserId} is not a teacher or admin", Context.UserIdentifier);
            return false;
        }

        if (int.TryParse(Context.UserIdentifier, out int teacherId))
        {
            var isAssigned = await _dbContext.PhanCongGiamThis.AnyAsync(p => p.MaCaThi == maCaThi && p.MaGiamThi == teacherId);
            if (!isAssigned)
            {
                _logger.LogWarning("Security Violation: Teacher {TeacherId} is not assigned to exam {MaCaThi}", teacherId, maCaThi);
            }
            return isAssigned;
        }

        return false;
    }
}
