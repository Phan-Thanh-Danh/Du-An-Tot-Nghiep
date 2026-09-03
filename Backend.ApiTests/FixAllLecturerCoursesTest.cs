using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class FixAllLecturerCoursesTest
    {
        [Test]
        public async Task FixCoursesForLecturer()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var lecturer = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
            Assert.That(lecturer, Is.Not.Null);

            var teacherP12 = await db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "p12test_teacher01@lms.local");

            // Update all courses of Class SD1902 (MaLop = 2) to MaGiaoVien = 15 (lecturer01@edulms.local)
            var sd1902Courses = await db.KhoaHocs.Where(k => k.MaLop == 2).ToListAsync();
            foreach (var c in sd1902Courses)
            {
                c.MaGiaoVien = lecturer.MaNguoiDung;
            }

            // Also specifically ensure Course 26 (Cơ sở dữ liệu - SD1902) is MaGiaoVien = 15
            var course26 = await db.KhoaHocs.FirstOrDefaultAsync(k => k.MaKhoaHoc == 26);
            if (course26 != null)
            {
                course26.MaGiaoVien = lecturer.MaNguoiDung;
                course26.TrangThai = "da_xuat_ban";
            }

            // Also check all COM102 courses
            var allCom102 = await db.KhoaHocs.Where(k => k.TieuDe.Contains("Cơ sở dữ liệu") && k.MaLop == 2).ToListAsync();
            foreach (var c in allCom102)
            {
                c.MaGiaoVien = lecturer.MaNguoiDung;
                c.TrangThai = "da_xuat_ban";
            }

            await db.SaveChangesAsync();

            // Verify
            var verified = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.GiaoVien)
                .Where(k => k.MaLop == 2 && k.MaGiaoVien == lecturer.MaNguoiDung)
                .ToListAsync();

            TestContext.Progress.WriteLine($"=== VERIFIED COURSES FOR LECTURER ID={lecturer.MaNguoiDung} (SD1902) ===");
            foreach (var c in verified)
            {
                TestContext.Progress.WriteLine($"Course ID={c.MaKhoaHoc}, Title='{c.TieuDe}', Subject='{c.MonHoc?.TenMonHoc}' ({c.MonHoc?.MaCodeMonHoc}), Teacher='{c.GiaoVien?.HoTen}' ({c.GiaoVien?.Email})");
            }
        }
    }
}
