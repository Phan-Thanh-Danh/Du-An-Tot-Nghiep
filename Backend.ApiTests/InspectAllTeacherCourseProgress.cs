using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectAllTeacherCourseProgress
    {
        [Test]
        public async Task TestAllTeacherCourseProgress()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var teacher = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(teacher, Is.Not.Null);

            var controller = new TeacherClassesController(db, null!);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = teacher.MaNguoiDung,
                Email = teacher.Email,
                Role = "Teacher"
            };

            var courses = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Where(k => k.MaGiaoVien == teacher.MaNguoiDung)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Teacher {teacher.HoTen} (ID={teacher.MaNguoiDung}) has {courses.Count} courses:");

            foreach (var c in courses)
            {
                var progressRes = await controller.GetCourseProgress(c.MaKhoaHoc);
                if (progressRes.Result is OkObjectResult ok && ok.Value != null)
                {
                    dynamic apiRes = ok.Value;
                    dynamic data = apiRes.Data;
                    TestContext.Progress.WriteLine($" Course ID={c.MaKhoaHoc}, Title={c.TieuDe}, OverallProgress={data.overallProgress}%, ActiveStudents={data.activeStudents}");
                    int countExcellent = 0, countGood = 0, countWarning = 0, countDanger = 0;
                    foreach (var s in data.students)
                    {
                        if (s.status == "excellent") countExcellent++;
                        else if (s.status == "good") countGood++;
                        else if (s.status == "warning") countWarning++;
                        else if (s.status == "danger") countDanger++;
                    }
                    TestContext.Progress.WriteLine($"   Status breakdown: Excellent={countExcellent}, Good={countGood}, Warning={countWarning}, Danger={countDanger}");
                }
            }
        }
    }
}
