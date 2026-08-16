using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class AssignLecturerAndInspectDbTest
    {
        [Test]
        public async Task AssignLecturerToCom102AndCheck()
        {
            var connStr = "Server=localhost,1433;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            // 1. Tìm tài khoản giảng viên (p12test_lecturer01 hoặc lecturer)
            var lecturer = await db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email.Contains("lecturer"));

            if (lecturer == null)
            {
                lecturer = await db.NguoiDungs
                    .FirstOrDefaultAsync(u => u.VaiTroChinh == "Teacher" || u.VaiTroChinh == "Lecturer");
            }

            Assert.That(lecturer, Is.Not.Null, "Không tìm thấy tài khoản giảng viên");
            TestContext.Progress.WriteLine($"Found Lecturer: ID={lecturer.MaNguoiDung}, Name={lecturer.HoTen}, Email={lecturer.Email}, VaiTroChinh={lecturer.VaiTroChinh}");

            // 2. Tìm tài khoản sinh viên p12test
            var student = await db.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email.Contains("p12test_student01"));

            Assert.That(student, Is.Not.Null, "Không tìm thấy sinh viên p12test");
            TestContext.Progress.WriteLine($"Found Student: ID={student.MaNguoiDung}, Name={student.HoTen}, ClassID={student.MaLop}");

            // 3. Tìm môn học COM102
            var com102Subject = await db.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaCodeMonHoc == "COM102");
            Assert.That(com102Subject, Is.Not.Null, "Không tìm thấy môn COM102");

            // 4. Tìm các khóa học (KhoaHoc) của COM102
            var courses = await db.KhoaHocs
                .Include(k => k.Lop)
                .Include(k => k.GiaoVien)
                .Where(k => k.MaMonHoc == com102Subject.MaMonHoc)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Found {courses.Count} KhoaHoc for COM102:");
            foreach (var c in courses)
            {
                TestContext.Progress.WriteLine($" - Course ID: {c.MaKhoaHoc}, Title: {c.TieuDe}, ClassID: {c.MaLop}, Teacher: {c.GiaoVien?.HoTen} (ID: {c.MaGiaoVien})");
            }

            // Gán giảng viên lecturer vào tất cả KhoaHoc của COM102 (đặc biệt là lớp của sinh viên student.MaLop)
            foreach (var c in courses)
            {
                c.MaGiaoVien = lecturer.MaNguoiDung;
            }

            // Nếu chưa có khóa học cho lớp của sinh viên, tạo mới
            if (student.MaLop.HasValue)
            {
                var studentCourse = courses.FirstOrDefault(c => c.MaLop == student.MaLop.Value);
                if (studentCourse == null)
                {
                    var newCourse = new KhoaHoc
                    {
                        MaMonHoc = com102Subject.MaMonHoc,
                        MaLop = student.MaLop.Value,
                        MaGiaoVien = lecturer.MaNguoiDung,
                        MaDonVi = student.MaDonVi,
                        TieuDe = "Cơ sở dữ liệu - " + com102Subject.TenMonHoc,
                        TrangThai = "da_xuat_ban",
                        NgayTao = DateTime.UtcNow
                    };
                    db.KhoaHocs.Add(newCourse);
                    TestContext.Progress.WriteLine($"Created new KhoaHoc for ClassID {student.MaLop.Value}");
                }
            }

            // 5. Kiểm tra bài học thứ 3 của COM102: "Cách tạo Bảng (Table) và thao tác cấu trúc dữ liệu"
            var lessons = await db.BaiHocs
                .Include(b => b.Chuong)
                .Where(b => b.Chuong!.MaMonHoc == com102Subject.MaMonHoc)
                .OrderBy(b => b.ThuTu)
                .ToListAsync();

            TestContext.Progress.WriteLine($"Total lessons in COM102: {lessons.Count}");
            for (int i = 0; i < lessons.Count; i++)
            {
                var l = lessons[i];
                TestContext.Progress.WriteLine($" Lesson {i + 1}: ID={l.MaBaiHoc}, Title={l.TieuDe}, ThuTu={l.ThuTu}");
            }

            await db.SaveChangesAsync();
            TestContext.Progress.WriteLine("SUCCESSFULLY UPDATED DATABASE!");
        }
    }
}
