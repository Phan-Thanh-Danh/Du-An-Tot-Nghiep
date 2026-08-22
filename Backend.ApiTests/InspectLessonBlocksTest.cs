using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectLessonBlocksTest
    {
        [Test]
        public async Task InspectBlocks()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lessonIds = new[] { 66, 68, 69, 70, 71, 153, 155, 156, 157, 158 };
            var lessons = await db.BaiHocs
                .Include(b => b.BaiHocNoiDungs)
                .Where(b => lessonIds.Contains(b.MaBaiHoc))
                .ToListAsync();

            foreach (var l in lessons)
            {
                TestContext.Progress.WriteLine($"\n=== Lesson {l.MaBaiHoc}: \"{l.TieuDe}\" ===");
                TestContext.Progress.WriteLine($"  UrlTapTin: {l.UrlTapTin}");
                TestContext.Progress.WriteLine($"  LoaiBaiHoc: {l.LoaiBaiHoc}");
                TestContext.Progress.WriteLine($"  TrangThai: {l.TrangThai}");
                TestContext.Progress.WriteLine($"  Content Blocks Count: {l.BaiHocNoiDungs.Count}");

                foreach (var c in l.BaiHocNoiDungs)
                {
                    TestContext.Progress.WriteLine($"    Block {c.MaNoiDung}: Type={c.LoaiNoiDung}, TrangThai={c.TrangThai}, FileUrl={c.UrlTapTin}, QuizId={c.MaDeKiemTra}, HtmlLen={c.NoiDungHtml?.Length ?? 0}, JsonLen={c.NoiDungJson?.Length ?? 0}");
                }
            }
        }
    }
}
