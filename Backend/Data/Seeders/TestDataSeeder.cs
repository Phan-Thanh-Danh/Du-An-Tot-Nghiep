using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeders;

public static class TestDataSeeder
{
    public static async Task<string> SeedFreshTermForTestingAsync(ApplicationDbContext db)
    {
        // 0. Khóa tất cả các học kỳ cũ lại để HK1-2029 là kỳ tương lai duy nhất hợp lệ
        var oldTerms = await db.HocKys.Where(x => x.MaCodeHocKy != "HK1-2029").ToListAsync();
        foreach (var t in oldTerms)
        {
            t.DaKhoa = true;
        }
        
        // 1. Tạo học kỳ hoàn toàn mới, chưa từng có trong 8 kỳ hiện tại
        var lopSD1901 = await db.LopHanhChinhs.FirstOrDefaultAsync(l => l.TenLop.Contains("SD1901") && l.ConHoatDong);
        if (lopSD1901 == null) return "Không tìm thấy lớp SD1901.";

        var newTerm = await db.HocKys.FirstOrDefaultAsync(x => x.MaCodeHocKy == "HK1-2029");
        if (newTerm == null)
        {
            newTerm = new HocKy
            {
                MaDonVi = lopSD1901.MaDonVi,
                MaCodeHocKy = "HK1-2029",
                TenHocKy = "Học kỳ 1 năm 2029",
                NgayBatDau = new DateOnly(2029, 1, 6),
                NgayKetThuc = new DateOnly(2029, 4, 28),
                NamHoc = "2028-2029",
                ThuTuTrongNam = 1,
                DaKhoa = false
            };
            db.HocKys.Add(newTerm);
            await db.SaveChangesAsync();
        }

        // 2. Tìm 1 môn học SD1901 CHƯA từng học (tránh trùng với 3 khóa học đã publish)
        var maMonHocDaHoc = await db.KhoaHocs
            .Where(k => k.MaLop == lopSD1901.MaLop)
            .Select(k => k.MaMonHoc)
            .Distinct()
            .ToListAsync();

        var monHocMoi = await db.DanhMucMonHocs
            .FirstOrDefaultAsync(m => m.ConHoatDong && !maMonHocDaHoc.Contains(m.MaMonHoc));

        if (monHocMoi == null) return "Không tìm thấy môn học mới nào cho lớp này.";

        var giaoVien = await db.NguoiDungs
            .FirstOrDefaultAsync(u => (u.VaiTroChinh == "Teacher" || u.VaiTroChinh == "giao_vien") && u.TrangThai == "hoat_dong");

        if (giaoVien == null) return "Không tìm thấy giáo viên nào hợp lệ.";

        // Check if course already exists
        var existingCourse = await db.KhoaHocs.FirstOrDefaultAsync(c => c.MaHocKy == newTerm.MaHocKy && c.MaLop == lopSD1901.MaLop && c.MaMonHoc == monHocMoi.MaMonHoc);
        if (existingCourse != null) return $"Khóa học {monHocMoi.TenMonHoc} đã tồn tại trong kỳ này.";

        // 3. Tạo khóa học mới, chưa có TKB nào — trạng thái nháp
        var newCourse = new KhoaHoc
        {
            MaDonVi = lopSD1901.MaDonVi,
            MaGiaoVien = giaoVien.MaNguoiDung,
            MaMonHoc = monHocMoi.MaMonHoc,
            MaHocKy = newTerm.MaHocKy,
            MaLop = lopSD1901.MaLop,
            TieuDe = monHocMoi.TenMonHoc,
            TrangThai = "nhap",
            NgayTao = DateTime.UtcNow
        };
        db.KhoaHocs.Add(newCourse);
        await db.SaveChangesAsync();
        return $"Đã seed thành công khóa học {monHocMoi.TenMonHoc} cho học kỳ 2029.";
    }
}
