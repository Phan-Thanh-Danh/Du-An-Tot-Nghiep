using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class ReassignAllSd1902CoursesTest
    {
        [Test]
        public async Task ReassignSd1902CoursesToLecturers()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // Tìm tất cả tài khoản giảng viên test
            var lecturers = await db.NguoiDungs
                .Where(u => u.Email.Contains("lecturer") || u.Email.Contains("teacher") || u.VaiTroChinh == "giao_vien" || u.VaiTroChinh == "Teacher")
                .ToListAsync();

            TestContext.Progress.WriteLine($"Found {lecturers.Count} lecturer accounts in DB.");

            var mainLecturer = lecturers.FirstOrDefault(u => u.Email == "lecturer01@edulms.local") ?? lecturers.First();
            var p12Teacher = lecturers.FirstOrDefault(u => u.Email == "p12test_teacher01@lms.local");

            TestContext.Progress.WriteLine($"Main Lecturer: ID={mainLecturer.MaNguoiDung}, Name={mainLecturer.HoTen}, Email={mainLecturer.Email}");
            if (p12Teacher != null)
            {
                TestContext.Progress.WriteLine($"P12 Teacher: ID={p12Teacher.MaNguoiDung}, Name={p12Teacher.HoTen}, Email={p12Teacher.Email}");
            }

            // Lấy tất cả khóa học của lớp SD1902 (MaLop = 2)
            var sd1902Courses = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Where(k => k.MaLop == 2)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Found {sd1902Courses.Count} courses for Class SD1902 (ID=2):");
            foreach (var c in sd1902Courses)
            {
                TestContext.Progress.WriteLine($" - Course ID={c.MaKhoaHoc}, Title='{c.TieuDe}', Subject='{c.MonHoc?.TenMonHoc}' (Code={c.MonHoc?.MaCodeMonHoc}), Current Teacher ID={c.MaGiaoVien}");
                // Gán giảng viên chính
                c.MaGiaoVien = mainLecturer.MaNguoiDung;
            }

            // Đảm bảo có môn COM102 (Cơ sở dữ liệu) cho lớp SD1902
            var com102Subject = await db.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaCodeMonHoc == "COM102");
            Assert.That(com102Subject, Is.Not.Null);

            var com102Course = sd1902Courses.FirstOrDefault(c => c.MaMonHoc == com102Subject.MaMonHoc);
            if (com102Course == null)
            {
                var newCourse = new KhoaHoc
                {
                    MaMonHoc = com102Subject.MaMonHoc,
                    MaLop = 2,
                    MaGiaoVien = mainLecturer.MaNguoiDung,
                    MaDonVi = mainLecturer.MaDonVi,
                    TieuDe = "Cơ sở dữ liệu - SD1902",
                    TrangThai = "da_xuat_ban",
                    NgayTao = DateTime.UtcNow
                };
                db.KhoaHocs.Add(newCourse);
                TestContext.Progress.WriteLine("Created new COM102 course for SD1902");
            }
            else
            {
                com102Course.MaGiaoVien = mainLecturer.MaNguoiDung;
            }

            // Nếu có tài khoản p12Teacher, tạo thêm khóa học cho p12Teacher dạy lớp SD1902 nếu chưa có
            if (p12Teacher != null)
            {
                var p12Course = await db.KhoaHocs.FirstOrDefaultAsync(k => k.MaLop == 2 && k.MaGiaoVien == p12Teacher.MaNguoiDung && k.MaMonHoc == com102Subject.MaMonHoc);
                if (p12Course == null)
                {
                    db.KhoaHocs.Add(new KhoaHoc
                    {
                        MaMonHoc = com102Subject.MaMonHoc,
                        MaLop = 2,
                        MaGiaoVien = p12Teacher.MaNguoiDung,
                        MaDonVi = p12Teacher.MaDonVi,
                        TieuDe = "Cơ sở dữ liệu - SD1902 (GV: P12 Test)",
                        TrangThai = "da_xuat_ban",
                        NgayTao = DateTime.UtcNow
                    });
                }
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("Successfully assigned SD1902 to lecturers!");
        }
    }
}
