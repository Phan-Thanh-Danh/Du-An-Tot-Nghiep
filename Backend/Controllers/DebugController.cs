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
        public DebugController(ApplicationDbContext context) { _context = context; }

        [HttpGet("course/{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
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
    }
}
