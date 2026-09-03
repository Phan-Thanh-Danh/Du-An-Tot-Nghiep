using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class DbCurriculumInspector
    {
        [Test]
        public async Task InspectSubjectsAndLessons()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var chapters = await db.Chuongs
                .Include(c => c.MonHoc)
                .Include(c => c.BaiHocs)
                    .ThenInclude(b => b.BaiHocNoiDungs)
                .OrderBy(c => c.MaMonHoc)
                .ThenBy(c => c.ThuTu)
                .ToListAsync();

            TestContext.Progress.WriteLine($"=== TOTAL CHAPTERS IN DB: {chapters.Count} ===");
            foreach (var ch in chapters)
            {
                TestContext.Progress.WriteLine($"\n[MON_HOC] {ch.MonHoc?.MaCodeMonHoc} - {ch.MonHoc?.TenMonHoc} | [CHUONG] {ch.TieuDe} (Lessons: {ch.BaiHocs.Count})");
                foreach (var bh in ch.BaiHocs.OrderBy(b => b.ThuTu))
                {
                    TestContext.Progress.WriteLine($"   -> [BAI_HOC] ID: {bh.MaBaiHoc} | Title: {bh.TieuDe} | Url: {bh.UrlTapTin} | Contents: {bh.BaiHocNoiDungs.Count}");
                    foreach (var nd in bh.BaiHocNoiDungs)
                    {
                        TestContext.Progress.WriteLine($"      -> [NOI_DUNG] ID: {nd.MaNoiDung} | Type: {nd.LoaiNoiDung} | Url: {nd.UrlTapTin} | Key: {nd.StorageKey}");
                    }
                }
            }
        }
    }
}
