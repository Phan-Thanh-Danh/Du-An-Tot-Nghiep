using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class DbChapterCleanerTest
    {
        [Test]
        public async Task CleanAndStandardizeAllChapters()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // 1. Clean COM103: remove dummy chapters 8 and 9 (and their dummy lessons)
            var com103 = await db.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaCodeMonHoc == "COM103");
            if (com103 != null)
            {
                var dummyChapters = await db.Chuongs
                    .Include(c => c.BaiHocs)
                    .Where(c => c.MaMonHoc == com103.MaMonHoc && (c.MaChuong == 8 || c.MaChuong == 9))
                    .ToListAsync();

                foreach (var dc in dummyChapters)
                {
                    if (dc.BaiHocs != null && dc.BaiHocs.Count > 0)
                    {
                        var lessonIds = dc.BaiHocs.Select(b => b.MaBaiHoc).ToList();
                        var progresses = await db.TienDoBaiHocs.Where(t => lessonIds.Contains(t.MaBaiHoc)).ToListAsync();
                        db.TienDoBaiHocs.RemoveRange(progresses);
                        db.BaiHocs.RemoveRange(dc.BaiHocs);
                    }
                    db.Chuongs.Remove(dc);
                }
                await db.SaveChangesAsync();
                TestContext.Progress.WriteLine("Cleaned dummy chapters 8 and 9 in COM103.");
            }

            // 2. Clean and standardize all subjects
            var subjects = await db.DanhMucMonHocs.ToListAsync();
            foreach (var sub in subjects)
            {
                var chapters = await db.Chuongs
                    .Include(c => c.BaiHocs)
                    .Where(c => c.MaMonHoc == sub.MaMonHoc)
                    .OrderBy(c => c.ThuTu)
                    .ThenBy(c => c.MaChuong)
                    .ToListAsync();

                if (chapters.Count == 0) continue;

                // Fix WEB101 order: HTML (Phan 1) first, CSS (Phan 2) second
                if (sub.MaCodeMonHoc == "WEB101")
                {
                    var htmlChap = chapters.FirstOrDefault(c => c.TieuDe != null && c.TieuDe.Contains("HTML"));
                    var cssChap = chapters.FirstOrDefault(c => c.TieuDe != null && c.TieuDe.Contains("CSS"));
                    if (htmlChap != null && cssChap != null)
                    {
                        htmlChap.ThuTu = 1;
                        cssChap.ThuTu = 2;
                    }
                }

                int seq = 1;
                foreach (var chap in chapters.OrderBy(c => c.ThuTu).ThenBy(c => c.MaChuong))
                {
                    chap.ThuTu = seq++;

                    // Clean TieuDe: strip "Chương \d+:\s*", "Phần \d+:\s*"
                    if (!string.IsNullOrWhiteSpace(chap.TieuDe))
                    {
                        var cleaned = Regex.Replace(chap.TieuDe, @"^(Chương|Phần|Bài)\s*\d+\s*[:\-]\s*", "", RegexOptions.IgnoreCase).Trim();
                        if (!string.IsNullOrEmpty(cleaned))
                        {
                            chap.TieuDe = cleaned;
                        }
                    }
                }
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("Standardized all chapters across all subjects in Database!");
        }
    }
}
