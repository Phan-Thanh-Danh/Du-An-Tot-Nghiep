using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Public endpoint — không cần đăng nhập.
    /// Cho phép nhà tuyển dụng xác thực bằng khen của ứng viên.
    /// </summary>
    [HttpGet("certificates/{code}/verify")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyCertificate(
        [FromRoute] string code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 20)
        {
            return BadRequest(new
            {
                valid = false,
                message = "Mã xác thực không hợp lệ."
            });
        }

        var result = await _context.KhenThuongs
            .AsNoTracking()
            .Where(k => k.MaCodeXacThuc == code && !k.DaHuy)
            .Select(k => new
            {
                valid = true,
                hoTen = k.HoTenSnapshot ?? (k.HocSinh != null ? k.HocSinh.HoTen : null),
                danhHieu = k.DanhHieuSnapshot ?? k.LoaiKhenThuong,
                ngayCap = k.NgayCap ?? k.CapLuc,
                tenHocKy = k.TenHocKySnapshot ?? (k.HocKy != null ? k.HocKy.TenHocKy : null),
                donVi = k.DotKhenThuong != null
                    ? k.DotKhenThuong.TenDot
                    : k.DonVi != null ? k.DonVi.TenDonVi : null,
                maCodeXacThuc = k.MaCodeXacThuc
                // KHÔNG trả: email, so_dien_thoai, mat_khau_hash, MaHocSinh
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return Ok(new
            {
                valid = false,
                message = "Không tìm thấy bằng khen với mã xác thực này."
            });
        }

        return Ok(result);
    }
}
