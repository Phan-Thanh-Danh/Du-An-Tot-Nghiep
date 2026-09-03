using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Backend.ApiTests
{
    public class ClassWorkspaceInspector
    {
        [Test]
        public async Task InspectClassWorkspaceData()
        {
            var connStr = TestDatabaseSafetyGuard.GetVerifiedTestConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;

            using var db = new ApplicationDbContext(options);

            var khoaHoc = await db.KhoaHocs
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .FirstOrDefaultAsync(k => k.MaKhoaHoc == 26 || (k.Lop != null && k.Lop.TenLop.Contains("SD1902")));

            Assert.That(khoaHoc, Is.Not.Null);
            TestContext.Progress.WriteLine($"KhoaHoc: ID={khoaHoc.MaKhoaHoc}, TieuDe={khoaHoc.TieuDe}, MaMonHoc={khoaHoc.MaMonHoc}, MaLop={khoaHoc.MaLop}, MaGiaoVien={khoaHoc.MaGiaoVien}");

            // Check students in class
            var students = await db.NguoiDungs
                .Where(n => n.MaLop == khoaHoc.MaLop && (n.VaiTroChinh == "hoc_sinh" || n.VaiTroChinh == "Student"))
                .ToListAsync();
            TestContext.Progress.WriteLine($"Students count in class {khoaHoc.MaLop}: {students.Count}");

            // Check lessons for this subject
            var lessons = await db.BaiHocs
                .Where(b => b.Chuong != null && b.Chuong.MaMonHoc == khoaHoc.MaMonHoc)
                .ToListAsync();
            TestContext.Progress.WriteLine($"Lessons count for MonHoc {khoaHoc.MaMonHoc}: {lessons.Count}");

            // Check TienDoBaiHoc
            var lessonIds = lessons.Select(l => l.MaBaiHoc).ToList();
            var tienDos = await db.TienDoBaiHocs
                .Where(t => lessonIds.Contains(t.MaBaiHoc))
                .ToListAsync();
            TestContext.Progress.WriteLine($"TienDoBaiHoc total records for this course lessons: {tienDos.Count}");

            foreach (var st in students.Take(5))
            {
                var completedCount = tienDos.Count(t => t.MaHocSinh == st.MaNguoiDung && t.HoanThanhLuc != null);
                TestContext.Progress.WriteLine($" - Student {st.HoTen} (ID={st.MaNguoiDung}): completed {completedCount}/{lessons.Count} lessons");
            }

            // Check BuoiHoc for this course
            var buoiHocs = await db.BuoiHocs
                .Where(b => b.MaKhoaHoc == khoaHoc.MaKhoaHoc)
                .OrderBy(b => b.NgayHoc)
                .ToListAsync();
            TestContext.Progress.WriteLine($"BuoiHocs count: {buoiHocs.Count}");
            foreach (var bh in buoiHocs)
            {
                var diemDanhCount = await db.DiemDanhs.CountAsync(d => d.MaBuoiHoc == bh.MaBuoiHoc);
                var coMatCount = await db.DiemDanhs.CountAsync(d => d.MaBuoiHoc == bh.MaBuoiHoc && d.TrangThai == "co_mat");
                TestContext.Progress.WriteLine($" - BuoiHoc ID={bh.MaBuoiHoc}, Ngay={bh.NgayHoc}, TrangThaiBuoi={bh.TrangThaiBuoi}, TrangThaiDiemDanh={bh.TrangThaiDiemDanh}, DiemDanhCount={diemDanhCount}, CoMat={coMatCount}");
            }
        }
    }
}
