using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class FixSd1902OfficialLecturerTest
    {
        [Test]
        public async Task SetOfficialLecturerForSd1902()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var tranThiGV = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "teacher.cntt@lms.local" || u.Email == "p12test_teacher01@lms.local");
            Assert.That(tranThiGV, Is.Not.Null);
            TestContext.Progress.WriteLine($"Official Lecturer: ID={tranThiGV.MaNguoiDung}, Name=\"{tranThiGV.HoTen}\", Email=\"{tranThiGV.Email}\"");

            // Check KhoaHoc 26 and 3184
            var kh26 = await db.KhoaHocs.Include(k => k.GiaoVien).FirstOrDefaultAsync(k => k.MaKhoaHoc == 26);
            var kh3184 = await db.KhoaHocs.Include(k => k.GiaoVien).FirstOrDefaultAsync(k => k.MaKhoaHoc == 3184);

            if (kh26 != null)
            {
                TestContext.Progress.WriteLine($"Updating KhoaHoc 26 (current teacher: {kh26.GiaoVien?.HoTen}) -> {tranThiGV.HoTen}");
                kh26.MaGiaoVien = tranThiGV.MaNguoiDung;
            }

            if (kh3184 != null)
            {
                // Check if kh3184 is referenced by ThoiKhoaBieu
                var hasTkB = await db.ThoiKhoaBieus.AnyAsync(t => t.MaKhoaHoc == 3184);
                if (!hasTkB)
                {
                    TestContext.Progress.WriteLine("Removing orphan duplicate KhoaHoc 3184");
                    db.KhoaHocs.Remove(kh3184);
                }
                else
                {
                    kh3184.MaGiaoVien = tranThiGV.MaNguoiDung;
                }
            }

            // Also check all other KhoaHocs of SD1902 for IT subjects: COM101, COM102, COM103, GEN101, GEN102, CTDL101
            var sd1902KhoaHocs = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .Where(k => k.MaLop == 2)
                .ToListAsync();

            TestContext.Progress.WriteLine($"\n--- SD1902 Course Assignments ---");
            foreach (var k in sd1902KhoaHocs)
            {
                TestContext.Progress.WriteLine($"  KhoaHoc ID={k.MaKhoaHoc}, Subject={k.MonHoc?.MaCodeMonHoc} (\"{k.MonHoc?.TenMonHoc}\"), Teacher=\"{k.GiaoVien?.HoTen}\" (TeacherId={k.MaGiaoVien})");
            }

            // Remove any other duplicate KhoaHocs for SD1902 if not referenced by ThoiKhoaBieu
            var dups = sd1902KhoaHocs
                .GroupBy(k => k.MaMonHoc)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var g in dups)
            {
                var keep = g.First();
                var others = g.Skip(1).ToList();
                foreach (var o in others)
                {
                    var hasTkB = await db.ThoiKhoaBieus.AnyAsync(t => t.MaKhoaHoc == o.MaKhoaHoc);
                    if (!hasTkB)
                    {
                        TestContext.Progress.WriteLine($"Removing redundant KhoaHoc ID={o.MaKhoaHoc} for subject {o.MaMonHoc}");
                        db.KhoaHocs.Remove(o);
                    }
                }
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("\nSaved official lecturer updates!");
        }
    }
}
