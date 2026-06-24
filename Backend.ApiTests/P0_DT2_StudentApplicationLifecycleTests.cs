using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT2_StudentApplicationLifecycleTests : ApiTestBase
{
    private const string StudentEmail = "student.cntt01@lms.local";
    private const string OtherStudentEmail = "student.tkdh01@lms.local";
    private const string TeacherEmail = "teacher.csharp.a@lms.local";
    private const string TestPrefix = "NUnit P0-DT2";

    [Test]
    public async Task Anonymous_List_ShouldReturn401()
    {
        using var client = new HttpClient { BaseAddress = BaseUri };
        using var response = await client.GetAsync("api/student/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task NonStudent_List_ShouldReturnForbidden()
    {
        using var teacherClient = await CreateAuthenticatedClientAsync(TeacherEmail);
        using var response = await teacherClient.GetAsync("api/student/applications");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task CreateDraft_EmptyForm_ShouldReturnCreatedAndAuthoritativeFields()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);

        using var response = await CreateDraftAsync(studentClient, ApplicationTypes.Confirmation, null, null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(data, "loaiDon"), Is.EqualTo(ApplicationTypes.Confirmation));
            Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo(ApplicationStatuses.Draft));
            Assert.That(GetRequiredString(data, "rowVersion"), Is.Not.Empty);
            Assert.That(GetRequiredString(data, "tieuDe"), Does.Contain("Đơn xác nhận"));
            Assert.That(GetRequiredProperty(data, "duLieuBieuMau").EnumerateObject().Any(), Is.False);
        });
    }

    [Test]
    public async Task CreateDraft_UnknownField_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);

        using var response = await CreateDraftAsync(
            studentClient,
            ApplicationTypes.Confirmation,
            $"{TestPrefix} unknown",
            new { field_khong_co = "x" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task ListAndDetail_ShouldReturnOwnApplicationsAndSafeFields()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} detail", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var listResponse = await studentClient.GetAsync("api/student/applications?pageIndex=1&pageSize=20");
        Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(listResponse));
        using var listRoot = await GetRootAsync(listResponse);
        var items = GetDataItems(listRoot.RootElement).EnumerateArray().ToList();
        Assert.That(items.Any(x => GetInt32(x, "maDonTu") == created.MaDonTu), Is.True);

        using var detailResponse = await studentClient.GetAsync($"api/student/applications/{created.MaDonTu}");
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(detailResponse));
        var detailBody = await detailResponse.Content.ReadAsStringAsync();
        var lowerBody = detailBody.ToLowerInvariant();
        Assert.Multiple(() =>
        {
            Assert.That(lowerBody, Does.Not.Contain("storagekey"));
            Assert.That(lowerBody, Does.Not.Contain("tenfileluu"));
            Assert.That(lowerBody, Does.Not.Contain("filehash"));
            Assert.That(lowerBody, Does.Not.Contain("urlbangchung"));
            Assert.That(lowerBody, Does.Not.Contain("ghichunoibo"));
            Assert.That(lowerBody, Does.Not.Contain("ghichu\""));
            Assert.That(lowerBody, Does.Not.Contain("snapshotjson"));
            Assert.That(lowerBody, Does.Not.Contain("ketquaxulyjson"));
            Assert.That(lowerBody, Does.Not.Contain("nhatkytudong"));
            Assert.That(lowerBody, Does.Not.Contain("password"));
            Assert.That(lowerBody, Does.Not.Contain("hash"));
        });
    }

    [Test]
    public async Task OtherStudent_CannotAccessApplication_ShouldReturn404()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var otherClient = await CreateAuthenticatedClientAsync(OtherStudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} ownership", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var response = await otherClient.GetAsync($"api/student/applications/{created.MaDonTu}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UpdateDraft_ShouldReturnOk_AndStaleRowVersionShouldConflict()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} update", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        var firstUpdate = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = $"{TestPrefix} update changed",
            duLieuBieuMau = new
            {
                loai_xac_nhan = "khac",
                muc_dich_su_dung = "NUnit changed",
                so_ban = 2
            },
            rowVersion = created.RowVersion
        });
        Assert.That(firstUpdate.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(firstUpdate));

        var staleUpdate = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = $"{TestPrefix} stale",
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit stale",
                so_ban = 1
            },
            rowVersion = created.RowVersion
        });

        Assert.That(staleUpdate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(staleUpdate));
    }

    [Test]
    public async Task UpdateDraft_NoOp_ShouldNotCreateNewLog()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} noop", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var before = await CountLogsAsync(created.MaDonTu);

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = created.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit",
                so_ban = 1
            },
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        var after = await CountLogsAsync(created.MaDonTu);
        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Submit_ValidConfirmation_ShouldSetSubmittedSlaAndPublicLog()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} submit", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new
        {
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.Multiple(() =>
        {
            Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo(ApplicationStatuses.Submitted));
            Assert.That(GetRequiredProperty(data, "ngayNop").ValueKind, Is.Not.EqualTo(JsonValueKind.Null));
            Assert.That(GetRequiredProperty(data, "hanXuLyLuc").ValueKind, Is.Not.EqualTo(JsonValueKind.Null));
            Assert.That(GetRequiredProperty(data, "timeline").EnumerateArray().Any(x => GetRequiredString(x, "hanhDong") == ApplicationActions.Submit), Is.True);
        });
    }

    [Test]
    public async Task Submit_MissingRequired_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} missing", null);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new
        {
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_ForeignDiemSo_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var foreignScore = await CreateForeignScoreAsync();
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.GradeAppeal, $"{TestPrefix} foreign score", new
        {
            ma_hoc_ky = foreignScore.MaHocKy,
            ma_mon_hoc = foreignScore.MaMonHoc,
            ma_diem_so = foreignScore.MaDiemSo,
            cot_diem = "qua_trinh",
            ly_do = "NUnit foreign score"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_RetakeWithoutMatchingScore_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var pair = await GetSubjectTermWithoutStudentScoreAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.RetakeExam, $"{TestPrefix} no matching score", new
        {
            ma_hoc_ky = pair.MaHocKy,
            ma_mon_hoc = pair.MaMonHoc,
            ly_do = "NUnit no score"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_EvidenceRequiredWithoutAttachment_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence missing", new
        {
            noi_dung = "NUnit cần minh chứng"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new
        {
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_EvidenceMetadataValid_ShouldReturnOk()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence valid", new
        {
            noi_dung = "NUnit có minh chứng"
        });
        await AddAttachmentAsync(created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new
        {
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await CancelDirectlyAsync(created.MaDonTu);
    }

    [Test]
    public async Task Submit_EvidenceOnlyDeleted_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence deleted", new
        {
            noi_dung = "NUnit minh chứng đã xóa"
        });
        await AddAttachmentAsync(created.MaDonTu, deleted: true);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_EvidenceLegacyUrl_ShouldReturnOk()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence legacy url", new
        {
            noi_dung = "NUnit minh chứng legacy"
        });
        await SetLegacyEvidenceUrlAsync(created.MaDonTu);
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = refreshed.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await CancelDirectlyAsync(created.MaDonTu);
    }

    [Test]
    public async Task Submit_EvidenceInvalidContentType_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence bad content", new
        {
            noi_dung = "NUnit minh chứng sai type"
        });
        await AddAttachmentAsync(created.MaDonTu, contentType: "application/x-msdownload");

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_EvidencePerFileOverflow_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence huge file", new
        {
            noi_dung = "NUnit minh chứng quá dung lượng"
        });
        await AddAttachmentAsync(created.MaDonTu, sizeBytes: 11L * 1024 * 1024);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Submit_EvidenceTotalOverflow_ShouldReturnBadRequest()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.TransferSchool, $"{TestPrefix} evidence total huge", new
        {
            noi_dung = "NUnit tổng dung lượng quá lớn"
        });
        await AddAttachmentAsync(created.MaDonTu, sizeBytes: 9L * 1024 * 1024);
        await AddAttachmentAsync(created.MaDonTu, sizeBytes: 9L * 1024 * 1024);
        await AddAttachmentAsync(created.MaDonTu, sizeBytes: 9L * 1024 * 1024);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Resubmit_FromSupplement_ShouldMoveToInReviewPreserveSubmitDateAndClearSupplement()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} resubmit", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await MarkNeedSupplementAsync(created.MaDonTu);
        var supplement = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/resubmit", new
        {
            rowVersion = supplement.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "trangThai"), Is.EqualTo(ApplicationStatuses.InReview));

        await using var db = CreateDbContext();
        var entity = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == created.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(entity.NgayNop, Is.Not.Null);
            Assert.That(entity.NoiDungYeuCauBoSung, Is.Null);
            Assert.That(entity.NguoiDuyetHienTai, Is.Not.Null);
        });
    }

    [Test]
    public async Task Cancel_Submitted_ShouldSetCanceledAndKeepRow()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} cancel", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var submitted = await SubmitAsync(studentClient, created);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/cancel", new
        {
            rowVersion = submitted.RowVersion,
            lyDo = "NUnit hủy đơn"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var exists = await db.DonTus.AsNoTracking().AnyAsync(x => x.MaDonTu == created.MaDonTu && x.TrangThai == ApplicationStatuses.Cancelled);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ConcurrentSubmit_SameRowVersion_ShouldHaveOneSuccessOneConflict()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} concurrency", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var first = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });
        using var second = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });
        var firstDescription = await DescribeResponseAsync(first);
        var secondDescription = await DescribeResponseAsync(second);

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK), firstDescription);
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict).Or.EqualTo(HttpStatusCode.BadRequest), secondDescription);
        });

        var submitLogs = await CountLogsAsync(created.MaDonTu, ApplicationActions.Submit);
        Assert.That(submitLogs, Is.EqualTo(1));
    }

    [Test]
    public async Task ConcurrentDuplicateSubmit_TwoDifferentDrafts_ShouldAllowOnlyOne()
    {
        using var firstClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var firstDraft = await CreateDraftAndReadAsync(firstClient, ApplicationTypes.TransferSchool, $"{TestPrefix} dup one", new
        {
            noi_dung = "NUnit duplicate transfer"
        });
        var secondDraft = await CreateDraftAndReadAsync(secondClient, ApplicationTypes.TransferSchool, $"{TestPrefix} dup two", new
        {
            noi_dung = "NUnit duplicate transfer"
        });
        await AddAttachmentAsync(firstDraft.MaDonTu);
        await AddAttachmentAsync(secondDraft.MaDonTu);

        using var first = await firstClient.PostAsJsonAsync($"api/student/applications/{firstDraft.MaDonTu}/submit", new { rowVersion = firstDraft.RowVersion });
        using var second = await secondClient.PostAsJsonAsync($"api/student/applications/{secondDraft.MaDonTu}/submit", new { rowVersion = secondDraft.RowVersion });
        var statuses = new[] { first.StatusCode, second.StatusCode };
        var firstDescription = await DescribeResponseAsync(first);
        var secondDescription = await DescribeResponseAsync(second);

        Assert.Multiple(() =>
        {
            Assert.That(statuses.Count(x => x == HttpStatusCode.OK), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
            Assert.That(statuses.Count(x => x == HttpStatusCode.Conflict), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
        });

        await using var db = CreateDbContext();
        var studentId = await GetStudentIdAsync(StudentEmail);
        var activeCount = await db.DonTus.AsNoTracking().CountAsync(x =>
            x.MaHocSinh == studentId &&
            x.LoaiDon == ApplicationTypes.TransferSchool &&
            x.TieuDe != null &&
            x.TieuDe.StartsWith(TestPrefix) &&
            x.TieuDe.Contains("dup") &&
            (x.TrangThai == ApplicationStatuses.Submitted ||
             x.TrangThai == ApplicationStatuses.InReview ||
             x.TrangThai == ApplicationStatuses.NeedSupplement));
        var submitLogs = await db.NhatKyDuyetDons.AsNoTracking().CountAsync(x =>
            (x.MaDonTu == firstDraft.MaDonTu || x.MaDonTu == secondDraft.MaDonTu) &&
            x.HanhDong == ApplicationActions.Submit);

        Assert.Multiple(() =>
        {
            Assert.That(activeCount, Is.EqualTo(1));
            Assert.That(submitLogs, Is.EqualTo(1));
        });

        await CancelDirectlyAsync(firstDraft.MaDonTu);
        await CancelDirectlyAsync(secondDraft.MaDonTu);
    }

    [Test]
    public async Task UpdateNoOp_WithStaleRowVersion_ShouldReturn409()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} noop stale", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var changed = await PutApplicationAndReadAsync(studentClient, created.MaDonTu, new
        {
            tieuDe = created.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit changed",
                so_ban = 1
            },
            rowVersion = created.RowVersion
        });

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = changed.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit changed",
                so_ban = 1
            },
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task UpdateNoOp_WithFreshRowVersion_ShouldReturn200WithoutLog()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} noop fresh", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var before = await CountLogsAsync(created.MaDonTu);

        var result = await PutApplicationAndReadAsync(studentClient, created.MaDonTu, new
        {
            tieuDe = created.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit",
                so_ban = 1
            },
            rowVersion = created.RowVersion
        });

        var after = await CountLogsAsync(created.MaDonTu);
        Assert.Multiple(() =>
        {
            Assert.That(result.RowVersion, Is.EqualTo(created.RowVersion));
            Assert.That(after, Is.EqualTo(before));
        });
    }

    [Test]
    public async Task StudentA_UpdateSubmitResubmitCancelStudentBApplication_ShouldReturn404()
    {
        using var ownerClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var otherClient = await CreateAuthenticatedClientAsync(OtherStudentEmail);
        var created = await CreateDraftAndReadAsync(ownerClient, ApplicationTypes.Confirmation, $"{TestPrefix} idor all", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await MarkNeedSupplementAsync(created.MaDonTu);
        var supplement = await GetApplicationAsync(ownerClient, created.MaDonTu);

        using var update = await otherClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = $"{TestPrefix} stolen",
            duLieuBieuMau = new { loai_xac_nhan = "dang_hoc", muc_dich_su_dung = "NUnit", so_ban = 1 },
            rowVersion = supplement.RowVersion
        });
        using var submit = await otherClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = supplement.RowVersion });
        using var resubmit = await otherClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/resubmit", new { rowVersion = supplement.RowVersion });
        using var cancel = await otherClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/cancel", new { rowVersion = supplement.RowVersion, lyDo = "stolen" });

        Assert.Multiple(() =>
        {
            Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(resubmit.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(cancel.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task LockedStudent_Returns403()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        await WithMutatedStudentAsync(StudentEmail, status: UserStatuses.DbLocked, role: null, async () =>
        {
            using var response = await studentClient.GetAsync("api/student/applications");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task TokenStudentButDatabaseRoleChanged_Returns403()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        await WithMutatedStudentAsync(StudentEmail, status: null, role: AuthRoles.ToDatabaseCode(AuthRoles.Teacher), async () =>
        {
            using var response = await studentClient.GetAsync("api/student/applications");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), await DescribeResponseAsync(response));
        });
    }

    [Test]
    public async Task ExistingDraft_WithInactiveAssignedTemplate_ShouldStillSubmit()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} inactive assigned", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var templateId = await SetAssignedTemplateActiveAsync(created.MaDonTu, false);
        try
        {
            using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        }
        finally
        {
            await SetTemplateActiveAsync(templateId, true);
            await CancelDirectlyAsync(created.MaDonTu);
        }
    }

    [Test]
    public async Task LegacyDraft_WithoutTemplate_ShouldAttachActiveTemplate()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} legacy attach", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await ClearApplicationTemplateAsync(created.MaDonTu);
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = refreshed.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await using var db = CreateDbContext();
        var hasTemplate = await db.DonTus.AsNoTracking().AnyAsync(x => x.MaDonTu == created.MaDonTu && x.MaMauDon != null);
        Assert.That(hasTemplate, Is.True);
    }

    [Test]
    public async Task LegacyDraft_WithoutAvailableTemplate_ShouldReturn409()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} legacy no active", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        var templateId = await ClearApplicationTemplateAndDeactivateActiveTemplateAsync(created.MaDonTu, ApplicationTypes.Confirmation);
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);
        try
        {
            using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = refreshed.RowVersion });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
        }
        finally
        {
            await SetTemplateActiveAsync(templateId, true);
        }
    }

    [Test]
    public async Task FormValidator_DuplicateRawProperty_ShouldFail()
    {
        var validator = new ApplicationFormDataValidator();
        var template = BuildTemplate("""
            {"fields":[{"key":"ly_do","label":"Lý do","type":"textarea","required":true}]}
            """);

        var exception = Assert.Throws<ApiException>(() => validator.Validate(template, """{"ly_do":"A","LY_DO":"B"}""", ApplicationFormValidationMode.Draft));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public void FormValidator_Normalization_ShouldRemoveOptionalNullAndKeepFalseZero()
    {
        var validator = new ApplicationFormDataValidator();
        var template = BuildTemplate("""
            {"fields":[
              {"key":"text","label":"Text","type":"text","required":false},
              {"key":"flag","label":"Flag","type":"boolean","required":false},
              {"key":"count","label":"Count","type":"number","required":false},
              {"key":"tags","label":"Tags","type":"multiselect","required":false,"options":[{"value":"A","label":"A"},{"value":"B","label":"B"}]},
              {"key":"blank","label":"Blank","type":"text","required":false}
            ]}
            """);

        var result = validator.Validate(template, """{"count":0,"flag":false,"tags":[],"blank":"   ","text":"  hello  "}""", ApplicationFormValidationMode.Draft);

        Assert.Multiple(() =>
        {
            Assert.That(result.NormalizedJson, Is.EqualTo("""{"text":"hello","flag":false,"count":0}"""));
            Assert.That(result.Values.ContainsKey("flag"), Is.True);
            Assert.That(result.Values.ContainsKey("count"), Is.True);
            Assert.That(result.Values.ContainsKey("tags"), Is.False);
            Assert.That(result.Values.ContainsKey("blank"), Is.False);
        });
    }

    [Test]
    public void FormValidator_DateSelectMultiselectAndRelatedEntity_ShouldValidateCanonicalValues()
    {
        var validator = new ApplicationFormDataValidator();
        var template = BuildTemplate("""
            {"fields":[
              {"key":"ngay","label":"Ngày","type":"date","required":true},
              {"key":"luc","label":"Lúc","type":"datetime","required":true},
              {"key":"loai","label":"Loại","type":"select","required":true,"options":[{"value":"A","label":"A"}]},
              {"key":"nhom","label":"Nhóm","type":"multiselect","required":true,"options":[{"value":"x","label":"X"},{"value":"y","label":"Y"}]},
              {"key":"hoc_ky","label":"Học kỳ","type":"related_entity","required":true,"relatedEntity":"hoc_ky"}
            ]}
            """);

        var result = validator.Validate(template, """{"luc":"2026-06-24T10:30:00Z","ngay":"2026-06-24","loai":"a","nhom":["X","y"],"hoc_ky":1}""", ApplicationFormValidationMode.Submit);

        Assert.That(result.NormalizedJson, Is.EqualTo("""{"ngay":"2026-06-24","luc":"2026-06-24T10:30:00.0000000+00:00","loai":"A","nhom":["x","y"],"hoc_ky":1}"""));
    }

    [Test]
    public void FormValidator_InvalidDateAndRelatedEntity_ShouldFail()
    {
        var validator = new ApplicationFormDataValidator();
        var template = BuildTemplate("""
            {"fields":[
              {"key":"ngay","label":"Ngày","type":"date","required":true},
              {"key":"hoc_ky","label":"Học kỳ","type":"related_entity","required":true,"relatedEntity":"hoc_ky"}
            ]}
            """);

        Assert.Multiple(() =>
        {
            Assert.That(() => validator.Validate(template, """{"ngay":"24/06/2026","hoc_ky":1}""", ApplicationFormValidationMode.Submit), Throws.TypeOf<ApiException>());
            Assert.That(() => validator.Validate(template, """{"ngay":"2026-06-24","hoc_ky":0}""", ApplicationFormValidationMode.Submit), Throws.TypeOf<ApiException>());
        });
    }

    [Test]
    public void SubmissionRuleRegistry_ShouldCoverAllApplicationTypes()
    {
        var ruleTypes = new[]
        {
            typeof(LeaveApplicationSubmissionRule),
            typeof(RetakeExamApplicationSubmissionRule),
            typeof(TransferSchoolApplicationSubmissionRule),
            typeof(CertificateApplicationSubmissionRule),
            typeof(OtherApplicationSubmissionRule),
            typeof(GradeAppealApplicationSubmissionRule),
            typeof(AcademicPauseApplicationSubmissionRule),
            typeof(ChangeMajorApplicationSubmissionRule),
            typeof(ChangeCampusApplicationSubmissionRule),
            typeof(ConfirmationApplicationSubmissionRule),
            typeof(WithdrawalApplicationSubmissionRule)
        };

        using var db = CreateDbContext();
        var rules = ruleTypes.Select(t => (IApplicationSubmissionRule)Activator.CreateInstance(t, db)!).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(rules.Select(x => x.SupportedType).Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(ApplicationTypes.All.Count));
            Assert.That(rules.Select(x => x.SupportedType), Is.EquivalentTo(ApplicationTypes.All));
        });
    }

    private async Task<HttpResponseMessage> CreateDraftAsync(HttpClient client, string type, string? title, object? form)
    {
        return await client.PostAsJsonAsync("api/student/applications", new
        {
            loaiDon = type,
            tieuDe = title,
            duLieuBieuMau = form
        });
    }

    private async Task<ApplicationSnapshot> CreateDraftAndReadAsync(HttpClient client, string type, string? title, object? form)
    {
        using var response = await CreateDraftAsync(client, type, title, form);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private async Task<ApplicationSnapshot> GetApplicationAsync(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"api/student/applications/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private async Task<ApplicationSnapshot> SubmitAsync(HttpClient client, ApplicationSnapshot application)
    {
        using var response = await client.PostAsJsonAsync($"api/student/applications/{application.MaDonTu}/submit", new
        {
            rowVersion = application.RowVersion
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private static ApplicationSnapshot ReadApplication(JsonElement data)
    {
        return new ApplicationSnapshot(
            GetInt32(data, "maDonTu"),
            GetRequiredString(data, "tieuDe"),
            GetRequiredString(data, "trangThai"),
            GetRequiredString(data, "rowVersion"));
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
            password = "Admin@123"
        });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Inconclusive($"Không login được account seed {email}. {await DescribeResponseAsync(response)}");
        }

        using var root = await GetRootAsync(response);
        return GetRequiredString(root.RootElement, "accessToken");
    }

    private async Task AddAttachmentAsync(
        int applicationId,
        bool deleted = false,
        string contentType = "application/pdf",
        long sizeBytes = 1024)
    {
        await using var db = CreateDbContext();
        var studentId = await db.DonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => x.MaHocSinh)
            .FirstAsync();

        db.TepDinhKemDonTus.Add(new TepDinhKemDonTu
        {
            MaDonTu = applicationId,
            StorageKey = $"nunit/p0-dt2/{Guid.NewGuid():N}.pdf",
            TenFileGoc = "minh-chung.pdf",
            TenFileLuu = $"{Guid.NewGuid():N}.pdf",
            ContentType = contentType,
            KichThuocByte = sizeBytes,
            NguoiTaiLen = studentId,
            NgayTao = DateTime.UtcNow,
            DaXoa = deleted,
            NguoiXoa = deleted ? studentId : null,
            NgayXoa = deleted ? DateTime.UtcNow : null
        });
        await db.SaveChangesAsync();
    }

    private static async Task SetLegacyEvidenceUrlAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.UrlBangChung = "legacy/minh-chung.pdf";
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task<DiemSo> CreateForeignScoreAsync()
    {
        await using var db = CreateDbContext();
        var otherStudent = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == OtherStudentEmail)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = $"P0DT2F{Guid.NewGuid():N}"[..20],
            TenMonHoc = "NUnit P0-DT2 môn điểm ngoại lai",
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);
        var termId = await db.HocKys.AsNoTracking()
            .Where(x => x.MaDonVi == otherStudent.MaDonVi)
            .Select(x => x.MaHocKy)
            .FirstAsync();
        await db.SaveChangesAsync();
        var score = new DiemSo
        {
            MaDonVi = otherStudent.MaDonVi,
            MaHocSinh = otherStudent.MaNguoiDung,
            MaMonHoc = subject.MaMonHoc,
            MaHocKy = termId,
            DiemQuaTrinh = 7,
            GpaMonHoc = 7,
            TrangThai = "dat",
            NamNhapHoc = 2024
        };
        db.DiemSos.Add(score);
        await db.SaveChangesAsync();
        return score;
    }

    private static async Task<(int MaMonHoc, int MaHocKy)> GetSubjectTermWithoutStudentScoreAsync(string email)
    {
        await using var db = CreateDbContext();
        _ = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = $"P0DT2{Guid.NewGuid():N}"[..20],
            TenMonHoc = "NUnit P0-DT2 môn không có điểm",
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);
        var termId = await db.HocKys.AsNoTracking()
            .Select(x => x.MaHocKy)
            .FirstAsync();
        await db.SaveChangesAsync();
        return (subject.MaMonHoc, termId);
    }

    private async Task MarkNeedSupplementAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        var adminId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.Admin) || x.VaiTroChinh == AuthRoles.ToDatabaseCode(AuthRoles.SuperAdmin))
            .Select(x => x.MaNguoiDung)
            .FirstAsync();

        application.TrangThai = ApplicationStatuses.NeedSupplement;
        application.NgayNop = application.NgayTao.AddMinutes(5);
        application.NgayCapNhat = DateTime.UtcNow;
        application.NguoiDuyetHienTai = adminId;
        application.NoiDungYeuCauBoSung = "NUnit yêu cầu bổ sung";
        db.NhatKyDuyetDons.Add(new NhatKyDuyetDon
        {
            MaDonTu = applicationId,
            MaNguoiDuyet = adminId,
            NguonThucHien = "user",
            HanhDong = ApplicationActions.RequestSupplement,
            TrangThaiCu = ApplicationStatuses.InReview,
            TrangThaiMoi = ApplicationStatuses.NeedSupplement,
            GhiChuCongKhai = "NUnit yêu cầu bổ sung",
            HienThiChoHocSinh = true,
            NgayTao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private async Task CancelDirectlyAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstOrDefaultAsync(x => x.MaDonTu == applicationId);
        if (application is not null)
        {
            application.TrangThai = ApplicationStatuses.Cancelled;
            application.NgayCapNhat = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    private async Task<int> CountLogsAsync(int applicationId, string? action = null)
    {
        await using var db = CreateDbContext();
        var query = db.NhatKyDuyetDons.AsNoTracking().Where(x => x.MaDonTu == applicationId);
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.HanhDong == action);
        }

        return await query.CountAsync();
    }

    private static async Task<int> GetStudentIdAsync(string email)
    {
        await using var db = CreateDbContext();
        return await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaNguoiDung)
            .FirstAsync();
    }

    private static async Task WithMutatedStudentAsync(string email, string? status, string? role, Func<Task> action)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.FirstAsync(x => x.Email == email);
        var oldStatus = student.TrangThai;
        var oldRole = student.VaiTroChinh;
        if (status is not null)
        {
            student.TrangThai = status;
        }

        if (role is not null)
        {
            student.VaiTroChinh = role;
        }

        await db.SaveChangesAsync();
        try
        {
            await action();
        }
        finally
        {
            student.TrangThai = oldStatus;
            student.VaiTroChinh = oldRole;
            await db.SaveChangesAsync();
        }
    }

    private static async Task<int> SetAssignedTemplateActiveAsync(int applicationId, bool active)
    {
        await using var db = CreateDbContext();
        var templateId = await db.DonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => x.MaMauDon!.Value)
            .FirstAsync();
        var template = await db.MauDonTus.FirstAsync(x => x.MaMauDon == templateId);
        template.DangHoatDong = active;
        template.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return templateId;
    }

    private static async Task SetTemplateActiveAsync(int templateId, bool active)
    {
        await using var db = CreateDbContext();
        var template = await db.MauDonTus.FirstAsync(x => x.MaMauDon == templateId);
        template.DangHoatDong = active;
        template.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task ClearApplicationTemplateAsync(int applicationId)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.MaMauDon = null;
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task<int> ClearApplicationTemplateAndDeactivateActiveTemplateAsync(int applicationId, string type)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.MaMauDon = null;
        application.NgayCapNhat = DateTime.UtcNow;
        var template = await db.MauDonTus
            .Where(x => x.LoaiDon == type && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .FirstAsync();
        template.DangHoatDong = false;
        template.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return template.MaMauDon;
    }

    private async Task<ApplicationSnapshot> PutApplicationAndReadAsync(HttpClient client, int applicationId, object request)
    {
        using var response = await client.PutAsJsonAsync($"api/student/applications/{applicationId}", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        return ReadApplication(GetRequiredProperty(root.RootElement, "data"));
    }

    private static MauDonTu BuildTemplate(string json)
    {
        return new MauDonTu
        {
            MaMauDon = 1,
            LoaiDon = ApplicationTypes.Other,
            TenMau = "NUnit template",
            PhienBan = 1,
            CauHinhJson = json,
            DangHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var connectionString = GetConnectionString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static string GetConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var root = FindRepoRoot();
        foreach (var file in new[] { "appsettings.Development.json", "appsettings.json" })
        {
            var path = Path.Combine(root, "Backend", file);
            if (!File.Exists(path))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var value = document.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        Assert.Fail("Không tìm thấy connection string để chạy P0-DT2 tests.");
        throw new InvalidOperationException();
    }

    private static string FindRepoRoot()
    {
        var directory = TestContext.CurrentContext.TestDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (Directory.Exists(Path.Combine(directory, "Backend")) &&
                Directory.Exists(Path.Combine(directory, "Backend.ApiTests")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        Assert.Fail("Không tìm thấy repo root.");
        throw new InvalidOperationException();
    }

    private sealed record ApplicationSnapshot(int MaDonTu, string TieuDe, string TrangThai, string RowVersion);
}
