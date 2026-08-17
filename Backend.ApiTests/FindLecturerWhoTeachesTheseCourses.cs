using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class FindLecturerWhoTeachesTheseCourses
    {
        [Test]
        public async Task FindTeacherAccount()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // Tìm các khóa học có trong ảnh của user
            var coursesInPhoto = await db.KhoaHocs
                .Include(k => k.GiaoVien)
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Include(k => k.HocKy)
                .Where(k => k.MaLop == 2)
                .ToListAsync();

            TestContext.Progress.WriteLine($"=== ALL COURSES FOR SD1902 (MaLop=2) ===");
            foreach (var c in coursesInPhoto)
            {
                TestContext.Progress.WriteLine($"Course ID={c.MaKhoaHoc}, Subject='{c.MonHoc?.TenMonHoc}' (Code={c.MonHoc?.MaCodeMonHoc}), TeacherID={c.MaGiaoVien}, TeacherEmail='{c.GiaoVien?.Email}', TeacherName='{c.GiaoVien?.HoTen}', Term='{c.HocKy?.TenHocKy}', Status='{c.TrangThai}', DonVi={c.MaDonVi}");
            }

            // Tìm các khóa học môn COM102 (Cơ sở dữ liệu) trong toàn hệ thống
            var com102Courses = await db.KhoaHocs
                .Include(k => k.GiaoVien)
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Include(k => k.HocKy)
                .Where(k => k.MonHoc.MaCodeMonHoc == "COM102" || k.TieuDe.Contains("Cơ sở dữ liệu"))
                .ToListAsync();

            TestContext.Progress.WriteLine($"\n=== ALL COM102 COURSES IN DB ===");
            foreach (var c in com102Courses)
            {
                TestContext.Progress.WriteLine($"Course ID={c.MaKhoaHoc}, Title='{c.TieuDe}', ClassID={c.MaLop}, ClassName='{c.Lop?.TenLop}', TeacherID={c.MaGiaoVien}, TeacherEmail='{c.GiaoVien?.Email}', Status='{c.TrangThai}', DonVi={c.MaDonVi}");
            }
        }
    }
}
