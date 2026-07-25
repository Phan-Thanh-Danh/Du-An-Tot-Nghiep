using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DebugController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private IActionResult? EnsureDevelopmentOnly()
        {
            if (!_env.IsDevelopment()) return NotFound();
            return null;
        }

        [HttpGet("course/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var guard = EnsureDevelopmentOnly();
            if (guard != null) return guard;
            var khoaHoc = await _context.KhoaHocs.Include(k => k.MonHoc).Include(k => k.Lop).FirstOrDefaultAsync(k => k.MaKhoaHoc == id);
            if (khoaHoc == null) return NotFound();
            return Ok(new {
                MaLop = khoaHoc.MaLop,
                TenLop = khoaHoc.Lop?.TenLop,
                MaMonHoc = khoaHoc.MaMonHoc,
                TenMonHoc = khoaHoc.MonHoc?.TenMonHoc,
                TieuDe = khoaHoc.TieuDe,
                StudentCount = await _context.NguoiDungs.CountAsync(n => n.MaLop == khoaHoc.MaLop && n.VaiTroChinh == "hoc_sinh")
            });
        }

        [HttpPost("reset-cathi")]
        public async Task<IActionResult> ResetCaThi()
        {
            var guard = EnsureDevelopmentOnly();
            if (guard != null) return guard;

            try
            {
                var teacher = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == "lecturer01@edulms.local");
                if (teacher == null) return BadRequest("Teacher lecturer01@edulms.local not found");

                var students = await _context.NguoiDungs.Where(u => u.Email.Contains("sd1904") && u.VaiTroChinh == "hoc_sinh").ToListAsync();
                
                var kyThi = await _context.KyThis.FirstOrDefaultAsync();
                if (kyThi == null) 
                {
                    kyThi = new KyThi { TenKyThi = "Kỳ thi Test", MaHocKy = 1, NgayTao = DateTime.UtcNow };
                    _context.KyThis.Add(kyThi);
                    await _context.SaveChangesAsync();
                }

                // Delete all existing exam data to avoid duplicates
                _context.ThiSinhCaThis.RemoveRange(_context.ThiSinhCaThis);
                _context.PhanCongGiamThis.RemoveRange(_context.PhanCongGiamThis);
                _context.CaThis.RemoveRange(_context.CaThis);
                _context.LichThiTongs.RemoveRange(_context.LichThiTongs);
                await _context.SaveChangesAsync();

                string[] codes = { "COM102", "COM103", "WEB101" };
                string[] names = { "COM102 - Cơ sở dữ liệu", "COM103 - Lập trình C#", "WEB101 - Thiết kế Web" };

                for (int i = 0; i < 3; i++)
                {
                    var monHoc = await _context.DanhMucMonHocs.FirstOrDefaultAsync(m => m.MaCodeMonHoc == codes[i]);
                    if (monHoc == null) continue;

                    var deKiemTra = await _context.DeKiemTras.FirstOrDefaultAsync(d => d.MaMonHoc == monHoc.MaMonHoc);

                    var lichThi = new LichThiTong
                    {
                        MaKyThi = kyThi.MaKyThi,
                        MaMonHoc = monHoc.MaMonHoc,
                        MaDeKiemTra = deKiemTra?.MaDeKiemTra,
                        HinhThucThi = "online_tap_trung",
                        NgayThiDuKien = DateTime.UtcNow,
                        TrangThai = "da_gui_ve_co_so",
                        NgayTao = DateTime.UtcNow,
                    };
                    _context.LichThiTongs.Add(lichThi);
                    await _context.SaveChangesAsync();

                    var caThi = new CaThi
                    {
                        MaLichThiTong = lichThi.MaLichThiTong,
                        TenCaThi = names[i],
                        MaDonVi = 1,
                        NgayThi = DateTime.UtcNow.Date,
                        ThoiGianBatDau = DateTime.UtcNow.AddHours(-1),
                        ThoiGianKetThuc = DateTime.UtcNow.AddDays(1),
                        TrangThai = "da_san_sang",
                        NgayTao = DateTime.UtcNow
                    };
                    _context.CaThis.Add(caThi);
                    await _context.SaveChangesAsync();

                    _context.PhanCongGiamThis.Add(new PhanCongGiamThi
                    {
                        MaCaThi = caThi.MaCaThi,
                        MaGiamThi = teacher.MaNguoiDung,
                        VaiTroGiamThi = "giam_thi_chinh",
                        TrangThai = "da_xac_nhan",
                        NgayTao = DateTime.UtcNow
                    });

                    foreach (var student in students)
                    {
                        _context.ThiSinhCaThis.Add(new ThiSinhCaThi
                        {
                            MaCaThi = caThi.MaCaThi,
                            MaHocSinh = student.MaNguoiDung,
                            TrangThaiDuThi = "cho_thi",
                            NgayTao = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return Ok(new { success = true, teacherId = teacher.MaNguoiDung, studentCount = students.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
    }
}
}
