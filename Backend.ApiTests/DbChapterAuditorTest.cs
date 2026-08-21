using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class DbChapterAuditorTest
    {
        [Test]
        public async Task AuditAndCleanAllChapters()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var subjects = await db.DanhMucMonHocs
                .OrderBy(m => m.MaCodeMonHoc)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Auditing {subjects.Count} subjects...\n");

            foreach (var sub in subjects)
            {
                var chapters = await db.Chuongs
                    .Include(c => c.BaiHocs)
                    .Where(c => c.MaMonHoc == sub.MaMonHoc)
                    .OrderBy(c => c.ThuTu)
                    .ToListAsync();

                if (chapters.Count == 0) continue;

                TestContext.Progress.WriteLine($"=== Subject: {sub.MaCodeMonHoc} - {sub.TenMonHoc} ({chapters.Count} chapters) ===");

                foreach (var c in chapters)
                {
                    TestContext.Progress.WriteLine($"   Chapter ID: {c.MaChuong}, ThuTu: {c.ThuTu}, Title: '{c.TieuDe}', Lessons: {c.BaiHocs.Count}");
                }
            }
        }
    }
}
