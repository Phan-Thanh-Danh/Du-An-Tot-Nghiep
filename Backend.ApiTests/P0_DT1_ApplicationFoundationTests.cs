using System.Net;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Services.Applications;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
[NonParallelizable]
public class P0_DT1_ApplicationFoundationApiTests : ApiTestBase
{
    [Test]
    public async Task GetSchemaTypes_Authenticated_ShouldReturnOk()
    {
        using var response = await Client.GetAsync("api/applications/schema/types");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.GetArrayLength(), Is.EqualTo(11));
    }

    [Test]
    public async Task GetSchemaStatuses_Authenticated_ShouldReturnOk()
    {
        using var response = await Client.GetAsync("api/applications/schema/statuses");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.GetArrayLength(), Is.EqualTo(7));
    }

    [Test]
    public async Task GetTemplates_ShouldReturnElevenActiveTemplates()
    {
        using var response = await Client.GetAsync("api/applications/templates");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(data.GetArrayLength(), Is.EqualTo(11));
    }

    [Test]
    public async Task GetTemplate_ValidType_ShouldReturnOk()
    {
        using var response = await Client.GetAsync("api/applications/templates/nghi_phep");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await DescribeResponseAsync(response));
        using var root = await GetRootAsync(response);
        var data = GetRequiredProperty(root.RootElement, "data");
        Assert.That(GetRequiredString(data, "loaiDon"), Is.EqualTo(ApplicationTypes.Leave));
        Assert.That(GetRequiredString(data, "cauHinhJson"), Does.Contain("fields"));
    }

    [Test]
    public async Task GetTemplate_InvalidType_ShouldReturnBadRequest()
    {
        using var response = await Client.GetAsync("api/applications/templates/khong_hop_le");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task GetTemplate_MissingInactiveType_ShouldReturnNotFound_WhenNoActiveTemplate()
    {
        await using var context = CreateDbContext();
        var template = await context.MauDonTus.FirstOrDefaultAsync(x => x.LoaiDon == ApplicationTypes.Other);
        if (template is null)
        {
            Assert.Inconclusive("Chưa có template khac để kiểm tra inactive template.");
            return;
        }

        var oldActive = template.DangHoatDong;
        template.DangHoatDong = false;
        await context.SaveChangesAsync();

        try
        {
            using var response = await Client.GetAsync("api/applications/templates/khac");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), await DescribeResponseAsync(response));
        }
        finally
        {
            template.DangHoatDong = oldActive;
            await context.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Unauthorized_ShouldReturn401()
    {
        using var anonymousClient = new HttpClient { BaseAddress = BaseUri };
        using var response = await anonymousClient.GetAsync("api/applications/schema/types");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), await DescribeResponseAsync(response));
    }

    [Test]
    public async Task OnlyOneActiveTemplatePerType_ShouldBeEnforced()
    {
        await using var context = CreateDbContext();
        var activeCount = await context.MauDonTus
            .Where(x => x.DangHoatDong)
            .GroupBy(x => x.LoaiDon)
            .Select(x => new { Type = x.Key, Count = x.Count() })
            .ToListAsync();

        Assert.That(activeCount, Has.Count.EqualTo(11));
        Assert.That(activeCount.All(x => x.Count == 1), Is.True);
    }

    [Test]
    public async Task SeededTemplates_FromDatabase_ShouldPassTemplateValidator()
    {
        await using var context = CreateDbContext();
        var validator = new ApplicationTemplateValidator();
        var templates = await context.MauDonTus
            .Where(x => x.DangHoatDong)
            .OrderBy(x => x.LoaiDon)
            .ToListAsync();

        Assert.That(templates, Has.Count.EqualTo(11));

        foreach (var template in templates)
        {
            Assert.DoesNotThrow(
                () => validator.Validate(template.CauHinhJson),
                $"Template {template.LoaiDon} phải hợp lệ theo ApplicationTemplateValidator.");

            using var document = JsonDocument.Parse(template.CauHinhJson);
            var fields = document.RootElement.GetProperty("fields");
            Assert.That(fields.GetArrayLength(), Is.GreaterThanOrEqualTo(1), template.LoaiDon);
        }

        var businessTemplates = templates
            .Where(x => x.LoaiDon is ApplicationTypes.Leave
                or ApplicationTypes.RetakeExam
                or ApplicationTypes.GradeAppeal
                or ApplicationTypes.AcademicPause
                or ApplicationTypes.ChangeCampus
                or ApplicationTypes.Confirmation
                or ApplicationTypes.Withdrawal)
            .ToList();

        Assert.That(businessTemplates, Has.Count.EqualTo(7));
        Assert.That(businessTemplates.All(x =>
        {
            using var document = JsonDocument.Parse(x.CauHinhJson);
            return document.RootElement.GetProperty("fields").GetArrayLength() >= 3;
        }), Is.True);
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
        var environmentConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var root = FindRepoRoot();
        var configPath = Path.Combine(root, "Backend", "appsettings.Development.json");
        if (!File.Exists(configPath))
        {
            configPath = Path.Combine(root, "Backend", "appsettings.json");
        }

        var json = File.ReadAllText(configPath, Encoding.UTF8);
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var connectionString = document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail("Không tìm thấy ConnectionStrings:DefaultConnection để test P0-DT1.");
        }

        return connectionString!;
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

        Assert.Fail("Không tìm thấy repo root từ thư mục test output.");
        throw new InvalidOperationException("Unreachable after Assert.Fail.");
    }
}

[TestFixture]
public class P0_DT1_ApplicationFoundationUnitTests
{
    [Test]
    public void StateMachine_ValidTransitions_ShouldPass()
    {
        var machine = new ApplicationStateMachine();

        Assert.Multiple(() =>
        {
            Assert.That(machine.CanTransition(ApplicationStatuses.Draft, ApplicationStatuses.Submitted), Is.True);
            Assert.That(machine.CanTransition(ApplicationStatuses.Draft, ApplicationStatuses.Cancelled), Is.True);
            Assert.That(machine.CanTransition(ApplicationStatuses.Submitted, ApplicationStatuses.InReview), Is.True);
            Assert.That(machine.CanTransition(ApplicationStatuses.InReview, ApplicationStatuses.NeedSupplement), Is.True);
            Assert.That(machine.CanTransition(ApplicationStatuses.InReview, ApplicationStatuses.Approved), Is.True);
            Assert.That(machine.CanTransition(ApplicationStatuses.NeedSupplement, ApplicationStatuses.InReview), Is.True);
        });
    }

    [Test]
    public void StateMachine_InvalidTransitions_ShouldFail()
    {
        var machine = new ApplicationStateMachine();

        Assert.Multiple(() =>
        {
            Assert.That(machine.CanTransition(ApplicationStatuses.NeedSupplement, ApplicationStatuses.Submitted), Is.False);
            Assert.That(machine.CanTransition(ApplicationStatuses.Approved, ApplicationStatuses.InReview), Is.False);
            Assert.That(machine.CanTransition(ApplicationStatuses.Rejected, ApplicationStatuses.InReview), Is.False);
            Assert.That(machine.CanTransition(ApplicationStatuses.Cancelled, ApplicationStatuses.Submitted), Is.False);
            Assert.Throws<ApiException>(() => machine.EnsureTransitionAllowed(ApplicationStatuses.Approved, ApplicationStatuses.Cancelled));
        });
    }

    [Test]
    public void StateMachine_TerminalStatuses_ShouldNotHaveAllowedTargets()
    {
        var machine = new ApplicationStateMachine();

        Assert.Multiple(() =>
        {
            Assert.That(machine.IsTerminal(ApplicationStatuses.Approved), Is.True);
            Assert.That(machine.IsTerminal(ApplicationStatuses.Rejected), Is.True);
            Assert.That(machine.IsTerminal(ApplicationStatuses.Cancelled), Is.True);
            Assert.That(machine.GetAllowedTargets(ApplicationStatuses.Approved), Is.Empty);
            Assert.That(machine.GetAllowedTargets(ApplicationStatuses.Rejected), Is.Empty);
            Assert.That(machine.GetAllowedTargets(ApplicationStatuses.Cancelled), Is.Empty);
        });
    }

    [Test]
    public void TemplateValidator_ValidJson_ShouldPass()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """
            {
              "fields": [
                {"key":"ly_do","label":"Lý do","type":"textarea","required":true,"maxLength":1000},
                {"key":"ma_hoc_ky","label":"Học kỳ","type":"related_entity","required":true,"relatedEntity":"hoc_ky"},
                {"key":"loai","label":"Loại","type":"select","required":true,"options":[{"value":"a","label":"A"}]}
              ]
            }
            """;

        Assert.DoesNotThrow(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_MalformedJson_ShouldReturnApiException()
    {
        var validator = new ApplicationTemplateValidator();
        var exception = Assert.Throws<ApiException>(() => validator.Validate("""{"fields":["""));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Message, Is.EqualTo("Cấu hình mẫu đơn không phải JSON hợp lệ."));
        });
    }

    [Test]
    public void TemplateValidator_EmptyFields_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_RelatedEntityWithoutRelatedEntity_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"hoc_ky","label":"Học kỳ","type":"related_entity"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_SelectWithoutOptions_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"loai","label":"Loại","type":"select"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_MultiselectWithoutOptions_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"loai","label":"Loại","type":"multiselect"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_DuplicateOptionValue_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """
            {"fields":[
              {"key":"loai","label":"Loại","type":"select","options":[
                {"value":"A","label":"A"},
                {"value":"a","label":"A duplicate"}
              ]}
            ]}
            """;

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_RequiredStringInsteadOfBoolean_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"ly_do","label":"Lý do","type":"textarea","required":"true"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_EvidenceRequiredNumberInsteadOfBoolean_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"ly_do","label":"Lý do","type":"textarea","evidenceRequired":1}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_ValidRelatedEntity_ShouldPass()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"hoc_ky","label":"Học kỳ","type":"related_entity","relatedEntity":"hoc_ky"}]}""";

        Assert.DoesNotThrow(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_ValidSelectOptions_ShouldPass()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """
            {"fields":[
              {"key":"loai","label":"Loại","type":"select","required":true,"options":[
                {"value":"a","label":"A"},
                {"value":"b","label":"B"}
              ]}
            ]}
            """;

        Assert.DoesNotThrow(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_DuplicateKey_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """
            {"fields":[
              {"key":"ly_do","label":"Lý do","type":"textarea"},
              {"key":"ly_do","label":"Lý do 2","type":"text"}
            ]}
            """;

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_InvalidFieldType_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"a","label":"A","type":"script"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_InvalidRelatedEntity_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"a","label":"A","type":"related_entity","relatedEntity":"bang_tu_do"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void TemplateValidator_ScriptHtmlSqlLikeConfig_ShouldFail()
    {
        var validator = new ApplicationTemplateValidator();
        var json = """{"fields":[{"key":"a","label":"<script>alert(1)</script>","type":"textarea"}]}""";

        Assert.Throws<ApiException>(() => validator.Validate(json));
    }

    [Test]
    public void EfModel_DonTuRowVersion_ShouldBeMapped()
    {
        using var context = CreateMetadataContext();
        var property = context.Model.FindEntityType(typeof(Backend.Models.DonTu))!
            .FindProperty(nameof(Backend.Models.DonTu.RowVersion));

        Assert.That(property, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(property!.IsConcurrencyToken, Is.True);
            Assert.That(property.ValueGenerated, Is.EqualTo(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate));
        });
    }

    [Test]
    public void EfModel_TemplateIndexes_ShouldBeMapped()
    {
        using var context = CreateMetadataContext();
        var entity = context.Model.FindEntityType(typeof(Backend.Models.MauDonTu))!;
        var indexNames = entity.GetIndexes().Select(x => x.GetDatabaseName()).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(indexNames, Does.Contain("UX_MauDonTu_loai_don_phien_ban"));
            Assert.That(indexNames, Does.Contain("UX_MauDonTu_loai_don_active"));
            Assert.That(entity.GetIndexes().Single(x => x.GetDatabaseName() == "UX_MauDonTu_loai_don_active").GetFilter(), Is.EqualTo("[dang_hoat_dong] = 1"));
        });
    }

    [Test]
    public void EfModel_ForeignKeys_ShouldUseNoAction()
    {
        using var context = CreateMetadataContext();
        var attachmentEntity = context.Model.FindEntityType(typeof(Backend.Models.TepDinhKemDonTu))!;

        Assert.That(attachmentEntity.GetForeignKeys().All(x => x.DeleteBehavior == DeleteBehavior.NoAction), Is.True);
    }

    private static ApplicationDbContext CreateMetadataContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LMS_Metadata_Only;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options);
    }
}
