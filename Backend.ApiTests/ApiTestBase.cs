using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

public abstract class ApiTestBase
{
    protected HttpClient Client { get; private set; } = null!;
    protected Uri BaseUri { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        var baseUrl = Environment.GetEnvironmentVariable("LMS_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5097";
        }

        BaseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        Client = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var loginResponse = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email = "superadmin@lms.local",
            password = "Admin@123"
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Login thất bại. Hãy kiểm tra backend đang chạy và DB đã seed. Status={(int)loginResponse.StatusCode}. Body={body}");
        }

        using var root = await GetRootAsync(loginResponse);
        var token = GetOptionalString(root.RootElement, "accessToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            Assert.Fail("Login thất bại. Response không có accessToken.");
        }

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client.Dispose();
    }

    protected static async Task<JsonDocument> GetRootAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            Assert.Fail("Response body rỗng.");
        }

        try
        {
            return JsonDocument.Parse(content);
        }
        catch (JsonException exception)
        {
            Assert.Fail($"Response không phải JSON hợp lệ. Body={content}. Error={exception.Message}");
            throw;
        }
    }

    protected async Task<TimetableItem> GetPublishedTimetableAsync()
    {
        using var response = await Client.GetAsync("api/thoi-khoa-bieu?trangThai=da_xuat_ban&pageIndex=1&pageSize=20");
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement);

        foreach (var item in items.EnumerateArray())
        {
            var timetable = ReadTimetable(item);
            if (string.Equals(timetable.TrangThai, "da_xuat_ban", StringComparison.OrdinalIgnoreCase) &&
                timetable.NgayBatDau is not null &&
                timetable.NgayKetThuc is not null)
            {
                return timetable;
            }
        }

        Assert.Inconclusive("Không có TKB da_xuat_ban có ngày bắt đầu/kết thúc để test generate.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    protected async Task<TimetableItem> GetDraftTimetableAsync()
    {
        using var response = await Client.GetAsync("api/thoi-khoa-bieu?trangThai=nhap&pageIndex=1&pageSize=20");
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement);
        foreach (var item in items.EnumerateArray())
        {
            return ReadTimetable(item);
        }

        Assert.Inconclusive("Không có TKB trạng thái nhap để test generate fail.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    protected async Task<TimetableItem> GetCanceledTimetableAsync()
    {
        using var response = await Client.GetAsync("api/thoi-khoa-bieu?trangThai=da_huy&pageIndex=1&pageSize=20");
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement);
        foreach (var item in items.EnumerateArray())
        {
            return ReadTimetable(item);
        }

        Assert.Inconclusive("Không có TKB da_huy để test generate fail.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    protected async Task<GenerateSessionsResult> GenerateSessionsAsync(int maTkb)
    {
        using var response = await Client.PostAsync($"api/thoi-khoa-bieu/{maTkb}/generate-sessions", null);
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);
        var data = GetRequiredProperty(root.RootElement, "data");

        return new GenerateSessionsResult(
            GetInt32(data, "maTkb"),
            GetInt32(data, "totalDates"),
            GetInt32(data, "created"),
            GetInt32(data, "skippedExisting"));
    }

    protected async Task<BuoiHocItem> GetFirstSessionByTimetableAsync(int maTkb)
    {
        using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={maTkb}&pageIndex=1&pageSize=1");
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = GetDataItems(root.RootElement);
        foreach (var item in items.EnumerateArray())
        {
            return ReadBuoiHoc(item);
        }

        Assert.Inconclusive($"Không có BuoiHoc nào cho maTkb={maTkb} sau khi generate.");
        throw new InvalidOperationException("Unreachable after Assert.Inconclusive.");
    }

    protected static IEnumerable<BuoiHocItem> ReadBuoiHocItems(JsonElement root)
    {
        var items = GetDataItems(root);
        foreach (var item in items.EnumerateArray())
        {
            yield return ReadBuoiHoc(item);
        }
    }

    protected static JsonElement GetDataItems(JsonElement root)
    {
        var data = GetRequiredProperty(root, "data");
        return GetRequiredProperty(data, "items");
    }

    protected static bool GetBoolean(JsonElement element, string propertyName)
    {
        return GetRequiredProperty(element, propertyName).GetBoolean();
    }

    protected static int GetInt32(JsonElement element, string propertyName)
    {
        return GetRequiredProperty(element, propertyName).GetInt32();
    }

    protected static string? GetOptionalString(JsonElement element, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var property) ||
            property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return property.GetString();
    }

    protected static string GetRequiredString(JsonElement element, string propertyName)
    {
        var value = GetOptionalString(element, propertyName);
        Assert.That(value, Is.Not.Null.And.Not.Empty, $"Thiếu field {propertyName}.");
        return value!;
    }

    protected static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        if (TryGetPropertyIgnoreCase(element, propertyName, out var property))
        {
            return property;
        }

        Assert.Fail($"Response thiếu field {propertyName}.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }

    protected static bool HasProperty(JsonElement element, string propertyName)
    {
        return TryGetPropertyIgnoreCase(element, propertyName, out _);
    }

    protected static async Task<string> DescribeResponseAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return $"Status={(int)response.StatusCode}. Body={body}";
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
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

    private static TimetableItem ReadTimetable(JsonElement item)
    {
        return new TimetableItem(
            GetInt32(item, "maTkb"),
            GetInt32(item, "maKhoaHoc"),
            GetInt32(item, "maCaHoc"),
            GetInt32(item, "maPhong"),
            GetOptionalString(item, "trangThai"),
            GetOptionalString(item, "ngayBatDau"),
            GetOptionalString(item, "ngayKetThuc"));
    }

    private static BuoiHocItem ReadBuoiHoc(JsonElement item)
    {
        return new BuoiHocItem(
            GetInt32(item, "maBuoiHoc"),
            GetInt32(item, "maTkb"),
            GetInt32(item, "maKhoaHoc"),
            GetInt32(item, "maCaHoc"),
            GetInt32(item, "maPhong"),
            GetInt32(item, "maGiaoVien"),
            GetRequiredString(item, "ngayHoc"),
            GetOptionalString(item, "trangThaiBuoi"),
            GetOptionalString(item, "trangThaiDiemDanh"));
    }

    protected sealed record TimetableItem(
        int MaTkb,
        int MaKhoaHoc,
        int MaCaHoc,
        int MaPhong,
        string? TrangThai,
        string? NgayBatDau,
        string? NgayKetThuc);

    protected sealed record GenerateSessionsResult(
        int MaTkb,
        int TotalDates,
        int Created,
        int SkippedExisting);

    protected sealed record BuoiHocItem(
        int MaBuoiHoc,
        int MaTkb,
        int MaKhoaHoc,
        int MaCaHoc,
        int MaPhong,
        int MaGiaoVien,
        string NgayHoc,
        string? TrangThaiBuoi,
        string? TrangThaiDiemDanh);
}
