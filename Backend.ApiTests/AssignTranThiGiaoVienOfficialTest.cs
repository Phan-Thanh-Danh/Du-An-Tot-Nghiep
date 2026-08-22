using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class AssignTranThiGiaoVienOfficialTest
    {
        [Test]
        public async Task AssignTranThiGiaoVienToCnttClasses()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var tranThiGV = await db.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == 15);
            Assert.That(tranThiGV, Is.Not.Null);
            TestContext.Progress.WriteLine($"Target Official Lecturer: ID={tranThiGV.MaNguoiDung}, Name=\"{tranThiGV.HoTen}\", Email=\"{tranThiGV.Email}\"");

            // Update all courses of SD1901 (MaLop=1) and SD1902 (MaLop=2)
            var targetCourses = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Where(k => k.MaLop == 1 || k.MaLop == 2)
                .ToListAsync();

            foreach (var k in targetCourses)
            {
                k.MaGiaoVien = tranThiGV.MaNguoiDung;
                TestContext.Progress.WriteLine($"Set KhoaHoc ID={k.MaKhoaHoc} (Class: {k.Lop?.TenLop}, Subject: {k.MonHoc?.MaCodeMonHoc}) -> Teacher: {tranThiGV.HoTen}");
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("Successfully assigned Trần Thị Giảng Viên to all SD1901 & SD1902 courses!");
        }
    }
}
