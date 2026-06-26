using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Applications;
using Backend.Models;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT4_AdminApplicationQueueAssignmentTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string OtherStudentEmail = "student.tkdh01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string ParentEmail = "parent01@lms.local";
    private const string PrincipalEmail = "principal@lms.local";
    private const string CampusAdminEmail = "campusadmin.hcm@lms.local";
    private const string AdminEmail = "admin@lms.local";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string TestPrefix = "NUnit P0-DT4";

    [OneTimeSetUp]
    public void ValidateP0Dt4Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
        ValidateSharedStorageRoot();
    }

    [Test]
    public async Task Anonymous_Queue_ShouldReturn401()
    {
        using var client = new HttpClient { BaseAddress = BaseUri };
        using var response = await client.GetAsync("api/admin/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Student_Queue_ShouldReturn403()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var response = await studentClient.GetAsync("api/admin/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [TestCase(TeacherEmail)]
    [TestCase(ParentEmail)]
    public async Task NonAdminQueueRoles_ShouldReturn403(string email)
    {
        using var client = await CreateAuthenticatedClientAsync(email);
        using var response = await client.GetAsync("api/admin/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task LockedActor_Queue_ShouldReturn403()
    {
        await WithLoggedInThenMutatedUserAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), UserStatuses.DbLocked, async client =>
        {
            using var response = await client.GetAsync("api/admin/applications");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task DatabaseRoleChangedAfterLogin_ShouldUseDatabaseRole()
    {
        await WithLoggedInThenMutatedUserAsync(SuperAdminEmail, AuthRoles.ToDatabaseCode(AuthRoles.Teacher), UserStatuses.DbActive, async client =>
        {
            using var response = await client.GetAsync("api/admin/applications");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task Queue_Default_ShouldIncludeSubmittedAndInReviewOnly()
    {
        var submitted = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var inReview = await CreateApplicationAsync(ApplicationStatuses.InReview);
        var supplement = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement);
        var draft = await CreateApplicationAsync(ApplicationStatuses.Draft);
        var approved = await CreateApplicationAsync(ApplicationStatuses.Approved);

        using var response = await Client.GetAsync($"api/admin/applications?search={Uri.EscapeDataString(TestPrefix)}&pageIndex=1&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
        Assert.Multiple(() =>
        {
            Assert.That(ids, Does.Contain(submitted.MaDonTu));
            Assert.That(ids, Does.Contain(inReview.MaDonTu));
            Assert.That(ids, Does.Not.Contain(supplement.MaDonTu));
            Assert.That(ids, Does.Not.Contain(draft.MaDonTu));
            Assert.That(ids, Does.Not.Contain(approved.MaDonTu));
        });
    }

    [Test]
    public async Task Queue_ExplicitSupplementStatus_ShouldIncludeNeedSupplement()
    {
        var supplement = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement);

        using var response = await Client.GetAsync($"api/admin/applications?status={ApplicationStatuses.NeedSupplement}&type={ApplicationTypes.Confirmation}&assignmentState=all&slaStatus=all&search={Uri.EscapeDataString(TestPrefix)}&sortBy=submittedAt&sortDirection=asc&pageIndex=1&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
        Assert.That(ids, Does.Contain(supplement.MaDonTu));
    }

    [Test]
    public async Task Queue_FiltersAndSearch_ShouldReturnExpectedRows()
    {
        var assignee = await CreateAssignableUserAsync();
        var title = $"NUnit P0-DT4 searchable {Guid.NewGuid():N}";
        var submittedAt = DateTime.UtcNow.AddDays(-3);
        var application = await CreateApplicationAsync(
            ApplicationStatuses.Submitted,
            assigneeId: assignee,
            submittedAt: submittedAt,
            titleOverride: title);

        var urls = new[]
        {
            $"api/admin/applications?status={ApplicationStatuses.Submitted}&search={application.MaDonTu}",
            $"api/admin/applications?type={ApplicationTypes.Confirmation}&search={application.MaDonTu}",
            $"api/admin/applications?campusId={application.MaDonVi}&search={application.MaDonTu}",
            $"api/admin/applications?studentId={await GetUserIdAsync(StudentEmail)}&search={application.MaDonTu}",
            $"api/admin/applications?assigneeId={assignee}&search={application.MaDonTu}",
            $"api/admin/applications?assignmentState=all&search={application.MaDonTu}",
            $"api/admin/applications?assignmentState=assigned&search={application.MaDonTu}",
            $"api/admin/applications?submittedFrom={submittedAt:O}&submittedTo={submittedAt:O}&search={application.MaDonTu}",
            $"api/admin/applications?search={Uri.EscapeDataString(title)}",
            $"api/admin/applications?search={Uri.EscapeDataString(StudentEmail)}&pageIndex=1&pageSize=100",
            $"api/admin/applications?search={Uri.EscapeDataString(title)}&sortBy=submittedAt&sortDirection=asc&pageIndex=1&pageSize=10"
        };

        foreach (var url in urls)
        {
            using var response = await Client.GetAsync(url);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
            using var root = await GetRootAsync(response);
            var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
            Assert.That(ids, Does.Contain(application.MaDonTu), url);
        }
    }

    [Test]
    public async Task Queue_AssignmentStateAndSlaFilters_ShouldReturnExpectedRows()
    {
        var assignee = await CreateAssignableUserAsync();
        var unassigned = await CreateApplicationAsync(ApplicationStatuses.Submitted, titleOverride: $"NUnit P0-DT4 unassigned {Guid.NewGuid():N}");
        var assigned = await CreateApplicationAsync(ApplicationStatuses.Submitted, assigneeId: assignee, titleOverride: $"NUnit P0-DT4 assigned {Guid.NewGuid():N}");
        var noDeadline = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: null, titleOverride: $"NUnit P0-DT4 none {Guid.NewGuid():N}");
        await SetDeadlineAsync(noDeadline.MaDonTu, null);
        var onTrack = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(48), titleOverride: $"NUnit P0-DT4 ontrack {Guid.NewGuid():N}");
        var dueSoon = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(2), titleOverride: $"NUnit P0-DT4 duesoon {Guid.NewGuid():N}");
        var overdue = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(-2), titleOverride: $"NUnit P0-DT4 overdue {Guid.NewGuid():N}");
        var paused = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-2), titleOverride: $"NUnit P0-DT4 paused {Guid.NewGuid():N}");

        await AssertQueueContainsAsync($"api/admin/applications?assignmentState=unassigned&search={unassigned.MaDonTu}", unassigned.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?assignmentState=assigned&search={assigned.MaDonTu}", assigned.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?assignmentState=mine&search={Uri.EscapeDataString(TestPrefix)}", assigned.MaDonTu, shouldContain: false);
        await AssertQueueContainsAsync($"api/admin/applications?slaStatus=none&search={noDeadline.MaDonTu}", noDeadline.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?slaStatus=on_track&search={onTrack.MaDonTu}", onTrack.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?slaStatus=due_soon&search={dueSoon.MaDonTu}", dueSoon.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?slaStatus=overdue&search={overdue.MaDonTu}", overdue.MaDonTu);
        await AssertQueueContainsAsync($"api/admin/applications?status={ApplicationStatuses.NeedSupplement}&slaStatus=paused&search={paused.MaDonTu}", paused.MaDonTu);
    }

    [TestCase("status=bad_status")]
    [TestCase("type=bad_type")]
    [TestCase("assignmentState=bad")]
    [TestCase("slaStatus=bad")]
    [TestCase("sortBy=bad")]
    [TestCase("sortDirection=sideways")]
    [TestCase("pageIndex=0")]
    [TestCase("pageSize=0")]
    [TestCase("pageSize=101")]
    public async Task Queue_InvalidQuery_ShouldReturn400(string query)
    {
        using var response = await Client.GetAsync($"api/admin/applications?{query}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Queue_InvalidSubmittedRangeAndLongSearch_ShouldReturn400()
    {
        using var invalidRange = await Client.GetAsync($"api/admin/applications?submittedFrom={DateTime.UtcNow:O}&submittedTo={DateTime.UtcNow.AddDays(-1):O}");
        using var longSearch = await Client.GetAsync($"api/admin/applications?search={new string('x', 101)}");

        Assert.That(invalidRange.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(invalidRange));
        Assert.That(longSearch.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(longSearch));
    }

    [Test]
    public async Task Queue_Pagination_ShouldBeDeterministic()
    {
        var token = $"NUnit P0-DT4 paging {Guid.NewGuid():N}";
        var first = await CreateApplicationAsync(ApplicationStatuses.Submitted, submittedAt: DateTime.UtcNow.AddDays(-3), titleOverride: $"{token} 1");
        var second = await CreateApplicationAsync(ApplicationStatuses.Submitted, submittedAt: DateTime.UtcNow.AddDays(-2), titleOverride: $"{token} 2");
        var third = await CreateApplicationAsync(ApplicationStatuses.Submitted, submittedAt: DateTime.UtcNow.AddDays(-1), titleOverride: $"{token} 3");

        using var page1 = await Client.GetAsync($"api/admin/applications?search={Uri.EscapeDataString(token)}&sortBy=submittedAt&sortDirection=asc&pageIndex=1&pageSize=2");
        using var page2 = await Client.GetAsync($"api/admin/applications?search={Uri.EscapeDataString(token)}&sortBy=submittedAt&sortDirection=asc&pageIndex=2&pageSize=2");

        Assert.That(page1.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(page1));
        Assert.That(page2.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(page2));
        using var root1 = await GetRootAsync(page1);
        using var root2 = await GetRootAsync(page2);
        Assert.That(GetDataItems(root1.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")), Is.EqualTo(new[] { first.MaDonTu, second.MaDonTu }));
        Assert.That(GetDataItems(root2.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")), Is.EqualTo(new[] { third.MaDonTu }));
    }

    [Test]
    public async Task QueueSummary_ShouldReturnSlaCounts()
    {
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(-1));
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(2));
        await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(48));
        await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-1));

        using var response = await Client.GetAsync($"api/admin/applications/queue-summary?search={Uri.EscapeDataString(TestPrefix)}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "totalActive"), Is.GreaterThanOrEqualTo(3));
            Assert.That(GetInt32(data, "needSupplement"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "waitingForSupplement"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "overdue"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "dueSoon"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "unassigned"), Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task Detail_ShouldReturnSafeFieldsAndTimeline()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddAttachmentMetadataAndObjectAsync(application.MaDonTu);

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        var lower = body.ToLowerInvariant();
        using var root = JsonDocument.Parse(body);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "maDonTu"), Is.EqualTo(application.MaDonTu));
            Assert.That(GetRequiredProperty(data, "attachments").GetArrayLength(), Is.EqualTo(1));
            Assert.That(GetRequiredProperty(data, "timeline").GetArrayLength(), Is.GreaterThanOrEqualTo(1));
            Assert.That(lower, Does.Not.Contain("storagekey"));
            Assert.That(lower, Does.Not.Contain("filehash"));
            Assert.That(lower, Does.Not.Contain("tenfileluu"));
            Assert.That(lower, Does.Not.Contain("matkhau"));
            Assert.That(lower, Does.Not.Contain("password"));
        });
    }

    [Test]
    public async Task QueueSummary_EmptyScope_ShouldReturnAllZeros()
    {
        var isolatedCampusId = await CreateDifferentCampusAsync();
        using var response = await Client.GetAsync($"api/admin/applications/queue-summary?campusId={isolatedCampusId}&search=no-match-{Guid.NewGuid():N}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "active"), Is.Zero);
            Assert.That(GetInt32(data, "totalActive"), Is.Zero);
            Assert.That(GetInt32(data, "submitted"), Is.Zero);
            Assert.That(GetInt32(data, "inReview"), Is.Zero);
            Assert.That(GetInt32(data, "needSupplement"), Is.Zero);
            Assert.That(GetInt32(data, "waitingForSupplement"), Is.Zero);
            Assert.That(GetInt32(data, "unassigned"), Is.Zero);
            Assert.That(GetInt32(data, "assigned"), Is.Zero);
            Assert.That(GetInt32(data, "assignedToMe"), Is.Zero);
            Assert.That(GetInt32(data, "overdue"), Is.Zero);
            Assert.That(GetInt32(data, "dueSoon"), Is.Zero);
        });
    }

    [Test]
    public async Task QueueSummary_ShouldUseOneTaggedAggregateCommand()
    {
        await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var interceptor = new TaggedCommandCounterInterceptor("P0-DT4.1 QueueSummaryAggregate");
        await using var db = CreateDbContext(interceptor);
        var actor = await db.NguoiDungs.AsNoTracking().FirstAsync(x => x.Email == SuperAdminEmail);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Items["CurrentUser"] = new CurrentUserContext
        {
            UserId = actor.MaNguoiDung,
            Email = actor.Email,
            Role = AuthRoles.FromDatabaseCode(actor.VaiTroChinh),
            CampusId = actor.MaDonVi,
            Status = actor.TrangThai
        };
        var scopeService = new ApplicationCampusScopeService(db, accessor);
        var service = new ApplicationAdminQueueService(
            db,
            scopeService,
            Options.Create(new ApplicationQueueOptions { SlaWarningBeforeHours = 24 }));

        var summary = await service.GetQueueSummaryAsync(new AdminApplicationQueryParameters
        {
            Search = TestPrefix
        });

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalActive, Is.GreaterThanOrEqualTo(1));
            Assert.That(interceptor.TaggedCommandCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task QueueSummary_AliasesAndStatusBuckets_ShouldBeConsistent()
    {
        await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await CreateApplicationAsync(ApplicationStatuses.InReview);
        await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-48));
        await CreateApplicationAsync(ApplicationStatuses.Draft);
        await CreateApplicationAsync(ApplicationStatuses.Approved);

        using var response = await Client.GetAsync($"api/admin/applications/queue-summary?search={Uri.EscapeDataString(TestPrefix)}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "active"), Is.EqualTo(GetInt32(data, "totalActive")));
            Assert.That(GetInt32(data, "waitingForSupplement"), Is.EqualTo(GetInt32(data, "needSupplement")));
            Assert.That(GetInt32(data, "submitted"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "inReview"), Is.GreaterThanOrEqualTo(1));
            Assert.That(GetInt32(data, "needSupplement"), Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task QueueSummary_NeedSupplementDeadline_ShouldNotCountOverdueOrDueSoon()
    {
        var titleToken = $"NUnit P0-DT4 paused {Guid.NewGuid():N}";
        await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-48), titleOverride: titleToken);
        await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(2), titleOverride: titleToken);

        using var response = await Client.GetAsync($"api/admin/applications/queue-summary?search={Uri.EscapeDataString(titleToken)}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(data, "needSupplement"), Is.EqualTo(2));
            Assert.That(GetInt32(data, "overdue"), Is.Zero);
            Assert.That(GetInt32(data, "dueSoon"), Is.Zero);
        });
    }

    [Test]
    public async Task Queue_SlaStatusAndRemainingMinutes_ShouldBeCorrect()
    {
        var none = await CreateApplicationAsync(ApplicationStatuses.Submitted, titleOverride: $"NUnit P0-DT4 sla-none {Guid.NewGuid():N}");
        await SetDeadlineAsync(none.MaDonTu, null);
        var onTrack = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(48));
        var dueSoon = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(2));
        var overdue = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: DateTime.UtcNow.AddHours(-2));
        var paused = await CreateApplicationAsync(ApplicationStatuses.NeedSupplement, deadline: DateTime.UtcNow.AddHours(-2));
        var terminal = await CreateApplicationAsync(ApplicationStatuses.Approved, deadline: DateTime.UtcNow.AddHours(-2));

        var noneItem = await GetQueueItemAsync(none.MaDonTu, $"status={ApplicationStatuses.Submitted}&slaStatus=none");
        var onTrackItem = await GetQueueItemAsync(onTrack.MaDonTu, "slaStatus=on_track");
        var dueSoonItem = await GetQueueItemAsync(dueSoon.MaDonTu, "slaStatus=due_soon");
        var overdueItem = await GetQueueItemAsync(overdue.MaDonTu, "slaStatus=overdue");
        var pausedItem = await GetQueueItemAsync(paused.MaDonTu, $"status={ApplicationStatuses.NeedSupplement}&slaStatus=paused");
        var terminalItem = await GetQueueItemAsync(terminal.MaDonTu, $"status={ApplicationStatuses.Approved}&slaStatus=none");

        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(GetRequiredProperty(noneItem, "sla"), "status"), Is.EqualTo("none"));
            Assert.That(GetRequiredString(GetRequiredProperty(onTrackItem, "sla"), "status"), Is.EqualTo("on_track"));
            Assert.That(GetRequiredString(GetRequiredProperty(dueSoonItem, "sla"), "status"), Is.EqualTo("due_soon"));
            Assert.That(GetRequiredString(GetRequiredProperty(overdueItem, "sla"), "status"), Is.EqualTo("overdue"));
            Assert.That(GetRequiredString(GetRequiredProperty(pausedItem, "sla"), "status"), Is.EqualTo("paused"));
            Assert.That(GetRequiredString(GetRequiredProperty(terminalItem, "sla"), "status"), Is.EqualTo("none"));
            Assert.That(GetInt32(GetRequiredProperty(dueSoonItem, "sla"), "remainingMinutes"), Is.Positive);
            Assert.That(GetInt32(GetRequiredProperty(overdueItem, "sla"), "remainingMinutes"), Is.Negative);
        });
    }

    [Test]
    public async Task ReceiveAssignAndReassign_ShouldNotChangeDeadline()
    {
        var deadline = DateTime.UtcNow.AddHours(12);
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var received = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: deadline);
        var assigned = await CreateApplicationAsync(ApplicationStatuses.Submitted, deadline: deadline);
        var reassigned = await CreateApplicationAsync(ApplicationStatuses.InReview, deadline: deadline, assigneeId: firstAssignee);

        using var receive = await Client.PostAsJsonAsync($"api/admin/applications/{received.MaDonTu}/receive", new { rowVersion = received.RowVersion });
        using var assign = await Client.PostAsJsonAsync($"api/admin/applications/{assigned.MaDonTu}/assign", new { assigneeId = firstAssignee, rowVersion = assigned.RowVersion });
        using var reassign = await Client.PostAsJsonAsync($"api/admin/applications/{reassigned.MaDonTu}/assign", new { assigneeId = secondAssignee, rowVersion = reassigned.RowVersion, reason = "NUnit P0-DT4 đủ 10 ký tự" });

        Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(receive));
        Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(assign));
        Assert.That(reassign.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(reassign));
        await using var db = CreateDbContext();
        var deadlines = await db.DonTus.AsNoTracking()
            .Where(x => new[] { received.MaDonTu, assigned.MaDonTu, reassigned.MaDonTu }.Contains(x.MaDonTu))
            .Select(x => x.HanXuLyLuc)
            .ToListAsync();
        Assert.That(deadlines, Has.All.EqualTo(deadline).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public async Task QueueSummary_ShouldRespectCoreFilters()
    {
        var assignee = await CreateAssignableUserAsync();
        var submittedAt = DateTime.UtcNow.AddDays(-2);
        var title = $"NUnit P0-DT4 summary filter {Guid.NewGuid():N}";
        var application = await CreateApplicationAsync(
            ApplicationStatuses.Submitted,
            assigneeId: assignee,
            submittedAt: submittedAt,
            titleOverride: title);
        var search = Uri.EscapeDataString(title);

        using var byCampus = await Client.GetAsync($"api/admin/applications/queue-summary?campusId={application.MaDonVi}&search={search}");
        using var byStudent = await Client.GetAsync($"api/admin/applications/queue-summary?studentId={await GetUserIdAsync(StudentEmail)}&search={search}");
        using var byAssignee = await Client.GetAsync($"api/admin/applications/queue-summary?assigneeId={assignee}&search={search}");
        using var byType = await Client.GetAsync($"api/admin/applications/queue-summary?type={ApplicationTypes.Confirmation}&search={search}");
        using var byDate = await Client.GetAsync($"api/admin/applications/queue-summary?submittedFrom={submittedAt.AddMinutes(-1):O}&submittedTo={submittedAt.AddMinutes(1):O}&search={search}");

        foreach (var response in new[] { byCampus, byStudent, byAssignee, byType, byDate })
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
            using var root = await GetRootAsync(response);
            Assert.That(GetInt32(GetRequiredProperty(root.RootElement, "data"), "totalActive"), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task Detail_Timeline_ShouldNotExposeSnapshotJsonProperty()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Assign, "{\"fromAssigneeId\":1,\"toAssigneeId\":2}");

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("snapshotJson").IgnoreCase);
    }

    [Test]
    public async Task Detail_Timeline_ShouldMapAssignmentMetadata()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Reassign, "{\"fromAssigneeId\":1,\"toAssigneeId\":2,\"reasonProvided\":true}");

        var timeline = await GetTimelineAsync(application.MaDonTu);
        var metadata = FindMetadata(timeline, ApplicationActions.Reassign);

        Assert.Multiple(() =>
        {
            Assert.That(GetInt32(metadata, "fromAssigneeId"), Is.EqualTo(1));
            Assert.That(GetInt32(metadata, "toAssigneeId"), Is.EqualTo(2));
            Assert.That(GetBoolean(metadata, "reasonProvided"), Is.True);
        });
    }

    [Test]
    public async Task Detail_Timeline_ShouldMapEvidenceMetadata()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Update, "{\"operation\":\"upload_evidence\",\"attachmentIds\":[1,2],\"attachmentId\":3,\"fileCount\":2}");

        var timeline = await GetTimelineAsync(application.MaDonTu);
        var metadata = FindMetadata(timeline, ApplicationActions.Update);

        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(metadata, "operation"), Is.EqualTo("upload_evidence"));
            Assert.That(GetRequiredProperty(metadata, "attachmentIds").EnumerateArray().Select(x => x.GetInt32()), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(GetInt32(metadata, "attachmentId"), Is.EqualTo(3));
            Assert.That(GetInt32(metadata, "fileCount"), Is.EqualTo(2));
        });
    }

    [TestCase("[1,2,3]")]
    public async Task Detail_Timeline_InvalidSnapshot_ShouldReturn200WithNullMetadata(string snapshot)
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Assign, snapshot);

        var timeline = await GetTimelineAsync(application.MaDonTu);
        var item = timeline.EnumerateArray().Last();

        Assert.That(GetRequiredProperty(item, "metadata").ValueKind, Is.EqualTo(JsonValueKind.Null));
    }

    [Test]
    public void TimelineMetadataSanitizer_MalformedSnapshot_ShouldReturnNull()
    {
        var metadata = ApplicationTimelineMetadataSanitizer.Sanitize("{not-json");

        Assert.That(metadata, Is.Null);
    }

    [Test]
    public async Task Detail_Timeline_UnknownAndSensitiveKeys_ShouldNotLeak()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Assign,
            "{\"toAssigneeId\":2,\"storageKey\":\"secret/key\",\"fileHash\":\"hash\",\"password\":\"pw\",\"secret\":\"s\",\"connectionString\":\"cs\",\"fieldValues\":{\"x\":1}}");

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(body, Does.Not.Contain("storageKey").IgnoreCase);
            Assert.That(body, Does.Not.Contain("fileHash").IgnoreCase);
            Assert.That(body, Does.Not.Contain("password").IgnoreCase);
            Assert.That(body, Does.Not.Contain("secret").IgnoreCase);
            Assert.That(body, Does.Not.Contain("connectionString").IgnoreCase);
            Assert.That(body, Does.Not.Contain("fieldValues").IgnoreCase);
            Assert.That(body, Does.Not.Contain("snapshotJson").IgnoreCase);
        });
    }

    [Test]
    public async Task Detail_Timeline_ChangedFields_ShouldBeTrimmedDistinctAndBounded()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var fields = Enumerable.Range(1, 105).Select(i => $"\" field{i} \"").Concat(["\"field1\"", "\"\"", "1"]);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Assign, $"{{\"changedFields\":[{string.Join(",", fields)}]}}");

        var metadata = FindMetadata(await GetTimelineAsync(application.MaDonTu), ApplicationActions.Assign);
        var changedFields = GetRequiredProperty(metadata, "changedFields").EnumerateArray().Select(x => x.GetString()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(changedFields, Has.Count.EqualTo(100));
            Assert.That(changedFields[0], Is.EqualTo("field1"));
            Assert.That(changedFields.Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(changedFields.Count));
        });
    }

    [Test]
    public async Task Detail_Timeline_ChangedFields_ShouldDropSensitiveNames()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Assign,
            "{\"changedFields\":[\"password\",\"connectionString\",\"storage_key\",\"fileHash\",\"token\",\"safeField\"]}");

        var metadata = FindMetadata(await GetTimelineAsync(application.MaDonTu), ApplicationActions.Assign);
        var changedFields = GetRequiredProperty(metadata, "changedFields").EnumerateArray().Select(x => x.GetString()).ToList();

        Assert.That(changedFields, Is.EqualTo(new[] { "safeField" }));
    }

    [Test]
    public void TimelineMetadataSanitizer_ArrayScan_ShouldCapInspectedItems()
    {
        var invalidStrings = string.Join(",", Enumerable.Repeat("1", 100));
        var invalidInts = string.Join(",", Enumerable.Repeat("\"bad\"", 100));

        var stringMetadata = ApplicationTimelineMetadataSanitizer.Sanitize($"{{\"changedFields\":[{invalidStrings},\"safeField\"]}}");
        var intMetadata = ApplicationTimelineMetadataSanitizer.Sanitize($"{{\"attachmentIds\":[{invalidInts},1]}}");

        Assert.Multiple(() =>
        {
            Assert.That(stringMetadata, Is.Null);
            Assert.That(intMetadata, Is.Null);
        });
    }

    [Test]
    public async Task Detail_Timeline_AttachmentIds_ShouldBePositiveDistinctAndBounded()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var ids = Enumerable.Range(1, 105).Select(i => i.ToString()).Concat(["1", "0", "-2", "\"bad\""]);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Update, $"{{\"operation\":\"delete_evidence\",\"attachmentIds\":[{string.Join(",", ids)}]}}");

        var metadata = FindMetadata(await GetTimelineAsync(application.MaDonTu), ApplicationActions.Update);
        var attachmentIds = GetRequiredProperty(metadata, "attachmentIds").EnumerateArray().Select(x => x.GetInt32()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(attachmentIds, Has.Count.EqualTo(100));
            Assert.That(attachmentIds, Is.EqualTo(Enumerable.Range(1, 100).ToArray()));
        });
    }

    [Test]
    public async Task Detail_Timeline_UnsupportedOperation_ShouldNotReflectRawValue()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await AddTimelineLogAsync(application.MaDonTu, ApplicationActions.Update, "{\"operation\":\"drop_database\",\"toAssigneeId\":2}");

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(body, Does.Not.Contain("drop_database"));
            using var root = JsonDocument.Parse(body);
            var metadata = FindMetadata(GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "timeline"), ApplicationActions.Update);
            Assert.That(HasProperty(metadata, "operation"), Is.False);
            Assert.That(GetInt32(metadata, "toAssigneeId"), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Receive_SubmittedUnassigned_ShouldSetInReviewAssigneeAndPublicLog()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var updated = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == application.MaDonTu);
        var superAdminId = await GetUserIdAsync("superadmin@lms.local");
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Receive)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.TrangThai, Is.EqualTo(ApplicationStatuses.InReview));
            Assert.That(updated.NguoiDuyetHienTai, Is.EqualTo(superAdminId));
            Assert.That(updated.NguoiXuLyCuoi, Is.Null);
            Assert.That(log.HienThiChoHocSinh, Is.True);
            Assert.That(log.GhiChuCongKhai, Is.EqualTo("Đơn đã được tiếp nhận để xử lý."));
            Assert.That(log.SnapshotJson, Does.Contain("toAssigneeId"));
        });
    }

    [Test]
    public async Task Receive_StaleRowVersion_ShouldReturn409()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await TouchApplicationAsync(application.MaDonTu);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Receive_AlreadyAssigned_ShouldReturn409()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted, assigneeId: assignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Receive_NonSubmitted_ShouldReturn400()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
        {
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_SubmittedUnassigned_ShouldSetAssigneeInReviewAndPublicLog()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var updated = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == application.MaDonTu);
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Assign)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updated.TrangThai, Is.EqualTo(ApplicationStatuses.InReview));
            Assert.That(updated.NguoiDuyetHienTai, Is.EqualTo(assignee));
            Assert.That(log.HienThiChoHocSinh, Is.True);
            Assert.That(log.SnapshotJson, Does.Contain("toAssigneeId"));
        });
    }

    [Test]
    public async Task Assign_ReassignRequiresReason()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("123456789")]
    public async Task Assign_ReassignInvalidReason_ShouldReturn400(string? reason)
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion,
            reason
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [TestCase(10)]
    [TestCase(1000)]
    public async Task Assign_ReassignValidReasonBoundaries_ShouldReturnOk(int length)
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion,
            reason = new string('x', length)
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_ReassignReasonTooLong_ShouldReturn400()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion,
            reason = new string('x', 1001)
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_ReassignWithReason_ShouldCreateInternalLog()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = secondAssignee,
            rowVersion = application.RowVersion,
            reason = "NUnit P0-DT4 đổi người xử lý do phân tải công việc."
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var log = await db.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HanhDong == ApplicationActions.Reassign)
            .SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(log.HienThiChoHocSinh, Is.False);
            Assert.That(log.GhiChuNoiBo, Does.Contain("NUnit P0-DT4"));
            Assert.That(log.SnapshotJson, Does.Contain("reasonProvided"));
        });
    }

    [Test]
    public async Task Assign_SameAssigneeNoOp_ShouldValidateRowVersionAndNotLog()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: assignee);
        var beforeLogs = await CountLogsAsync(application.MaDonTu);
        var beforeUpdatedAt = await GetApplicationUpdatedAtAsync(application.MaDonTu);

        using var ok = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(ok));
        Assert.That(await CountLogsAsync(application.MaDonTu), Is.EqualTo(beforeLogs));
        Assert.That(await GetApplicationUpdatedAtAsync(application.MaDonTu), Is.EqualTo(beforeUpdatedAt));

        await TouchApplicationAsync(application.MaDonTu);
        using var stale = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(stale));
    }

    [TestCase(ApplicationStatuses.Draft)]
    [TestCase(ApplicationStatuses.Approved)]
    [TestCase(ApplicationStatuses.Rejected)]
    [TestCase(ApplicationStatuses.Cancelled)]
    public async Task Assign_NonAssignableStatuses_ShouldReturn400(string status)
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(status);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = assignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [TestCase(AuthRoles.Student)]
    [TestCase(AuthRoles.Teacher)]
    [TestCase(AuthRoles.Parent)]
    [TestCase(AuthRoles.Principal)]
    public async Task Assign_DisallowedAssigneeRole_ShouldReturn404(string role)
    {
        var userId = await CreateUserInStudentCampusAsync(role, UserStatuses.DbActive);
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = userId,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Assign_InactiveAssignee_ShouldReturn404()
    {
        var userId = await CreateUserInStudentCampusAsync(AuthRoles.AcademicStaff, UserStatuses.DbLocked);
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = userId,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AcademicStaff_CanReceiveButCannotAssign()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), async client =>
        {
            using var receive = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
            {
                rowVersion = application.RowVersion
            });
            Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(receive));

            var fresh = await GetApplicationSnapshotFromDbAsync(application.MaDonTu);
            var assignee = await CreateAssignableUserAsync();
            using var assign = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
            {
                assigneeId = assignee,
                rowVersion = fresh.RowVersion
            });
            Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(assign));
        });
    }

    [Test]
    public async Task Principal_CanReadButCannotReceive()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.Principal), async client =>
        {
            using var detail = await client.GetAsync($"api/admin/applications/{application.MaDonTu}");
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detail));

            using var receive = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new
            {
                rowVersion = application.RowVersion
            });
            Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(receive));
        });
    }

    [Test]
    public async Task Principal_QueueDetailReceiveAssign_Matrix()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var assignee = await CreateAssignableUserAsync();
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.Principal), async client =>
        {
            using var queue = await client.GetAsync("api/admin/applications");
            using var detail = await client.GetAsync($"api/admin/applications/{application.MaDonTu}");
            using var receive = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
            using var assign = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = assignee, rowVersion = application.RowVersion });

            Assert.That(queue.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(queue));
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detail));
            Assert.That(receive.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(receive));
            Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(assign));
        });
    }

    [Test]
    public async Task ScopedUser_DetailOutsideCampus_ShouldReturn404_AndListFilterOutsideScope403()
    {
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherCampusApplication = await CreateApplicationForEmailAsync(
            StudentEmail,
            ApplicationStatuses.Submitted,
            campusOverride: otherCampusId);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            using var detail = await client.GetAsync($"api/admin/applications/{otherCampusApplication.MaDonTu}");
            Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(detail));

            using var list = await client.GetAsync($"api/admin/applications?maDonVi={otherCampusApplication.MaDonVi}");
            Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(list));
        });
    }

    [Test]
    public async Task ScopedRoles_ShouldRespectCampusVisibility()
    {
        var ownApplication = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherApplication = await CreateApplicationForEmailAsync(StudentEmail, ApplicationStatuses.Submitted, campusOverride: otherCampusId);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            using var own = await client.GetAsync($"api/admin/applications?campusId={ownApplication.MaDonVi}&search={ownApplication.MaDonTu}");
            using var other = await client.GetAsync($"api/admin/applications?campusId={otherApplication.MaDonVi}");

            Assert.That(own.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(own));
            Assert.That(other.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(other));
        });
    }

    [Test]
    public async Task GlobalRoles_ShouldSeeMultipleCampuses()
    {
        var ownApplication = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherApplication = await CreateApplicationForEmailAsync(StudentEmail, ApplicationStatuses.Submitted, campusOverride: otherCampusId);

        using var adminClient = await CreateAuthenticatedClientAsync(AdminEmail);
        using var superAdminOwn = await Client.GetAsync($"api/admin/applications/{ownApplication.MaDonTu}");
        using var superAdminOther = await Client.GetAsync($"api/admin/applications/{otherApplication.MaDonTu}");
        using var adminOwn = await adminClient.GetAsync($"api/admin/applications/{ownApplication.MaDonTu}");
        using var adminOther = await adminClient.GetAsync($"api/admin/applications/{otherApplication.MaDonTu}");

        Assert.That(superAdminOwn.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(superAdminOwn));
        Assert.That(superAdminOther.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(superAdminOther));
        Assert.That(adminOwn.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(adminOwn));
        Assert.That(adminOther.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(adminOther));
    }

    [Test]
    public async Task EvidenceOutsideScope_ShouldReturn404_AndStudentForbidden()
    {
        var otherCampusId = await CreateDifferentCampusAsync();
        var application = await CreateApplicationForEmailAsync(StudentEmail, ApplicationStatuses.Submitted, campusOverride: otherCampusId);
        var attachmentId = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async scopedClient =>
        {
            using var scoped = await scopedClient.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");
            Assert.That(scoped.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(scoped));
        });

        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var student = await studentClient.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");
        Assert.That(student.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(student));
    }

    [Test]
    public async Task Assignees_InvalidOutsideScope_ShouldReturnNotFoundOnAssign()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var otherCampusId = await CreateDifferentCampusAsync();
        var otherCampusAssignee = await CreateAssignableUserInCampusAsync(otherCampusId);

        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            using var response = await client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
            {
                assigneeId = otherCampusAssignee,
                rowVersion = application.RowVersion
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task Assign_GlobalActorAssigneeFromDifferentApplicationCampus_ShouldReturn404()
    {
        var applicationCampusId = await CreateDifferentCampusAsync();
        var application = await CreateApplicationForEmailAsync(
            StudentEmail,
            ApplicationStatuses.Submitted,
            campusOverride: applicationCampusId);
        var baseCampusAssignee = await CreateAssignableUserAsync();

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new
        {
            assigneeId = baseCampusAssignee,
            rowVersion = application.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AdminDownloadAttachment_ShouldReturnFileWithSafeHeaders()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var attachmentId = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu);

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Multiple(() =>
        {
            Assert.That(bytes, Is.EqualTo(Encoding.UTF8.GetBytes("P0-DT4 evidence")));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            Assert.That(response.Headers.TryGetValues("X-Content-Type-Options", out var values), Is.True);
            Assert.That(values!.Single(), Is.EqualTo("nosniff"));
            Assert.That(response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName, Does.Contain("p0dt4.pdf"));
        });
    }

    [Test]
    public async Task AdminDownload_DeletedOrMissing_ShouldReturn404()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var deleted = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu, deleted: true);

        using var deletedResponse = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{deleted}/download");
        using var missingResponse = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/999999999/download");

        Assert.That(deletedResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(deletedResponse));
        Assert.That(missingResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(missingResponse));
    }

    [Test]
    public async Task AdminDownload_MissingPhysicalObject_ShouldReturn404()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var attachmentId = await AddAttachmentMetadataOnlyAsync(application.MaDonTu);

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AdminDownload_StorageUnavailable_ShouldReturn503AndNotExposeStorageKey()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var attachmentId = await AddAttachmentMetadataOnlyAsync(application.MaDonTu, storageKey: "../bad/key");

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(body, Does.Not.Contain("../bad/key"));
            Assert.That(body, Does.Not.Contain("storageKey").IgnoreCase);
        });
    }

    [Test]
    public async Task AdminDownload_LegacyUnsafeFilename_ShouldBeSanitized()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        var attachmentId = await AddAttachmentMetadataAndObjectAsync(application.MaDonTu, originalFileName: "../unsafe\r\nname.pdf");

        using var response = await Client.GetAsync($"api/admin/applications/{application.MaDonTu}/attachments/{attachmentId}/download");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var disposition = response.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
        Assert.Multiple(() =>
        {
            Assert.That(disposition, Does.Contain("name.pdf"));
            Assert.That(disposition, Does.Not.Contain(".."));
            Assert.That(disposition, Does.Not.Contain("\r"));
            Assert.That(disposition, Does.Not.Contain("\n"));
        });
    }

    [Test]
    public async Task Receive_ConcurrentSameRowVersion_ShouldHaveOneSuccessOneConflict()
    {
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var firstTask = firstClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
        var secondTask = secondClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
        var responses = await Task.WhenAll(firstTask, secondTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(application.MaDonTu, ApplicationActions.Receive), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Test]
    public async Task ConcurrentReassign_SameRowVersion_ShouldHaveOne200One409()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var thirdAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: firstAssignee);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var firstTask = firstClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = secondAssignee, rowVersion = application.RowVersion, reason = "NUnit P0-DT4 concurrency one" });
        var secondTask = secondClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = thirdAssignee, rowVersion = application.RowVersion, reason = "NUnit P0-DT4 concurrency two" });
        var responses = await Task.WhenAll(firstTask, secondTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(application.MaDonTu, ApplicationActions.Reassign), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Test]
    public async Task ConcurrentReceiveAndAssign_ShouldHaveOne200One409()
    {
        var assignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var receiveTask = firstClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/receive", new { rowVersion = application.RowVersion });
        var assignTask = secondClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = assignee, rowVersion = application.RowVersion });
        var responses = await Task.WhenAll(receiveTask, assignTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(application.MaDonTu, ApplicationActions.Receive) + await CountLogsAsync(application.MaDonTu, ApplicationActions.Assign), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Test]
    public async Task ConcurrentAssignDifferentTargets_ShouldHaveOne200One409()
    {
        var firstAssignee = await CreateAssignableUserAsync();
        var secondAssignee = await CreateAssignableUserAsync();
        var application = await CreateApplicationAsync(ApplicationStatuses.Submitted);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var firstTask = firstClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = firstAssignee, rowVersion = application.RowVersion });
        var secondTask = secondClient.PostAsJsonAsync($"api/admin/applications/{application.MaDonTu}/assign", new { assigneeId = secondAssignee, rowVersion = application.RowVersion });
        var responses = await Task.WhenAll(firstTask, secondTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountLogsAsync(application.MaDonTu, ApplicationActions.Assign), Is.EqualTo(1));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private async Task<ApplicationSnapshot> CreateApplicationAsync(
        string status,
        DateTime? deadline = null,
        int? assigneeId = null,
        DateTime? submittedAt = null,
        string? titleOverride = null)
    {
        return await CreateApplicationForEmailAsync(StudentEmail, status, deadline, assigneeId, submittedAt: submittedAt, titleOverride: titleOverride);
    }

    private static async Task<ApplicationSnapshot> CreateApplicationForEmailAsync(
        string email,
        string status,
        DateTime? deadline = null,
        int? assigneeId = null,
        int? campusOverride = null,
        DateTime? submittedAt = null,
        string? titleOverride = null,
        string? typeOverride = null)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == ApplicationTypes.Confirmation && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = campusOverride ?? student.MaDonVi,
            MaMauDon = templateId,
            LoaiDon = typeOverride ?? ApplicationTypes.Confirmation,
            TieuDe = titleOverride ?? $"{TestPrefix} {status} {Guid.NewGuid():N}",
            DuLieuBieuMau = "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed,
            NguoiDuyetHienTai = assigneeId,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = status == ApplicationStatuses.Draft ? null : submittedAt ?? now.AddMinutes(-5),
            HanXuLyLuc = deadline ?? now.AddHours(48)
        };
        db.DonTus.Add(application);
        await db.SaveChangesAsync();
        db.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = student.MaNguoiDung,
            NguonThucHien = "user",
            HanhDong = ApplicationActions.Submit,
            TrangThaiCu = ApplicationStatuses.Draft,
            TrangThaiMoi = status,
            GhiChuCongKhai = "NUnit P0-DT4 tạo đơn.",
            HienThiChoHocSinh = true,
            NgayTao = now
        });
        await db.SaveChangesAsync();
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion));
    }

    private static async Task<int> CreateAssignableUserAsync()
    {
        return await CreateAssignableUserForEmailCampusAsync(StudentEmail);
    }

    private static async Task<int> CreateAssignableUserForEmailCampusAsync(string email)
    {
        await using var db = CreateDbContext();
        var campusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        return await CreateAssignableUserInCampusAsync(campusId);
    }

    private static async Task<int> CreateAssignableUserInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt4-assignee-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT4 Assignee",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task<int> CreateDifferentCampusAsync()
    {
        await using var db = CreateDbContext();
        var baseCampusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == StudentEmail)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        var campus = new DonVi
        {
            TenDonVi = $"NUnit P0-DT4 campus {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        Assert.That(campus.MaDonVi, Is.Not.EqualTo(baseCampusId));
        return campus.MaDonVi;
    }

    private static async Task<int> CreateUserInStudentCampusAsync(string role, string status)
    {
        await using var db = CreateDbContext();
        var campusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == StudentEmail)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt4-user-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT4 User",
            VaiTroChinh = AuthRoles.ToDatabaseCode(role),
            TrangThai = status,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task AddTimelineLogAsync(int applicationId, string action, string? snapshotJson)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == applicationId);
        db.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = applicationId,
            MaNguoiDuyet = application.MaHocSinh,
            NguonThucHien = "user",
            HanhDong = action,
            TrangThaiCu = application.TrangThai,
            TrangThaiMoi = application.TrangThai,
            GhiChuCongKhai = "NUnit P0-DT4 timeline.",
            GhiChuNoiBo = "NUnit P0-DT4 internal.",
            SnapshotJson = snapshotJson,
            HienThiChoHocSinh = false,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private async Task<JsonElement> GetTimelineAsync(int applicationId)
    {
        using var response = await Client.GetAsync($"api/admin/applications/{applicationId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "timeline").Clone();
    }

    private static JsonElement FindMetadata(JsonElement timeline, string action)
    {
        foreach (var item in timeline.EnumerateArray())
        {
            if (string.Equals(GetRequiredString(item, "hanhDong"), action, StringComparison.OrdinalIgnoreCase))
            {
                return GetRequiredProperty(item, "metadata");
            }
        }

        Assert.Fail($"Không tìm thấy timeline action {action}.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }

    private async Task AssertQueueContainsAsync(string url, int applicationId, bool shouldContain = true)
    {
        using var response = await Client.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var ids = GetDataItems(root.RootElement).EnumerateArray().Select(x => GetInt32(x, "maDonTu")).ToHashSet();
        Assert.That(ids.Contains(applicationId), Is.EqualTo(shouldContain), url);
    }

    private async Task<JsonElement> GetQueueItemAsync(int applicationId, string query)
    {
        using var response = await Client.GetAsync($"api/admin/applications?{query}&search={applicationId}&pageIndex=1&pageSize=10");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        foreach (var item in GetDataItems(root.RootElement).EnumerateArray())
        {
            if (GetInt32(item, "maDonTu") == applicationId)
            {
                return item.Clone();
            }
        }

        Assert.Fail($"Không tìm thấy queue item {applicationId}.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }

    private static async Task SetDeadlineAsync(int applicationId, DateTime? deadline)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.HanXuLyLuc = deadline;
        await db.SaveChangesAsync();
    }

    private static async Task<int> AddAttachmentMetadataAndObjectAsync(
        int applicationId,
        bool deleted = false,
        string? originalFileName = null)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == applicationId);
        var storageKey = $"applications/evidence/{application.MaDonVi}/{applicationId}/p0dt4-{Guid.NewGuid():N}.pdf";
        var root = GetSharedTestStorageRoot();
        var path = Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("P0-DT4 evidence"));
        var attachment = new TepDinhKemDonTu
        {
            MaDonTu = applicationId,
            StorageKey = storageKey,
            TenFileGoc = originalFileName ?? "p0dt4.pdf",
            TenFileLuu = Path.GetFileName(path),
            ContentType = "application/pdf",
            KichThuocByte = Encoding.UTF8.GetByteCount("P0-DT4 evidence"),
            FileHash = "hidden",
            NguoiTaiLen = application.MaHocSinh,
            NgayTao = DateTime.UtcNow,
            DaXoa = deleted,
            NguoiXoa = deleted ? application.MaHocSinh : null,
            NgayXoa = deleted ? DateTime.UtcNow : null
        };
        db.TepDinhKemDonTus.Add(attachment);
        await db.SaveChangesAsync();
        return attachment.MaTep;
    }

    private static async Task<int> AddAttachmentMetadataOnlyAsync(
        int applicationId,
        string? storageKey = null,
        string? originalFileName = null)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == applicationId);
        var attachment = new TepDinhKemDonTu
        {
            MaDonTu = applicationId,
            StorageKey = storageKey ?? $"applications/evidence/{application.MaDonVi}/{applicationId}/missing-{Guid.NewGuid():N}.pdf",
            TenFileGoc = originalFileName ?? "missing.pdf",
            TenFileLuu = "missing.pdf",
            ContentType = "application/pdf",
            KichThuocByte = 1,
            FileHash = "hidden",
            NguoiTaiLen = application.MaHocSinh,
            NgayTao = DateTime.UtcNow
        };
        db.TepDinhKemDonTus.Add(attachment);
        await db.SaveChangesAsync();
        return attachment.MaTep;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await LoginAsync(email);
        var client = new HttpClient { BaseAddress = BaseUri };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> LoginAsync(string email)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"Không login được account seed {email}. {await DescribeResponseAsync(response)}");
        }

        using var root = await GetRootAsync(response);
        return GetRequiredString(root.RootElement, "accessToken");
    }

    private async Task WithMutatedUserAndLoginAsync(string email, string role, Func<HttpClient, Task> action)
    {
        await using var db = CreateDbContext();
        var user = await db.NguoiDungs.FirstAsync(x => x.Email == email);
        var oldRole = user.VaiTroChinh;
        var oldStatus = user.TrangThai;
        user.VaiTroChinh = role;
        user.TrangThai = UserStatuses.DbActive;
        await db.SaveChangesAsync();

        try
        {
            using var outerClient = new HttpClient { BaseAddress = BaseUri };
            using var loginResponse = await outerClient.PostAsJsonAsync("api/auth/login", new
            {
                email,
                password = GetSharedTestPassword()
            });
            Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(loginResponse));
            using var root = await GetRootAsync(loginResponse);
            var token = GetRequiredString(root.RootElement, "accessToken");
            using var client = new HttpClient { BaseAddress = outerClient.BaseAddress };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await action(client);
        }
        finally
        {
            user.VaiTroChinh = oldRole;
            user.TrangThai = oldStatus;
            await db.SaveChangesAsync();
        }
    }

    private async Task WithLoggedInThenMutatedUserAsync(
        string email,
        string role,
        string status,
        Func<HttpClient, Task> action)
    {
        using var loginResponse = await Client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = GetSharedTestPassword()
        });
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(loginResponse));
        using var root = await GetRootAsync(loginResponse);
        var token = GetRequiredString(root.RootElement, "accessToken");

        await using var db = CreateDbContext();
        var user = await db.NguoiDungs.FirstAsync(x => x.Email == email);
        var oldRole = user.VaiTroChinh;
        var oldStatus = user.TrangThai;
        user.VaiTroChinh = role;
        user.TrangThai = status;
        await db.SaveChangesAsync();

        try
        {
            using var client = new HttpClient { BaseAddress = BaseUri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await action(client);
        }
        finally
        {
            user.VaiTroChinh = oldRole;
            user.TrangThai = oldStatus;
            await db.SaveChangesAsync();
        }
    }

    private static async Task TouchApplicationAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.NgayCapNhat = DateTime.UtcNow.AddSeconds(1);
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountLogsAsync(int applicationId, string? action = null)
    {
        await using var db = CreateDbContext();
        var query = db.NhatKyDuyetDons.AsNoTracking().Where(x => x.MaDonTu == applicationId);
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.HanhDong == action);
        }

        return await query.CountAsync();
    }

    private static async Task<DateTime> GetApplicationUpdatedAtAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        return await db.DonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => x.NgayCapNhat)
            .SingleAsync();
    }

    private static async Task<int> GetUserIdAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static async Task<ApplicationSnapshot> GetApplicationSnapshotFromDbAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.AsNoTracking().SingleAsync(x => x.MaDonTu == applicationId);
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion));
    }

    private static ApplicationDbContext CreateDbContext(DbCommandInterceptor? interceptor = null)
    {
        var connectionString = GetSharedTestConnectionString();
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString);
        if (interceptor is not null)
        {
            builder.AddInterceptors(interceptor);
        }

        var options = builder.Options;
        return new ApplicationDbContext(options);
    }

    private sealed record ApplicationSnapshot(int MaDonTu, int MaDonVi, string RowVersion);

    private sealed class TaggedCommandCounterInterceptor : DbCommandInterceptor
    {
        private readonly string _tag;

        public TaggedCommandCounterInterceptor(string tag)
        {
            _tag = tag;
        }

        public int TaggedCommandCount { get; private set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            CountIfTagged(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            CountIfTagged(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void CountIfTagged(DbCommand command)
        {
            if (command.CommandText.Contains(_tag, StringComparison.Ordinal))
            {
                TaggedCommandCount++;
            }
        }
    }
}
