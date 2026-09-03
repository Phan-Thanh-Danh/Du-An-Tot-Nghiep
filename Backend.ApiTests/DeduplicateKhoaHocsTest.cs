using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class DeduplicateKhoaHocsTest
    {
        [Test]
        public async Task DeduplicateAllKhoaHocs()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var allKhoaHocs = await db.KhoaHocs
                .Include(k => k.Lop)
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .ToListAsync();

            var duplicates = allKhoaHocs
                .GroupBy(k => new { k.MaLop, k.MaMonHoc })
                .Where(g => g.Count() > 1)
                .ToList();

            TestContext.Progress.WriteLine($"Found {duplicates.Count} (Class, Subject) pairs with duplicate KhoaHocs.");

            foreach (var group in duplicates)
            {
                TestContext.Progress.WriteLine($"\nPair: Class ID={group.Key.MaLop} (\"{group.First().Lop?.TenLop}\"), Subject ID={group.Key.MaMonHoc} (\"{group.First().MonHoc?.TenMonHoc}\")");
                // The canonical one is the latest one
                var canonical = group.OrderByDescending(k => k.MaKhoaHoc).First();
                TestContext.Progress.WriteLine($"  -> Keeping canonical KhoaHoc ID={canonical.MaKhoaHoc}, Teacher=\"{canonical.GiaoVien?.HoTen}\" (TeacherId={canonical.MaGiaoVien})");

                var toRemove = group.Where(k => k.MaKhoaHoc != canonical.MaKhoaHoc).ToList();
                foreach (var old in toRemove)
                {
                    TestContext.Progress.WriteLine($"  -> Removing duplicate KhoaHoc ID={old.MaKhoaHoc}, Teacher=\"{old.GiaoVien?.HoTen}\" (TeacherId={old.MaGiaoVien})");
                    db.KhoaHocs.Remove(old);
                }
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("\nAll duplicate KhoaHocs successfully cleaned up!");
        }
    }
}
