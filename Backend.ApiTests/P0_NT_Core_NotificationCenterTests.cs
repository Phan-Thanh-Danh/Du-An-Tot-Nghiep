using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_NT_Core_NotificationCenterTests : ApiTestBase
{
    [Test]
    public async Task SuperAdmin_PreviewAllSystem_ShouldReturnActiveUserCount()
    {
        var expectedCount = await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.NguoiDung WHERE trang_thai = N'hoat_dong'");

        using var response = await Client.PostAsJsonAsync("api/admin/notifications/preview-recipients", new
        {
            phamViGui = "toan_he_thong",
            targetType = "toan_he_thong"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "count"), Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task SuperAdmin_CreateAllSystem_ShouldCreateOneNotificationAndManyRecipients()
    {
        var title = UniqueTitle("all-system");
        var expectedCount = await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.NguoiDung WHERE trang_thai = N'hoat_dong'");

        var id = await CreateNotificationAsync(new
        {
            tieuDe = title,
            noiDungJson = EditorJson("Thông báo toàn hệ thống"),
            mucDo = "important",
            phamViGui = "toan_he_thong",
            targetType = "toan_he_thong"
        });

        var contentRows = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.ThongBao WHERE ma_thong_bao = @id",
            new SqlParameter("@id", id));
        var recipientRows = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.ThongBaoNguoiNhan WHERE ma_thong_bao = @id",
            new SqlParameter("@id", id));

        Assert.That(contentRows, Is.EqualTo(1));
        Assert.That(recipientRows, Is.EqualTo(expectedCount));
    }

    [Test]
    public async Task CampusAdmin_CreateCampusNotification_InScope_ShouldReturnOk()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        using var campusClient = await CreateAuthenticatedClientAsync("campusadmin.hcm@lms.local");

        using var response = await campusClient.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = UniqueTitle("campus"),
            noiDungJson = EditorJson("Thông báo trong cơ sở"),
            mucDo = "info",
            phamViGui = "don_vi",
            targetType = "campus",
            targetIds = new[] { student.MaDonVi }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Admin_CreateCampusNotification_OutOfScope_ShouldReturnForbidden()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@lms.local");

        using var response = await adminClient.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = UniqueTitle("forbidden-campus"),
            noiDungJson = EditorJson("Không được gửi ngoài scope"),
            mucDo = "info",
            phamViGui = "don_vi",
            targetType = "campus",
            targetIds = new[] { student.MaDonVi }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ResolveClass_ShouldOnlyCreateRecipientsForStudentsInClass()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        Assert.That(student.MaLop, Is.Not.Null);
        var expected = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.NguoiDung WHERE ma_lop = @classId AND vai_tro_chinh = N'hoc_sinh' AND trang_thai = N'hoat_dong'",
            new SqlParameter("@classId", student.MaLop!.Value));

        var id = await CreateNotificationAsync(new
        {
            tieuDe = UniqueTitle("class"),
            noiDungJson = EditorJson("Thông báo lớp"),
            mucDo = "info",
            phamViGui = "lop_hanh_chinh",
            targetType = "class",
            targetIds = new[] { student.MaLop.Value }
        });

        var actual = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.ThongBaoNguoiNhan WHERE ma_thong_bao = @id",
            new SqlParameter("@id", id));
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task ResolveUser_OutOfScope_ShouldReturnForbidden()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        using var adminClient = await CreateAuthenticatedClientAsync("admin@lms.local");

        using var response = await adminClient.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = UniqueTitle("forbidden-user"),
            noiDungJson = EditorJson("Không được gửi user ngoài scope"),
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { student.MaNguoiDung }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task DuplicateRecipients_ShouldCreateOneRecipientRow()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var id = await CreateNotificationAsync(new
        {
            tieuDe = UniqueTitle("duplicate"),
            noiDungJson = EditorJson("Trùng recipient"),
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { student.MaNguoiDung, student.MaNguoiDung }
        });

        var count = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.ThongBaoNguoiNhan WHERE ma_thong_bao = @id AND ma_nguoi_nhan = @userId",
            new SqlParameter("@id", id),
            new SqlParameter("@userId", student.MaNguoiDung));
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task ZeroRecipients_ShouldNotCreateNotification()
    {
        var title = UniqueTitle("zero");
        using var response = await Client.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = title,
            noiDungJson = EditorJson("Không có recipient"),
            mucDo = "info",
            phamViGui = "vai_tro",
            targetType = "vai_tro",
            roleCodes = new[] { "role_khong_ton_tai" }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
        var count = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.ThongBao WHERE tieu_de = @title",
            new SqlParameter("@title", title));
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task EditorJson_InvalidJson_ShouldReturn400()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        using var response = await Client.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = UniqueTitle("bad-json"),
            noiDungJson = "{ invalid",
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { student.MaNguoiDung }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task EditorJson_Base64Image_ShouldReturn400()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        using var response = await Client.PostAsJsonAsync("api/admin/notifications", new
        {
            tieuDe = UniqueTitle("base64"),
            noiDungJson = "{\"time\":1,\"blocks\":[{\"type\":\"image\",\"data\":{\"file\":{\"url\":\"data:image/png;base64,AAAA\"}}}]}",
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { student.MaNguoiDung }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UserList_ShouldOnlyReturnCurrentUserNotifications()
    {
        var studentA = await GetUserAsync("student.cntt01@lms.local");
        var studentB = await GetUserAsync("student.tkdh01@lms.local");
        var titleA = UniqueTitle("student-a");
        var titleB = UniqueTitle("student-b");
        await CreateNotificationAsync(titleA, studentA.MaNguoiDung);
        await CreateNotificationAsync(titleB, studentB.MaNguoiDung);

        using var clientA = await CreateAuthenticatedClientAsync(studentA.Email);
        using var response = await clientA.GetAsync($"api/notifications/me?keyword={Uri.EscapeDataString(titleA[..18])}&pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var titles = GetDataItems(root.RootElement).EnumerateArray()
            .Select(x => GetRequiredString(x, "tieuDe"))
            .ToList();

        Assert.That(titles, Has.Some.EqualTo(titleA));
        Assert.That(titles, Has.None.EqualTo(titleB));
    }

    [Test]
    public async Task UserDetail_OtherUserNotification_ShouldReturn404()
    {
        var studentA = await GetUserAsync("student.cntt01@lms.local");
        var studentB = await GetUserAsync("student.tkdh01@lms.local");
        var idForA = await CreateNotificationAsync(UniqueTitle("private"), studentA.MaNguoiDung);
        using var clientB = await CreateAuthenticatedClientAsync(studentB.Email);

        using var response = await clientB.GetAsync($"api/notifications/{idForA}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UnreadCount_AndMarkRead_ShouldUseRecipientState()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var id = await CreateNotificationAsync(UniqueTitle("read"), student.MaNguoiDung);
        using var client = await CreateAuthenticatedClientAsync(student.Email);

        using var unreadBefore = await client.GetAsync("api/notifications/me/unread-count");
        Assert.That(unreadBefore.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(unreadBefore));

        using var read1 = await client.PatchAsync($"api/notifications/{id}/read", null);
        using var read2 = await client.PatchAsync($"api/notifications/{id}/read", null);
        Assert.That(read1.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(read1));
        Assert.That(read2.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(read2));

        var row = await QueryRecipientStateAsync(id, student.MaNguoiDung);
        Assert.That(row.DaDoc, Is.True);
        Assert.That(row.DocLuc, Is.Not.Null);
    }

    [Test]
    public async Task MarkAllRead_ShouldOnlyUpdateCurrentUser()
    {
        var studentA = await GetUserAsync("student.cntt01@lms.local");
        var studentB = await GetUserAsync("student.tkdh01@lms.local");
        var idA = await CreateNotificationAsync(UniqueTitle("read-all-a"), studentA.MaNguoiDung);
        var idB = await CreateNotificationAsync(UniqueTitle("read-all-b"), studentB.MaNguoiDung);
        using var clientA = await CreateAuthenticatedClientAsync(studentA.Email);

        using var response = await clientA.PatchAsync("api/notifications/read-all", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        Assert.That((await QueryRecipientStateAsync(idA, studentA.MaNguoiDung)).DaDoc, Is.True);
        Assert.That((await QueryRecipientStateAsync(idB, studentB.MaNguoiDung)).DaDoc, Is.False);
    }

    [Test]
    public async Task HideNotification_ShouldNotDeleteOriginalRows()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var id = await CreateNotificationAsync(UniqueTitle("hide"), student.MaNguoiDung);
        using var client = await CreateAuthenticatedClientAsync(student.Email);

        using var response = await client.DeleteAsync($"api/notifications/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        var contentRows = await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.ThongBao WHERE ma_thong_bao = @id", new SqlParameter("@id", id));
        var state = await QueryRecipientStateAsync(id, student.MaNguoiDung);
        Assert.That(contentRows, Is.EqualTo(1));
        Assert.That(state.DaAn, Is.True);
    }

    [Test]
    public async Task AdminList_ShouldReturnRecipientReadUnreadHiddenCounts()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var title = UniqueTitle("admin-list");
        var id = await CreateNotificationAsync(title, student.MaNguoiDung);
        using var studentClient = await CreateAuthenticatedClientAsync(student.Email);
        using var _ = await studentClient.PatchAsync($"api/notifications/{id}/read", null);

        using var response = await Client.GetAsync($"api/admin/notifications?keyword={Uri.EscapeDataString(title)}&pageIndex=1&pageSize=10");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var item = GetDataItems(root.RootElement).EnumerateArray().First();
        Assert.That(GetInt32(item, "recipientCount"), Is.EqualTo(1));
        Assert.That(GetInt32(item, "readCount"), Is.EqualTo(1));
        Assert.That(GetInt32(item, "unreadCount"), Is.EqualTo(0));
        Assert.That(GetInt32(item, "hiddenCount"), Is.EqualTo(0));
    }

    [Test]
    public async Task AdminRecipients_ShouldPageRecipientRows()
    {
        var studentA = await GetUserAsync("student.cntt01@lms.local");
        var studentB = await GetUserAsync("student.tkdh01@lms.local");
        var id = await CreateNotificationAsync(new
        {
            tieuDe = UniqueTitle("recipients"),
            noiDungJson = EditorJson("Recipient paging"),
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { studentA.MaNguoiDung, studentB.MaNguoiDung }
        });

        using var response = await Client.GetAsync($"api/admin/notifications/{id}/recipients?pageIndex=1&pageSize=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetDataItems(root.RootElement).GetArrayLength(), Is.EqualTo(1));
        Assert.That(GetInt32(data, "totalItems"), Is.EqualTo(2));
    }

    [Test]
    public async Task AdminScope_ShouldNotExposeOtherCampusNotification()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var id = await CreateNotificationAsync(UniqueTitle("scope"), student.MaNguoiDung);
        using var adminClient = await CreateAuthenticatedClientAsync("admin@lms.local");

        using var response = await adminClient.GetAsync($"api/admin/notifications/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Statistics_ShouldReturnRecipientTotals()
    {
        var student = await GetUserAsync("student.cntt01@lms.local");
        var id = await CreateNotificationAsync(UniqueTitle("stats"), student.MaNguoiDung);

        using var response = await Client.GetAsync($"api/admin/notifications/{id}/statistics");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "tongNguoiNhan"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "tongChuaDoc"), Is.EqualTo(1));
    }

    [Test]
    public async Task Schema_ShouldHaveUniqueRecipientIndex()
    {
        var count = await ScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM sys.indexes
            WHERE name = N'UQ_ThongBaoNguoiNhan_ThongBao_NguoiNhan'
              AND is_unique = 1
            """);

        Assert.That(count, Is.EqualTo(1));
    }

    private async Task<int> CreateNotificationAsync(string title, int recipientId)
    {
        return await CreateNotificationAsync(new
        {
            tieuDe = title,
            noiDungJson = EditorJson("Thông báo test"),
            mucDo = "info",
            phamViGui = "nguoi_dung",
            targetType = "users",
            targetIds = new[] { recipientId }
        });
    }

    private async Task<int> CreateNotificationAsync(object request)
    {
        using var response = await Client.PostAsJsonAsync("api/admin/notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        return GetInt32(data, "maThongBao");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var token = GetRequiredString(root.RootElement, "accessToken");
        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<UserSeed> GetUserAsync(string email)
    {
        await using var connection = new SqlConnection(GetSharedTestConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP 1 ma_nguoi_dung, email, ma_don_vi, ma_lop
            FROM dbo.NguoiDung
            WHERE email = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            Assert.Fail($"Không tìm thấy seed user {email}.");
        }

        return new UserSeed(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetInt32(2),
            reader.IsDBNull(3) ? null : reader.GetInt32(3));
    }

    private static async Task<RecipientState> QueryRecipientStateAsync(int notificationId, int userId)
    {
        await using var connection = new SqlConnection(GetSharedTestConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP 1 da_doc, doc_luc, da_an
            FROM dbo.ThongBaoNguoiNhan
            WHERE ma_thong_bao = @id AND ma_nguoi_nhan = @userId
            """;
        command.Parameters.AddWithValue("@id", notificationId);
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            Assert.Fail($"Không tìm thấy recipient state cho notification {notificationId}, user {userId}.");
        }

        return new RecipientState(
            reader.GetBoolean(0),
            reader.IsDBNull(1) ? null : reader.GetDateTime(1),
            reader.GetBoolean(2));
    }

    private static async Task<T> ScalarAsync<T>(string sql, params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(GetSharedTestConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(result!, typeof(T));
    }

    private static string UniqueTitle(string prefix) => $"NUnit NT Core {prefix} {Guid.NewGuid():N}";

    private static string EditorJson(string text)
    {
        return JsonSerializer.Serialize(new
        {
            time = 1710000000000,
            blocks = new[]
            {
                new
                {
                    type = "paragraph",
                    data = new { text }
                }
            }
        });
    }

    private sealed record UserSeed(int MaNguoiDung, string Email, int MaDonVi, int? MaLop);

    private sealed record RecipientState(bool DaDoc, DateTime? DocLuc, bool DaAn);
}
