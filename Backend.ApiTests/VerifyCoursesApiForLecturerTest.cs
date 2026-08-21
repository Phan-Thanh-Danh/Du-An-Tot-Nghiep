using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Courses;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.Audit;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class VerifyCoursesApiForLecturerTest
    {
        [Test]
        public async Task VerifyGetCoursesForLecturer()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(lecturer, Is.Not.Null);

            var httpContext = new DefaultHttpContext();
            var currentUser = new CurrentUserContext
            {
                UserId = lecturer.MaNguoiDung,
                Email = lecturer.Email,
                Role = "Teacher",
                CampusId = lecturer.MaDonVi
            };
            httpContext.Items["CurrentUser"] = currentUser;

            var httpContextAccessor = new Moq.Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var auditLogService = Moq.Mock.Of<IAuditLogService>();
            var eligibilityService = Moq.Mock.Of<ICourseTeacherEligibilityService>();
            var schedContextService = Moq.Mock.Of<IAcademicSchedulingContextService>();

            var courseService = new CourseService(db, httpContextAccessor.Object, auditLogService, eligibilityService, schedContextService);
            var controller = new CoursesController(courseService, Moq.Mock.Of<ICourseAssignmentSuggestionService>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var queryParams = new KhoaHocQueryParameters
            {
                PageIndex = 1,
                PageSize = 50
            };

            var actionResult = await controller.GetCourses(queryParams, default);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiRes = okResult.Value as ApiResponseDto<PagedResultDto<KhoaHocDto>>;
            Assert.That(apiRes, Is.Not.Null);
            Assert.That(apiRes.Data, Is.Not.Null);

            TestContext.Progress.WriteLine($"Total courses returned for lecturer {lecturer.Email}: {apiRes.Data.Items.Count}");
            foreach (var c in apiRes.Data.Items)
            {
                TestContext.Progress.WriteLine($" - Course ID={c.MaKhoaHoc}, Class='{c.TenLop}', Subject='{c.TenMonHoc}', Title='{c.TieuDe}', SiSo={c.SiSo}, Term='{c.TenHocKy}'");
            }

            // Verify COM102 exists
            var com102Course = apiRes.Data.Items.FirstOrDefault(c => c.TenMonHoc.Contains("Cơ sở dữ liệu") || c.TieuDe.Contains("Cơ sở dữ liệu"));
            Assert.That(com102Course, Is.Not.Null, "COM102 course should be in the list for lecturer01!");
            Assert.That(com102Course.SiSo, Is.GreaterThan(0), "SiSo should be > 0 (30 students for SD1902)!");
        }
    }
}
