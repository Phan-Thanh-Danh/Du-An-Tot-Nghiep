using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class CheckLessonUrlsTest
    {
        [Test]
        public async Task CheckAllLessonUrls()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var sampleLessons = await db.BaiHocs
                .Where(b => b.MaBaiHoc >= 18)
                .Take(10)
                .ToListAsync();

            foreach (var l in sampleLessons)
            {
                TestContext.Progress.WriteLine($"ID: {l.MaBaiHoc} | Title: {l.TieuDe} | UrlTapTin: {l.UrlTapTin}");
            }
        }
    }
}
