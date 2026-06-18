using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

[Authorize]
public class ExamMonitoringHub : Hub
{
    private readonly ILogger<ExamMonitoringHub> _logger;

    public ExamMonitoringHub(ILogger<ExamMonitoringHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Giám thị/Admin tham gia vào nhóm theo dõi ca thi.
    /// </summary>
    public async Task JoinExamRoom(int maCaThi)
    {
        var groupName = $"exam-{maCaThi}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} joined exam room {Group}", Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("JoinedRoom", new { maCaThi, message = $"Đã tham gia phòng thi {maCaThi}" });
    }

    /// <summary>
    /// Rời nhóm theo dõi ca thi.
    /// </summary>
    public async Task LeaveExamRoom(int maCaThi)
    {
        var groupName = $"exam-{maCaThi}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} left exam room {Group}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Sinh viên gửi log vi phạm realtime (chuyển tab, mất focus, mất camera...).
    /// Broadcast tới giám thị trong cùng phòng thi.
    /// </summary>
    public async Task SendViolationLog(int maCaThi, int maHocSinh, string loaiViPham, string? chiTiet)
    {
        var groupName = $"exam-{maCaThi}";
        var payload = new
        {
            maHocSinh,
            loaiViPham,
            chiTiet,
            thoiDiem = DateTime.UtcNow,
            connectionId = Context.ConnectionId
        };

        await Clients.Group(groupName).SendAsync("ViolationDetected", payload);
        _logger.LogWarning("Violation from student {StudentId} in exam {ExamSession}: {Type}",
            maHocSinh, maCaThi, loaiViPham);
    }

    /// <summary>
    /// Cập nhật trạng thái kết nối sinh viên (online/offline/reconnecting).
    /// </summary>
    public async Task UpdateStudentStatus(int maCaThi, int maHocSinh, string status)
    {
        var groupName = $"exam-{maCaThi}";
        var payload = new
        {
            maHocSinh,
            status,
            thoiDiem = DateTime.UtcNow
        };

        await Clients.Group(groupName).SendAsync("StudentStatusUpdated", payload);
    }

    /// <summary>
    /// Giám thị gửi cảnh báo tới sinh viên cụ thể.
    /// </summary>
    public async Task SendWarningToStudent(string studentConnectionId, string message)
    {
        await Clients.Client(studentConnectionId).SendAsync("WarningReceived", new
        {
            message,
            thoiDiem = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {ConnectionId} disconnected from ExamMonitoringHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
