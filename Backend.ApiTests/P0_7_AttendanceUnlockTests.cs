using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_7_AttendanceUnlockTests : ApiTestBase
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
    public async Task Teacher_CreateUnlockRequest_ShouldReturnOk()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            Assert.That(pending.RequestId, Is.GreaterThan(0));
            Assert.That(pending.Status, Is.EqualTo("cho_duyet"));
        }
        finally
        {
            await RejectIfPendingAsync(pending.RequestId);
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task DuplicatePendingUnlockRequest_ShouldReturnBadRequest()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var response = await pending.TeacherClient.PostAsJsonAsync(
                $"api/buoi-hoc/{pending.SessionId}/attendance/unlock-requests",
                new { lyDo = "NUnit P0-7 tạo trùng yêu cầu đang chờ duyệt" });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
        }
        finally
        {
            await RejectIfPendingAsync(pending.RequestId);
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task Admin_GetUnlockRequests_ShouldReturnOk()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var listResponse = await Client.GetAsync("api/admin/attendance/unlock-requests?pageIndex=1&pageSize=20");
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(listResponse));

            using var listRoot = await GetRootAsync(listResponse);
            var items = GetDataItems(listRoot.RootElement).EnumerateArray().ToList();
            Assert.That(items.Any(x => GetInt32(x, "maYcMoKhoa") == pending.RequestId), Is.True);

            using var detailResponse = await Client.GetAsync($"api/admin/attendance/unlock-requests/{pending.RequestId}");
            Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detailResponse));
        }
        finally
        {
            await RejectIfPendingAsync(pending.RequestId);
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task Admin_ApproveUnlockRequest_ShouldReopenAttendance()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var response = await Client.PostAsJsonAsync(
                $"api/admin/attendance/unlock-requests/{pending.RequestId}/approve",
                new { ghiChu = "NUnit P0-7 approve mở lại điểm danh trong 10 phút" });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

            using var root = await GetRootAsync(response);
            var data = GetRequiredProperty(root.RootElement, "data");
            Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo("da_duyet"));
            Assert.That(HasProperty(data, "moKhoaDenLuc"), Is.True);

            using var attendanceResponse = await pending.TeacherClient.GetAsync($"api/buoi-hoc/{pending.SessionId}/attendance");
            Assert.That(attendanceResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(attendanceResponse));

            using var attendanceRoot = await GetRootAsync(attendanceResponse);
            var attendance = GetRequiredProperty(attendanceRoot.RootElement, "data");
            Assert.That(GetRequiredString(attendance, "trangThaiDiemDanh"), Is.EqualTo("dang_diem_danh"));
        }
        finally
        {
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task Teacher_UpdateAfterApprove_ShouldReturnOk()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var approveResponse = await Client.PostAsJsonAsync(
                $"api/admin/attendance/unlock-requests/{pending.RequestId}/approve",
                new { ghiChu = "NUnit P0-7 approve để test update sau mở khóa" });
            Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(approveResponse));

            var studentId = await GetFirstAttendanceStudentIdAsync(pending.TeacherClient, pending.SessionId);
            using var updateResponse = await pending.TeacherClient.PatchAsJsonAsync(
                $"api/buoi-hoc/{pending.SessionId}/attendance/{studentId}",
                new { trangThai = "co_mat" });

            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(updateResponse));

            using var root = await GetRootAsync(updateResponse);
            var data = GetRequiredProperty(root.RootElement, "data");
            Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo("co_mat"));
        }
        finally
        {
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task Admin_RejectUnlockRequest_ShouldReturnOk()
    {
        var pending = await CreatePendingUnlockRequestAsync();
        try
        {
            using var response = await Client.PostAsJsonAsync(
                $"api/admin/attendance/unlock-requests/{pending.RequestId}/reject",
                new { lyDoTuChoi = "NUnit P0-7 từ chối yêu cầu mở khóa" });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

            using var root = await GetRootAsync(response);
            var data = GetRequiredProperty(root.RootElement, "data");
            Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo("tu_choi"));
            Assert.That(GetRequiredString(data, "lyDoTuChoi"), Is.EqualTo("NUnit P0-7 từ chối yêu cầu mở khóa"));
        }
        finally
        {
            pending.TeacherClient.Dispose();
        }
    }

    [Test]
    public async Task Student_CreateUnlockRequest_ShouldReturnForbidden()
    {
        var studentToken = await LoginAsync("student.cntt01@lms.local");
        using var studentClient = CreateClient(studentToken);

        using var response = await studentClient.PostAsJsonAsync(
            "api/buoi-hoc/1/attendance/unlock-requests",
            new { lyDo = "NUnit P0-7 student không được tạo yêu cầu" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var response = await anonymousClient.PostAsJsonAsync(
            "api/buoi-hoc/1/attendance/unlock-requests",
            new { lyDo = "NUnit P0-7 anonymous" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    private async Task<PendingUnlockContext> CreatePendingUnlockRequestAsync()
    {
        var contexts = await GetSubmittedAttendanceContextsAsync();
        foreach (var context in contexts)
        {
            using var response = await context.TeacherClient.PostAsJsonAsync(
                $"api/buoi-hoc/{context.SessionId}/attendance/unlock-requests",
                new { lyDo = "NUnit P0-7 cần mở khóa để chỉnh điểm danh" });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using var root = await GetRootAsync(response);
                var data = GetRequiredProperty(root.RootElement, "data");
                return new PendingUnlockContext(
                    context.SessionId,
                    context.TeacherClient,
                    GetInt32(data, "maYcMoKhoa"),
                    GetRequiredString(data, "trangThai"));
            }

            context.TeacherClient.Dispose();
        }

        Assert.Inconclusive("Không tạo được yêu cầu mở khóa pending; có thể dữ liệu test đã có pending request cho các buổi phù hợp.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<List<SubmittedAttendanceContext>> GetSubmittedAttendanceContextsAsync()
    {
        var sessions = await GetCandidateSessionsAsync();
        var result = new List<SubmittedAttendanceContext>();

        foreach (var session in sessions)
        {
            foreach (var teacherEmail in TeacherEmails)
            {
                var token = await LoginAsync(teacherEmail);
                var teacherClient = CreateClient(token);
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

                result.Add(new SubmittedAttendanceContext(session.MaBuoiHoc, teacherClient));
                if (result.Count >= 8)
                {
                    return result;
                }
            }
        }

        if (result.Count == 0)
        {
            Assert.Inconclusive("Không tìm được BuoiHoc đã gửi/đã khóa với teacher seed phù hợp để test P0-7.");
        }

        return result;
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
            Assert.Inconclusive("Không có BuoiHoc phù hợp để test P0-7.");
        }

        return sessions;
    }

    private static SessionCandidate ReadSessionCandidate(JsonElement item)
    {
        return new SessionCandidate(
            GetInt32(item, "maBuoiHoc"),
            GetOptionalString(item, "trangThaiBuoi"),
            GetOptionalString(item, "trangThaiDiemDanh"));
    }

    private async Task<bool> TeacherCanViewAttendanceAsync(HttpClient teacherClient, int sessionId)
    {
        using var response = await teacherClient.GetAsync($"api/buoi-hoc/{sessionId}/attendance");
        return response.StatusCode == HttpStatusCode.OK;
    }

    private async Task<int> GetFirstAttendanceStudentIdAsync(HttpClient teacherClient, int sessionId)
    {
        using var response = await teacherClient.GetAsync($"api/buoi-hoc/{sessionId}/attendance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var students = GetRequiredProperty(data, "students").EnumerateArray().ToList();
        if (students.Count == 0)
        {
            Assert.Inconclusive("Buổi học không có sinh viên trong danh sách điểm danh.");
        }

        return GetInt32(students[0], "maHocSinh");
    }

    private async Task RejectIfPendingAsync(int requestId)
    {
        using var response = await Client.PostAsJsonAsync(
            $"api/admin/attendance/unlock-requests/{requestId}/reject",
            new { lyDoTuChoi = "NUnit P0-7 cleanup pending request" });

        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.BadRequest)
        {
            Assert.Fail($"Không cleanup được unlock request {requestId}. {await DescribeResponseAsync(response)}");
        }
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

    private HttpClient CreateClient(string token)
    {
        var client = new HttpClient
        {
            BaseAddress = BaseUri
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private sealed record SessionCandidate(
        int MaBuoiHoc,
        string? TrangThaiBuoi,
        string? TrangThaiDiemDanh);

    private sealed record SubmittedAttendanceContext(
        int SessionId,
        HttpClient TeacherClient);

    private sealed record PendingUnlockContext(
        int SessionId,
        HttpClient TeacherClient,
        int RequestId,
        string Status);
}
