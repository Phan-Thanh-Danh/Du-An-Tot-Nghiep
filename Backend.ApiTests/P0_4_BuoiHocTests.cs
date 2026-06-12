using System.Net;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_4_BuoiHocTests : ApiTestBase
{
    [Test]
    public async Task GenerateSessions_WithPublishedTimetable_ShouldReturnOk()
    {
        var timetable = await GetPublishedTimetableAsync();

        using var response = await Client.PostAsync($"api/thoi-khoa-bieu/{timetable.MaTkb}/generate-sessions", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maTkb"), Is.EqualTo(timetable.MaTkb));
        Assert.That(GetInt32(data, "totalDates"), Is.GreaterThan(0));
        Assert.That(GetInt32(data, "created"), Is.GreaterThanOrEqualTo(0));
        Assert.That(GetInt32(data, "skippedExisting"), Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task GenerateSessions_SameTimetableTwice_ShouldNotCreateDuplicates()
    {
        var timetable = await GetPublishedTimetableAsync();

        await GenerateSessionsAsync(timetable.MaTkb);
        var secondResult = await GenerateSessionsAsync(timetable.MaTkb);

        Assert.That(secondResult.TotalDates, Is.GreaterThan(0));
        Assert.That(secondResult.Created, Is.EqualTo(0));
        Assert.That(secondResult.SkippedExisting, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetBuoiHocList_ShouldReturnOk()
    {
        using var response = await Client.GetAsync("api/buoi-hoc?pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(HasProperty(data, "items"), Is.True);
        Assert.That(GetInt32(data, "pageIndex"), Is.EqualTo(1));
        Assert.That(GetInt32(data, "pageSize"), Is.EqualTo(20));
    }

    [Test]
    public async Task GetBuoiHoc_ByMaTkb_ShouldReturnItemsAfterGenerate()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);

        using var response = await Client.GetAsync($"api/buoi-hoc?maTkb={timetable.MaTkb}&pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var items = ReadBuoiHocItems(root.RootElement).ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(items.All(x => x.MaTkb == timetable.MaTkb), Is.True);
        Assert.That(items.All(x => string.Equals(x.TrangThaiBuoi, "du_kien", StringComparison.OrdinalIgnoreCase) ||
                                   !string.IsNullOrWhiteSpace(x.TrangThaiBuoi)), Is.True);
        Assert.That(items.All(x => string.Equals(x.TrangThaiDiemDanh, "chua_mo", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task GetBuoiHocDetail_ShouldReturnOk()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);
        var session = await GetFirstSessionByTimetableAsync(timetable.MaTkb);

        using var response = await Client.GetAsync($"api/buoi-hoc/{session.MaBuoiHoc}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetInt32(data, "maBuoiHoc"), Is.EqualTo(session.MaBuoiHoc));
        Assert.That(GetInt32(data, "maTkb"), Is.EqualTo(timetable.MaTkb));
        Assert.That(GetRequiredString(data, "ngayHoc"), Is.Not.Empty);
        Assert.That(GetInt32(data, "maCaHoc"), Is.GreaterThan(0));
        Assert.That(GetInt32(data, "maPhong"), Is.GreaterThan(0));
        Assert.That(GetInt32(data, "maGiaoVien"), Is.GreaterThan(0));
    }

    [Test]
    public async Task GenerateSessions_WithDraftTimetable_ShouldReturnBadRequest()
    {
        var timetable = await GetDraftTimetableAsync();

        using var response = await Client.PostAsync($"api/thoi-khoa-bieu/{timetable.MaTkb}/generate-sessions", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var message = GetOptionalString(root.RootElement, "message") ?? string.Empty;
        var errors = HasProperty(root.RootElement, "errors")
            ? string.Join(" ", GetRequiredProperty(root.RootElement, "errors").EnumerateArray().Select(x => x.GetString()))
            : string.Empty;
        var combined = $"{message} {errors}";

        Assert.That(combined, Does.Contain("xuất bản").IgnoreCase);
    }

    [Test]
    public async Task GenerateSessions_WithCanceledTimetable_ShouldReturnBadRequest()
    {
        var timetable = await GetCanceledTimetableAsync();

        using var response = await Client.PostAsync($"api/thoi-khoa-bieu/{timetable.MaTkb}/generate-sessions", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.False);
    }

    [Test]
    public async Task GetBuoiHoc_FilterByKhoaHoc_ShouldReturnOnlyThatCourse()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);

        using var response = await Client.GetAsync($"api/buoi-hoc?maKhoaHoc={timetable.MaKhoaHoc}&pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = ReadBuoiHocItems(root.RootElement).ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(items.All(x => x.MaKhoaHoc == timetable.MaKhoaHoc), Is.True);
    }

    [Test]
    public async Task GetBuoiHoc_FilterByDateRange_ShouldReturnOnlyDatesInRange()
    {
        var timetable = await GetPublishedTimetableAsync();
        await GenerateSessionsAsync(timetable.MaTkb);
        var firstSession = await GetFirstSessionByTimetableAsync(timetable.MaTkb);
        var date = DateOnly.Parse(firstSession.NgayHoc);

        using var response = await Client.GetAsync($"api/buoi-hoc?ngayTu={date:yyyy-MM-dd}&ngayDen={date:yyyy-MM-dd}&pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var root = await GetRootAsync(response);
        var items = ReadBuoiHocItems(root.RootElement).ToList();
        Assert.That(items, Is.Not.Empty);
        Assert.That(items.All(x =>
        {
            var ngayHoc = DateOnly.Parse(x.NgayHoc);
            return ngayHoc >= date && ngayHoc <= date;
        }), Is.True);
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient
        {
            BaseAddress = BaseUri
        };

        using var response = await anonymousClient.GetAsync("api/buoi-hoc?pageIndex=1&pageSize=20");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }
}
