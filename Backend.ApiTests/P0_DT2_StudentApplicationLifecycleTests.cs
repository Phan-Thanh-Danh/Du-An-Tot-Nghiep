using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json;
using System.Reflection;
using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
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

    [OneTimeSetUp]
    public void ValidateP0Dt2Environment()
    {
        _ = GetConnectionString();
    }

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
    public async Task LegacyDraft_UpdateNoOp_ShouldPersistAssignedTemplate()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} legacy update noop", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await ClearApplicationTemplateAsync(created.MaDonTu);
        var legacy = await GetApplicationAsync(studentClient, created.MaDonTu);

        var updated = await PutApplicationAndReadAsync(studentClient, created.MaDonTu, new
        {
            tieuDe = legacy.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit",
                so_ban = 1
            },
            rowVersion = legacy.RowVersion
        });

        await using var db = CreateDbContext();
        var entity = await db.DonTus.AsNoTracking().FirstAsync(x => x.MaDonTu == created.MaDonTu);
        var hiddenLog = await db.NhatKyDuyetDons.AsNoTracking().AnyAsync(x =>
            x.MaDonTu == created.MaDonTu &&
            x.HanhDong == ApplicationActions.Update &&
            !x.HienThiChoHocSinh &&
            x.SnapshotJson != null &&
            x.SnapshotJson.Contains("\"templateAssigned\":true"));

        Assert.Multiple(() =>
        {
            Assert.That(entity.MaMauDon, Is.Not.Null);
            Assert.That(updated.RowVersion, Is.Not.EqualTo(legacy.RowVersion));
            Assert.That(hiddenLog, Is.True);
        });
    }

    [Test]
    public async Task Update_MissingDuLieuBieuMau_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} update missing data", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = created.TieuDe,
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Update_NullDuLieuBieuMau_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} update null data", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = created.TieuDe,
            duLieuBieuMau = (object?)null,
            rowVersion = created.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Update_ExplicitEmptyObject_ShouldBeAcceptedForDraft_WhenTemplateAllows()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Other, $"{TestPrefix} update empty object", new
        {
            noi_dung = "NUnit"
        });

        var updated = await PutApplicationAndReadAsync(studentClient, created.MaDonTu, new
        {
            tieuDe = created.TieuDe,
            duLieuBieuMau = new { },
            rowVersion = created.RowVersion
        });

        Assert.That(updated.TrangThai, Is.EqualTo(ApplicationStatuses.Draft));
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
    public async Task CrossCheckScore_FromDifferentCampus_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var differentCampusId = await GetDifferentCampusIdAsync(StudentEmail);
        var score = await CreateStudentScoreAsync(StudentEmail, differentCampusId);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.GradeAppeal, $"{TestPrefix} cross score campus", new
        {
            ma_hoc_ky = score.MaHocKy,
            ma_mon_hoc = score.MaMonHoc,
            ma_diem_so = score.MaDiemSo,
            cot_diem = "qua_trinh",
            ly_do = "NUnit cross campus score"
        });
        await AddAttachmentAsync(created.MaDonTu);

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
    public async Task RetakeDuplicate_Subject12_ShouldNotConflictWithSubject123()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var termId = await GetStudentTermIdAsync(StudentEmail);
        var subject12 = await CreateSubjectAndScoreAsync(StudentEmail, 12, termId, null);
        var subject123 = await CreateSubjectAndScoreAsync(StudentEmail, 123, termId, null);
        await AddActiveApplicationAsync(StudentEmail, ApplicationTypes.RetakeExam, $$"""{"ma_hoc_ky":{{termId}},"ma_mon_hoc":{{subject123}},"ly_do":"NUnit 123"}""");
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.RetakeExam, $"{TestPrefix} retake 12 no substring conflict", new
        {
            ma_hoc_ky = termId,
            ma_mon_hoc = subject12,
            ly_do = "NUnit subject 12"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await CancelDirectlyAsync(created.MaDonTu);
    }

    [Test]
    public async Task RetakeDuplicate_ExactSubjectAndTerm_ShouldConflict()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var termId = await GetStudentTermIdAsync(StudentEmail);
        var subjectId = await CreateSubjectAndScoreAsync(StudentEmail, null, termId, null);
        await AddActiveApplicationAsync(StudentEmail, ApplicationTypes.RetakeExam, $$"""{"ma_hoc_ky":{{termId}},"ma_mon_hoc":{{subjectId}},"ly_do":"NUnit exact"}""");
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.RetakeExam, $"{TestPrefix} retake exact conflict", new
        {
            ma_hoc_ky = termId,
            ma_mon_hoc = subjectId,
            ly_do = "NUnit exact"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Retake_ScoreFromDifferentCampus_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var termId = await GetStudentTermIdAsync(StudentEmail);
        var differentCampusId = await GetDifferentCampusIdAsync(StudentEmail);
        var subjectId = await CreateSubjectAndScoreAsync(StudentEmail, null, termId, differentCampusId);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.RetakeExam, $"{TestPrefix} retake different campus score", new
        {
            ma_hoc_ky = termId,
            ma_mon_hoc = subjectId,
            ly_do = "NUnit different campus"
        });

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GradeAppealDuplicate_DifferentScoreColumn_ShouldNotConflict()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var score = await CreateStudentScoreAsync(StudentEmail);
        await AddActiveApplicationAsync(StudentEmail, ApplicationTypes.GradeAppeal, $$"""{"ma_hoc_ky":{{score.MaHocKy}},"ma_mon_hoc":{{score.MaMonHoc}},"ma_diem_so":{{score.MaDiemSo}},"cot_diem":"qua_trinh","ly_do":"NUnit qua trinh"}""");
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.GradeAppeal, $"{TestPrefix} grade different column", new
        {
            ma_hoc_ky = score.MaHocKy,
            ma_mon_hoc = score.MaMonHoc,
            ma_diem_so = score.MaDiemSo,
            cot_diem = "giua_ky",
            ly_do = "NUnit giữa kỳ"
        });
        await AddAttachmentAsync(created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        await CancelDirectlyAsync(created.MaDonTu);
    }

    [Test]
    public async Task GradeAppealDuplicate_ExactScoreAndColumn_ShouldConflict()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var score = await CreateStudentScoreAsync(StudentEmail);
        await AddActiveApplicationAsync(StudentEmail, ApplicationTypes.GradeAppeal, $$"""{"ma_hoc_ky":{{score.MaHocKy}},"ma_mon_hoc":{{score.MaMonHoc}},"ma_diem_so":{{score.MaDiemSo}},"cot_diem":"qua_trinh","ly_do":"NUnit exact"}""");
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.GradeAppeal, $"{TestPrefix} grade exact conflict", new
        {
            ma_hoc_ky = score.MaHocKy,
            ma_mon_hoc = score.MaMonHoc,
            ma_diem_so = score.MaDiemSo,
            cot_diem = "qua_trinh",
            ly_do = "NUnit exact"
        });
        await AddAttachmentAsync(created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task AcademicPause_FractionalMonths_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var termId = await GetStudentTermIdAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.AcademicPause, $"{TestPrefix} pause fractional", new
        {
            ma_hoc_ky_bat_dau = termId,
            thoi_luong_du_kien = 1.5m,
            ly_do = "NUnit fractional",
            cam_ket_lien_he = true
        });
        await AddAttachmentAsync(created.MaDonTu);

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
    public async Task Submit_TemplateFieldEvidenceRequiredButAbsentEvidence_ShouldReturn400()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Other, $"{TestPrefix} field evidence absent", new { });
        await AssignCustomTemplateAsync(created.MaDonTu, ApplicationTypes.Other, """
            {"fields":[
              {"key":"ghi_chu","label":"Ghi chú","type":"textarea","required":false,"evidenceRequired":true}
            ]}
            """);
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = refreshed.RowVersion });

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
        using var setupClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var firstClient = await CreateAuthenticatedClientAsync(StudentEmail);
        using var secondClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(setupClient, ApplicationTypes.Confirmation, $"{TestPrefix} concurrency", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });

        var firstTask = firstClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });
        var secondTask = secondClient.PostAsJsonAsync($"api/student/applications/{created.MaDonTu}/submit", new { rowVersion = created.RowVersion });
        var responses = await Task.WhenAll(firstTask, secondTask);
        using var first = responses[0];
        using var second = responses[1];
        var statuses = responses.Select(x => x.StatusCode).ToList();
        var firstDescription = await DescribeResponseAsync(first);
        var secondDescription = await DescribeResponseAsync(second);

        Assert.Multiple(() =>
        {
            Assert.That(statuses.Count(x => x == HttpStatusCode.OK), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
            Assert.That(statuses.Count(x => x == HttpStatusCode.Conflict), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
            Assert.That(statuses, Does.Not.Contain(HttpStatusCode.BadRequest), $"{firstDescription}\n{secondDescription}");
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

        var firstTask = firstClient.PostAsJsonAsync($"api/student/applications/{firstDraft.MaDonTu}/submit", new { rowVersion = firstDraft.RowVersion });
        var secondTask = secondClient.PostAsJsonAsync($"api/student/applications/{secondDraft.MaDonTu}/submit", new { rowVersion = secondDraft.RowVersion });
        var responses = await Task.WhenAll(firstTask, secondTask);
        using var first = responses[0];
        using var second = responses[1];
        var statuses = responses.Select(x => x.StatusCode).ToList();
        var firstDescription = await DescribeResponseAsync(first);
        var secondDescription = await DescribeResponseAsync(second);

        Assert.Multiple(() =>
        {
            Assert.That(statuses.Count(x => x == HttpStatusCode.OK), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
            Assert.That(statuses.Count(x => x == HttpStatusCode.Conflict), Is.EqualTo(1), $"{firstDescription}\n{secondDescription}");
            Assert.That(statuses, Does.Not.Contain(HttpStatusCode.BadRequest), $"{firstDescription}\n{secondDescription}");
        });

        await using var db = CreateDbContext();
        var activeCount = await db.DonTus.AsNoTracking().CountAsync(x =>
            (x.MaDonTu == firstDraft.MaDonTu || x.MaDonTu == secondDraft.MaDonTu) &&
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
    public async Task Update_LegacyArrayJson_ShouldNotReturn500()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} legacy array json", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await SetLegacyFormJsonAsync(created.MaDonTu, "[]");
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = refreshed.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit fixed",
                so_ban = 1
            },
            rowVersion = refreshed.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task Update_LegacyDuplicateCaseInsensitiveKeys_ShouldNotReturn500()
    {
        using var studentClient = await CreateAuthenticatedClientAsync(StudentEmail);
        var created = await CreateDraftAndReadAsync(studentClient, ApplicationTypes.Confirmation, $"{TestPrefix} legacy duplicate keys", new
        {
            loai_xac_nhan = "dang_hoc",
            muc_dich_su_dung = "NUnit",
            so_ban = 1
        });
        await SetLegacyFormJsonAsync(created.MaDonTu, """{"ly_do":"A","LY_DO":"B"}""");
        var refreshed = await GetApplicationAsync(studentClient, created.MaDonTu);

        using var response = await studentClient.PutAsJsonAsync($"api/student/applications/{created.MaDonTu}", new
        {
            tieuDe = refreshed.TieuDe,
            duLieuBieuMau = new
            {
                loai_xac_nhan = "dang_hoc",
                muc_dich_su_dung = "NUnit fixed",
                so_ban = 1
            },
            rowVersion = refreshed.RowVersion
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
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
    public void FormValidator_OptionalEvidenceRequiredFieldAbsent_ShouldStillRequireEvidence()
    {
        var validator = new ApplicationFormDataValidator();
        var template = BuildTemplate("""
            {"fields":[
              {"key":"ghi_chu","label":"Ghi chú","type":"textarea","required":false,"evidenceRequired":true}
            ]}
            """);

        var result = validator.Validate(template, "{}", ApplicationFormValidationMode.Draft);

        Assert.That(result.RequiresEvidence, Is.True);
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
    public void FormValidator_MalformedTemplate_ShouldReturnControlledApiException()
    {
        var validator = new ApplicationFormDataValidator();
        var exception = Assert.Throws<ApiException>(() => validator.Validate(BuildTemplate("""{"fields":["""), "{}", ApplicationFormValidationMode.Draft));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(exception.Message, Is.EqualTo("Cấu hình mẫu đơn không hợp lệ."));
        });
    }

    [Test]
    public void FormValidator_TemplateMissingFields_ShouldReturnControlledApiException()
    {
        var validator = new ApplicationFormDataValidator();
        var exception = Assert.Throws<ApiException>(() => validator.Validate(BuildTemplate("""{}"""), "{}", ApplicationFormValidationMode.Draft));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void FormValidator_TemplateMissingRequiredDefinition_ShouldReturnControlledApiException()
    {
        var validator = new ApplicationFormDataValidator();
        var exception = Assert.Throws<ApiException>(() => validator.Validate(BuildTemplate("""{"fields":[{"key":"a","label":"A"}]}"""), "{}", ApplicationFormValidationMode.Draft));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public void TryGetInt_DecimalOutsideIntRange_ShouldReturnFalse()
    {
        Assert.That(InvokeTryGetInt(new Dictionary<string, object?> { ["value"] = (decimal)int.MaxValue + 1 }, "value", out _), Is.False);
    }

    [Test]
    public void TryGetInt_IntegralDecimalWithinRange_ShouldReturnTrue()
    {
        var ok = InvokeTryGetInt(new Dictionary<string, object?> { ["value"] = 123m }, "value", out var value);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.True);
            Assert.That(value, Is.EqualTo(123));
        });
    }

    [Test]
    public void Submission_MissingRegisteredRule_ShouldFailClosed()
    {
        using var db = CreateDbContext();
        var service = new StudentApplicationService(
            db,
            new HttpContextAccessor(),
            new ApplicationTemplateValidator(),
            new ApplicationFormDataValidator(),
            new ApplicationReferenceValidator(db),
            new ApplicationEvidenceValidator(db),
            new ApplicationStateMachine(),
            []);

        var method = typeof(StudentApplicationService).GetMethod("GetSubmissionRule", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(service, [ApplicationTypes.Other]));
        var apiException = exception!.InnerException as ApiException;

        Assert.Multiple(() =>
        {
            Assert.That(apiException, Is.Not.Null);
            Assert.That(apiException!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(apiException.Message, Is.EqualTo("Chưa cấu hình quy tắc nghiệp vụ cho loại đơn."));
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
            password = GetP0Dt2TestPassword()
        });

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"Không login được account seed {email}. {await DescribeResponseAsync(response)}");
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

    private static async Task<int> GetStudentTermIdAsync(string email)
    {
        await using var db = CreateDbContext();
        var campusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        return await db.HocKys.AsNoTracking()
            .Where(x => x.MaDonVi == campusId)
            .Select(x => x.MaHocKy)
            .FirstAsync();
    }

    private static async Task<int> GetDifferentCampusIdAsync(string email)
    {
        await using var db = CreateDbContext();
        var campusId = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.MaDonVi)
            .FirstAsync();
        var existing = await db.DonVis.AsNoTracking()
            .Where(x => x.MaDonVi != campusId && x.ConHoatDong)
            .Select(x => (int?)x.MaDonVi)
            .FirstOrDefaultAsync();
        if (existing.HasValue)
        {
            return existing.Value;
        }

        var campus = new DonVi
        {
            TenDonVi = "NUnit P0-DT2 campus",
            CapDonVi = "co_so",
            ConHoatDong = true,
            NgayTao = DateTime.UtcNow
        };
        db.DonVis.Add(campus);
        await db.SaveChangesAsync();
        return campus.MaDonVi;
    }

    private static async Task<int> CreateSubjectAndScoreAsync(
        string email,
        int? codeSuffix,
        int termId,
        int? scoreCampusOverride)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var suffix = codeSuffix?.ToString(CultureInfo.InvariantCulture) ?? Guid.NewGuid().ToString("N")[..6];
        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = $"P0DT2S{suffix}{Guid.NewGuid():N}"[..20],
            TenMonHoc = $"NUnit P0-DT2 subject {suffix}",
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);
        await db.SaveChangesAsync();
        db.DiemSos.Add(new DiemSo
        {
            MaDonVi = scoreCampusOverride ?? student.MaDonVi,
            MaHocSinh = student.MaNguoiDung,
            MaMonHoc = subject.MaMonHoc,
            MaHocKy = termId,
            DiemQuaTrinh = 5,
            DiemGiuaKy = 5,
            DiemCuoiKy = 5,
            GpaMonHoc = 5,
            TrangThai = "rot",
            NamNhapHoc = 2024
        });
        await db.SaveChangesAsync();
        return subject.MaMonHoc;
    }

    private static async Task<DiemSo> CreateStudentScoreAsync(string email, int? scoreCampusOverride = null)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var subject = new DanhMucMonHoc
        {
            MaCodeMonHoc = $"P0DT2G{Guid.NewGuid():N}"[..20],
            TenMonHoc = "NUnit P0-DT2 grade subject",
            SoTinChi = 3,
            ConHoatDong = true
        };
        db.DanhMucMonHocs.Add(subject);
        var termId = await db.HocKys.AsNoTracking()
            .Where(x => x.MaDonVi == student.MaDonVi)
            .Select(x => x.MaHocKy)
            .FirstAsync();
        await db.SaveChangesAsync();
        var score = new DiemSo
        {
            MaDonVi = scoreCampusOverride ?? student.MaDonVi,
            MaHocSinh = student.MaNguoiDung,
            MaMonHoc = subject.MaMonHoc,
            MaHocKy = termId,
            DiemQuaTrinh = 6,
            DiemGiuaKy = 6,
            DiemCuoiKy = 6,
            GpaMonHoc = 6,
            TrangThai = "dat",
            NamNhapHoc = 2024
        };
        db.DiemSos.Add(score);
        await db.SaveChangesAsync();
        return score;
    }

    private static async Task SetLegacyFormJsonAsync(int applicationId, string json)
    {
        await using var db = CreateDbContext();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.DuLieuBieuMau = json;
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task AddActiveApplicationAsync(string email, string type, string formJson)
    {
        await using var db = CreateDbContext();
        var student = await db.NguoiDungs.AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new { x.MaNguoiDung, x.MaDonVi })
            .FirstAsync();
        var templateId = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .Select(x => x.MaMauDon)
            .FirstAsync();
        db.DonTus.Add(new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = student.MaDonVi,
            MaMauDon = templateId,
            LoaiDon = type,
            TieuDe = $"{TestPrefix} active duplicate {Guid.NewGuid():N}",
            DuLieuBieuMau = formJson,
            TrangThai = ApplicationStatuses.Submitted,
            TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow,
            NgayNop = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task AssignCustomTemplateAsync(int applicationId, string type, string configJson)
    {
        await using var db = CreateDbContext();
        var maxVersion = await db.MauDonTus.AsNoTracking()
            .Where(x => x.LoaiDon == type)
            .MaxAsync(x => x.PhienBan);
        var template = new MauDonTu
        {
            LoaiDon = type,
            TenMau = "NUnit P0-DT2 custom template",
            PhienBan = maxVersion + 1,
            CauHinhJson = configJson,
            DangHoatDong = false,
            BatBuocMinhChung = false,
            SoTepToiDa = 5,
            DungLuongTepToiDaByte = 10L * 1024 * 1024,
            TongDungLuongToiDaByte = 25L * 1024 * 1024,
            SlaGio = 72,
            NgayTao = DateTime.UtcNow
        };
        db.MauDonTus.Add(template);
        await db.SaveChangesAsync();
        var application = await db.DonTus.FirstAsync(x => x.MaDonTu == applicationId);
        application.MaMauDon = template.MaMauDon;
        application.DuLieuBieuMau = "{}";
        application.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
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

    private static bool InvokeTryGetInt(IReadOnlyDictionary<string, object?> values, string key, out int value)
    {
        var type = typeof(ApplicationFormDataValidator).Assembly.GetType("Backend.Services.Applications.ApplicationFormDataExtensions")!;
        var method = type.GetMethod("TryGetInt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        object?[] parameters = [values, key, 0];
        var ok = (bool)method.Invoke(null, parameters)!;
        value = (int)parameters[2]!;
        return ok;
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
        var connectionString = Environment.GetEnvironmentVariable("P0_DT2_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail("Thiếu env var P0_DT2_TEST_CONNECTION_STRING cho P0-DT2 tests.");
        }

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException exception)
        {
            Assert.Fail($"P0_DT2_TEST_CONNECTION_STRING không hợp lệ: {exception.Message}");
            throw;
        }

        if (string.IsNullOrWhiteSpace(builder.InitialCatalog) ||
            !builder.InitialCatalog.StartsWith("LMS_DT2_", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("P0-DT2 tests chỉ được chạy trên database có tên bắt đầu bằng 'LMS_DT2_'.");
        }

        var backendConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(backendConnectionString))
        {
            Assert.Fail("Thiếu env var ConnectionStrings__DefaultConnection cho backend test server.");
        }

        var backendBuilder = new SqlConnectionStringBuilder(backendConnectionString);
        if (!string.Equals(backendBuilder.InitialCatalog, builder.InitialCatalog, StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("Backend test server phải dùng cùng isolated DT2 database với P0_DT2_TEST_CONNECTION_STRING.");
        }

        return connectionString!;
    }

    private static string GetP0Dt2TestPassword()
    {
        var password = Environment.GetEnvironmentVariable("P0_DT2_TEST_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            Assert.Fail("Thiếu env var P0_DT2_TEST_PASSWORD cho P0-DT2 tests.");
        }

        return password!;
    }

    private sealed record ApplicationSnapshot(int MaDonTu, string TieuDe, string TrangThai, string RowVersion);
}
