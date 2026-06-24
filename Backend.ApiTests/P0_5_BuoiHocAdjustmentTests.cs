using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_5_BuoiHocAdjustmentTests : ApiTestBase






































































{
    [Test]
    public async Task ChangeTeacher_WithValidSubstitute_ShouldReturnOk()
    {
        var session = await GetEditableSessionAsync();
        var substituteTeacherId = await FindAvailableTeacherAsync(session);

        using var response = await Client.PutAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/change-teacher",
            new
            {
                maGiaoVienDayThay = substituteTeacherId,
                lyDoThayDoi = "NUnit P0-5 đổi giáo viên dạy thay"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maGiaoVienDayThay"), Is.EqualTo(substituteTeacherId));
        Assert.That(GetRequiredString(data, "trangThaiBuoi"), Is.EqualTo("day_thay"));
        Assert.That(GetRequiredString(data, "loaiThayDoi"), Is.EqualTo("doi_giang_vien"));
    }

    [Test]
    public async Task ChangeRoom_WithValidRoom_ShouldReturnOk()
    {
        var session = await GetEditableSessionAsync();
        var room = await FindAvailableRoomAsync(session);

        using var response = await Client.PutAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/change-room",
            new
            {
                maPhong = room.MaPhong,
                lyDoThayDoi = "NUnit P0-5 đổi phòng"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maPhong"), Is.EqualTo(room.MaPhong));
        Assert.That(GetRequiredString(data, "trangThaiBuoi"), Is.EqualTo("doi_lich"));
        Assert.That(GetRequiredString(data, "loaiThayDoi"), Is.EqualTo("doi_phong"));
    }

    [Test]
    public async Task ChangeShift_WithValidShift_ShouldReturnOk()
    {
        var session = await GetEditableSessionAsync();
        var shift = await FindAvailableShiftAsync(session);

        using var response = await Client.PutAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/change-shift",
            new
            {
                maCaHoc = shift.MaCaHoc,
                lyDoThayDoi = "NUnit P0-5 đổi ca"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maCaHoc"), Is.EqualTo(shift.MaCaHoc));
        Assert.That(GetRequiredString(data, "trangThaiBuoi"), Is.EqualTo("doi_lich"));
        Assert.That(GetRequiredString(data, "loaiThayDoi"), Is.EqualTo("doi_ca"));
    }

    [Test]
    public async Task Cancel_WithEditableSession_ShouldReturnOk()
    {
        var session = await GetEditableSessionAsync();

        using var response = await Client.PatchAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/cancel",
            new
            {
                lyDoThayDoi = "NUnit P0-5 hủy buổi"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThaiBuoi"), Is.EqualTo("da_huy"));
        Assert.That(GetRequiredString(data, "loaiThayDoi"), Is.EqualTo("huy_buoi"));
    }

    [Test]
    public async Task ChangeCanceledSession_ShouldReturnBadRequest()
    {
        var session = await GetEditableSessionAsync();

        using var cancelResponse = await Client.PatchAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/cancel",
            new
            {
                lyDoThayDoi = "NUnit P0-5 chuẩn bị buổi đã hủy"
            });
        Assert.That(cancelResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(cancelResponse));

        using var response = await Client.PutAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/change-room",
            new
            {
                maPhong = session.MaPhong,
                lyDoThayDoi = "NUnit P0-5 thử đổi buổi đã hủy"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ChangeRoom_WithRoomConflict_ShouldReturnConflict()
    {
        var pair = await FindRoomConflictPairAsync();

        using var response = await Client.PutAsJsonAsync(
            $"api/buoi-hoc/{pair.Source.MaBuoiHoc}/change-room",
            new
            {
                maPhong = pair.Conflict.MaPhong,
                lyDoThayDoi = "NUnit P0-5 tạo xung đột phòng"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.False);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetBoolean(data, "hasConflict"), Is.True);
        var conflicts = GetRequiredProperty(data, "conflicts").EnumerateArray().ToList();
        Assert.That(conflicts.Any(x => string.Equals(GetRequiredString(x, "type"), "room", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task Unauthorized_ChangeRoom_ShouldReturn401()
    {
        var session = await GetEditableSessionAsync();

        using var anonymousClient = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var response = await anonymousClient.PutAsJsonAsync(
            $"api/buoi-hoc/{session.MaBuoiHoc}/change-room",
            new
            {
                maPhong = session.MaPhong,
                lyDoThayDoi = "NUnit P0-5 unauthorized"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    private async Task<SessionInfo> GetEditableSessionAsync()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);

        var sessions = await GetSessionsAsync($"api/buoi-hoc?maTkb={timetable.MaTkb}&pageIndex=1&pageSize=100");
        var editable = sessions.FirstOrDefault(IsEditable);
        if (editable is not null)
        {
            return await EnrichOrganizationAsync(editable);
        }

        Assert.Inconclusive($"Không có BuoiHoc có thể điều chỉnh cho maTkb={timetable.MaTkb}.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<int> FindAvailableTeacherAsync(SessionInfo session)
    {
        var courses = await GetCoursesAsync();
        var busyTeacherIds = (await GetSessionsAsync(
                $"api/buoi-hoc?ngayTu={session.NgayHoc}&ngayDen={session.NgayHoc}&maCaHoc={session.MaCaHoc}&pageIndex=1&pageSize=100"))
            .Where(x => x.MaBuoiHoc != session.MaBuoiHoc && !IsCanceled(x))
            .Select(x => x.MaGiaoVienDayThay ?? x.MaGiaoVien)
            .ToHashSet();

        var candidate = courses.FirstOrDefault(x =>
            x.MaDonVi == session.MaDonVi &&
            x.MaGiaoVien != session.MaGiaoVien &&
            !busyTeacherIds.Contains(x.MaGiaoVien));

        if (candidate is not null)
        {
            return candidate.MaGiaoVien;
        }

        Assert.Inconclusive("Không có giáo viên cùng cơ sở và rảnh cùng ngày/ca để test đổi giáo viên.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<RoomInfo> FindAvailableRoomAsync(SessionInfo session)
    {
        if (!session.MaDonVi.HasValue)
        {
            Assert.Inconclusive("Không xác định được cơ sở của buổi học để test đổi phòng.");
            throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
        }

        var rooms = await GetRoomsAsync(session.MaDonVi.Value);
        var busyRoomIds = (await GetSessionsAsync(
                $"api/buoi-hoc?ngayTu={session.NgayHoc}&ngayDen={session.NgayHoc}&maCaHoc={session.MaCaHoc}&pageIndex=1&pageSize=100"))
            .Where(x => x.MaBuoiHoc != session.MaBuoiHoc && !IsCanceled(x))
            .Select(x => x.MaPhong)
            .ToHashSet();

        var candidate = rooms.FirstOrDefault(x =>
            x.MaPhong != session.MaPhong &&
            !busyRoomIds.Contains(x.MaPhong));

        if (candidate is not null)
        {
            return candidate;
        }

        Assert.Inconclusive("Không có phòng cùng cơ sở và rảnh cùng ngày/ca để test đổi phòng.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<ShiftInfo> FindAvailableShiftAsync(SessionInfo session)
    {
        var shifts = await GetActiveShiftsAsync();
        var sessionsOnDate = await GetSessionsAsync(
            $"api/buoi-hoc?ngayTu={session.NgayHoc}&ngayDen={session.NgayHoc}&pageIndex=1&pageSize=100");

        foreach (var shift in shifts.Where(x => x.MaCaHoc != session.MaCaHoc))
        {
            var hasConflict = sessionsOnDate
                .Where(x => x.MaBuoiHoc != session.MaBuoiHoc && !IsCanceled(x) && x.MaCaHoc == shift.MaCaHoc)
                .Any(x =>
                    (x.MaGiaoVienDayThay ?? x.MaGiaoVien) == (session.MaGiaoVienDayThay ?? session.MaGiaoVien) ||
                    x.MaLop == session.MaLop ||
                    x.MaPhong == session.MaPhong);

            if (!hasConflict)
            {
                return shift;
            }
        }

        Assert.Inconclusive("Không có ca học rảnh cùng ngày để test đổi ca.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<(SessionInfo Source, SessionInfo Conflict)> FindRoomConflictPairAsync()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);
        var allSessions = await GetSessionsAsync("api/buoi-hoc?pageIndex=1&pageSize=100");
        var editableSessions = allSessions.Where(IsEditable).ToList();

        foreach (var source in editableSessions)
        {
            var conflict = allSessions.FirstOrDefault(x =>
                x.MaBuoiHoc != source.MaBuoiHoc &&
                !IsCanceled(x) &&
                x.NgayHoc == source.NgayHoc &&
                x.MaCaHoc == source.MaCaHoc &&
                x.MaPhong != source.MaPhong);

            if (conflict is not null)
            {
                return (source, conflict);
            }
        }

        Assert.Inconclusive("Không có 2 BuoiHoc cùng ngày/ca và khác phòng để test xung đột đổi phòng.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    private async Task<List<SessionInfo>> GetSessionsAsync(string url)
    {
        using var response = await Client.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(ReadSession)
            .ToList();
    }

    private async Task<List<CourseInfo>> GetCoursesAsync()
    {
        using var response = await Client.GetAsync("api/courses?pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(item => new CourseInfo(
                GetInt32(item, "maKhoaHoc"),
                GetInt32(item, "maDonVi"),
                GetInt32(item, "maGiaoVien"),
                GetInt32(item, "maLop")))
            .ToList();
    }

    private async Task<SessionInfo> EnrichOrganizationAsync(SessionInfo session)
    {
        var courses = await GetCoursesAsync();
        var course = courses.FirstOrDefault(x => x.MaKhoaHoc == session.MaKhoaHoc);
        if (course is null)
        {
            Assert.Inconclusive($"Không tìm thấy khóa học {session.MaKhoaHoc} để xác định cơ sở test.");
            throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
        }

        return session with
        {
            MaDonVi = course.MaDonVi
        };
    }

    private async Task<List<RoomInfo>> GetRoomsAsync(int organizationId)
    {
        using var response = await Client.GetAsync($"api/master-data/rooms?maDonVi={organizationId}&trangThaiPhong=hoat_dong&pageIndex=1&pageSize=100");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        return GetDataItems(root.RootElement)
            .EnumerateArray()
            .Select(item => new RoomInfo(
                GetInt32(item, "maPhong"),
                GetInt32(item, "maDonVi")))
            .ToList();
    }

    private async Task<List<ShiftInfo>> GetActiveShiftsAsync()
    {
        using var response = await Client.GetAsync("api/ca-hoc/active");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return data
            .EnumerateArray()
            .Select(item => new ShiftInfo(GetInt32(item, "maCaHoc")))
            .ToList();
    }

    private static SessionInfo ReadSession(JsonElement item)
    {
        return new SessionInfo(
            GetInt32(item, "maBuoiHoc"),
            GetInt32(item, "maTkb"),
            GetInt32(item, "maKhoaHoc"),
            GetOptionalInt32(item, "maHocKy"),
            GetInt32(item, "maLop"),
            GetInt32(item, "maCaHoc"),
            GetInt32(item, "maPhong"),
            GetInt32(item, "maGiaoVien"),
            GetOptionalInt32(item, "maGiaoVienDayThay"),
            GetRequiredString(item, "ngayHoc"),
            GetRequiredString(item, "trangThaiBuoi"),
            GetOptionalString(item, "trangThaiDiemDanh"),
            GetOptionalInt32(item, "maDonVi"));
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

    private static bool IsEditable(SessionInfo session)
    {
        return !string.Equals(session.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(session.TrangThaiBuoi, "da_dien_ra", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(session.TrangThaiDiemDanh, "da_khoa", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCanceled(SessionInfo session)
    {
        return string.Equals(session.TrangThaiBuoi, "da_huy", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record SessionInfo(
        int MaBuoiHoc,
        int MaTkb,
        int MaKhoaHoc,
        int? MaHocKy,
        int MaLop,
        int MaCaHoc,
        int MaPhong,
        int MaGiaoVien,
        int? MaGiaoVienDayThay,
        string NgayHoc,
        string TrangThaiBuoi,
        string? TrangThaiDiemDanh,
        int? MaDonVi);

    private sealed record CourseInfo(
        int MaKhoaHoc,
        int MaDonVi,
        int MaGiaoVien,
        int MaLop);

    private sealed record RoomInfo(
        int MaPhong,
        int MaDonVi);

    private sealed record ShiftInfo(int MaCaHoc);
}
