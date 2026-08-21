using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services.Grading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TeacherLessonSeekAndQuestionBankTest
    {
        private ApplicationDbContext _db = null!;
        private TeacherClassesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            _db = new ApplicationDbContext(options);

            // Find a teacher who teaches COM102 (MaMonHoc = 3)
            var khoaHoc = _db.KhoaHocs.FirstOrDefault(k => k.MaMonHoc == 3);
            int teacherId = khoaHoc?.MaGiaoVien ?? 1;

            var mockGradeService = new Mock<IGradeAggregationService>();
            _controller = new TeacherClassesController(_db, mockGradeService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = teacherId,
                Email = "teacher@lms.local",
                Role = "Teacher"
            };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [TearDown]
        public void TearDown()
        {
            _db?.Dispose();
        }

        [Test]
        public async Task TestGetSubjectLessonsDetail_ReturnsCom102()
        {
            var mockStorage = new Mock<Backend.Services.Storage.IR2StorageService>();
            mockStorage.Setup(s => s.GetPresignedStreamUrl(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                .Returns((string k, TimeSpan? exp) => $"https://media.example.local/{k}");

            var result = await _controller.GetSubjectLessonsDetail("COM102", mockStorage.Object);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

            var okObj = result.Result as OkObjectResult;
            var json = JsonSerializer.Serialize(okObj!.Value);
            TestContext.Progress.WriteLine($"JSON: {json}");
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var data = root.TryGetProperty("Data", out var d1) ? d1 : root.GetProperty("data");

            string code = data.TryGetProperty("Code", out var c1) ? c1.GetString()! : data.GetProperty("code").GetString()!;
            string name = data.TryGetProperty("Name", out var n1) ? n1.GetString()! : data.GetProperty("name").GetString()!;

            TestContext.Progress.WriteLine($"Subject Code: {code}, Name: {name}");
            Assert.That(code, Is.EqualTo("COM102"));
        }

        [Test]
        public async Task TestToggleSubjectSeekAll_And_SingleSeekSync()
        {
            // 1. Toggle ALL lessons for COM102 to locked (allowSeek = false)
            var toggleAllRes1 = await _controller.ToggleSubjectSeekAll("COM102", new TeacherClassesController.ToggleAllSeekRequest { LockAll = true });
            var okAll1 = toggleAllRes1.Result as ObjectResult;
            Assert.That(okAll1!.StatusCode ?? 200, Is.EqualTo(200));

            // Verify all lessons in DB for COM102 have allowSeek:false
            var lessons = await _db.BaiHocs
                .Where(b => _db.Chuongs.Any(c => c.MaMonHoc == 3 && c.MaChuong == b.MaChuong))
                .AsNoTracking()
                .ToListAsync();

            Assert.That(lessons.Count, Is.GreaterThan(0));
            Assert.That(lessons.All(l => l.DieuKienMoKhoa != null && l.DieuKienMoKhoa.Contains("\"allowSeek\":false")), Is.True);
            TestContext.Progress.WriteLine($"Successfully locked seek for all {lessons.Count} lessons of COM102");

            // 2. Toggle ALL lessons for COM102 to unlocked (allowSeek = true)
            var toggleAllRes2 = await _controller.ToggleSubjectSeekAll("COM102", new TeacherClassesController.ToggleAllSeekRequest { LockAll = false });
            var okAll2 = toggleAllRes2.Result as ObjectResult;
            Assert.That(okAll2!.StatusCode ?? 200, Is.EqualTo(200));

            var lessons2 = await _db.BaiHocs
                .Where(b => _db.Chuongs.Any(c => c.MaMonHoc == 3 && c.MaChuong == b.MaChuong))
                .AsNoTracking()
                .ToListAsync();

            Assert.That(lessons2.All(l => l.DieuKienMoKhoa != null && l.DieuKienMoKhoa.Contains("\"allowSeek\":true")), Is.True);
            TestContext.Progress.WriteLine($"Successfully unlocked seek for all {lessons2.Count} lessons of COM102");
        }

        [Test]
        public async Task TestGetSubjectQuestionBank_ReturnsQuestions()
        {
            var res = await _controller.GetSubjectQuestionBank("COM102");
            Assert.That(res.Result, Is.TypeOf<OkObjectResult>());

            var okObj = res.Result as OkObjectResult;
            var json = JsonSerializer.Serialize(okObj!.Value);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var items = root.TryGetProperty("Data", out var d) ? d : root.GetProperty("data");
            int count = items.GetArrayLength();
            TestContext.Progress.WriteLine($"Total question bank for COM102: {count}");
            Assert.That(count, Is.GreaterThan(0));
        }

        [Test]
        public async Task TestAddQuizQuestionToLesson_DebugException()
        {
            var q = await _db.CauHois.FirstOrDefaultAsync(c => c.MaMonHoc == 3 && c.ConHoatDong);
            Assert.That(q, Is.Not.Null, "Question for COM102 must exist");

            var b = await _db.BaiHocs.FirstOrDefaultAsync(b => _db.Chuongs.Any(c => c.MaMonHoc == 3 && c.MaChuong == b.MaChuong));
            Assert.That(b, Is.Not.Null, "Lesson for COM102 must exist");

            TestContext.Progress.WriteLine($"Testing AddQuizQuestionToLesson with LessonId={b!.MaBaiHoc}, QuestionId={q!.MaCauHoi}");

            var res = await _controller.AddQuizQuestionToLesson(b.MaBaiHoc, new TeacherClassesController.AddQuizQuestionRequest { QuestionId = q.MaCauHoi });
            var json = JsonSerializer.Serialize(res);
            TestContext.Progress.WriteLine($"AddQuizQuestion Result: {json}");

            if (res.Result is ObjectResult obj)
            {
                var valJson = JsonSerializer.Serialize(obj.Value);
                TestContext.Progress.WriteLine($"Status: {obj.StatusCode}, Value: {valJson}");
            }
        }
    }
}
