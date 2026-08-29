using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectCom102LessonsTest
    {
        [Test]
        public async Task InspectAllLessons()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var subject = await db.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaCodeMonHoc == "COM102" || m.TenMonHoc.Contains("Cơ sở dữ liệu"));
            if (subject == null)
            {
                TestContext.Progress.WriteLine("COM102 Subject NOT FOUND");
                return;
            }

            TestContext.Progress.WriteLine($"Subject: {subject.MaCodeMonHoc} - {subject.TenMonHoc} (ID: {subject.MaMonHoc})");

            var chapters = await db.Chuongs
                .Where(c => c.MaMonHoc == subject.MaMonHoc)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            foreach (var ch in chapters)
            {
                TestContext.Progress.WriteLine($"\n--- Chapter {ch.ThuTu}: {ch.TieuDe} (ID: {ch.MaChuong}) ---");
                var lessons = await db.BaiHocs
                    .Where(b => b.MaChuong == ch.MaChuong)
                    .OrderBy(b => b.ThuTu)
                    .ToListAsync();

                foreach (var l in lessons)
                {
                    TestContext.Progress.WriteLine($"Lesson: ID={l.MaBaiHoc}, Order={l.ThuTu}, Type={l.LoaiBaiHoc}, Duration={l.ThoiLuongGiay}s, HasFile={!string.IsNullOrEmpty(l.UrlTapTin)}, FileUrl={l.UrlTapTin}, Title=\"{l.TieuDe}\"");
                }
            }
        }
    }
}
