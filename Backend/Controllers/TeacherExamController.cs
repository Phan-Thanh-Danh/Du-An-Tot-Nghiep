using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = AuthRoles.Teacher)]
public class TeacherExamController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherExamController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser != null) return currentUser.UserId;
        var claim = User.FindFirst(CustomClaimTypes.UserId);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    [HttpPost("exams")]
    public async Task<ActionResult<ApiResponseDto>> CreateExam(CreateTeacherExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized(ApiResponseDto.Fail("Không xác định được người dùng."));

        if (string.IsNullOrWhiteSpace(request.TenKyThi))
            return BadRequest(ApiResponseDto.Fail("Tên đề thi là bắt buộc."));

        if (request.ThoiGianThi <= 0)
            return BadRequest(ApiResponseDto.Fail("Thời gian thi phải lớn hơn 0."));

        var khoaHoc = await _context.KhoaHocs
            .Include(k => k.MonHoc)
            .Include(k => k.Lop)
            .FirstOrDefaultAsync(k =>
                k.MaGiaoVien == userId &&
                k.MonHoc != null && k.MonHoc.TenMonHoc == request.TenMonHoc &&
                k.Lop != null && k.Lop.MaCodeLop == request.MaLop);

        if (khoaHoc == null)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy lớp/môn học phù hợp hoặc bạn không phải giảng viên phụ trách."));

        if (!string.IsNullOrEmpty(request.ThoiGianMo) && !string.IsNullOrEmpty(request.ThoiGianDong))
        {
            if (DateTime.Parse(request.ThoiGianMo) >= DateTime.Parse(request.ThoiGianDong))
                return BadRequest(ApiResponseDto.Fail("Thời gian mở phải trước thời gian đóng."));
        }

        var cauHinh = new
        {
            request.MoTa,
            request.DiemDat,
            request.SoLanLamToiDa,
            request.XaoTronCauHoi,
            request.HienThiKetQua,
            request.ChoPhepLamTruoc,
            ThoiGianMo = request.ThoiGianMo,
            ThoiGianDong = request.ThoiGianDong,
            MaLop = request.MaLop,
            TenMonHoc = request.TenMonHoc
        };

        var deKiemTra = new DeKiemTra
        {
            MaMonHoc = khoaHoc.MaMonHoc,
            MaHocKy = khoaHoc.MaHocKy,
            TieuDe = request.TenKyThi,
            ThoiGianPhut = request.ThoiGianThi,
            LoaiDeThi = request.LoaiDeThi,
            HinhThucThi = request.LoaiDeThi,
            TrangThai = request.TrangThai ?? "ban_nhap",
            MaNguoiSoan = userId,
            NgayTao = DateTime.UtcNow,
            CauHinhDeThi = System.Text.Json.JsonSerializer.Serialize(cauHinh)
        };

        _context.DeKiemTras.Add(deKiemTra);
        await _context.SaveChangesAsync();

        if (request.DanhSachCauHoi != null && request.DanhSachCauHoi.Count > 0)
        {
            foreach (var item in request.DanhSachCauHoi)
            {
                if (int.TryParse(item.MaCauHoi, out int maCauHoi))
                {
                    var exists = await _context.CauHois.AnyAsync(c => c.MaCauHoi == maCauHoi);
                    if (!exists) continue;

                    _context.CauHoiDeKiemTras.Add(new CauHoiDeKiemTra
                    {
                        MaDeKiemTra = deKiemTra.MaDeKiemTra,
                        MaCauHoi = maCauHoi,
                        DiemSo = item.DiemSo
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponseDto.Ok("Tạo đề thi thành công"));
    }
}

public class CreateTeacherExamRequest
{
    public string TenKyThi { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public string MaLop { get; set; } = string.Empty;
    public string LoaiDeThi { get; set; } = string.Empty;
    public int ThoiGianThi { get; set; }
    public decimal? DiemDat { get; set; }
    public int? SoLanLamToiDa { get; set; }
    public bool XaoTronCauHoi { get; set; }
    public bool HienThiKetQua { get; set; }
    public bool ChoPhepLamTruoc { get; set; }
    public string? ThoiGianMo { get; set; }
    public string? ThoiGianDong { get; set; }
    public string? TrangThai { get; set; }
    public List<TeacherExamQuestionItem> DanhSachCauHoi { get; set; } = [];
}

public class TeacherExamQuestionItem
{
    public string MaCauHoi { get; set; } = string.Empty;
    public decimal DiemSo { get; set; }
}
