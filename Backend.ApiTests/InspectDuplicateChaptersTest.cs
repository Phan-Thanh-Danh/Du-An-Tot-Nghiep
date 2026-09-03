using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectDuplicateChaptersTest
    {
        [Test]
        public async Task CheckAllSubjectsChapters()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var chapters = await db.Chuongs
                .Include(c => c.MonHoc)
                .Include(c => c.BaiHocs)
                .OrderBy(c => c.MaMonHoc)
                .ThenBy(c => c.ThuTu)
                .ToListAsync();

            var groups = chapters.GroupBy(c => c.MaMonHoc);

            foreach (var g in groups)
            {
                var first = g.First();
                TestContext.Progress.WriteLine($"\n=== Subject ID={g.Key}, Code={first.MonHoc?.MaCodeMonHoc}, Name=\"{first.MonHoc?.TenMonHoc}\", Total Chapters={g.Count()} ===");
                foreach (var c in g)
                {
                    TestContext.Progress.WriteLine($"   Chapter ID={c.MaChuong}, ThuTu={c.ThuTu}, Title=\"{c.TieuDe}\", Lessons={c.BaiHocs.Count}");
                }
            }
        }
    }
}
