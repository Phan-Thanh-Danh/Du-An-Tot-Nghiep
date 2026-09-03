using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class CheckAllCoursesTienDo
    {
        [Test]
        public async Task TestCheckAllCoursesTienDo()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var courses = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Total courses in DB: {courses.Count}");

            foreach (var c in courses.Take(10))
            {
                var lessonIds = await db.BaiHocs
                    .Where(b => b.Chuong != null && b.Chuong.MaMonHoc == c.MaMonHoc)
                    .Select(b => b.MaBaiHoc)
                    .ToListAsync();

                var students = await db.NguoiDungs
                    .Where(n => n.MaLop == c.MaLop && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                    .ToListAsync();

                var tienDos = await db.TienDoBaiHocs
                    .Where(t => lessonIds.Contains(t.MaBaiHoc))
                    .ToListAsync();

                TestContext.Progress.WriteLine($"Course ID={c.MaKhoaHoc} ({c.TieuDe}): {students.Count} students, {lessonIds.Count} lessons, {tienDos.Count} tienDo records");

                if (students.Count > 0 && lessonIds.Count > 0)
                {
                    var progressList = students.Select(st =>
                    {
                        var completed = tienDos.Count(t => t.MaHocSinh == st.MaNguoiDung && t.HoanThanhLuc != null);
                        return (int)Math.Round((decimal)completed / lessonIds.Count * 100);
                    }).ToList();

                    TestContext.Progress.WriteLine($"   Distinct progresses: [{string.Join(", ", progressList.Distinct())}]");
                }
            }
        }
    }
}
