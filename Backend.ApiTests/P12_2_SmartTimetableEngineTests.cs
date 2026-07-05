using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Data;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class ST_OccupationMapTests
{
    [Test]
    public void OccupyTeacher_ThenIsOccupied_ReturnsTrue()
    {
        var map = new OccupationMap();
        map.OccupyTeacher(1, 2, 3, 100);
        Assert.That(map.IsTeacherOccupied(1, 2, 3, 100), Is.True);
    }

    [Test]
    public void OccupyTeacher_DifferentSlot_NotOccupied()
    {
        var map = new OccupationMap();
        map.OccupyTeacher(1, 2, 3, 100);
        Assert.That(map.IsTeacherOccupied(1, 2, 4, 100), Is.False);
        Assert.That(map.IsTeacherOccupied(1, 2, 3, 101), Is.False);
        Assert.That(map.IsTeacherOccupied(2, 2, 3, 100), Is.False);
    }

    [Test]
    public void OccupyClass_ThenIsOccupied_ReturnsTrue()
    {
        var map = new OccupationMap();
        map.OccupyClass(1, 2, 3, 50);
        Assert.That(map.IsClassOccupied(1, 2, 3, 50), Is.True);
    }

    [Test]
    public void OccupyRoom_ThenIsOccupied_ReturnsTrue()
    {
        var map = new OccupationMap();
        map.OccupyRoom(1, 2, 3, 10);
        Assert.That(map.IsRoomOccupied(1, 2, 3, 10), Is.True);
    }

    [Test]
    public void Occupy_SameSlotTwice_DoesNotDuplicate()
    {
        var map = new OccupationMap();
        map.OccupyTeacher(1, 2, 3, 100);
        map.OccupyTeacher(1, 2, 3, 100);
        Assert.That(map.TeacherCount, Is.EqualTo(1));
    }

    [Test]
    public void Clear_ResetsAllOccupations()
    {
        var map = new OccupationMap();
        map.OccupyTeacher(1, 2, 3, 100);
        map.OccupyClass(1, 2, 3, 50);
        map.OccupyRoom(1, 2, 3, 10);
        map.Clear();
        Assert.Multiple(() =>
        {
            Assert.That(map.TeacherCount, Is.EqualTo(0));
            Assert.That(map.ClassCount, Is.EqualTo(0));
            Assert.That(map.RoomCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void IsTeacherOccupied_EmptyMap_ReturnsFalse()
    {
        var map = new OccupationMap();
        Assert.That(map.IsTeacherOccupied(1, 2, 3, 100), Is.False);
    }

    [Test]
    public void IsClassOccupied_EmptyMap_ReturnsFalse()
    {
        var map = new OccupationMap();
        Assert.That(map.IsClassOccupied(1, 2, 3, 50), Is.False);
    }

    [Test]
    public void IsRoomOccupied_EmptyMap_ReturnsFalse()
    {
        var map = new OccupationMap();
        Assert.That(map.IsRoomOccupied(1, 2, 3, 10), Is.False);
    }

    [Test]
    public void MultipleOccupations_CountsTrackedCorrectly()
    {
        var map = new OccupationMap();
        map.OccupyTeacher(1, 2, 3, 100);
        map.OccupyTeacher(1, 3, 3, 100);
        map.OccupyTeacher(1, 2, 4, 100);
        map.OccupyClass(1, 2, 3, 50);
        map.OccupyClass(1, 2, 3, 51);
        map.OccupyRoom(1, 2, 3, 10);
        Assert.Multiple(() =>
        {
            Assert.That(map.TeacherCount, Is.EqualTo(3));
            Assert.That(map.ClassCount, Is.EqualTo(2));
            Assert.That(map.RoomCount, Is.EqualTo(1));
        });
    }
}

[TestFixture]
[NonParallelizable]
public class ST_SmartTimetableApiTests : ApiTestBase
{
    private static string SmartTimetableEndpoint => "api/thoi-khoa-bieu/xep-lich-thong-minh";
    private static string DraftsEndpoint => "api/thoi-khoa-bieu/drafts";

    [Test]
    public async Task Generate_WithValidRequest_ShouldReturnDraft()
    {
        var (hocKy, donVi) = await GetTestKeysAsync();

        using var response = await Client.PostAsJsonAsync(SmartTimetableEndpoint, new
        {
            maHocKy = hocKy,
            maDonVi = donVi
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.TryGetProperty("draftId", out _), Is.True);
        Assert.That(GetOptionalString(data, "trangThai"), Is.EqualTo("draft"));
    }

    [Test]
    public async Task Generate_WithInvalidTerm_ShouldReturnBadRequest()
    {
        using var response = await Client.PostAsJsonAsync(SmartTimetableEndpoint, new
        {
            maHocKy = -1,
            maDonVi = 1
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Generate_WithInvalidOrganization_ShouldReturnForbidden()
    {
        var (hocKy, _) = await GetTestKeysAsync();

        using var response = await Client.PostAsJsonAsync(SmartTimetableEndpoint, new
        {
            maHocKy = hocKy,
            maDonVi = -1
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GetDraft_WithValidId_ShouldReturnDraft()
    {
        var draftId = await CreateSampleDraftAsync();

        using var response = await Client.GetAsync($"{DraftsEndpoint}/{draftId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        var returnedId = GetRequiredString(data, "draftId");
        Assert.That(returnedId, Is.EqualTo(draftId.ToString()));
    }

    [Test]
    public async Task GetDraft_WithInvalidId_ShouldReturnNotFound()
    {
        using var response = await Client.GetAsync($"{DraftsEndpoint}/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ListDrafts_ShouldReturnDrafts()
    {
        var (hocKy, donVi) = await GetTestKeysAsync();
        await CreateSampleDraftAsync();

        using var response = await Client.GetAsync($"{DraftsEndpoint}?maDonVi={donVi}&maHocKy={hocKy}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(data.GetArrayLength(), Is.GreaterThan(0));
    }

    [Test]
    public async Task CheckConflicts_WithValidRequest_ShouldReturnResults()
    {
        var (hocKy, donVi) = await GetTestKeysAsync();
        var course = await GetFirstCourseAsync();

        using var response = await Client.PostAsJsonAsync($"{SmartTimetableEndpoint}/check-xung-dot-batch", new
        {
            maHocKy = hocKy,
            maDonVi = donVi,
            items = new[]
            {
                new { maKhoaHoc = course, thuTrongTuan = 2, maCaHoc = 1, maPhong = (int?)null }
            }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        Assert.That(GetBoolean(root.RootElement, "success"), Is.True);

        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredProperty(data, "results").ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task DeleteDraft_WithValidId_ShouldSucceed()
    {
        var draftId = await CreateSampleDraftAsync();

        using var deleteResponse = await Client.DeleteAsync($"{DraftsEndpoint}/{draftId}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(deleteResponse));

        using var getResponse = await Client.GetAsync($"{DraftsEndpoint}/{draftId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(getResponse));
    }

    [Test]
    public async Task DeleteDraft_WithInvalidId_ShouldReturnNotFound()
    {
        using var response = await Client.DeleteAsync($"{DraftsEndpoint}/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient { BaseAddress = BaseUri };

        using var response = await anonymousClient.PostAsJsonAsync(SmartTimetableEndpoint, new
        {
            maHocKy = 1,
            maDonVi = 1
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    private async Task<(int HocKy, int DonVi)> GetTestKeysAsync()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(GetSharedTestConnectionString());
        await using var db = new ApplicationDbContext(optionsBuilder.Options);

        var term = await db.HocKys.AsNoTracking().FirstOrDefaultAsync();
        var campus = await db.DonVis.AsNoTracking().FirstOrDefaultAsync();

        Assert.That(term, Is.Not.Null, "Không tìm thấy HocKy trong DB test.");
        Assert.That(campus, Is.Not.Null, "Không tìm thấy DonVi trong DB test.");

        return (term!.MaHocKy, campus!.MaDonVi);
    }

    private async Task<int> GetFirstCourseAsync()
    {
        using var response = await Client.GetAsync("api/courses?pageIndex=1&pageSize=1");
        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var items = GetRequiredProperty(data, "items");
        foreach (var item in items.EnumerateArray())
        {
            return GetInt32(item, "maKhoaHoc");
        }
        Assert.Inconclusive("Không có KhoaHoc nào.");
        throw new InvalidOperationException("Unreachable.");
    }

    private async Task<Guid> CreateSampleDraftAsync()
    {
        var (hocKy, donVi) = await GetTestKeysAsync();

        using var response = await Client.PostAsJsonAsync(SmartTimetableEndpoint, new
        {
            maHocKy = hocKy,
            maDonVi = donVi
        });

        Assert.That(response.IsSuccessStatusCode, Is.True, await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var draftId = GetRequiredString(data, "draftId");
        return Guid.Parse(draftId);
    }
}
