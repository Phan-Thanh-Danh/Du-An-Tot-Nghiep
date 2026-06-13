using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_6_AttendanceTests : ApiTestBase
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
    public async Task StartAttendance_ShouldCreateDefaultAbsentRows()
    {
        var context = await PrepareAttendanceContextAsync();

        using var response = await context.TeacherClient.PostAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/start",
            null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThaiDiemDanh"), Is.EqualTo("dang_diem_danh"));

        var students = GetRequiredProperty(data, "students").EnumerateArray().ToList();
        Assert.That(students, Is.Not.Empty);
        Assert.That(students.All(x => string.Equals(GetRequiredString(x, "trangThai"), "vang", StringComparison.OrdinalIgnoreCase)), Is.True);
        Assert.That(students.All(x => GetInt32(x, "heSoVang") == 1), Is.True);
    }

    [Test]
    public async Task StartAttendance_Twice_ShouldNotDuplicateRows()
    {
        var context = await PrepareAttendanceContextAsync();

        var firstCount = await StartAndGetStudentCountAsync(context);
        var secondCount = await StartAndGetStudentCountAsync(context);

        Assert.That(secondCount, Is.EqualTo(firstCount));
    }

    [Test]
    public async Task GetAttendanceDetail_ShouldReturnOk()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);

        using var response = await context.TeacherClient.GetAsync($"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maBuoiHoc"), Is.EqualTo(context.Session.MaBuoiHoc));
        Assert.That(GetRequiredProperty(data, "students").GetArrayLength(), Is.GreaterThan(0));
    }

    [Test]
    public async Task UpdateOneStudent_ShouldReturnOk()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);
        var student = await GetFirstAttendanceStudentAsync(context);

        using var response = await context.TeacherClient.PatchAsJsonAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/{student.MaHocSinh}",
            new { trangThai = "co_mat" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo("co_mat"));
        Assert.That(GetInt32(data, "heSoVang"), Is.EqualTo(0));
    }

    [Test]
    public async Task BulkUpdate_ShouldReturnOk()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);
        var students = await GetAttendanceStudentsAsync(context);
        if (students.Count == 0)
        {
            Assert.Inconclusive("Không có sinh viên để test bulk update.");
        }

        var statuses = new[] { "co_mat", "di_muon", "co_phep", "vang" };
        var items = students
            .Take(4)
            .Select((student, index) => new
            {
                maSinhVien = student.MaHocSinh,
                trangThai = statuses[index % statuses.Length]
            })
            .ToList();

        using var response = await context.TeacherClient.PutAsJsonAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/bulk",
            new { items });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var responseStudents = GetRequiredProperty(data, "students").EnumerateArray().ToList();
        foreach (var item in items)
        {
            var actual = responseStudents.First(x => GetInt32(x, "maHocSinh") == item.maSinhVien);
            Assert.That(GetRequiredString(actual, "trangThai"), Is.EqualTo(item.trangThai));
        }
    }

    [Test]
    public async Task SubmitAttendance_ShouldSetDaGui()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);

        using var response = await context.TeacherClient.PostAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/submit",
            null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThaiDiemDanh"), Is.EqualTo("da_gui"));
        Assert.That(HasProperty(data, "diemDanhDaGuiLuc"), Is.True);
    }

    [Test]
    public async Task UpdateAfterSubmit_ShouldReturnBadRequest()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);
        var student = await GetFirstAttendanceStudentAsync(context);

        using var submitResponse = await context.TeacherClient.PostAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/submit",
            null);
        Assert.That(submitResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(submitResponse));

        using var response = await context.TeacherClient.PatchAsJsonAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/{student.MaHocSinh}",
            new { trangThai = "co_mat" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task StartCanceledSession_ShouldReturnBadRequest()
    {
        var context = await PrepareAttendanceContextAsync();

        using var cancelResponse = await Client.PatchAsJsonAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/cancel",
            new { lyDoThayDoi = "NUnit P0-6 chuẩn bị buổi đã hủy" });
        Assert.That(cancelResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(cancelResponse));

        using var response = await context.TeacherClient.PostAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/start",
            null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        var session = await GetCandidateSessionAsync();
        using var anonymousClient = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var response = await anonymousClient.PostAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/attendance/start",
            null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task StudentAttendance_ShouldReturnOwnRecords()
    {
        var context = await PrepareAttendanceContextAsync();
        await StartAndGetStudentCountAsync(context);
        var studentToken = await LoginAsync("student.cntt01@lms.local");
        using var studentClient = CreateClient(studentToken);

        using var response = await studentClient.GetAsync("api/student/attendance?pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(HasProperty(data, "items"), Is.True);
    }

    [Test]
    public async Task NonAssignedTeacher_ShouldReturnForbidden()
    {
        var session = await GetCandidateSessionAsync();

        foreach (var teacherEmail in TeacherEmails)
        {
            var token = await LoginAsync(teacherEmail);
            using var teacherClient = CreateClient(token);
            using var response = await teacherClient.PostAsync(
                $"api/buoi-hoc/{session.MaBuoiHoc}/attendance/start",
                null);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                Assert.Pass();
            }
        }

        Assert.Inconclusive("Không tìm được teacher seed không phụ trách buổi học để test 403.");
    }

    private async Task<AttendanceTestContext> PrepareAttendanceContextAsync()
    {
        var sessions = await GetCandidateSessionsAsync();
        foreach (var session in sessions)
        {
            foreach (var teacherEmail in TeacherEmails)
            {
                var token = await LoginAsync(teacherEmail);
                var teacherClient = CreateClient(token);
                using var response = await teacherClient.PostAsync(
                    $"api/buoi-hoc/{session.MaBuoiHoc}/attendance/start",
                    null);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new AttendanceTestContext(session, teacherClient, teacherEmail);
                }

                teacherClient.Dispose();
            }
        }

        var createdSession = await CreateAttendanceReadySessionAsync();
        foreach (var teacherEmail in TeacherEmails)
        {
            var token = await LoginAsync(teacherEmail);
            var teacherClient = CreateClient(token);
            using var response = await teacherClient.PostAsync(
                $"api/buoi-hoc/{createdSession.MaBuoiHoc}/attendance/start",
                null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new AttendanceTestContext(createdSession, teacherClient, teacherEmail);
            }

            teacherClient.Dispose();
        }

        Assert.Inconclusive("Không tạo được buổi học có teacher/sinh viên active phù hợp để test điểm danh.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<SessionCandidate> GetCandidateSessionAsync()
    {
        var sessions = await GetCandidateSessionsAsync();
        var session = sessions.FirstOrDefault();
        if (session is not null)
        {
            return session;
        }

        Assert.Inconclusive("Không có BuoiHoc trạng thái chua_mo để test P0-6.");
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

        var result = new List<SessionCandidate>();
        foreach (var maTkb in timetables)
        {
            await GenerateSessionsAsync(maTkb);

            using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={maTkb}&pageIndex=1&pageSize=100");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

            using var root = await GetRootAsync(response);
            result.AddRange(GetDataItems(root.RootElement)
                .EnumerateArray()
                .Select(ReadSessionCandidate)
                .Where(x =>
                    !string.Equals(x.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.TrangThaiDiemDanh, "chua_mo", StringComparison.OrdinalIgnoreCase)));
        }

        if (result.Count == 0)
        {
            Assert.Inconclusive("Không có BuoiHoc trạng thái chua_mo để test P0-6.");
        }

        return result;
    }

    private async Task<SessionCandidate> CreateAttendanceReadySessionAsync()
    {
        var courses = await GetCourseCandidatesAsync();
        var shifts = await GetActiveShiftsAsync();
        if (courses.Count == 0)
        {
            Assert.Inconclusive("Không có course seed thuộc lớp có sinh viên active (SD1901/MKT1901/TKDH1901) để tạo dữ liệu test điểm danh.");
        }

        if (shifts.Count == 0)
        {
            Assert.Inconclusive("Không có ca học active để tạo TKB test điểm danh.");
        }

        foreach (var course in courses)
        {
            var rooms = await GetRoomsAsync(course.MaDonVi);
            if (rooms.Count == 0)
            {
                continue;
            }

            foreach (var dayOfWeek in Enumerable.Range(1, 7))
            {
                foreach (var shift in shifts)
                {
                    foreach (var room in rooms)
                    {
                        var createdTimetableId = await TryCreatePublishedTimetableAsync(course, room, shift, dayOfWeek);
                        if (!createdTimetableId.HasValue)
                        {
                            continue;
                        }

                        await GenerateSessionsAsync(createdTimetableId.Value);
                        var sessions = await GetSessionsByTimetableAsync(createdTimetableId.Value);
                        var session = sessions.FirstOrDefault(x =>
                            !string.Equals(x.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.TrangThaiDiemDanh, "chua_mo", StringComparison.OrdinalIgnoreCase));

                        if (session is not null)
                        {
                            return session;
                        }
                    }
                }
            }
        }

        Assert.Inconclusive("Không tạo được TKB/buổi học mới cho lớp có sinh viên active; có thể đã hết tổ hợp thứ/ca/phòng không xung đột.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<int?> TryCreatePublishedTimetableAsync(
        CourseCandidate course,
        RoomCandidate room,
        ShiftCandidate shift,
        int dayOfWeek)
    {
        if (!course.MaHocKy.HasValue)
        {
            return null;
        }

        var term = await GetTermAsync(course.MaHocKy.Value);
        var startDate = term.NgayBatDau;
        var endDate = startDate.AddDays(7) <= term.NgayKetThuc
            ? startDate.AddDays(7)
            : term.NgayKetThuc;

        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu", new
        {
            maKhoaHoc = course.MaKhoaHoc,
            thuTrongTuan = dayOfWeek,
            maCaHoc = shift.MaCaHoc,
            maPhong = room.MaPhong,
            ngayBatDau = startDate.ToString("yyyy-MM-dd"),
            ngayKetThuc = endDate.ToString("yyyy-MM-dd"),
            trangThai = "da_xuat_ban"
        });

        if (response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
        {
            return null;
        }

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return GetInt32(data, "maTkb");
    }

    private async Task<TermCandidate> GetTermAsync(int termId)
    {
        using var response = await Client.GetAsync($"api/master-data/academic-terms/{termId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return new TermCandidate(
            DateOnly.Parse(GetRequiredString(data, "ngayBatDau")),
            DateOnly.Parse(GetRequiredString(data, "ngayKetThuc")));
    }

    private async Task<List<SessionCandidate>> GetSessionsByTimetableAsync(int maTkb)
    {
        using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={maTkb}&pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadSessionCandidate)
            .ToList();
    }

    private async Task<List<CourseCandidate>> GetCourseCandidatesAsync()
    {
        using var response = await Client.GetAsync("api/courses?pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(item => new CourseCandidate(
                GetInt32(item, "maKhoaHoc"),
                GetInt32(item, "maDonVi"),
                GetOptionalInt32(item, "maHocKy"),
                GetRequiredString(item, "tenLop")))
            .Where(x =>
                x.TenLop.StartsWith("SD1901", StringComparison.OrdinalIgnoreCase) ||
                x.TenLop.StartsWith("MKT1901", StringComparison.OrdinalIgnoreCase) ||
                x.TenLop.StartsWith("TKDH1901", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<List<RoomCandidate>> GetRoomsAsync(int organizationId)
    {
        using var response = await Client.GetAsync($"api/master-data/rooms?maDonVi={organizationId}&trangThaiPhong=hoat_dong&pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(item => new RoomCandidate(GetInt32(item, "maPhong")))
            .ToList();
    }

    private async Task<List<ShiftCandidate>> GetActiveShiftsAsync()
    {
        using var response = await Client.GetAsync("api/ca-hoc/active");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return data.EnumerateArray()
            .Select(item => new ShiftCandidate(GetInt32(item, "maCaHoc")))
            .ToList();
    }

    private async Task<List<SessionCandidate>> GetAnySessionCandidatesAsync()
    {
        using var response = await Client.GetAsync("api/buoi-hoc?pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var result = GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadSessionCandidate)
            .Where(x =>
                !string.Equals(x.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (result.Count == 0)
        {
            Assert.Inconclusive("Không có BuoiHoc để test P0-6.");
        }

        return result;
    }

    private async Task<int> StartAndGetStudentCountAsync(AttendanceTestContext context)
    {
        using var response = await context.TeacherClient.PostAsync(
            $"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance/start",
            null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return GetRequiredProperty(data, "students").GetArrayLength();
    }

    private async Task<List<AttendanceStudent>> GetAttendanceStudentsAsync(AttendanceTestContext context)
    {
        using var response = await context.TeacherClient.GetAsync($"api/buoi-hoc/{context.Session.MaBuoiHoc}/attendance");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return GetRequiredProperty(data, "students")
            .EnumerateArray()
            .Select(x => new AttendanceStudent(GetInt32(x, "maHocSinh"), GetRequiredString(x, "trangThai")))
            .ToList();
    }

    private async Task<AttendanceStudent> GetFirstAttendanceStudentAsync(AttendanceTestContext context)
    {
        var students = await GetAttendanceStudentsAsync(context);
        if (students.Count == 0)
        {
            Assert.Inconclusive("Không có sinh viên trong danh sách điểm danh.");
        }

        return students[0];
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

    private static SessionCandidate ReadSessionCandidate(JsonElement item)
    {
        return new SessionCandidate(
            GetInt32(item, "maBuoiHoc"),
            GetInt32(item, "maKhoaHoc"),
            GetOptionalString(item, "trangThaiBuoi"),
            GetOptionalString(item, "trangThaiDiemDanh"));
    }

    private sealed record SessionCandidate(
        int MaBuoiHoc,
        int MaKhoaHoc,
        string? TrangThaiBuoi,
        string? TrangThaiDiemDanh);

    private sealed record AttendanceTestContext(
        SessionCandidate Session,
        HttpClient TeacherClient,
        string TeacherEmail);

    private sealed record AttendanceStudent(int MaHocSinh, string TrangThai);

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

    private sealed record CourseCandidate(int MaKhoaHoc, int MaDonVi, int? MaHocKy, string TenLop);

    private sealed record RoomCandidate(int MaPhong);

    private sealed record ShiftCandidate(int MaCaHoc);

    private sealed record TermCandidate(DateOnly NgayBatDau, DateOnly NgayKetThuc);
}
