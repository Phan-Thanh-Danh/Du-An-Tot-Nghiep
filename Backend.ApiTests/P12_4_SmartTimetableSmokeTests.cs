using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class P12_4_SmartTimetableSmokeTests : ApiTestBase
{
    private const int MaDonVi = 1;
    private const int MaHocKy = 1;
    private const int TotalCourses = 20;

    /// <summary>
    /// P12.3 — Group A: generate draft seeded DB
    /// </summary>
    [Test]
    [Order(1)]
    public async Task Generate_WithSeededData_ShouldProcessAllCourses()
    {
        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu/generate", new
        {
            maHocKy = MaHocKy,
            maDonVi = MaDonVi
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Generate failed: {body}");
        Assert.That(body, Does.Contain("success").And.Contain("data"), $"Unexpected response: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var draftId = GetRequiredString(data, "draftId");
        var trangThai = GetRequiredString(data, "trangThai");
        var tongCourse = GetInt32(data, "tongCourse");
        var soXepDuoc = GetInt32(data, "soXepDuoc");
        var soKhongXepDuoc = GetInt32(data, "soKhongXepDuoc");
        var score = data.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : 0;

        Assert.That(Guid.TryParse(draftId, out _), Is.True, $"draftId không phải GUID: {draftId}");
        Assert.That(trangThai, Is.EqualTo("draft"));
        Assert.That(tongCourse, Is.EqualTo(TotalCourses), $"Phải xử lý {TotalCourses} courses");
        Assert.That(soXepDuoc + soKhongXepDuoc, Is.EqualTo(TotalCourses));
        Assert.That(soXepDuoc, Is.GreaterThan(0), $"Phải xếp được ít nhất 1 course (soXepDuoc={soXepDuoc})");
        TestContext.WriteLine($"Draft {draftId}: xepDuoc={soXepDuoc}, khongXepDuoc={soKhongXepDuoc}, score={score:F2}");

        var items = GetRequiredProperty(data, "items");
        Assert.That(items.GetArrayLength(), Is.EqualTo(TotalCourses));
        foreach (var item in items.EnumerateArray())
        {
            var itemStatus = GetRequiredString(item, "trangThai");
            Assert.That(itemStatus, Is.AnyOf("xep_duoc", "khong_xep_duoc"),
                $"Item {item} có trạng thái không hợp lệ: {itemStatus}");
        }

        // Lưu draftId để test tiếp
        _lastDraftId = Guid.Parse(draftId);
        _lastDraftJson = data;

        if (soKhongXepDuoc > 0)
        {
            var failedItems = items.EnumerateArray()
                .Where(i => GetRequiredString(i, "trangThai") == "khong_xep_duoc")
                .ToList();
            foreach (var fi in failedItems)
            {
                var loi = GetRequiredProperty(fi, "loi");
                var maKhoaHoc = GetInt32(fi, "maKhoaHoc");
                var loiText = string.Join("; ", loi.EnumerateArray().Select(l => l.GetString()));
                TestContext.WriteLine($"  Course {maKhoaHoc}: {loiText}");
            }
        }
    }

    [Test]
    [Order(2)]
    public async Task GetDraft_AfterGenerate_ShouldMatchGeneratedData()
    {
        Assert.That(_lastDraftId, Is.Not.EqualTo(Guid.Empty), "Chưa có draftId từ test Generate");

        using var response = await Client.GetAsync($"api/thoi-khoa-bieu/drafts/{_lastDraftId}");
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Get draft failed: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var draftId = GetRequiredString(data, "draftId");
        Assert.That(draftId, Is.EqualTo(_lastDraftId.ToString()));
        Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo("draft"));
        Assert.That(GetInt32(data, "tongCourse"), Is.EqualTo(TotalCourses));
    }

    [Test]
    [Order(3)]
    public async Task ListDrafts_AfterGenerate_ShouldContainOurDraft()
    {
        using var response = await Client.GetAsync($"api/thoi-khoa-bieu/drafts?maDonVi={MaDonVi}&maHocKy={MaHocKy}");
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"List drafts failed: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var items = data.ValueKind == JsonValueKind.Array
            ? data
            : GetRequiredProperty(data, "items");

        var found = items.EnumerateArray().Any(i =>
        {
            var id = GetOptionalString(i, "draftId");
            return id == _lastDraftId.ToString();
        });
        Assert.That(found, Is.True, $"Không tìm thấy draft {_lastDraftId} trong danh sách");
    }

    [Test]
    [Order(4)]
    public async Task Publish_ValidDraft_ShouldCreateSessions()
    {
        Assert.That(_lastDraftId, Is.Not.EqualTo(Guid.Empty), "Chưa có draftId");

        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu/publish", new
        {
            draftId = _lastDraftId
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Publish failed: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var buoiHocDaTao = GetInt32(data, "buoiHocDaTao");
        var buoiHocLoi = GetInt32(data, "buoiHocLoi");

        Assert.That(buoiHocDaTao, Is.GreaterThan(0), $"Phải tạo được ít nhất 1 buổi học (created={buoiHocDaTao})");
        Assert.That(data.TryGetProperty("success", out var successProp) ? successProp.GetBoolean() : true, Is.True,
            "Publish result success=false");
        TestContext.WriteLine($"Publish: created={buoiHocDaTao}, errors={buoiHocLoi}");

        if (buoiHocLoi > 0)
        {
            var chiTietLoi = GetRequiredProperty(data, "chiTietLoi");
            foreach (var err in chiTietLoi.EnumerateArray())
            {
                TestContext.WriteLine($"  Error: {err.GetString()}");
            }
        }

        _published = true;
    }

    [Test]
    [Order(5)]
    public async Task DeleteDraft_AfterPublish_ShouldFail()
    {
        Assert.That(_published, Is.True, "Cần publish trước khi test delete");

        using var response = await Client.DeleteAsync($"api/thoi-khoa-bieu/drafts/{_lastDraftId}");
        var body = await response.Content.ReadAsStringAsync();
        Assert.That((int)response.StatusCode, Is.EqualTo(400).Or.EqualTo(409),
            $"Delete published draft phải trả 400/409, nhận: {(int)response.StatusCode}");
        TestContext.WriteLine($"Delete published draft đúng: {body}");
    }

    [Test]
    [Order(6)]
    public async Task CheckXungDot_Batch_ShouldDetectConflicts()
    {
        var request = new
        {
            maHocKy = MaHocKy,
            maDonVi = MaDonVi,
            items = new[]
            {
                new { maKhoaHoc = 1, thuTrongTuan = 2, maCaHoc = 1, maPhong = 1 },
                new { maKhoaHoc = 999, thuTrongTuan = 3, maCaHoc = 2, maPhong = 5 }
            }
        };

        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu/check-xung-dot-batch", request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Conflict check failed: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var results = GetRequiredProperty(data, "results");
        Assert.That(results.GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    [Order(7)]
    public async Task Generate_WithCourseFilter_ShouldOnlyScheduleFilteredCourses()
    {
        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu/generate", new
        {
            maHocKy = MaHocKy,
            maDonVi = MaDonVi,
            maKhoaHocFilter = new[] { 1, 6, 11 }
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.IsSuccessStatusCode, Is.True, $"Filtered generate failed: {body}");

        using var root = JsonDocument.Parse(body);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "tongCourse"), Is.EqualTo(3));
    }

    [Test]
    [Order(8)]
    public async Task Publish_AlreadyPublishedDraft_ShouldFail()
    {
        Assert.That(_published, Is.True);

        using var response = await Client.PostAsJsonAsync("api/thoi-khoa-bieu/publish", new
        {
            draftId = _lastDraftId
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.That((int)response.StatusCode, Is.EqualTo(400).Or.EqualTo(409),
            $"Publish again phải fail, nhận: {(int)response.StatusCode}. Body={body}");
        TestContext.WriteLine($"Publish again đúng fail: {body}");
    }

    // ============================================================
    // Fixture state
    // ============================================================
    private Guid _lastDraftId;
    private JsonElement _lastDraftJson;
    private bool _published;
}
