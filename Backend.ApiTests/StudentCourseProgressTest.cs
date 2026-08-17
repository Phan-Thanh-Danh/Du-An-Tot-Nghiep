using System;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class StudentCourseProgressTest
    {
        [Test]
        public async Task TestCompleteLessonWithRawAndPrefixedId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApplicationDbContext(options);

            var student = new NguoiDung
            {
                MaNguoiDung = 1001,
                HoTen = "Nguyen Van Test",
                Email = "test_student@lms.local",
                VaiTroChinh = "hoc_sinh"
            };
            db.NguoiDungs.Add(student);

            var lesson = new BaiHoc
            {
                MaBaiHoc = 83,
                TieuDe = "Test Lesson 83",
                LoaiBaiHoc = "video",
                TrangThai = "da_xuat_ban"
            };
            db.BaiHocs.Add(lesson);
            await db.SaveChangesAsync();

            var controller = new StudentCoursesController();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["CurrentUser"] = new CurrentUserContext
            {
                UserId = student.MaNguoiDung,
                Role = "Student"
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // 1. Test with raw integer ID "83"
            var res1 = await controller.CompleteLesson("COM103", "83", db, 50);
            Assert.That(res1.Result, Is.InstanceOf<OkObjectResult>());

            var prog1 = await db.TienDoBaiHocs.FirstOrDefaultAsync(t => t.MaHocSinh == student.MaNguoiDung && t.MaBaiHoc == 83);
            Assert.That(prog1, Is.Not.Null);
            Assert.That((int)prog1.PhanTramTienDo, Is.EqualTo(50));

            // 2. Test with prefixed ID "l83"
            var res2 = await controller.CompleteLesson("COM103", "l83", db, 100);
            Assert.That(res2.Result, Is.InstanceOf<OkObjectResult>());

            var prog2 = await db.TienDoBaiHocs.FirstOrDefaultAsync(t => t.MaHocSinh == student.MaNguoiDung && t.MaBaiHoc == 83);
            Assert.That(prog2, Is.Not.Null);
            Assert.That((int)prog2.PhanTramTienDo, Is.EqualTo(100));
            Assert.That(prog2.HoanThanhLuc, Is.Not.Null);

            TestContext.Progress.WriteLine($"TestCompleteLesson PASSED: Lesson {lesson.MaBaiHoc} progress successfully saved to DB as {prog2.PhanTramTienDo}%!");
        }
    }
}
