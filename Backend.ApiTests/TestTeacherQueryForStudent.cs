using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class TestTeacherQueryForStudent
    {
        [Test]
        public async Task TestLecturerQueryMismatch()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var students = await db.NguoiDungs.Where(u => u.MaNguoiDung == 23 || u.MaNguoiDung == 24).ToListAsync();
            foreach (var student in students)
            {
                TestContext.Progress.WriteLine($"\n==========================================");
                TestContext.Progress.WriteLine($"Student ID={student.MaNguoiDung}, Name=\"{student.HoTen}\", MaLop={student.MaLop}");

                // 1. Check list query (GetCourses)
                var coursesInList = await db.KhoaHocs
                    .Include(k => k.MonHoc)
                    .Include(k => k.GiaoVien)
                    .Where(k => k.MaLop == student.MaLop && k.TrangThai == "da_xuat_ban")
                    .ToListAsync();

                foreach (var k in coursesInList)
                {
                    TestContext.Progress.WriteLine($"  [GetCourses List] KhoaHoc ID={k.MaKhoaHoc}, SubjectCode={k.MonHoc?.MaCodeMonHoc}, SubjectName=\"{k.MonHoc?.TenMonHoc}\", Teacher=\"{k.GiaoVien?.HoTen}\" (TeacherId={k.MaGiaoVien})");
                }

                // 2. Check detail query (GetCourseDetail)
                var courseCode = "COM102";
                var assignedCourse = await db.KhoaHocs
                    .Include(k => k.MonHoc)
                    .Include(k => k.GiaoVien)
                    .Include(k => k.HocKy)
                    .FirstOrDefaultAsync(k => k.MaLop == student.MaLop && k.MonHoc!.MaCodeMonHoc == courseCode && k.TrangThai == "da_xuat_ban");

                TestContext.Progress.WriteLine($"  [GetCourseDetail for COM102] AssignedCourse ID={assignedCourse?.MaKhoaHoc}, Teacher=\"{assignedCourse?.GiaoVien?.HoTen}\" (TeacherId={assignedCourse?.MaGiaoVien})");
                
                // Let's also check all KhoaHocs for COM102 in DB
                var allCom102KhoaHocs = await db.KhoaHocs
                    .Include(k => k.Lop)
                    .Include(k => k.GiaoVien)
                    .Where(k => k.MonHoc!.MaCodeMonHoc == courseCode)
                    .ToListAsync();

                TestContext.Progress.WriteLine($"  --- All KhoaHocs for COM102 in DB ---");
                foreach (var kh in allCom102KhoaHocs)
                {
                    TestContext.Progress.WriteLine($"     KhoaHoc ID={kh.MaKhoaHoc}, ClassId={kh.MaLop}, ClassName=\"{kh.Lop?.TenLop}\", Teacher=\"{kh.GiaoVien?.HoTen}\" (TeacherId={kh.MaGiaoVien}), TrangThai=\"{kh.TrangThai}\"");
                }
            }
        }
    }
}
