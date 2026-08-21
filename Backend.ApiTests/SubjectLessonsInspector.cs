using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class SubjectLessonsInspector
    {
        [Test]
        public async Task InspectSubjectLessons()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var teacher = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(teacher, Is.Not.Null);

            // Teacher courses
            var teacherCourses = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Include(k => k.HocKy)
                .Where(k => k.MaGiaoVien == teacher.MaNguoiDung)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Teacher {teacher.HoTen} teaches {teacherCourses.Count} courses:");

            var groupedSubjects = teacherCourses.GroupBy(k => k.MaMonHoc).ToList();
            foreach (var g in groupedSubjects)
            {
                var monHoc = await db.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaMonHoc == g.Key);
                var chapters = await db.Chuongs
                    .Include(c => c.BaiHocs)
                    .Where(c => c.MaMonHoc == g.Key)
                    .OrderBy(c => c.ThuTu)
                    .ToListAsync();

                var totalLessons = chapters.SelectMany(c => c.BaiHocs).Count();
                var classes = g.Select(k => k.Lop?.TenLop).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                TestContext.Progress.WriteLine($"Subject ID={g.Key}, Code={monHoc?.MaCodeMonHoc}, Name={monHoc?.TenMonHoc}, Classes=[{string.Join(", ", classes)}], Chapters={chapters.Count}, Lessons={totalLessons}");
                foreach (var ch in chapters)
                {
                    TestContext.Progress.WriteLine($"   Chapter ID={ch.MaChuong}, Title={ch.TieuDe}, Lessons={ch.BaiHocs.Count}");
                    foreach (var bh in ch.BaiHocs)
                    {
                        TestContext.Progress.WriteLine($"     Lesson ID={bh.MaBaiHoc}, Title={bh.TieuDe}, Type={bh.LoaiBaiHoc}, DurationSec={bh.ThoiLuongGiay}, FileUrl={bh.UrlTapTin}, HasContent={!string.IsNullOrEmpty(bh.NoiDungVanBan)}");
                    }
                }
            }

            // Check if there are assignments or exercises
            var monHocIds = groupedSubjects.Select(g => g.Key).ToList();
            var assignments = await db.BaiTaps
                .Where(b => monHocIds.Contains(b.MaMonHoc))
                .ToListAsync();
            TestContext.Progress.WriteLine($"Teacher subjects have {assignments.Count} assignments in DB.");
            foreach (var bt in assignments.Take(5))
            {
                TestContext.Progress.WriteLine($"   Assignment ID={bt.MaBaiTap}, Title={bt.TieuDe}, MonHocID={bt.MaMonHoc}");
            }
        }
    }
}
