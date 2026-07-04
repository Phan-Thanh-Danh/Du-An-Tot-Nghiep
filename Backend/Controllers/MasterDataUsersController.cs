using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/master-data/users")]
[Authorize]
public class MasterDataUsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MasterDataUsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers(CancellationToken cancellationToken)
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        var campusId = currentUser?.CampusId;

        var teachers = await _db.NguoiDungs
            .Where(u => u.VaiTroChinh == "giao_vien" && u.TrangThai == "hoat_dong" && (campusId == null || u.MaDonVi == campusId))
            .Select(u => new
            {
                MaNguoiDung = u.MaNguoiDung,
                HoTen = u.HoTen,
                ChuyenNganhs = _db.GiaoVienChuyenNganhs
                    .Where(g => g.MaGiaoVien == u.MaNguoiDung)
                    .Select(g => g.MaChuyenNganh)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponseDto<object>.Ok(teachers));
    }
}
