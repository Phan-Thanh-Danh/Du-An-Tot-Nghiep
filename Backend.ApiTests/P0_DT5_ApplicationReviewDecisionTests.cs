using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT5_ApplicationReviewDecisionTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string PrincipalEmail = "principal@lms.local";
    private const string CampusAdminEmail = "campusadmin.hcm@lms.local";
    private const string SuperAdminEmail = "superadmin@lms.local";
    private const string TestPrefix = "NUnit P0-DT5";

    [OneTimeSetUp]
    public void ValidateP0Dt5Environment()
    {
        ValidateSharedBackendDatabase();
        _ = GetSharedTestPassword();
        ValidateSharedStorageRoot();
    }

    [Test]
    public async Task Anonymous_RequestSupplement_ShouldReturn401()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var client = new HttpClient { BaseAddress = BaseUri };

        using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
        {
            request = "Vui lòng bổ sung hồ sơ còn thiếu.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AcademicStaff_AssignedApplication_CanRequestSupplement()
    {
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), async client =>
        {
            var assigneeId = await GetUserIdAsync(TeacherEmail);
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: assigneeId);

            using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
            {
                request = "Vui lòng bổ sung hồ sơ còn thiếu.",
                internalNote = "Ghi chú nội bộ DT5.",
                rowVersion = app.RowVersion
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
            var db = await GetApplicationFromDbAsync(app.MaDonTu);
            Assert.Multiple(() =>
            {
                Assert.That(db.TrangThai, Is.EqualTo(ApplicationStatuses.NeedSupplement));
                Assert.That(db.NguoiDuyetHienTai, Is.EqualTo(assigneeId));
                Assert.That(db.HanXuLyLuc, Is.EqualTo(app.Deadline));
                Assert.That(db.NoiDungYeuCauBoSung, Is.EqualTo("Vui lòng bổ sung hồ sơ còn thiếu."));
            });
        });
    }

    [Test]
    public async Task AcademicStaff_UnassignedToSelf_ShouldReturn403()
    {
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), async client =>
        {
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
            {
                request = "Vui lòng bổ sung hồ sơ còn thiếu.",
                rowVersion = app.RowVersion
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [TestCase("approve")]
    [TestCase("reject")]
    public async Task AcademicStaff_SensitiveDecision_ShouldReturn403(string action)
    {
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.AcademicStaff), async client =>
        {
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(TeacherEmail));
            using var response = action == "approve"
                ? await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion })
                : await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new { reason = "Không đáp ứng điều kiện xử lý.", rowVersion = app.RowVersion });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task SubCampusAdmin_CanRequestSupplement_ButApproveForbidden()
    {
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.SubCampusAdmin), async client =>
        {
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var supplement = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
            {
                request = "Vui lòng bổ sung hồ sơ còn thiếu.",
                rowVersion = app.RowVersion
            });
            Assert.That(supplement.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(supplement));

            var second = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var approve = await client.PostAsJsonAsync($"api/admin/applications/{second.MaDonTu}/approve", new { rowVersion = second.RowVersion });
            Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(approve));
        });
    }

    [Test]
    public async Task Principal_ApproveRejectSucceed_ButRequestSupplementForbidden()
    {
        await WithMutatedUserAndLoginAsync(TeacherEmail, AuthRoles.ToDatabaseCode(AuthRoles.Principal), async client =>
        {
            var principal = await GetUserAsync(TeacherEmail);
            var studentId = await CreateStudentInCampusAsync(principal.MaDonVi);
            var approveApp = await CreateApplicationAsync(ApplicationStatuses.InReview, studentId: studentId, campusId: principal.MaDonVi, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var approve = await client.PostAsJsonAsync($"api/admin/applications/{approveApp.MaDonTu}/approve", new { rowVersion = approveApp.RowVersion });
            Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(approve));

            var rejectApp = await CreateApplicationAsync(ApplicationStatuses.InReview, studentId: studentId, campusId: principal.MaDonVi, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var reject = await client.PostAsJsonAsync($"api/admin/applications/{rejectApp.MaDonTu}/reject", new { reason = "Không đáp ứng điều kiện xử lý.", rowVersion = rejectApp.RowVersion });
            Assert.That(reject.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(reject));

            var supplementApp = await CreateApplicationAsync(ApplicationStatuses.InReview, studentId: studentId, campusId: principal.MaDonVi, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var supplement = await client.PostAsJsonAsync($"api/admin/applications/{supplementApp.MaDonTu}/request-supplement", new
            {
                request = "Vui lòng bổ sung hồ sơ còn thiếu.",
                rowVersion = supplementApp.RowVersion
            });
            Assert.That(supplement.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(supplement));
        });
    }

    [Test]
    public async Task LockedActor_And_DatabaseRoleChanged_ShouldReturn403()
    {
        await WithLoggedInThenMutatedUserAsync(SuperAdminEmail, AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin), UserStatuses.DbLocked, async client =>
        {
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });

        await WithLoggedInThenMutatedUserAsync(SuperAdminEmail, AuthRoles.ToDatabaseCode(AuthRoles.Teacher), UserStatuses.DbActive, async client =>
        {
            var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
            using var response = await client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task CrossCampus_ShouldReturn404()
    {
        using var campusAdmin = await CreateAuthenticatedClientAsync(CampusAdminEmail);
        var otherCampus = await CreateDifferentCampusAsync();
        var studentId = await CreateStudentInCampusAsync(otherCampus);
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, studentId: studentId, campusId: otherCampus, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await campusAdmin.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CampusAdmin_CanOverrideAssignmentInScope()
    {
        using var campusAdmin = await CreateAuthenticatedClientAsync(CampusAdminEmail);
        var campus = await GetUserAsync(CampusAdminEmail);
        var studentId = await CreateStudentInCampusAsync(campus.MaDonVi);
        var app = await CreateApplicationAsync(
            ApplicationStatuses.InReview,
            studentId: studentId,
            campusId: campus.MaDonVi,
            assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await campusAdmin.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Detail_AllowedActions_ShouldReflectDecisionPermissions()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.GetAsync($"api/admin/applications/{app.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var allowed = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "allowedActions");
        Assert.Multiple(() =>
        {
            Assert.That(GetBoolean(allowed, "canRequestSupplement"), Is.True);
            Assert.That(GetBoolean(allowed, "canApprove"), Is.True);
            Assert.That(GetBoolean(allowed, "canReject"), Is.True);
        });
    }

    [TestCase("request-supplement")]
    [TestCase("approve")]
    [TestCase("reject")]
    public async Task UnassignedDecision_ShouldReturn409(string endpoint)
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview);
        using var response = endpoint switch
        {
            "request-supplement" => await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/{endpoint}", new { request = "Vui lòng bổ sung hồ sơ còn thiếu.", rowVersion = app.RowVersion }),
            "approve" => await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/{endpoint}", new { rowVersion = app.RowVersion }),
            _ => await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/{endpoint}", new { reason = "Không đáp ứng điều kiện xử lý.", rowVersion = app.RowVersion })
        };

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task RequestSupplement_StudentSeesPublicButNotInternal()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new
        {
            request = "Vui lòng bổ sung hồ sơ còn thiếu.",
            internalNote = "Ghi chú nội bộ tuyệt mật.",
            rowVersion = app.RowVersion
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));

        using var student = await CreateAuthenticatedClientAsync(StudentEmail);
        using var detail = await student.GetAsync($"api/student/applications/{app.MaDonTu}");
        Assert.That(detail.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detail));
        var body = await detail.Content.ReadAsStringAsync();
        using var root = JsonDocument.Parse(body);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetOptionalString(data, "noiDungYeuCauBoSung"), Is.EqualTo("Vui lòng bổ sung hồ sơ còn thiếu."));
            Assert.That(body, Does.Not.Contain("Ghi chú nội bộ tuyệt mật."));
        });
    }

    [Test]
    public async Task RequestSupplement_InvalidLengthWrongStateAndStaleRowVersion_ShouldFail()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var invalid = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new { request = "ngắn", rowVersion = app.RowVersion });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(invalid));

        var submitted = await CreateApplicationAsync(ApplicationStatuses.Submitted, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var wrongState = await Client.PostAsJsonAsync($"api/admin/applications/{submitted.MaDonTu}/request-supplement", new { request = "Vui lòng bổ sung hồ sơ còn thiếu.", rowVersion = submitted.RowVersion });
        Assert.That(wrongState.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(wrongState));

        await TouchApplicationAsync(app.MaDonTu);
        using var stale = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new { request = "Vui lòng bổ sung hồ sơ còn thiếu.", rowVersion = app.RowVersion });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(stale));
    }

    [TestCase("")]
    [TestCase("not-base64")]
    [TestCase("AQ==")]
    public async Task Decision_InvalidRowVersion_ShouldReturn400(string rowVersion)
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Approve_Success_ShouldMutateTimelineAuditAndAllowedActions()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail), rejectReason: "old", supplementRequest: "old");

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new
        {
            publicNote = "",
            internalNote = "Ghi chú duyệt nội bộ.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        var db = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(db.TrangThai, Is.EqualTo(ApplicationStatuses.Approved));
            Assert.That(db.NgayDuyet, Is.Not.Null);
            Assert.That(db.NguoiXuLyCuoi, Is.EqualTo(GetUserIdAsync(SuperAdminEmail).GetAwaiter().GetResult()));
            Assert.That(db.NguoiDuyetHienTai, Is.Null);
            Assert.That(db.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.Pending));
            Assert.That(db.HanXuLyLuc, Is.EqualTo(app.Deadline));
            Assert.That(db.LyDoTuChoi, Is.Null);
            Assert.That(db.NoiDungYeuCauBoSung, Is.Null);
            Assert.That(GetRequiredProperty(data, "allowedActions").GetProperty("canApprove").GetBoolean(), Is.False);
        });
        Assert.That(await CountLogsAsync(app.MaDonTu, ApplicationActions.Approve), Is.EqualTo(1));
        Assert.That(await CountAuditsAsync(app.MaDonTu, ApplicationActions.Approve), Is.EqualTo(1));
    }

    [Test]
    public async Task Approve_TimelineMetadataAndAudit_ShouldBeSafe()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var body = await response.Content.ReadAsStringAsync();
        using var root = JsonDocument.Parse(body);
        var timeline = GetRequiredProperty(GetRequiredProperty(root.RootElement, "data"), "timeline");
        var approveLog = timeline.EnumerateArray().Single(x => GetRequiredString(x, "hanhDong") == ApplicationActions.Approve);
        var metadata = GetRequiredProperty(approveLog, "metadata");
        Assert.Multiple(() =>
        {
            Assert.That(GetOptionalString(metadata, "decision"), Is.EqualTo("approve"));
            Assert.That(HasProperty(approveLog, "snapshotJson"), Is.False);
            Assert.That(body.ToLowerInvariant(), Does.Not.Contain("storagekey"));
            Assert.That(body.ToLowerInvariant(), Does.Not.Contain("filehash"));
        });
    }

    [Test]
    public async Task Approve_MissingEvidenceAndInvalidReference_ShouldFail()
    {
        var evidenceTemplateId = await CreateEvidenceRequiredTemplateAsync();
        var missingEvidence = await CreateApplicationAsync(
            ApplicationStatuses.InReview,
            assigneeId: await GetUserIdAsync(SuperAdminEmail),
            templateId: evidenceTemplateId,
            type: ApplicationTypes.Other,
            formJson: "{\"noi_dung\":\"Can minh chung\"}");
        using var missingEvidenceResponse = await Client.PostAsJsonAsync($"api/admin/applications/{missingEvidence.MaDonTu}/approve", new { rowVersion = missingEvidence.RowVersion });
        Assert.That(missingEvidenceResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(missingEvidenceResponse));

        var invalidTemplateId = await CreateInvalidReferenceTemplateAsync();
        var invalidReference = await CreateApplicationAsync(
            ApplicationStatuses.InReview,
            assigneeId: await GetUserIdAsync(SuperAdminEmail),
            templateId: invalidTemplateId,
            type: ApplicationTypes.Other,
            formJson: "{\"ma_diem_so\":99999999}");
        using var invalidReferenceResponse = await Client.PostAsJsonAsync($"api/admin/applications/{invalidReference.MaDonTu}/approve", new { rowVersion = invalidReference.RowVersion });
        Assert.That(invalidReferenceResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(invalidReferenceResponse));
    }

    [TestCase(ApplicationStatuses.Approved)]
    [TestCase(ApplicationStatuses.Rejected)]
    [TestCase(ApplicationStatuses.Cancelled)]
    public async Task Decision_FromTerminalState_ShouldReturn400(string status)
    {
        var app = await CreateApplicationAsync(status, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Reject_Success_ShouldSetReasonAndStudentVisibility()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail), supplementRequest: "old");

        using var response = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new
        {
            reason = "Không đáp ứng điều kiện xử lý.",
            internalNote = "Ghi chú từ chối nội bộ.",
            rowVersion = app.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var db = await GetApplicationFromDbAsync(app.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(db.TrangThai, Is.EqualTo(ApplicationStatuses.Rejected));
            Assert.That(db.LyDoTuChoi, Is.EqualTo("Không đáp ứng điều kiện xử lý."));
            Assert.That(db.NoiDungYeuCauBoSung, Is.Null);
            Assert.That(db.NgayDuyet, Is.Null);
            Assert.That(db.NguoiDuyetHienTai, Is.Null);
            Assert.That(db.TrangThaiXuLyNghiepVu, Is.EqualTo(ApplicationProcessingStatuses.NotProcessed));
        });

        using var student = await CreateAuthenticatedClientAsync(StudentEmail);
        using var detail = await student.GetAsync($"api/student/applications/{app.MaDonTu}");
        var body = await detail.Content.ReadAsStringAsync();
        using var root = JsonDocument.Parse(body);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetOptionalString(data, "lyDoTuChoi"), Is.EqualTo("Không đáp ứng điều kiện xử lý."));
            Assert.That(body, Does.Not.Contain("Ghi chú từ chối nội bộ."));
        });
    }

    [Test]
    public async Task Reject_InvalidReasonAndStaleRowVersion_ShouldFail()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var invalid = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new { reason = "ngắn", rowVersion = app.RowVersion });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(invalid));

        await TouchApplicationAsync(app.MaDonTu);
        using var stale = await Client.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new { reason = "Không đáp ứng điều kiện xử lý.", rowVersion = app.RowVersion });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(stale));
    }

    [Test]
    public async Task ConcurrentApproveAndReject_ShouldHaveOne200One409()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var approveTask = firstClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
        var rejectTask = secondClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/reject", new { reason = "Không đáp ứng điều kiện xử lý.", rowVersion = app.RowVersion });
        var responses = await Task.WhenAll(approveTask, rejectTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountDecisionLogsAsync(app.MaDonTu), Is.EqualTo(1));
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
    public async Task ConcurrentSupplementAndApprove_ShouldHaveOne200One409()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var supplementTask = firstClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/request-supplement", new { request = "Vui lòng bổ sung hồ sơ còn thiếu.", rowVersion = app.RowVersion });
        var approveTask = secondClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
        var responses = await Task.WhenAll(supplementTask, approveTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(await CountDecisionLogsAsync(app.MaDonTu), Is.EqualTo(1));
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
    public async Task ConcurrentReassignAndApprove_ShouldHaveOne200One409()
    {
        var app = await CreateApplicationAsync(ApplicationStatuses.InReview, assigneeId: await GetUserIdAsync(SuperAdminEmail));
        var assigneeId = await CreateAssignableUserInCampusAsync(app.MaDonVi);
        using var firstClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(SuperAdminEmail);

        var assignTask = firstClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/assign", new
        {
            assigneeId,
            rowVersion = app.RowVersion,
            lyDo = "Phân công lại để kiểm thử cạnh tranh."
        });
        var approveTask = secondClient.PostAsJsonAsync($"api/admin/applications/{app.MaDonTu}/approve", new { rowVersion = app.RowVersion });
        var responses = await Task.WhenAll(assignTask, approveTask);

        try
        {
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
            Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1), string.Join(", ", responses.Select(x => x.StatusCode)));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private static async Task<ApplicationSnapshot> CreateApplicationAsync(
        string status,
        int? studentId = null,
        int? campusId = null,
        int? assigneeId = null,
        int? templateId = null,
        string? type = null,
        string? formJson = null,
        string? rejectReason = null,
        string? supplementRequest = null)
    {
        await using var db = CreateDbContext();
        var student = studentId.HasValue
            ? await db.NguoiDungs.AsNoTracking().Where(x => x.MaNguoiDung == studentId.Value).Select(x => new { x.MaNguoiDung, x.MaDonVi }).FirstAsync()
            : await db.NguoiDungs.AsNoTracking().Where(x => x.Email == StudentEmail).Select(x => new { x.MaNguoiDung, x.MaDonVi }).FirstAsync();
        var applicationType = type ?? ApplicationTypes.Confirmation;
        var resolvedTemplateId = templateId ?? await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == applicationType && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        var now = DateTime.UtcNow;
        var deadline = now.AddHours(48);
        var application = new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = campusId ?? student.MaDonVi,
            MaMauDon = resolvedTemplateId,
            LoaiDon = applicationType,
            TieuDe = $"{TestPrefix} {status} {Guid.NewGuid():N}",
            DuLieuBieuMau = formJson ?? "{\"loai_xac_nhan\":\"dang_hoc\",\"muc_dich_su_dung\":\"NUnit\",\"so_ban\":1}",
            TrangThai = status,
            TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed,
            NguoiDuyetHienTai = assigneeId,
            LyDoTuChoi = rejectReason,
            NoiDungYeuCauBoSung = supplementRequest,
            NgayTao = now,
            NgayCapNhat = now,
            NgayNop = status == ApplicationStatuses.Draft ? null : now.AddMinutes(-10),
            NgayDuyet = status == ApplicationStatuses.Approved ? now : null,
            HanXuLyLuc = deadline
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
            GhiChuCongKhai = $"{TestPrefix} tạo đơn.",
            HienThiChoHocSinh = true,
            NgayTao = now
        });
        await db.SaveChangesAsync();
        return new ApplicationSnapshot(application.MaDonTu, application.MaDonVi, Convert.ToBase64String(application.RowVersion), deadline);
    }

    private static async Task<int> CreateEvidenceRequiredTemplateAsync()
    {
        return await CreateTemplateAsync(
            ApplicationTypes.Other,
            """
            {"fields":[{"key":"noi_dung","label":"Nội dung","type":"textarea","required":true,"evidenceRequired":true}]}
            """,
            true);
    }

    private static async Task<int> CreateInvalidReferenceTemplateAsync()
    {
        return await CreateTemplateAsync(
            ApplicationTypes.Other,
            """
            {"fields":[{"key":"ma_diem_so","label":"Điểm số","type":"related_entity","relatedEntity":"diem_so","required":true}]}
            """,
            false);
    }

    private static async Task<int> CreateTemplateAsync(string type, string config, bool evidenceRequired)
    {
        await using var db = CreateDbContext();
        var nextVersion = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type)
            .Select(x => (int?)x.PhienBan)
            .MaxAsync() ?? 0;
        var template = new MauDonTu
        {
            LoaiDon = type,
            TenMau = $"{TestPrefix} template {Guid.NewGuid():N}",
            PhienBan = nextVersion + 1,
            CauHinhJson = config,
            BatBuocMinhChung = evidenceRequired,
            SoTepToiDa = 5,
            DungLuongTepToiDaByte = 10 * 1024 * 1024,
            TongDungLuongToiDaByte = 25 * 1024 * 1024,
            DangHoatDong = false,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        db.MauDonTus.Add(template);
        await db.SaveChangesAsync();
        return template.MaMauDon;
    }

    private static async Task<int> CreateStudentInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt5-student-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT5 Student",
            VaiTroChinh = AuthRoles.ToDatabaseCode(AuthRoles.Student),
            TrangThai = UserStatuses.DbActive,
            NgayTao = DateTime.UtcNow
        };
        db.NguoiDungs.Add(user);
        await db.SaveChangesAsync();
        return user.MaNguoiDung;
    }

    private static async Task<int> CreateAssignableUserInCampusAsync(int campusId)
    {
        await using var db = CreateDbContext();
        var user = new NguoiDung
        {
            MaDonVi = campusId,
            Email = $"p0dt5-assignee-{Guid.NewGuid():N}@lms.local",
            HoTen = "NUnit P0-DT5 Assignee",
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
        var campus = new DonVi
        {
            TenDonVi = $"NUnit P0-DT5 campus {Guid.NewGuid():N}",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        return campus.MaDonVi;
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

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
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
            using var client = await CreateAuthenticatedClientAsync(email);
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
        var token = await LoginAsync(email);
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

    private static async Task<ApplicationDbRecord> GetApplicationFromDbAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        return await db.DonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => new ApplicationDbRecord(
                x.TrangThai,
                x.NguoiDuyetHienTai,
                x.NguoiXuLyCuoi,
                x.LyDoTuChoi,
                x.NoiDungYeuCauBoSung,
                x.NgayDuyet,
                x.HanXuLyLuc,
                x.TrangThaiXuLyNghiepVu))
            .SingleAsync();
    }

    private static async Task<UserRecord> GetUserAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new UserRecord(x.MaNguoiDung, x.MaDonVi))
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

    private static async Task TouchApplicationAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.NgayCapNhat = DateTime.UtcNow.AddSeconds(1);
        await db.SaveChangesAsync();
    }

    private static async Task<int> CountLogsAsync(int applicationId, string action)
    {
        await using var db = CreateDbContext();
        return await db.NhatKyDuyetDons.AsNoTracking().CountAsync(x => x.MaDonTu == applicationId && x.HanhDong == action);
    }

    private static async Task<int> CountDecisionLogsAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        return await db.NhatKyDuyetDons.AsNoTracking().CountAsync(x =>
            x.MaDonTu == applicationId &&
            (x.HanhDong == ApplicationActions.Approve ||
             x.HanhDong == ApplicationActions.Reject ||
             x.HanhDong == ApplicationActions.RequestSupplement));
    }

    private static async Task<int> CountAuditsAsync(int applicationId, string action)
    {
        await using var db = CreateDbContext();
        return await db.NhatKyKiemToans.AsNoTracking().CountAsync(x =>
            x.LoaiDoiTuong == nameof(DonTu) &&
            x.MaDoiTuong == applicationId.ToString() &&
            x.HanhDong == action);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetSharedTestConnectionString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed record ApplicationSnapshot(int MaDonTu, int MaDonVi, string RowVersion, DateTime Deadline);
    private sealed record ApplicationDbRecord(
        string TrangThai,
        int? NguoiDuyetHienTai,
        int? NguoiXuLyCuoi,
        string? LyDoTuChoi,
        string? NoiDungYeuCauBoSung,
        DateTime? NgayDuyet,
        DateTime? HanXuLyLuc,
        string TrangThaiXuLyNghiepVu);
    private sealed record UserRecord(int MaNguoiDung, int MaDonVi);
}
