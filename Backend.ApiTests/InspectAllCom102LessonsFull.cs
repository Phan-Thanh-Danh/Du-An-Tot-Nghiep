using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class InspectAllCom102LessonsFull
    {
        [Test]
        public async Task InspectAll20Lessons()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var chapters = await db.Chuongs
                .Include(c => c.BaiHocs)
                    .ThenInclude(b => b.BaiHocNoiDungs)
                .Where(c => c.MaMonHoc == 3)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();

            foreach (var ch in chapters)
            {
                TestContext.Progress.WriteLine($"\n==========================================");
                TestContext.Progress.WriteLine($"Chapter ID={ch.MaChuong}, ThuTu={ch.ThuTu}, Title=\"{ch.TieuDe}\"");
                TestContext.Progress.WriteLine($"==========================================");

                foreach (var b in ch.BaiHocs.OrderBy(x => x.ThuTu))
                {
                    TestContext.Progress.WriteLine($"  Lesson ID={b.MaBaiHoc}, Order={b.ThuTu}, Title=\"{b.TieuDe}\"");
                    TestContext.Progress.WriteLine($"    Type={b.LoaiBaiHoc}, Duration={b.ThoiLuongGiay}s, DieuKienMoKhoa=\"{b.DieuKienMoKhoa}\"");
                    TestContext.Progress.WriteLine($"    UrlTapTin=\"{b.UrlTapTin}\"");
                    TestContext.Progress.WriteLine($"    ContentBlocks Count={b.BaiHocNoiDungs.Count}");
                    foreach (var n in b.BaiHocNoiDungs)
                    {
                        TestContext.Progress.WriteLine($"      Block ID={n.MaNoiDung}, Type={n.LoaiNoiDung}, FileUrl=\"{n.UrlTapTin}\", QuizId={n.MaDeKiemTra}");
                    }
                }
            }
        }
    }
}
