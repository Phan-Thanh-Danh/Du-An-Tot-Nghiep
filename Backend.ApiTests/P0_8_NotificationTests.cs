using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_8_NotificationTests : ApiTestBase
{
    private static readonly string[] TeacherEmails =
    [
        "teacher.csharp.a@lms.local",
        "teacher.csharp.b@lms.local",
        "teacher.database.c@lms.local",
        "teacher.database.d@lms.local",
        "teacher.marketing.e@lms.local",
        "teacher.cntt@lms.local",
        "teacher.tkdh@lms.local",
        "teacher.mkt@lms.local"
    ];

    [Test]
    public async Task Admin_CreateManualNotification_ToUsers_ShouldReturnOk()
    {
        var recipient = await GetSeedStudentAsync();

        using var response = await CreateManualNotificationAsync(recipient.MaNguoiDung);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "loaiThongBao"), Is.EqualTo("manual"));
        Assert.That(GetInt32(data, "recipientCount"), Is.EqualTo(1));
    }

    [Test]
    public async Task User_GetMyNotifications_ShouldReturnOk()
    {
        var recipient = await GetSeedStudentAsync();
        await CreateManualNotificationAndGetIdAsync(recipient.MaNguoiDung);
        using var userClient = await CreateAuthenticatedClientAsync(recipient.Email);

        using var response = await userClient.GetAsync("api/notifications?pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement).EnumerateArray().ToList();
        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    public async Task User_GetUnreadCount_ShouldReturnOk()
    {
        var recipient = await GetSeedStudentAsync();
        await CreateManualNotificationAndGetIdAsync(recipient.MaNguoiDung);
        using var userClient = await CreateAuthenticatedClientAsync(recipient.Email);

        using var response = await userClient.GetAsync("api/notifications/unread-count");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "unreadCount"), Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task User_MarkNotificationAsRead_ShouldReturnOk()
    {
        var recipient = await GetSeedStudentAsync();
        var notificationId = await CreateManualNotificationAndGetRecipientNotificationIdAsync(recipient);
        using var userClient = await CreateAuthenticatedClientAsync(recipient.Email);

        using var response = await userClient.PatchAsync($"api/notifications/{notificationId}/read", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetBoolean(data, "daDoc"), Is.True);
    }

    [Test]
    public async Task User_MarkAllAsRead_ShouldReturnOk()
    {
        var recipient = await GetSeedStudentAsync();
        await CreateManualNotificationAndGetIdAsync(recipient.MaNguoiDung);
        await CreateManualNotificationAndGetIdAsync(recipient.MaNguoiDung);
        using var userClient = await CreateAuthenticatedClientAsync(recipient.Email);

        using var response = await userClient.PatchAsync("api/notifications/read-all", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "unreadCount"), Is.EqualTo(0));
    }

    [Test]
    public async Task Student_CannotCreateManualNotification_ShouldReturnForbidden()
    {
        var recipient = await GetSeedStudentAsync();
        using var studentClient = await CreateAuthenticatedClientAsync(recipient.Email);

        using var response = await studentClient.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = "NUnit P0-8 student forbidden",
            tomTat = "Student không được tạo thông báo.",
            noiDungJson = "{\"time\":1710000000000,\"blocks\":[]}",
            noiDungText = "Student không được tạo thông báo.",
            mucDo = "info",
            targetType = "users",
            targetIds = new[] { recipient.MaNguoiDung }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var response = await anonymousClient.GetAsync("api/notifications");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AutoNotification_WhenSessionCancelled_ShouldCreateNotification()
    {
        var context = await GetEditableSessionWithTeacherAsync();

        using var cancelResponse = await Client.PatchAsJsonAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/cancel",
            new { lyDoThayDoi = "NUnit P0-8 hủy buổi để kiểm tra auto notification" });
        Assert.That(cancelResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(cancelResponse));

        using var teacherClient = await CreateAuthenticatedClientAsync(context.TeacherEmail);
        var found = await HasNotificationTypeAsync(teacherClient, "session_cancelled");
        Assert.That(found, Is.True);
    }

    [Test]
    public async Task AutoNotification_WhenUnlockApproved_ShouldCreateNotification()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var approveResponse = await Client.PostAsJsonAsync(
                $"api/admin/attendance/unlock-requests/{pending.RequestId}/approve",
                new { ghiChu = "NUnit P0-8 approve để kiểm tra auto notification" });
            Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(approveResponse));

            var found = await HasNotificationTypeAsync(pending.TeacherClient, "attendance_unlock_approved");
            Assert.That(found, Is.True);
        }
        finally
        {
            pending.TeacherClient.Dispose();
        }
    }

    private async Task<HttpResponseMessage> CreateManualNotificationAsync(int recipientId)
    {
        return await Client.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = $"NUnit P0-8 thông báo {Guid.NewGuid():N}",
            tomTat = "Thông báo kiểm thử Notification Center.",
            noiDungJson = "{\"time\":1710000000000,\"blocks\":[]}",
            noiDungText = "Thông báo kiểm thử Notification Center.",
            mucDo = "important",
            targetType = "users",
            targetIds = new[] { recipientId }
        });
    }

    private async Task<int> CreateManualNotificationAndGetIdAsync(int recipientId)
    {
        using var response = await CreateManualNotificationAsync(recipientId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return GetInt32(data, "maThongBao");
    }

    private async Task<int> CreateManualNotificationAndGetRecipientNotificationIdAsync(UserCandidate recipient)
    {
        await CreateManualNotificationAndGetIdAsync(recipient.MaNguoiDung);
        using var userClient = await CreateAuthenticatedClientAsync(recipient.Email);
        using var response = await userClient.GetAsync("api/notifications?loaiThongBao=manual&pageIndex=1&pageSize=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement).EnumerateArray().ToList();
        Assert.That(items, Is.Not.Empty);
        return GetInt32(items[0], "maThongBao");
    }

    private async Task<UserCandidate> GetSeedStudentAsync()
    {
        const string preferredEmail = "student.cntt01@lms.local";
        using var response = await Client.GetAsync($"api/admin/users?keyword={Uri.EscapeDataString(preferredEmail)}&pageIndex=1&pageSize=10");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var match = GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadUserCandidate)
            .FirstOrDefault(x => string.Equals(x.Email, preferredEmail, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            return match;
        }

        Assert.Inconclusive("Không tìm thấy student seed student.cntt01@lms.local để test notification.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private static UserCandidate ReadUserCandidate(JsonElement item)
    {
        return new UserCandidate(
            GetInt32(item, "maNguoiDung"),
            GetRequiredString(item, "email"));
    }

    private async Task<SessionTeacherContext> GetEditableSessionWithTeacherAsync()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);

        using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={timetable.MaTkb}&pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var sessions = GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadSessionCandidate)
            .Where(IsEditable)
            .ToList();

        foreach (var session in sessions)
        {
            foreach (var teacherEmail in TeacherEmails)
            {
                using var teacherClient = await CreateAuthenticatedClientAsync(teacherEmail);
                using var attendanceResponse = await teacherClient.GetAsync($"api/buoi-hoc/{session.MaBuoiHoc}/attendance");
                if (attendanceResponse.StatusCode == HttpStatusCode.OK)
                {
                    return new SessionTeacherContext(session, teacherEmail);
                }
            }
        }

        Assert.Inconclusive("Không có BuoiHoc editable với teacher seed phù hợp để test auto notification hủy buổi.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<PendingUnlockContext> CreatePendingUnlockRequestAsync()
    {
        var sessions = await GetCandidateSessionsAsync();
        foreach (var session in sessions)
        {
            foreach (var teacherEmail in TeacherEmails)
            {
                var teacherClient = await CreateAuthenticatedClientAsync(teacherEmail);
                var status = session.TrangThaiDiemDanh;

                if (!await TeacherCanViewAttendanceAsync(teacherClient, session.MaBuoiHoc))
                {
                    teacherClient.Dispose();
                    continue;
                }

                if (string.Equals(status, "chua_mo", StringComparison.OrdinalIgnoreCase))
                {
                    using var startResponse = await teacherClient.PostAsync($"api/buoi-hoc/{session.MaBuoiHoc}/attendance/start", null);
                    if (startResponse.StatusCode != HttpStatusCode.OK)
                    {
                        teacherClient.Dispose();
                        continue;
                    }

                    status = "dang_diem_danh";
                }

                if (string.Equals(status, "dang_diem_danh", StringComparison.OrdinalIgnoreCase))
                {
                    using var submitResponse = await teacherClient.PostAsync($"api/buoi-hoc/{session.MaBuoiHoc}/attendance/submit", null);
                    if (submitResponse.StatusCode != HttpStatusCode.OK)
                    {
                        teacherClient.Dispose();
                        continue;
                    }
                }
                else if (!string.Equals(status, "da_gui", StringComparison.OrdinalIgnoreCase) &&
                         !string.Equals(status, "da_khoa", StringComparison.OrdinalIgnoreCase))
                {
                    teacherClient.Dispose();
                    continue;
                }

                using var requestResponse = await teacherClient.PostAsJsonAsync(
                    $"api/buoi-hoc/{session.MaBuoiHoc}/attendance/unlock-requests",
                    new { lyDo = "NUnit P0-8 tạo yêu cầu mở khóa để kiểm tra auto notification" });

                if (requestResponse.StatusCode == HttpStatusCode.OK)
                {
                    using var requestRoot = await GetRootAsync(requestResponse);
                    var data = GetRequiredProperty(requestRoot.RootElement, "data");
                    return new PendingUnlockContext(session.MaBuoiHoc, teacherClient, GetInt32(data, "maYcMoKhoa"));
                }

                teacherClient.Dispose();
            }
        }

        Assert.Inconclusive("Không tạo được unlock request pending để test auto notification approve.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<List<SessionCandidate>> GetCandidateSessionsAsync()
    {
        using var timetableResponse = await Client.GetAsync("api/thoi-khoa-bieu?trangThai=da_xuat_ban&pageIndex=1&pageSize=100");
        Assert.That(timetableResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(timetableResponse));

        using var timetableRoot = await GetRootAsync(timetableResponse);
        var timetables = GetDataItems(timetableRoot.RootElement)
            .EnumerateArray()
            .Select(item => GetInt32(item, "maTkb"))
            .ToList();

        foreach (var maTkb in timetables)
        {
            await GenerateSessionsAsync(maTkb);
        }

        using var response = await Client.GetAsync("api/buoi-hoc?pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var sessions = GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadSessionCandidate)
            .Where(x => !string.Equals(x.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (sessions.Count == 0)
        {
            Assert.Inconclusive("Không có BuoiHoc phù hợp để test auto notification.");
        }

        return sessions;
    }

    private async Task<bool> TeacherCanViewAttendanceAsync(HttpClient teacherClient, int sessionId)
    {
        using var response = await teacherClient.GetAsync($"api/buoi-hoc/{sessionId}/attendance");
        return response.StatusCode == HttpStatusCode.OK;
    }

    private static SessionCandidate ReadSessionCandidate(JsonElement item)
    {
        return new SessionCandidate(
            GetInt32(item, "maBuoiHoc"),
            GetOptionalString(item, "trangThaiBuoi"),
            GetOptionalString(item, "trangThaiDiemDanh"));
    }

    private static bool IsEditable(SessionCandidate session)
    {
        return !string.Equals(session.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(session.TrangThaiBuoi, "da_dien_ra", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(session.TrangThaiDiemDanh, "da_khoa", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> HasNotificationTypeAsync(HttpClient client, string notificationType)
    {
        using var response = await client.GetAsync($"api/notifications?loaiThongBao={notificationType}&pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement).EnumerateArray().Any();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await LoginAsync(email);
        var client = new HttpClient
        {
            BaseAddress = BaseUri
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> LoginAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = "Admin@123"
        });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Inconclusive($"Không login được account seed {email}. {await DescribeResponseAsync(response)}");
        }

        using var root = await GetRootAsync(response);
        var token = GetOptionalString(root.RootElement, "accessToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            Assert.Inconclusive($"Login {email} không trả accessToken.");
        }

        return token!;
    }

    private static int? GetOptionalInt32(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var property) ||
            property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return property.GetInt32();
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private sealed record UserCandidate(int MaNguoiDung, string Email);

    private sealed record SessionCandidate(
        int MaBuoiHoc,
        string? TrangThaiBuoi,
        string? TrangThaiDiemDanh);

    private sealed record SessionTeacherContext(SessionCandidate Session, string TeacherEmail);

    private sealed record PendingUnlockContext(
        int SessionId,
        HttpClient TeacherClient,
        int RequestId);
}
