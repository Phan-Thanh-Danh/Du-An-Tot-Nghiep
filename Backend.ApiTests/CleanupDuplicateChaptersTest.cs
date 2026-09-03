using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class CleanupDuplicateChaptersTest
    {
        [Test]
        public async Task MergeAndCleanupDuplicateChapters()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // 1. Move quiz and seek lock from lesson 161 to lesson 74
            var l161 = await db.BaiHocs.Include(b => b.BaiHocNoiDungs).FirstOrDefaultAsync(b => b.MaBaiHoc == 161);
            var l74 = await db.BaiHocs.Include(b => b.BaiHocNoiDungs).FirstOrDefaultAsync(b => b.MaBaiHoc == 74);

            if (l161 != null && l74 != null)
            {
                TestContext.Progress.WriteLine($"Found Lesson 161: DieuKienMoKhoa={l161.DieuKienMoKhoa}, Blocks={l161.BaiHocNoiDungs.Count}");
                l74.DieuKienMoKhoa = l161.DieuKienMoKhoa;

                var quizBlock = l161.BaiHocNoiDungs.FirstOrDefault(n => n.LoaiNoiDung == "quiz" || n.MaDeKiemTra != null);
                if (quizBlock != null)
                {
                    var existing74Quiz = l74.BaiHocNoiDungs.FirstOrDefault(n => n.MaDeKiemTra == quizBlock.MaDeKiemTra);
                    if (existing74Quiz == null)
                    {
                        quizBlock.MaBaiHoc = 74;
                        TestContext.Progress.WriteLine($"Moved Quiz Block {quizBlock.MaNoiDung} (QuizId={quizBlock.MaDeKiemTra}) to Lesson 74");
                    }
                }
                await db.SaveChangesAsync();
            }

            // 2. Find all duplicate chapters (MaChuong >= 47 and MaChuong <= 83)
            var dupChapters = await db.Chuongs
                .Include(c => c.BaiHocs)
                    .ThenInclude(b => b.BaiHocNoiDungs)
                .Where(c => c.MaChuong >= 47 && c.MaChuong <= 83)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Found {dupChapters.Count} duplicate chapters to remove.");

            foreach (var ch in dupChapters)
            {
                foreach (var b in ch.BaiHocs)
                {
                    // Remove student progress for dup lessons if any
                    var progs = await db.TienDoBaiHocs.Where(t => t.MaBaiHoc == b.MaBaiHoc).ToListAsync();
                    if (progs.Any()) db.TienDoBaiHocs.RemoveRange(progs);

                    // Remove content blocks
                    db.BaiHocNoiDungs.RemoveRange(b.BaiHocNoiDungs);
                }
                db.BaiHocs.RemoveRange(ch.BaiHocs);
            }
            db.Chuongs.RemoveRange(dupChapters);

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("Successfully merged and cleaned up duplicate chapters!");
        }
    }
}
