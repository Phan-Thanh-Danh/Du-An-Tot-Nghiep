using System.Net;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AI;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests;

[TestFixture]
public class AI_PromptTests
{
    internal const string Html = "<div class=\"certificate\"><div class=\"frame\"><h1>GIẤY KHEN</h1><h2>{{hoTen}}</h2><p>{{mssv}} {{tenHocKy}} {{danhHieu}} {{ngayCap}}</p></div></div>";
    internal const string Css = ".certificate { width: 900px; height: 600px; background: white; padding: 20px; } .frame { height: 90%; background: white; border: 4px solid red; text-align: center; }";
    internal static CurrentUserContext Staff => new() { UserId = 1, Role = AuthRoles.AcademicStaff, CampusId = 14 };
    internal static CurrentUserContext Principal => new() { UserId = 2, Role = AuthRoles.Principal, CampusId = 14 };
    internal static ApplicationDbContext Database() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static AiCertificateTemplateEditRequest Certificate(string instruction = "Nền vàng viền đen") => new()
        { Instruction = instruction, CurrentHtml = Html, CurrentCss = Css };

    [Test]
    public async Task Certificate_PreservesHtmlAndAppendsActualModelCss()
    {
        var ai = MockAi("""{"html":"","css":".certificate,.frame {background:#ffff00;} .frame {border:4px solid #000000;}","explanation":"Nền vàng, viền đen","changes":["Đổi nền và viền"]}""");
        var result = await CertificatePromptEditor.EditAsync(ai.Object, Certificate(), Principal, default);
        Assert.That(result.UpdatedHtml, Is.EqualTo(Html));
        Assert.That(result.UpdatedCss, Does.StartWith(Css).And.Contain("#facc15").And.Contain("#000000"));
        ai.Verify(x => x.ChatAsync(It.IsAny<AiChatRequest>(), It.IsAny<CurrentUserContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Certificate_InstructionOverridesExplicitColorAndBorderStyle()
    {
        var ai = MockAi("""{"html":"","css":".certificate { background: gold; } .frame { border-color: black; }","explanation":"Đã đổi nền và viền","changes":["Đổi nền và viền"]}""");
        var first = await CertificatePromptEditor.EditAsync(ai.Object, Certificate(), Principal, default);
        Assert.That(first.UpdatedCss, Does.Contain("#facc15").And.Contain("#000000"));

        var follow = Certificate("Giữ nền vàng, chỉ đổi viền sang màu xanh lá và nét đứt.");
        follow.CurrentHtml = first.UpdatedHtml;
        follow.CurrentCss = first.UpdatedCss;
        var second = await CertificatePromptEditor.EditAsync(ai.Object, follow, Principal, default);
        Assert.That(second.UpdatedCss, Does.Contain("#16a34a").And.Contain("border-style: dashed"));
    }

    [TestCase("not json")]
    [TestCase("""{"html":"<p>{{hoTen}}</p>","css":"","explanation":"done","changes":[]}""")]
    [TestCase("""{"html":"","css":"body{background:url(https://example.org/x)}","explanation":"done","changes":[]}""")]
    [TestCase("""{"html":"","css":"","explanation":"Cần mô tả rõ màu","changes":[]}""")]
    public void Certificate_InvalidOutputNeverReturnsPreset(string answer)
    {
        Assert.ThrowsAsync<ApiException>(() => CertificatePromptEditor.EditAsync(MockAi(answer).Object, Certificate("thiết kế đẹp hơn"), Principal, default));
    }

    [Test]
    public void Certificate_OfflineDoesNotFabricateDesign()
    {
        var ai = new Mock<IOllamaService>();
        ai.Setup(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException(503, "offline"));
        Assert.ThrowsAsync<ApiException>(() => CertificatePromptEditor.EditAsync(ai.Object, Certificate(), Principal, default));
    }

    [Test]
    public async Task Scheduling_QuestionUsesScopedRealFactsAndCannotGenerate()
    {
        using var db = Database();
        await SeedSchedule(db);
        var ai = new Mock<IOllamaService>(MockBehavior.Strict);
        var result = await Scheduling(db, ai.Object).InterpretIntentAsync(new() { Message = "lịch có ca tối nào không", SemesterId = 15 }, Staff);
        Assert.That(result.Summary, Does.Contain("Có 1 ca học mỗi tuần"));
        Assert.That(result.Summary, Does.Not.Contain("Khác cơ sở"));
        Assert.That(result.RequiresConfirmation, Is.False);
        Assert.That(result.CanPrepareSchedule, Is.False);
        Assert.That(result.RequestedPreferences, Is.Empty);
        Assert.That(await db.ScheduleGenerationJobs.CountAsync(), Is.Zero);
        ai.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Scheduling_FollowupProposesEveningExclusionWithoutCallingModel()
    {
        using var db = Database(); await SeedSchedule(db);
        var ai = new Mock<IOllamaService>(MockBehavior.Strict);
        var result = await Scheduling(db, ai.Object).InterpretIntentAsync(new()
        {
            Message = "vậy bỏ ca tối đi", SemesterId = 15,
            History = new() { new() { Role = "assistant", Content = "Có 1 ca tối mỗi tuần." } }
        }, Staff);
        Assert.That(result.ExcludeEvening, Is.True);
        Assert.That(result.RequiresConfirmation, Is.True);
        Assert.That(await db.ScheduleGenerationJobs.CountAsync(), Is.Zero);
        ai.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Scheduling_EveningQuestionUsesDeterministicAnswerBeforeModel()
    {
        using var db = Database(); await SeedSchedule(db);
        var ai = new Mock<IOllamaService>(MockBehavior.Strict);
        var result = await Scheduling(db, ai.Object).InterpretIntentAsync(new() { Message = "có ca tối không", SemesterId = 15 }, Staff);
        Assert.That(result.Intent, Is.EqualTo("query_schedule"));
        Assert.That(result.Summary, Does.Contain("Có 1 ca học"));
        Assert.That(result.CanPrepareSchedule, Is.False);
        ai.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Scheduling_CrossCampusAndWrongTermDeniedBeforeModel()
    {
        using var db = Database(); await SeedSchedule(db);
        var ai = new Mock<IOllamaService>(MockBehavior.Strict);
        Assert.ThrowsAsync<ApiException>(() => Scheduling(db, ai.Object).InterpretIntentAsync(new() { Message = "xem lịch", CampusId = 99, SemesterId = 15 }, Staff));
        Assert.ThrowsAsync<ApiException>(() => Scheduling(db, ai.Object).InterpretIntentAsync(new() { Message = "xem lịch", SemesterId = 99 }, Staff));
        ai.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Scheduling_UnsupportedPreferenceBlocksGeneration()
    {
        using var db = Database(); await SeedSchedule(db);
        var ai = MockAi("""{"intent":"prepare_schedule","summary":"Chưa hỗ trợ đổi giáo viên qua prompt","excludeEvening":false,"requestedPreferences":[],"unsupportedPreferences":["Đổi giáo viên"]}""");
        var result = await Scheduling(db, ai.Object).InterpretIntentAsync(new() { Message = "đổi giáo viên rồi xếp lại", SemesterId = 15 }, Staff);
        Assert.That(result.CanPrepareSchedule, Is.False);
    }

    [Test]
    public async Task GeneralChat_QuestionDoesNotTriggerQuizAndIncludesHistory()
    {
        using var db = Database(); using var cache = new MemoryCache(new MemoryCacheOptions());
        using var handler = new CaptureHandler("Nên phân hóa câu hỏi theo mục tiêu học tập.");
        using var http = new HttpClient(handler);
        var service = Ollama(db, cache, http);
        var result = await service.ChatAsync(new()
        {
            Message = "Hãy gợi ý cách xây dựng ngân hàng câu hỏi trắc nghiệm phân hóa tốt",
            History = new() { new() { Role = "user", Content = "Tôi đang dạy C#" }, new() { Role = "assistant", Content = "Bạn muốn ôn tập phần nào?" } }
        }, Principal);
        Assert.That(result.Action, Is.Null);
        using var payload = JsonDocument.Parse(handler.Payload!);
        Assert.That(payload.RootElement.GetProperty("messages").GetArrayLength(), Is.EqualTo(4));
        Assert.That(handler.Payload, Does.Not.Contain("0363").And.Not.Contain("9.93"));
        Assert.That(await db.DeKiemTras.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task StructuredCompletion_UsesSchemaAndActualFastModel()
    {
        using var db = Database(); using var cache = new MemoryCache(new MemoryCacheOptions());
        using var handler = new CaptureHandler("{\"ok\":true}"); using var http = new HttpClient(handler);
        await Ollama(db, cache, http).CompleteAsync("system", "user", new { type = "object" });
        using var payload = JsonDocument.Parse(handler.Payload!);
        Assert.That(payload.RootElement.GetProperty("format").GetProperty("type").GetString(), Is.EqualTo("object"));
        Assert.That(payload.RootElement.GetProperty("model").GetString(), Is.EqualTo("qwen2.5:3b"));
    }

    internal static OllamaService Ollama(ApplicationDbContext db, IMemoryCache cache, HttpClient http)
    {
        var options = Options.Create(new OllamaOptions());
        var resolver = new Mock<IAiAcademicQueryResolver>().Object;
        return new(http, options, new AiRequestGate(options, NullLogger<AiRequestGate>.Instance), db, NullLogger<OllamaService>.Instance, cache, resolver);
    }
    internal static Mock<IOllamaService> MockAi(string answer)
    {
        var ai = new Mock<IOllamaService>();
        ai.Setup(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(answer);
        return ai;
    }
    internal static string ScheduleAnswer(string intent, string summary, bool exclude = false) => JsonSerializer.Serialize(new
        { intent, summary, excludeEvening = exclude, requestedPreferences = Array.Empty<string>(), unsupportedPreferences = Array.Empty<string>() });
    internal static SchedulingAiService Scheduling(ApplicationDbContext db, IOllamaService ai)
    {
        var context = new Mock<IAcademicSchedulingContextService>();
        context.Setup(x => x.GetContextAsync(14, It.IsAny<CancellationToken>())).ReturnsAsync(new AcademicSchedulingContextDto
        { CanPrepareSchedule = true, SchedulableTerm = new() { MaHocKy = 15, TenHocKy = "Kỳ kiểm thử" } });
        return new(ai, context.Object, Mock.Of<ISmartTimetableService>(), db, NullLogger<SchedulingAiService>.Instance);
    }
    internal static async Task SeedSchedule(ApplicationDbContext db)
    {
        db.HocKys.Add(new() { MaHocKy = 15, MaDonVi = 14, TenHocKy = "Kỳ kiểm thử" });
        db.KhoaHocs.Add(new() { MaKhoaHoc = 1, MaDonVi = 14, MaHocKy = 15, TieuDe = "C#", TrangThai = "nhap" });
        db.KhoaHocs.Add(new() { MaKhoaHoc = 2, MaDonVi = 99, MaHocKy = 15, TieuDe = "Khác cơ sở", TrangThai = "nhap" });
        db.CaHocs.AddRange(new Backend.Models.CaHoc { MaCaHoc = 1, TenCa = "Ca sáng", Buoi = "Sáng", GioBatDau = new(7, 30), GioKetThuc = new(9, 30), ConHoatDong = true },
            new Backend.Models.CaHoc { MaCaHoc = 2, TenCa = "Ca tối", Buoi = "Tối", GioBatDau = new(18, 0), GioKetThuc = new(20, 0), ConHoatDong = true });
        db.ThoiKhoaBieus.AddRange(new ThoiKhoaBieu { MaKhoaHoc = 1, MaCaHoc = 1, ThuTrongTuan = 2, TrangThai = "da_xuat_ban" },
            new ThoiKhoaBieu { MaKhoaHoc = 1, MaCaHoc = 2, ThuTrongTuan = 3, TrangThai = "da_xuat_ban" },
            new ThoiKhoaBieu { MaKhoaHoc = 2, MaCaHoc = 2, ThuTrongTuan = 4, TrangThai = "da_xuat_ban" });
        await db.SaveChangesAsync();
    }
    private sealed class CaptureHandler(string answer) : HttpMessageHandler
    {
        public string? Payload { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Payload = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { message = new { content = answer }, done = true, done_reason = "stop" }), Encoding.UTF8, "application/json") };
        }
    }
}

[TestFixture, Explicit("Requires a running local Ollama with qwen2.5:3b; never writes the application database.")]
public class AI_LivePromptTests
{
    [Test]
    public async Task Real3B_CertificateColorsFollowupAndSchedulingQuestions()
    {
        using var db = AI_PromptTests.Database(); using var cache = new MemoryCache(new MemoryCacheOptions()); using var http = new HttpClient();
        var ai = AI_PromptTests.Ollama(db, cache, http);
        var design = await CertificatePromptEditor.EditAsync(ai, AI_PromptTests.Certificate(), AI_PromptTests.Principal, default);
        TestContext.Progress.WriteLine("DESIGN: " + JsonSerializer.Serialize(design));
        Assert.That(design.UpdatedHtml, Is.EqualTo(AI_PromptTests.Html));
        Assert.That(design.UpdatedCss, Is.Not.EqualTo(AI_PromptTests.Css));
        Assert.That(design.UpdatedCss[AI_PromptTests.Css.Length..].ToLowerInvariant(), Does.Match(@"background[^;}]*(:|\s)(\s*(yellow|gold|#ff0\b|#ffff00\b|#facc15\b))"));
        Assert.That(design.UpdatedCss[AI_PromptTests.Css.Length..].ToLowerInvariant(), Does.Match(@"border[^;}]*\b(black|#000000|#000)\b"));
        var follow = AI_PromptTests.Certificate("Giữ nguyên nền vàng, chỉ đổi viền sang màu xanh lá và nét đứt.");
        follow.CurrentHtml = design.UpdatedHtml; follow.CurrentCss = design.UpdatedCss;
        var second = await CertificatePromptEditor.EditAsync(ai, follow, AI_PromptTests.Principal, default);
        TestContext.Progress.WriteLine("FOLLOWUP: " + JsonSerializer.Serialize(second));
        Assert.That(second.UpdatedCss.ToLowerInvariant(), Does.Contain("#16a34a").And.Contain("border-style: dashed"));
        await AI_PromptTests.SeedSchedule(db);
        var service = AI_PromptTests.Scheduling(db, ai);
        var question = await service.InterpretIntentAsync(new() { Message = "lịch có ca tối nào không", SemesterId = 15 }, AI_PromptTests.Staff);
        TestContext.Progress.WriteLine("QUESTION: " + question.Summary);
        Assert.That(question.Intent, Is.EqualTo("query_schedule")); Assert.That(question.CanPrepareSchedule, Is.False);
        Assert.That(question.Summary, Does.Contain("1"));
        var edit = await service.InterpretIntentAsync(new() { Message = "vậy bỏ ca tối đi", SemesterId = 15,
            History = new() { new() { Role = "user", Content = "lịch có ca tối nào không" }, new() { Role = "assistant", Content = question.Summary } } }, AI_PromptTests.Staff);
        TestContext.Progress.WriteLine("EDIT: " + JsonSerializer.Serialize(edit));
        Assert.That(edit.Intent, Is.EqualTo("prepare_schedule")); Assert.That(edit.ExcludeEvening, Is.True);
        var artifact = Environment.GetEnvironmentVariable("AI_PROMPT_ARTIFACT_DIR");
        if (!string.IsNullOrWhiteSpace(artifact))
        {
            Directory.CreateDirectory(artifact);
            await File.WriteAllTextAsync(Path.Combine(artifact, "certificate-yellow-black.html"), $"<html><head><meta charset='utf-8'><style>{design.UpdatedCss}</style></head><body>{design.UpdatedHtml}</body></html>");
            await File.WriteAllTextAsync(Path.Combine(artifact, "certificate-followup.html"), $"<html><head><meta charset='utf-8'><style>{second.UpdatedCss}</style></head><body>{second.UpdatedHtml}</body></html>");
        }
    }
}
